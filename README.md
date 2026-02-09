# FlowTransact - Event-Driven Financial Transactions Platform

A microservices-based event-driven platform for handling financial transactions with Clean Architecture principles.

## Architecture

This solution implements Clean Architecture with the following bounded contexts:

### Transactions Service (Current Implementation)
- **Domain Layer**: Transaction aggregate with business rules and invariants
- **Application Layer**: MediatR commands and queries
- **Infrastructure Layer**: EF Core with PostgreSQL, MassTransit with Transactional Outbox
- **API Layer**: REST endpoints for transaction management

### Payments Service (Future Implementation)
- Will consume TransactionSubmitted events
- Handle payment processing and publish PaymentConfirmed/PaymentFailed events

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- PostgreSQL (via Docker)
- RabbitMQ (via Docker)

## Getting Started

1. **Start infrastructure services:**
   ```bash
   docker-compose up -d
   ```

2. **Navigate to the Transactions service:**
   ```bash
   cd src/transactions-service/Transactions.Api
   ```

3. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`.

## API Endpoints

### Create Transaction
```http
POST /api/transactions
Content-Type: application/json

{
  "customerId": "guid"
}
```

### Add Item to Transaction
```http
POST /api/transactions/{id}/items
Content-Type: application/json

{
  "productId": "guid",
  "productName": "string",
  "quantity": 1,
  "unitPrice": 10.99
}
```

### Submit Transaction
```http
POST /api/transactions/{id}/submit
```

### Get Transaction
```http
GET /api/transactions/{id}
```

## Domain Events

The system publishes the following domain events via MassTransit and RabbitMQ:

- `TransactionSubmitted`: Published when a transaction is submitted for payment processing

## Architecture Decisions

- **Clean Architecture**: Strict separation of concerns with dependency inversion
- **Domain-Driven Design**: Rich domain model with aggregates, entities, and domain events
- **CQRS**: Commands for writes, queries for reads (via MediatR)
- **Event-Driven**: Asynchronous communication between bounded contexts
- **Transactional Outbox**: Reliable event publishing using MassTransit's outbox pattern
- **PostgreSQL**: Relational database with EF Core
- **RabbitMQ**: Message broker for event distribution

## Project Structure

```
src/
├── transactions-service/
│   ├── Transactions.Api/           # API Layer
│   ├── Transactions.Application/   # Application Layer
│   ├── Transactions.Domain/        # Domain Layer
│   └── Transactions.Infrastructure/ # Infrastructure Layer
└── MoneyFellows.Contracts/         # Shared contracts
```

## Next Steps

1. Implement the Payments Service
2. Add correlation IDs for request tracing
3. Add structured logging
4. Add health checks
5. Add integration tests
6. Add observability (metrics, tracing)