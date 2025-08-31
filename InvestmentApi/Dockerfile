# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# katalog na bazę z prawami zapisu
RUN mkdir -p /data && chmod 777 /data
ENV DB_PATH=/data/investments.db
# (port dalej przez CMD albo Program.cs)

COPY --from=build /app/publish .
# jeśli używasz wariantu z PORT w CMD:
CMD ["sh", "-c", "dotnet InvestmentApi.dll --urls http://0.0.0.0:$PORT"]
