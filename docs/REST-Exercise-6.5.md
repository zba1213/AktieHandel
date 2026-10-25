# REST Exercise 6.5 – JWT Hacking & Manipulation

Udført lokalt den 25. oktober 2026 mod projektets API. Forsøgene er automatiseret
i `scripts/Test-JwtManipulation.ps1`; der er ikke brugt jwt.io eller Postman.

## Del 1: Payload kan læses

Et token fra `POST /api/auth/login` med `admin` / `1234` blev delt ved punktummerne.
Den midterste del blev Base64Url-dekodet til læsbar JSON uden brug af nøglen.

Følgende claims var synlige:

| Claim | Indhold |
|---|---|
| `sub` | `admin` |
| `jti` | Tokenets unikke ID |
| `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` | `admin` |
| `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | `Admin` |
| `exp` | Udløbstidspunkt som Unix-tid |
| `iss` | `AktieHandelRestApi` |
| `aud` | `AktieHandelClient` |

Svaret er altså ja: JWT-payloaden er kodet, ikke krypteret. Signaturen beskytter
integriteten, men skjuler ikke indholdet. Adgangskoder, CPR-numre og kortoplysninger
skal ikke placeres i payloaden. Login-adgangskoden findes ikke i dette token.

## Del 2: Manipulation uden ny signatur

Projektet understøtter allerede `user` / `1234`, så en midlertidig ændring af
AuthController var unødvendig. User-tokenets rolle blev ændret fra `User` til
`Admin`, mens den oprindelige signatur blev bevaret.

| Forsøg | Faktisk HTTP-status |
|---|---|
| User-token læser data | 200 |
| User-token forsøger DELETE | 403 |
| Ændret Admin-payload med original signatur forsøger DELETE | 401 |

Den oprindelige signatur passer ikke til det ændrede header/payload-indhold.
Signaturvalideringen afviser derfor tokenet, før endpointets kode udføres.

## Del 3: Kendt svag nøgle

Der er to rettelser til opgaveteksten:

1. `password123456789012345678901234` er 30 ASCII-bytes, ikke 32.
   Testen bruger `password12345678901234567890123456` (32 bytes), så HS256 kan anvendes.
2. `SuperAdmin` arver ikke rollen `Admin`. Et gyldigt SuperAdmin-token kan læse,
   men får stadig 403 på et endpoint med `[Authorize(Roles = "Admin")]`.

API'et blev startet med den svage nøgle som en `Jwt__Key`-miljøvariabel kun for
testprocessen. Konfigurationsfilen blev ikke ændret til den svage nøgle.
Et User-token fik ændret sin rolle og blev signeret igen med HMAC-SHA256 ved
hjælp af den kendte nøgle. Der blev ikke udført en ordbogsbaseret nøglesøgning;
forsøget demonstrerer konsekvensen af en kendt eller gættet nøgle.

| Forsøg | Faktisk HTTP-status |
|---|---|
| Signeret SuperAdmin-token læser | 200 |
| Signeret SuperAdmin-token forsøger DELETE | 403 |
| Forfalsket Admin-token opretter en testhandel og sletter den | DELETE: 200 |

Det forfalskede Admin-token bestod både signatur- og rollekontrollen.
Kendskab til en symmetrisk signeringsnøgle gør det muligt at udstede tokens
med vilkårlige roller.

## Del 4: Cleanup

`Jwt:Key` i appsettings.json er erstattet med 32 kryptografisk tilfældige bytes,
repræsenteret som 64 hextegn. Den svage nøgle var kun aktiv i testprocessen,
som blev stoppet. Den almindelige API-konfiguration blev derefter afprøvet:

| Forsøg efter cleanup | Faktisk HTTP-status |
|---|---|
| Token signeret med svag nøgle | 401 |
| Nyt Admin-token fra login | 200 |

Tidligere tokens skal erstattes ved nyt login efter nøgleskiftet.
Denne konfiguration er til øvelsen; ved deployment opbevares den aktive
signeringsnøgle uden for versionsstyrede filer, fx som `Jwt__Key`.

Ved push til GitHub er den lokale tilfældige nøgle udeladt. Den versionsstyrede
appsettings.json indeholder kun en offentlig demonstrationsnøgle. Ved en ny
kloning skal denne erstattes med en tilfældig nøgle, eller `Jwt__Key` sættes,
før cleanup-forsøget køres. Den lokale appsettings.json beholder den tilfældige nøgle.

## Gentag forsøget

Kør fra repositoryets rod i PowerShell 7:

```powershell
dotnet build AktieHandelRestApi/AktieHandelRestApi.csproj --no-restore -m:1
./scripts/Test-JwtManipulation.ps1
```

Scriptet bruger som standard port 5191, starter og stopper sine egne API-processer,
og fejler ved uventede HTTP-statuskoder. Det opretter kun testdata i disse
midlertidige processers hukommelse. Brug `-Port 5192`, hvis porten er optaget.
