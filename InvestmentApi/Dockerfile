# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY InvestmentApi/ InvestmentApi/
RUN dotnet restore InvestmentApi/InvestmentApi.csproj
RUN dotnet publish InvestmentApi/InvestmentApi.csproj -c Release -o /app/publish

# final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvestmentApi.dll"]
