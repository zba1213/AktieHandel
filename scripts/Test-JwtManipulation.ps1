param([int]$Port = 5191)

$ErrorActionPreference = 'Stop'
$apiDirectory = Join-Path $PSScriptRoot '../AktieHandelRestApi'
$baseUrl = "http://127.0.0.1:$Port"
$roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
# Opgavens nøgle har kun 30 ASCII-bytes. HS256 kræver mindst 32.
$weakKey = 'password12345678901234567890123456'

function ConvertTo-Base64Url([byte[]]$Bytes) {
    [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Read-Payload([string]$Token) {
    $part = $Token.Split('.')[1].Replace('-', '+').Replace('_', '/')
    $part = $part.PadRight($part.Length + (4 - $part.Length % 4) % 4, '=')
    [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($part)) | ConvertFrom-Json
}

function Change-Role([string]$Token, [string]$Role, [string]$SigningKey = '') {
    $parts = $Token.Split('.')
    $payload = Read-Payload $Token
    $payload.$roleClaim = $Role
    $encoded = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes(($payload | ConvertTo-Json -Compress)))
    $message = $parts[0] + '.' + $encoded
    if ($SigningKey) {
        $hmac = [Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($SigningKey))
        try { $signature = ConvertTo-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($message))) }
        finally { $hmac.Dispose() }
    } else { $signature = $parts[2] }
    $message + '.' + $signature
}

function Start-TestApi([string]$Key = '') {
    $info = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $info.WorkingDirectory = (Resolve-Path $apiDirectory).Path
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.ArgumentList.Add('bin/Debug/net10.0/AktieHandelRestApi.dll')
    $info.ArgumentList.Add('--urls')
    $info.ArgumentList.Add($baseUrl)
    if ($Key) { $info.Environment['Jwt__Key'] = $Key }
    else { $info.Environment.Remove('Jwt__Key') | Out-Null }
    $process = [Diagnostics.Process]::Start($info)
    try {
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            if ($process.HasExited) { throw 'API-processen stoppede under opstart.' }
            try {
                $null = Invoke-WebRequest "$baseUrl/swagger/v1/swagger.json" -NoProxy -TimeoutSec 1
                return $process
            } catch { Start-Sleep -Milliseconds 300 }
        }
        throw 'API kunne ikke starte.'
    } catch { if (!$process.HasExited) { $process.Kill($true) }; throw }
}

function Stop-TestApi($Process) {
    if ($Process -and !$Process.HasExited) { $Process.Kill($true); $Process.WaitForExit() }
}

function Login([string]$Username) {
    (Invoke-RestMethod "$baseUrl/api/auth/login" -Method Post -ContentType application/json `
        -Body (@{ username = $Username; password = '1234' } | ConvertTo-Json) -NoProxy -TimeoutSec 5).token
}

function Check([string]$Label, [string]$Method, [string]$Path, [string]$Token, [int]$Expected) {
    $response = Invoke-WebRequest "$baseUrl$Path" -Method $Method `
        -Headers @{ Authorization = "Bearer $Token" } -NoProxy -TimeoutSec 5 -SkipHttpErrorCheck
    if ([int]$response.StatusCode -ne $Expected) {
        throw "$Label : forventede $Expected, fik $($response.StatusCode)"
    }
    Write-Output "$Label : $Expected"
}

$process = $null
try {
    $process = Start-TestApi
    $adminToken = Login 'admin'
    $payload = Read-Payload $adminToken
    Write-Output ('Synlige claims: ' + ($payload.PSObject.Properties.Name -join ', '))
    if ($payload.$roleClaim -ne 'Admin' -or $payload.sub -ne 'admin') { throw 'Uventede claims' }
    $userToken = Login 'user'
    Check 'User læser' GET '/api/AktieHandels' $userToken 200
    Check 'User må ikke slette' DELETE '/api/AktieHandels/999999' $userToken 403
    $tampered = Change-Role $userToken 'Admin'
    Check 'Ændret payload med original signatur' DELETE '/api/AktieHandels/999999' $tampered 401
    Stop-TestApi $process

    # Svag nøgle gælder kun denne proces; appsettings.json ændres ikke.
    $process = Start-TestApi $weakKey
    $weakToken = Login 'user'
    $superAdmin = Change-Role $weakToken 'SuperAdmin' $weakKey
    Check 'Signeret SuperAdmin kan læse' GET '/api/AktieHandels' $superAdmin 200
    Check 'SuperAdmin er ikke Admin' DELETE '/api/AktieHandels/999999' $superAdmin 403
    $forgedAdmin = Change-Role $weakToken 'Admin' $weakKey
    $created = Invoke-RestMethod "$baseUrl/api/AktieHandels" -Method Post -ContentType application/json `
        -Headers @{ Authorization = "Bearer $forgedAdmin" } `
        -Body '{"navn":"JWT Test","antal":1,"handelsPris":100}' -NoProxy -TimeoutSec 5
    Check 'Forfalsket Admin sletter' DELETE "/api/AktieHandels/$($created.handelsId)" $forgedAdmin 200
    Stop-TestApi $process

    $process = Start-TestApi
    Check 'Svagt signeret token efter cleanup' GET '/api/AktieHandels' $forgedAdmin 401
    Check 'Nyt token efter cleanup' GET '/api/AktieHandels' (Login 'admin') 200
} finally { Stop-TestApi $process }
