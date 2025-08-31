# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# kopiujemy najpierw csproj (lepszy cache restore)
COPY ["InvestmentApi.csproj", "./"]
RUN dotnet restore

# teraz reszta źródeł i publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# katalog na bazę SQLite (wariant A)
# (jeśli używasz Persistent Disk na Render, /data będzie podmontowane — ta linia i chmod są opcjonalne)
RUN mkdir -p /data && chmod 777 /data
ENV DB_PATH=/data/investments.db

# aplikacja
COPY --from=build /app/publish .

# nasłuchiwanie na porcie Rendera (PORT jest nadawany w runtime)
CMD ["sh", "-c", "dotnet InvestmentApi.dll --urls http://0.0.0.0:$PORT"]
