# Bookify — Architecture

Bookify is an apartment booking API built with **.NET 10**, **Clean Architecture**, **CQRS** and **PostgreSQL**.
Users search for apartments, reserve a stay and review it afterwards. Admins confirm reservations once payment is received.

This document explains how the project is organised and **why** each decision was made.

---

## 1. Layers

```
Api  ──►  Infrastructure  ──►  Application  ──►  Domain
```

| Layer | Contains | Knows about |
|---|---|---|
| **Domain** | Entities, value objects, domain services, domain events, repository interfaces | Nothing (no NuGet packages at all) |
| **Application** | Commands, queries, handlers, validators, pipeline behaviors | Domain |
| **Infrastructure** | EF Core, PostgreSQL, Redis, Keycloak, Quartz jobs, Outbox | Application + Domain |
| **Api** | Controllers, authentication setup, error handling | All of the above |

**The rule:** dependencies only point inward. The quick test: `Bookify.Domain` must compile on its own.
This lets us swap any technical detail (database, identity provider, even MediatR) without touching business rules.

---

## 2. Domain

### Booking lifecycle

```
Reserved ──(admin)────────────► Confirmed ──(job after end date, or admin)──► Completed ──► can be reviewed
   │                                │
   ├──(admin)──► Rejected           └──(owner, until the start date)──► Cancelled
   └──(job, after 15 min)──► Rejected
```

Every transition is a method on `Booking` (`Confirm`, `Reject`, `Complete`, `Cancel`). Each one checks the rules first and only then changes state and raises an event.

### Result pattern, not exceptions
Business rules that can fail return `Result` / `Result<T>`. Exceptions are kept for real surprises (database down, bugs).
Rule: never read `result.Value` without checking `IsFailure`.

### Private constructors + factory methods
`Booking.Reserve`, `Apartment.Create`, `Review.Create`, `User.Create` return `Result<T>`.
Validation happens *before* the object exists, so an invalid aggregate can never be created.

### Value objects
`Name`, `Description`, `Address`, `Email`, `FirstName`, `LastName`, `Comment`, `Rating`, `DateRange`, `Money`, `Currency` are immutable `record`s (equality by value).
Each one that has rules exposes `Create(...)` returning a `Result`.
Rule of thumb: real validation/behaviour → value object; a plain closed list of options → enum (`Amenity`, `BookingStatus`).

### Pricing
`PricingService` (a domain service, because it needs both the apartment and the dates):

```
total = nightly price × nights + cleaning fee + amenities up-charge
```

Up-charge: garden/mountain view +5%, air conditioning +1%, parking +1%.
`Money.Add` fails if currencies differ, so USD and EUR can never be mixed silently.

### Domain events
Aggregates raise events (`BookingReservedDomainEvent`, …).
`IDomainEvent` is an empty interface — the Domain does not reference MediatR.
`Application` wraps events into MediatR notifications (`DomainEventNotification<T>`).

---

## 3. Application

### CQRS with marker interfaces
`ICommand`, `ICommand<T>`, `IQuery<T>` sit on top of MediatR and force every handler to return a `Result`.
Both command interfaces share an empty `IBaseCommand`, so behaviors can target "all commands".

### Vertical slices
One folder per use case (`Bookings/ReserveBooking/` holds the command, handler and validator together).
Some duplication between slices is accepted on purpose — it's cheaper than the wrong abstraction.

### Commands vs queries
- **Commands** load the full aggregate through a repository and call its methods.
- **Queries** skip the repository: `IApplicationDbContext` + `AsNoTracking()` + `.Select()` straight into the response DTO.

### Pipeline (in order)
```
Validation  →  UserSync  →  Logging  →  Handler
```
- **Validation** (FluentValidation): is the request well-formed? Runs before anything touches the database.
- **UserSync**: makes sure the authenticated user exists locally (see Authentication).
- **Logging**: logs expected failures (`Result.IsFailure`) as warnings.

Two kinds of validation, on purpose:

| Kind | Example | Where |
|---|---|---|
| Shape | "name is not empty", "dates are valid" | FluentValidation |
| Business | "currency exists", "dates are free" | Handler / Domain |

The Domain re-checks its own invariants anyway, so it stays safe even without the pipeline.

### Abstractions
`IUnitOfWork`, `IDateTimeProvider`, `IUserContext`, `IApplicationDbContext` live in Application (general capabilities).
Repository interfaces live in Domain (they speak about one aggregate).
Handlers never touch `HttpContext`; they ask `IUserContext` for the current user.

---

## 4. Infrastructure

### EF Core mapping
- Single-value value objects (`Name`, `Email`, `Rating`, …) → `HasConversion`.
- Composite value objects (`Address`, `Money`, `DateRange`) → `OwnsOne` (plain columns, no joins).
- Amenities → native PostgreSQL `integer[]`, written through the private backing field.
- `snake_case` table and column names (`EFCore.NamingConventions`).
- Foreign keys use `Restrict`: deleting a user or apartment never silently deletes bookings or reviews.

### Concurrency and double booking
- **Optimistic concurrency:** `Booking`, `User` and `Review` map PostgreSQL's `xmin` column as a concurrency token, so two admins acting on the same booking can't overwrite each other.
- **No overlapping bookings** — guaranteed by the database:

| Layer | Role |
|---|---|
| `IsOverlappingAsync` in the handler | Fast path with a friendly `Booking.Overlap` error |
| PostgreSQL exclusion constraint `ex_bookings_no_overlap` | The real guarantee, even if two requests race |
| `GlobalExceptionHandler` (SQLSTATE `23P01`) | Turns the constraint violation into a clean `409` |

```sql
EXCLUDE USING gist (
    apartment_id WITH =,
    daterange(duration_start, duration_end, '[]') WITH &&
) WHERE (status IN (1, 2, 5))   -- Reserved, Confirmed, Completed
```

Only active bookings block dates; rejected and cancelled ones free them.
`'[]'` means the check-out day is occupied (a business rule): the next guest can't check in that same day.
The constraint needs the `btree_gist` extension and hard-codes the status numbers, so it lives in a raw-SQL migration.

---

## 5. Api

Two paths for failures, both ending in a standard `ProblemDetails` response:

| Failure | Handled by | HTTP |
|---|---|---|
| Business / validation (`Result`) | `ApiController.HandleFailure` | `Validation`/`Failure` → 400, `NotFound` → 404, `Conflict` → 409, `Forbidden` → 403 |
| Unexpected / database | `GlobalExceptionHandler` | concurrency, unique and exclusion violations → 409; incomplete user profile → 400; anything else → 500 |

- Clients get **generic messages** for 5xx and database conflicts (raw exception text can leak schema details). The real error goes to the log, and the response includes a `traceId`.
- Unexpected exceptions are logged **once**, in `GlobalExceptionHandler`.

---

## 6. Authentication & Authorization (Keycloak)

- Keycloak issues JWTs; the API only validates them.
- Roles come from Keycloak (`realm_access.roles`) and are mapped to claims so `[Authorize(Roles = "Admin")]` works.
- **`User.Id` = the token's `sub` claim.** One identity, no mapping table.

### Lazy user sync
`UserSyncBehavior` creates the local `User` row the **first time** an authenticated user sends a command (not at registration). This also covers users created directly in Keycloak.
- A Redis flag (`user-synced:{id}`, 30 min) avoids hitting the database on every command.
- If two first requests race, the loser's insert fails, the change tracker is cleared and `ExistsAsync` confirms the other request won.
- If the token lacks email/first/last name, the API returns a clear `400`.
- Email is **not** unique in our database: Keycloak already enforces it, and the `sub` is the real identity.

### Roles vs ownership
- **Role** ("is this an admin?") → Keycloak.
- **Ownership** ("is this *my* booking?") → business data (`booking.UserId`).

Admin actions (`Confirm`, `Reject`, `Complete`, create apartment) are protected twice: `[Authorize(Roles = "Admin")]` on the endpoint **and** an `IsAdmin` check inside the handler.

### `NotFound`, not `Forbidden`
If a user asks for someone else's booking, the answer is the same `Booking.NotFound` as for a booking that doesn't exist — `403` would confirm that it exists.
Reads enforce this **inside the query**:

```csharp
.Where(b => b.Id == id && (b.UserId == userId || isAdmin))
```

### Registration
`POST /api/users` creates the account in Keycloak through its Admin API. It exists only because there is no frontend yet; a real frontend would talk to Keycloak directly.

---

## 7. Background jobs (Quartz)

| Job | Interval | Does |
|---|---|---|
| `RejectBookingsJob` | 1 min | Rejects bookings stuck in `Reserved` for more than 15 minutes |
| `CompleteBookingsJob` | 30 min | Completes `Confirmed` bookings whose end date has passed |
| `OutboxProcessorJob` | 30 sec | Publishes domain events from the outbox |

- Jobs are not part of an HTTP request, so each run creates its own DI scope (`IServiceScopeFactory`).
- Jobs **send normal commands** (`AutoRejectBookingCommand`, `AutoCompleteBookingCommand`) instead of calling the domain directly, so every operation goes through the same pipeline.
- Each booking is processed in its own `try/catch`: one failure never stops the run.
- Jobs use `[DisallowConcurrentExecution]`.
- `Confirm` stays manual (an admin received the money). Admins can also complete a booking manually, but the domain still refuses before the end date.

---

## 8. Outbox pattern

**Problem:** publishing events after `SaveChanges` is a separate step. If it fails, the booking is saved but its side effects (emails, notifications) are lost.

**Solution:** `SaveChangesAsync` converts domain events into `outbox_messages` rows **in the same transaction** as the aggregate. Both are saved, or neither is.

- `OutboxProcessorJob` reads unprocessed rows (20 per batch), publishes them to MediatR handlers and saves after **each** message.
- Failed messages are retried up to 5 times, then logged as `Critical` and left alone. Fresh messages are processed first, so a failing one can't block the queue.
- `DomainEventRegistry` maps short names (`"booking-confirmed"`) to event types, so renaming a class doesn't break stored messages. Saving an unregistered event fails immediately.
- Payloads are stored as `jsonb`; a partial index covers only unprocessed rows.
- Example consumer: `BookingConfirmedDomainEventHandler` (placeholder for "send a confirmation email").

**Guarantee:** at-least-once and eventually consistent (up to ~30 s). Handlers must be **idempotent**.

---

## 9. Caching & logging

- **Caching:** Redis through `IDistributedCache`, wrapped by `ResilientDistributedCache` (Decorator, registered with Scrutor). If Redis is down, reads return `null` instead of failing the request.
- **Logging:** Serilog → Console and Seq. Code only uses `ILogger<T>`. EF SQL commands are logged only in Development.

---

## 10. Not done yet

| Item | Note |
|---|---|
| Automated tests | Next step: domain unit tests + a Testcontainers test for the overlap constraint |
| Pagination | Needed on `GET /apartments`, `/bookings/me` and reviews before real traffic |
| Payment flow | `Confirm` is a manual admin action until a payment component exists |
| Apartment owner / host role | Needs an owner on `Apartment` |
| Multi-instance outbox | Needs `FOR UPDATE SKIP LOCKED` and retry back-off |
| Frontend login straight to Keycloak | Replaces `POST /api/users` |
| API versioning, secrets vault | Not needed for a local project |
