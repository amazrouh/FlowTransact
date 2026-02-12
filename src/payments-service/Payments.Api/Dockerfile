FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/payments-service/Payments.Api/Payments.Api.csproj", "src/payments-service/Payments.Api/"]
COPY ["src/payments-service/Payments.Application/Payments.Application.csproj", "src/payments-service/Payments.Application/"]
COPY ["src/payments-service/Payments.Domain/Payments.Domain.csproj", "src/payments-service/Payments.Domain/"]
COPY ["src/payments-service/Payments.Infrastructure/Payments.Infrastructure.csproj", "src/payments-service/Payments.Infrastructure/"]
COPY ["MoneyFellows.Contracts/MoneyFellows.Contracts.csproj", "MoneyFellows.Contracts/"]
RUN dotnet restore "src/payments-service/Payments.Api/Payments.Api.csproj"
COPY . .
WORKDIR "/src/src/payments-service/Payments.Api"
RUN dotnet build "Payments.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.Api.dll"]
