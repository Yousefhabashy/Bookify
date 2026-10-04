# Bookify

Apartment reservation REST API built with .NET 10, Clean Architecture and CQRS.
Users search for available apartments, reserve a stay and review it afterwards. Admins confirm reservations.

## Features

- Search apartments by date range (only available ones are returned)
- Reserve, cancel, confirm, reject and complete bookings
- Reviews (one per completed booking)
- Double-booking prevented at the database level (PostgreSQL exclusion constraint)
- Transactional outbox for domain events, with retries
- Background jobs: auto-reject pending bookings, auto-complete finished stays
- Authentication and roles through Keycloak (JWT)

## Tech stack

| Area | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core |
| Data | EF Core 10, PostgreSQL 17 |
| Messaging | MediatR (CQRS + pipeline behaviors), FluentValidation |
| Auth | Keycloak 26 (JWT bearer) |
| Cache | Redis |
| Jobs | Quartz.NET |
| Logging | Serilog + Seq |
| Tests | xUnit, FluentAssertions, NSubstitute, Testcontainers |

## Architecture

Domain ← Application ← Infrastructure ← Api (dependencies point inward).
Details and design decisions: [ARCHITECTURE.md](ARCHITECTURE.md).

## Getting started

**Prerequisites:** .NET 10 SDK, Docker.

1. Start the infrastructure:
```bash
   docker compose up -d
```
   | Service | URL |
   |---|---|
   | PostgreSQL | `localhost:5432` |
   | Keycloak | http://localhost:18080 |
   | Seq (logs) | http://localhost:5341 |
   | Redis | `localhost:6379` |

2. Configure Keycloak (see [Keycloak setup](#keycloak-setup)) and put the client secret in
   `Bookify.Api/appsettings.Development.json`:
```json
   "Keycloak": { "ClientSecret": "PUT_YOUR_CLIENT_SECRET_HERE" }
```
   The database password in the same file must match `docker-compose.yml`.

3. Apply the migrations:
```bash
   dotnet ef database update -p Bookify.Infrastructure -s Bookify.Api
```

4. Run the API:
```bash
   dotnet run --project Bookify.Api
```
   API: http://localhost:5104 — Swagger: http://localhost:5104/swagger

## Keycloak setup

1. Open http://localhost:18080 and sign in with the admin credentials from `docker-compose.yml`.
2. Create a realm named **`bookify`**.
3. Create a realm role named **`Admin`**.
4. Create a client **`bookify-api`**:
   - Client authentication: **On**
   - Service accounts roles: **On** (assign `manage-users` and `view-users` from `realm-management`; used by user registration)
   - Direct access grants: **On** (to get tokens with username and password)
5. Copy the client secret (Credentials tab) into the configuration.
6. Add an **Audience** mapper to the client's access token with included client audience `bookify-api`.
7. Create users with **email, first name and last name** (required) and assign `Admin` to the admin user.

Token request example:
```bash
curl -X POST http://localhost:18080/realms/bookify/protocol/openid-connect/token \
  -d "grant_type=password" -d "client_id=bookify-api" -d "client_secret=<secret>" \
  -d "username=<user>" -d "password=<password>"
```

## API

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/users` | Public (register) |
| GET | `/api/apartments?startDate=&endDate=` | Public (search available) |
| GET | `/api/apartments/{id}` | Public |
| POST | `/api/apartments` | Admin |
| POST | `/api/bookings` | User |
| GET | `/api/bookings/me` | User |
| GET | `/api/bookings/{id}` | Owner or Admin |
| PUT | `/api/bookings/{id}/cancel` | Owner |
| PUT | `/api/bookings/{id}/confirm` | Admin |
| PUT | `/api/bookings/{id}/reject` | Admin |
| PUT | `/api/bookings/{id}/complete` | Admin |
| POST | `/api/reviews` | User |
| GET | `/api/reviews/{id}` | Public |
| GET | `/api/reviews/apartment/{apartmentId}` | Public |

## Business rules

- Booking lifecycle: `Reserved → Confirmed → Completed`, or `Rejected` / `Cancelled`.
- The check-out day is blocked for new bookings (cleaning time).
- A booking can be cancelled while `Reserved` or `Confirmed`, as long as the stay has not started.
- Pending (`Reserved`) bookings are rejected automatically if not confirmed in time.
- Only a `Completed` booking can be reviewed, once.
- The start date of a booking cannot be in the past.

## Tests

```bash
dotnet test
```

| Project | Scope |
|---|---|
| `Domain.UnitTests` | Entities, value objects, pricing, booking state rules |
| `Application.UnitTests` | Command handlers with mocked repositories |
| `Application.IntegrationTests` | Real PostgreSQL via Testcontainers: constraints, outbox, user sync, search |

Integration tests need **Docker running**.

A Postman collection is available in [`Postman/`](Postman): import the collection and the environment, then fill the tokens.

## Project structure

```
Bookify.Domain/          Entities, value objects, domain rules
Bookify.Application/     Commands, queries, handlers, validators, pipeline behaviors
Bookify.Infrastructure/  EF Core, repositories, Keycloak, outbox, background jobs
Bookify.Api/             Controllers, middleware, startup
```
