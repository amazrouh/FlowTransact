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
- [ ] **Isolate cause of MassTransit Publish not reaching Payments consumer** - TransactionSubmitted was published correctly but never consumed; the workaround (Send to `queue:transaction-submitted`) works. Investigate why default pub/sub topology failed: entity name formatter differences, exchange binding, or other MassTransit/RabbitMQ topology nuances between the two services.

## Future Enhancements (Backlog)
- [ ] Implement Payments Service (bounded context)
- [ ] Add retry policies and dead letter queues
- [ ] Implement idempotent event consumers
- [ ] Add API versioning
- [ ] Add rate limiting and security headers
- [ ] Add OpenAPI/Swagger documentation
- [ ] Add metrics collection (Prometheus/Grafana)
- [ ] Add distributed tracing (OpenTelemetry)
- [ ] Add CI/CD pipeline configuration
- [ ] Add performance benchmarks and load testing