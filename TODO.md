# FlowTransact Development Tasks

## Current Priorities

### Observability & Monitoring
- [x] **Add Seq Logging Sink** - Implement Seq for structured log aggregation and real-time monitoring
  - Install `Serilog.Sinks.Seq` package
  - Configure Seq sink in `appsettings.json`
  - Add Seq container to `docker-compose.yml`
  - Test log ingestion and querying in Seq UI (run `docker-compose up seq` and open http://localhost:5341)
  - Set up basic dashboards for transaction monitoring

### Completed Features ✅
- [x] Domain modeling with DDD aggregates and events
- [x] Clean Architecture implementation (Domain/Application/Infrastructure/API layers)
- [x] Transaction aggregate with business rules
- [x] CQRS with MediatR commands and queries
- [x] Entity Framework Core with PostgreSQL
- [x] MassTransit with RabbitMQ for event-driven architecture
- [x] Transactional outbox pattern
- [x] Global exception handling with structured error responses
- [x] Correlation ID middleware for request tracing
- [x] Serilog structured logging (console output)
- [x] Health checks for service monitoring
- [x] Comprehensive unit and integration test suites
- [x] API integration tests with WebApplicationFactory
- [x] Optimistic concurrency control
- [x] Docker Compose setup with PostgreSQL and RabbitMQ

## Investigation (Backlog)
- [x] **MassTransit Publish** - Switched from Send to Publish. The `[EntityName("transaction-submitted")]` attribute on TransactionSubmitted ensures the exchange name matches the consumer queue. The previous Send workaround was redundant; Publish with EntityName should work.

## Future Enhancements (Backlog)
- [x] Implement Payments Service (bounded context)
- [x] **Retry policies and dead letter queues** - Incremental retry (3 attempts, 1s/6s/11s); Ignore ArgumentException/InvalidOperationException (no retry for validation errors); failed messages go to `_error` queue (MassTransit default)
- [x] **Implement idempotent event consumers** - MassTransit EF Inbox (InboxState) on all consumer endpoints
- [x] **Add API versioning** - Asp.Versioning.Mvc with v1.0 default; query string/header versioning
- [x] **Add OpenAPI/Swagger documentation** - SwaggerDoc with title, version, description for both APIs
- [x] **Add CI/CD pipeline configuration** - GitHub Actions workflow (.github/workflows/ci.yml) for build and test
- [x] **Payments Service Parity with Transactions Service** - Apply same improvements as Transactions service:
  - PaymentDto: GetPaymentQuery returns PaymentDto? instead of Payment?; controller uses DTOs
  - Validation pipeline: Add ValidationBehavior, validators for StartPaymentCommand, ConfirmPaymentCommand, FailPaymentCommand
  - GlobalExceptionHandler: Handle ValidationException with structured Errors (like Transactions)
  - Controller simplification: Remove redundant try/catch; rely on GlobalExceptionHandler (ensure CustomerMismatchException is handled)

## Skipped (Deferred – not implementing now)
- [ ] Add rate limiting
- [ ] Add security headers
- [ ] Add performance benchmarks and load testing
- [ ] Add metrics collection (Prometheus/Grafana)
- [ ] Add distributed tracing (OpenTelemetry)