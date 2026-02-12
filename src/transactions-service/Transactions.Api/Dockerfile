FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/transactions-service/Transactions.Api/Transactions.Api.csproj", "src/transactions-service/Transactions.Api/"]
COPY ["src/transactions-service/Transactions.Application/Transactions.Application.csproj", "src/transactions-service/Transactions.Application/"]
COPY ["src/transactions-service/Transactions.Domain/Transactions.Domain.csproj", "src/transactions-service/Transactions.Domain/"]
COPY ["src/transactions-service/Transactions.Infrastructure/Transactions.Infrastructure.csproj", "src/transactions-service/Transactions.Infrastructure/"]
COPY ["MoneyFellows.Contracts/MoneyFellows.Contracts.csproj", "MoneyFellows.Contracts/"]
RUN dotnet restore "src/transactions-service/Transactions.Api/Transactions.Api.csproj"
COPY . .
WORKDIR "/src/src/transactions-service/Transactions.Api"
RUN dotnet build "Transactions.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Transactions.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transactions.Api.dll"]
