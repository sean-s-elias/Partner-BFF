# Partner Integration BFF

.NET 8 API that accepts a partner transaction, validates it, checks the partner against a (simulated) external verification service, and publishes valid transactions to a queue.

## Architecture

Split into 4 projects (Clean Architecture style):

- **PartnerBFF.Api** – controllers, middleware, startup config
- **PartnerBFF.Application** – DTOs, interfaces, validation rules
- **PartnerBFF.Persistence** – RabbitMQ publisher, partner verification client, Polly resilience policy
- **PartnerBFF.Domain** – core domain model
- **PartnerBFF.Tests** – unit tests

Api depends on Application + Persistence. Persistence depends on Application. Application has no dependencies on the others — this keeps business rules separate from infrastructure (RabbitMQ, HTTP calls, etc), so those could be swapped out later without touching the core logic.

**Why this approach:** keeping business logic separate from infrastructure makes the core rules easier to test (no need to spin up a real database or queue to unit test validation logic), easier to change later (swapping RabbitMQ for something else only touches one layer), and keeps modules loosely coupled so a change in one place doesn't ripple through the rest of the codebase. It also scales well as the service grows — new partners, rules, or integrations can be added within their own layer without the layers bleeding into each other.

**Flow for `POST /api/v1/partner/transactions`:**
1. Validate payload (FluentValidation, runs automatically before the controller action)
2. Verify partnerId against the dummy verification endpoint, wrapped in a Polly retry + circuit breaker policy (the dummy endpoint fails ~30% of the time on purpose)
3. If both pass, publish the transaction to RabbitMQ
4. Any unhandled exception is caught by a global exception handler and returned as consistent JSON

## Running it

**With Docker:**
```
docker compose up --build
```
API: http://localhost:8080/swagger
RabbitMQ dashboard: http://localhost:15672 (guest/guest)

**Without Docker:**
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
then run `PartnerBFF.Api` normally and open the Swagger URL it prints.

## Tests

```
cd PartnerBFF.Tests
dotnet test
```

Covers validation rules, the resilience/retry logic, and the partner verification service (mocked HTTP).

## Notes / what I'd add with more time

- No idempotency check yet — same transaction can be submitted twice. In a real setup I'd have a consumer service persist processed transactions and check against that before accepting a new one.
- No auth on the endpoint yet — would add JWT bearer auth in production.
- Currency check uses a small hardcoded list, not a full ISO 4217 list.