# Investment App

Aplikacja do śledzenia portfela inwestycji kryptowalutowych. Składa się z backendu ASP.NET Core Web API oraz frontendu Angular.

## Struktura projektu

```text
Investment-App/
├─ InvestmentApi/                 # Backend ASP.NET Core Web API
├─ investment-ui/                 # Frontend Angular
├─ InwestmentApp.sln              # Solution .NET
├─ .gitignore                     # Reguły ignorowania plików lokalnych
└─ README.md                      # Ten plik
```

## Technologie

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 9
- SQLite
- Swagger/OpenAPI

### Frontend

- Angular 20
- standalone components
- Angular Router
- HttpClient
- Bootstrap
- Chart.js / ng2-charts
- CoinGecko API do aktualnych cen kryptowalut w PLN

## Uruchomienie backendu

1. Otwórz terminal w katalogu `InvestmentApi`.
2. Przywróć pakiety:

```bash
dotnet restore
```

3. Uruchom API:

```bash
dotnet run
```

Backend będzie dostępny lokalnie pod adresem wskazanym w `InvestmentApi/Properties/launchSettings.json`, domyślnie jest to port `5247`.

API udostępnia endpointy:

```text
GET    /api/investments
GET    /api/investments/{id}
POST   /api/investments
PUT    /api/investments/{id}
DELETE /api/investments/{id}
GET    /api/investments/summary
```

Swagger jest dostępny w środowisku developmentowym.

## Uruchomienie frontendu

1. Otwórz terminal w katalogu `investment-ui`.
2. Zainstaluj zależności:

```bash
npm install
```

3. Uruchom aplikację:

```bash
npm start
```

Frontend będzie dostępny pod adresem:

```text
http://localhost:4200
```

## Konfiguracja połączenia z bazą

Backend korzysta z SQLite. Connection string jest zapisany w konfiguracji:

```json
"ConnectionStrings": {
  "InvestmentConnection": "Data Source=investments.db"
}
```

Plik bazy danych `investments.db` jest ignorowany przez `.gitignore`. Przy starcie aplikacja wykonuje migracje bazy danych przez:

```csharp
context.Database.Migrate();
```

## Budowanie

Backend:

```bash
dotnet build InwestmentApp.sln
```

Frontend:

```bash
npm run build
```

## Testy frontendu

```bash
npm test
```

## CORS

Frontend łączy się z backendem przez:

```text
http://localhost:5247/api/investments
```

Backend pozwala na requesty z:

```text
http://localhost:4200
```

Jeżeli frontend lub API będzie uruchamiane na innym porcie, trzeba zaktualizować konfigurację CORS w `InvestmentApi/Program.cs`.

## Git

Repozytorium nie powinno zawierać plików lokalnych ani buildowych. Ignorowane są między innymi:

- `bin/`
- `obj/`
- `.vs/`
- `node_modules/`
- `dist/`
- `*.db`
- `*.suo`
- `*.user`

## Dalszy rozwój

Najbliższe sensowne kierunki rozwoju:

1. Dodać walidację formularza z komunikatami błędów dla użytkownika.
2. Dodać endpoint lub model dla aktualnej wartości portfela.
3. Dodać historię cen zamiast pobierania tylko aktualnej ceny.
4. Dodać autoryzację użytkowników.
5. Dodać environment config dla API URL.
6. Optymalizować rozmiar bundle’a Angulara.
