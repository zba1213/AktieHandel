# Python Exercise Consume REST

Python-klient til repositoryets ASP.NET Core AktieHandel API. Menuen understøtter
alle fem krav: vis liste, vis ét objekt, opret, opdater og slet.
Begge bonuskrav er med: JWT-login og visning af HTTP-statuskoder.

## Start

Kræver Python 3 og projektets .NET 10 SDK. Kør fra repositoryets rod.
Start først API'et i en terminal:

```powershell
dotnet run --project AktieHandelRestApi
```

Installer requests, og start klienten i en anden terminal:

```powershell
python -m pip install -r PythonRestClient/requirements.txt
python PythonRestClient/client.py
```

Hvis Python hedder `py` på din computer, kan du bruge `py` i stedet for `python`.
Hvis API'et kører på en anden adresse:

```powershell
python PythonRestClient/client.py --url http://localhost:5173
```

Vælg **1: Login**. `admin` / `1234` kan udføre alle funktioner;
`user` / `1234` kan kun læse. Adgangskoden indtastes skjult, og JWT'en gemmes
kun i klientens hukommelse og sendes som `Authorization: Bearer <token>`.
Ved udløbet token vælges login igen.

## Afprøv alle krav

Vælg menupunkterne 2–6 manuelt, eller kør:

```powershell
python PythonRestClient/client.py --demo
```

Indtast admin-adgangskoden. Demoen henter listen, opretter en ny testhandel,
henter den via ID, opdaterer den og sletter den igen. Den bruger ID fra
POST-svaret, så eksisterende handler ikke ændres.

| Kald | Forventet status |
|---|---|
| Login med korrekte oplysninger | 200 |
| Hent liste / eksisterende objekt | 200 |
| Opret | 201 |
| Opdater / slet eksisterende objekt | 200 |
| Ukendt ID | 404 |
| Ugyldige værdier | 400 |
| Intet token eller forkert login | 401 |
| User forsøger at oprette, opdatere eller slette | 403 |

Serveren opbevarer data i hukommelsen, så de forsvinder ved genstart.

## Pair programming

Opgaven kræver samarbejde mellem to personer. Brug programmet som udgangspunkt:
Én er driver og betjener tastaturet, mens den anden er navigator og gennemgår
requests-kald, JSON, JWT og statuskoder. Byt roller undervejs, og afprøv sammen
admin-login, user-login, ukendt ID og ugyldige værdier. Koden alene dokumenterer
ikke, at kravet om pair programming er gennemført.
