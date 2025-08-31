# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvestmentApi.csproj","./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Render wystawia zmienną PORT – musimy na niej nasłuchiwać
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
COPY --from=build /app/publish .
CMD ["dotnet", "InvestmentApi.dll"]
