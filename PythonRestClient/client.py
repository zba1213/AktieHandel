"""Konsolklient til AktieHandel API: CRUD, JWT og HTTP-statuskoder."""

import argparse
from getpass import getpass
import json
import math

import requests


class AktieHandelClient:
    def __init__(self, base_url):
        self.base_url = base_url.rstrip("/")
        self.session = requests.Session()

    def close(self):
        self.session.close()

    def request(self, method, path, data=None, hide_body=False):
        """Sender JSON og viser status før eventuelle HTTP-fejl håndteres."""
        response = self.session.request(
            method, self.base_url + path, json=data, timeout=10
        )
        print(f"{method} {path}: HTTP {response.status_code}")
        if response.content and not hide_body:
            try:
                print(json.dumps(response.json(), indent=2, ensure_ascii=False))
            except ValueError:
                print(response.text)
        if response.status_code == 401:
            print("Login kræves, eller dit token er udløbet. Vælg login igen.")
        elif response.status_code == 403:
            print("Du har ikke adgang. Ændringer kræver rollen Admin.")
        response.raise_for_status()
        return response.json() if response.content else None

    def login(self, username, password):
        # Et mislykket nyt login skal ikke efterlade det gamle token aktivt.
        self.session.headers.pop("Authorization", None)
        result = self.request(
            "POST", "/api/auth/login",
            {"username": username, "password": password}, hide_body=True
        )
        self.session.headers["Authorization"] = "Bearer " + result["token"]
        print(f"Logget ind som {username}.")

    def get_all(self):
        return self.request("GET", "/api/AktieHandels")

    def get_one(self, handel_id):
        return self.request("GET", f"/api/AktieHandels/{handel_id}")

    def create(self, handel):
        return self.request("POST", "/api/AktieHandels", handel)

    def update(self, handel_id, handel):
        return self.request("PUT", f"/api/AktieHandels/{handel_id}", handel)

    def delete(self, handel_id):
        return self.request("DELETE", f"/api/AktieHandels/{handel_id}")


def read_id():
    handel_id = int(input("Handels-ID: "))
    if handel_id <= 0:
        raise ValueError("ID skal være positivt.")
    return handel_id


def read_handel():
    navn = input("Aktienavn (mindst 4 tegn): ").strip()
    antal = int(input("Antal (negativt ved salg): "))
    pris = float(input("Handelspris: ").replace(",", "."))
    if len(navn) < 4 or antal == 0 or not math.isfinite(pris) or pris <= 0:
        raise ValueError("Navn skal have mindst 4 tegn, antal må ikke være 0, og pris skal være positiv.")
    return {"navn": navn, "antal": antal, "handelsPris": pris}


def menu(client):
    while True:
        print("\n1: Login\n2: Vis alle\n3: Vis én\n4: Opret\n5: Opdater\n6: Slet\n0: Afslut")
        choice = input("Vælg: ").strip()
        try:
            if choice == "0":
                return
            if choice == "1":
                client.login(input("Brugernavn: ").strip(), getpass("Adgangskode: "))
            elif choice == "2":
                client.get_all()
            elif choice == "3":
                client.get_one(read_id())
            elif choice == "4":
                client.create(read_handel())
            elif choice == "5":
                client.update(read_id(), read_handel())
            elif choice == "6":
                client.delete(read_id())
            else:
                print("Vælg et tal fra menuen.")
        except ValueError as error:
            print(f"Ugyldig indtastning: {error}")
        except requests.HTTPError:
            # Status og serverens fejlbesked er allerede vist af request().
            pass
        except requests.RequestException as error:
            print(f"Kunne ikke kontakte API'et: {error}")


def demo(client):
    """Afprøv alle fem funktioner på en ny testhandel, som slettes til sidst."""
    client.login("admin", getpass("Adgangskode til admin: "))
    client.get_all()
    created = client.create({"navn": "Python Demo", "antal": 10, "handelsPris": 100})
    handel_id = created["handelsId"]
    try:
        client.get_one(handel_id)
        updated = client.update(handel_id, {"navn": "Python Demo Updated", "antal": 20, "handelsPris": 125})
        if updated["antal"] != 20 or updated["handelsPris"] != 125:
            raise ValueError("API'et returnerede ikke de opdaterede værdier.")
    finally:
        client.delete(handel_id)
    print("Alle fem CRUD-funktioner gennemført.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="http://localhost:5173", help="API'ets basis-URL")
    parser.add_argument("--demo", action="store_true", help="Afprøv CRUD med en midlertidig testhandel")
    args = parser.parse_args()
    client = AktieHandelClient(args.url)
    try:
        if args.demo:
            demo(client)
        else:
            menu(client)
    except (requests.RequestException, ValueError) as error:
        print(f"Forsøget mislykkedes: {error}")
        return 1
    except (KeyboardInterrupt, EOFError):
        print("\nAfsluttet.")
    finally:
        client.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
