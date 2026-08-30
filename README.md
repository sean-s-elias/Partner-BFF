# Partner Integration BFF

.NET 8 API that accepts a partner transaction, validates it, checks the partner against a (simulated) external verification service, and publishes valid transactions to a queue.

## Architecture

Split into 5 projects - Clean Architecture style

- **PartnerBFF.Api**:  Controllers, middleware, configuration
- **PartnerBFF.Application**: Response DTOs, interfaces, validation rules
- **PartnerBFF.Persistence**:  RabbitMQ publisher, partner verification client, Polly resilience policy
- **PartnerBFF.Domain**: Core domain model
- **PartnerBFF.Tests**: Unit tests

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
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

then run `PartnerBFF.Api` normally and open the Swagger.

## Tests

```
cd PartnerBFF.Tests
dotnet test
```

Covers validation rules, the resilience/retry logic, and the partner verification service (mocked HTTP).

## What I'd add with more time

- To make this more properly event-driven, the ideal setup would be a separate consumer service that reads messages off the queue and persists processed transactions to a database. The producer would then check incoming transactions (partnerId + transactionReference) against that database before accepting them, to prevent duplicates.
- Securing the endpoint: in production, this would be secured with JWT Bearer authentication via Auth0. Auth0 would issue signed JWTs (Client Credentials flow for partner/machine-to-machine calls, or Authorization Code flow for user-facing clients), and the API would validate incoming tokens using JWT Bearer middleware registered in Program.cs, with [Authorize] applied to the controller/endpoint. Beyond authentication, I'd also define scopes (e.g. transactions:write) in Auth0 and enforce them with a policy ([Authorize(Policy = "TransactionsWrite")]), so a token is only accepted if it has actually been granted permission to submit transactions. This provides fine-grained control over what each partner/client is allowed to do, rather than an all-or-nothing authenticated/unauthenticated check. This wasn't implemented here due to the time-boxed scope, but the endpoint is structured so it could be added without changing the core logic.
