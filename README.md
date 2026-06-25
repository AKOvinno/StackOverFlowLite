# StackOverflow Lite

A simplified Question & Answer REST API built with ASP.NET Core 10, following Clean Architecture principles.

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core + PostgreSQL |
| Auth | ASP.NET Identity + JWT |
| Messaging | MediatR |
| Validation | FluentValidation |
| Caching / View tracking | Redis |
| Docs | Swagger / OpenAPI |
| Containers | Docker + Docker Compose |

---

## Project Structure

```
src/
├── StackOverFlowLite.Domain/          # Entities, enums — no dependencies
├── StackOverFlowLite.Application/     # MediatR handlers, DTOs, validators, interfaces
├── StackOverFlowLite.Infrastructure/  # EF Core, repositories, Redis, JWT, Identity
└── StackOverFlowLite.API/             # Controllers, middleware, Swagger, Program.cs
```

---

## Running with Docker

Make sure Docker Desktop (or Docker Engine + Compose) is installed, then run:

```bash
docker compose up --build
```

That's it. Docker starts PostgreSQL, Redis, and the API together. Migrations run automatically on startup.

Once running, open **http://localhost:8080** in your browser to reach the Swagger UI.

To stop everything:

```bash
docker compose down
```

To stop and wipe the database / cache volumes:

```bash
docker compose down -v
```

---

## Environment Variables

The `docker-compose.yml` already sets all required values. If you want to override any of them, edit the `environment` block under the `api` service, or create a `.env` file next to `docker-compose.yml`.

| Variable | Default | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | (postgres service) | PostgreSQL connection string |
| `ConnectionStrings__Redis` | `redis:6379` | Redis connection string |
| `JwtSettings__SecretKey` | `your-very-long-secret-key-at-least-32-chars!!` | JWT signing key — **change this in production** |
| `JwtSettings__Issuer` | `StackOverFlowLite` | JWT issuer |
| `JwtSettings__Audience` | `StackOverFlowLiteUsers` | JWT audience |
| `JwtSettings__ExpiryHours` | `24` | Token lifetime in hours |

---

## API Overview

All endpoints are documented in Swagger at **http://localhost:8080**.

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Create a new account |
| POST | `/api/auth/login` | Log in and receive a JWT |
| GET | `/api/auth/me` | Get your profile and reputation score |

Protected endpoints require the header `Authorization: Bearer <token>`.

### Questions

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/questions` | List all questions (filter by `?tag=name`) |
| GET | `/api/questions/{id}` | Get a single question (increments view count) |
| POST | `/api/questions` | Create a question |
| PUT | `/api/questions/{id}` | Update your question |
| DELETE | `/api/questions/{id}` | Delete your question |

### Answers

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/questions/{id}/answers` | List answers for a question |
| POST | `/api/questions/{id}/answers` | Post an answer |
| PUT | `/api/answers/{id}` | Update your answer |
| DELETE | `/api/answers/{id}` | Delete your answer |
| POST | `/api/answers/{id}/accept` | Accept an answer (question author only) |
| DELETE | `/api/answers/{id}/accept` | Remove accepted answer |

### Votes

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/votes/questions/{id}` | Upvote or downvote a question |
| POST | `/api/votes/answers/{id}` | Upvote or downvote an answer |

### Tags

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/tags` | List all tags |
| POST | `/api/tags` | Create a tag |

---

## Reputation Rules

| Event | Points |
|---|---|
| Question upvoted | +5 |
| Question downvoted | −1 |
| Answer upvoted | +10 |
| Answer downvoted | −2 |
| Answer accepted | +15 |
| Accepted answer removed | −15 |

Reputation never drops below zero. Users cannot gain reputation from votes on their own content.

---

## Running Locally (without Docker)

You need .NET 10 SDK, PostgreSQL, and Redis installed locally.

1. Update the connection strings in `src/StackOverFlowLite.API/appsettings.Development.json` to point at your local services.
2. Run the API:

```bash
cd src/StackOverFlowLite.API
dotnet run
```

Migrations apply automatically on startup.
