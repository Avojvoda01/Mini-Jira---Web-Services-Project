# Architecture Documentation

## Overview

Our architecture combines the following core patterns:

| Pattern                    | Purpose                                                |
| -------------------------- | ------------------------------------------------------ |
| **Onion Architecture**     | Domain at the center, external systems are replaceable |
| **Repository Pattern**     | Abstract database access via interfaces                |
| **CQRS**                   | Clearly separate read and write operations             |
| **Mediator (MediatR)**     | Loosely couple commands & queries                      |
| **Vertical Slices**        | Organize code by feature instead of technical layers   |

These are supported by a number of additional patterns and practices:

| Pattern / Practice              | Purpose                                                          |
| ------------------------------- | ---------------------------------------------------------------- |
| **Minimal API Endpoints**       | Thin HTTP layer that only dispatches to MediatR                  |
| **DTO Pattern**                 | Domain entities never cross the API boundary                     |
| **Dependency Injection**        | Constructor injection with scoped lifetimes                      |
| **EF Core Code-First**          | Schema defined in code via migrations + seeder                   |
| **JWT / Token-based Auth**      | Stateless authentication as a cross-cutting concern              |
| **.NET Aspire Orchestration**   | Local orchestration of server, frontend & database              |

> **Note on the codebase layout:** The patterns below describe *logical* layers.
> In this project they are **not** split across separate assemblies — everything
> lives in the single `MiniJiraAspire.Server` project and the separation is
> enforced by folder structure rather than by project boundaries:
>
> | Folder            | Responsibility                                              |
> | ----------------- | ----------------------------------------------------------- |
> | `Models/`         | Domain entities, DTOs, command/query/request/response types |
> | `Features/`       | MediatR command & query handlers (vertical slices)          |
> | `Endpoints/`      | Minimal API endpoints that dispatch to MediatR              |
> | `Persistence/`    | Repository ports & EF Core adapters                         |
> | `Services/`       | Cross-cutting services (e.g. `Services/Auth` JWT)           |
> | `Data/`           | `AppDbContext`, entity configurations, DB seeder            |
> | `Migrations/`     | EF Core code-first migrations                               |
> | `Chatbot/`        | Self-contained AI assistant feature (see below)             |

---

## Onion Architecture (Ports & Adapters)

The domain logic sits at the center. External systems – currently the database
and JWT token generation – communicate with the domain via clearly defined
**ports** (interfaces). Concrete implementations are **adapters**.

```text
         ─────────────────────────────────────────────────
                            [ Domain ]
              TaskItem, Epic, Project, Comment, User
         ─────────────────────────────────────────────────
              |                       |                  |
      [ ITaskRepository ]   [ IUserRepository ]   [ IJwtTokenService ]
              |                       |                  |
       [ EF Core + PG ]       [ EF Core + PG ]    [ JwtTokenService ]
```

> Ports are defined in `Persistence/Repositories/` (and `Services/Auth/`),
> adapters implement them in the same folders and are wired up in `Program.cs`.

### Benefits

* Domain models are independent of the persistence technology
* Adapters (e.g., the EF Core repositories) can be replaced without changing the handlers
* Highly testable – ports can be easily mocked

### A note on the domain model: it is intentionally anemic

Our entities (`TaskItem`, `Epic`, `Project`, `Comment`, `User`) are **plain data
holders** – properties only, no behavior. They do **not** enforce their own
invariants (there is, for example, no `TaskItem.ChangeStatus()` that would reject
an illegal status transition). This is a deliberate trade-off, not an oversight.

Where the logic actually lives:

* **Business rules / orchestration** → in the MediatR handlers (`Features/**`),
  e.g. uniqueness checks and password hashing in `CreateUserHandler`, or the role
  whitelist in `ChangeUserRoleHandler`.
* **State changes & data access** → in the repositories (`Persistence/**`),
  e.g. `TaskRepository.ChangeStatusAsync` simply sets the new status.

So "Domain at the center" here means *the entities are the stable core that
everything else depends on*, **not** that they are a rich, behavior-bearing domain
model in the DDD sense. For an application of this size that is a reasonable
choice: it keeps the entities simple and the logic easy to find in one handler per
use case. If the business rules grow more complex (multi-step validations, state
machines), the natural next step would be to pull that behavior into the entities
themselves.

### Repository Pattern

The Repository Pattern is the **adapter between the domain and the database**. The
handlers only know the interface (port), never the concrete implementation.

```text
Handler (Feature)
    |
ITaskRepository    ← Port (interface in Persistence/Repositories)
    |
TaskRepository     ← Adapter (EF Core implementation)
    |
 PostgreSQL
```

* Fits into the hexagonal architecture as an adapter

## CQRS (Command Query Responsibility Segregation)

CQRS separates **write operations (commands)** from **read operations (queries)**.
Each operation has its own handler. In the code this shows up as separate
`Commands/` and `Queries/` folders inside each feature slice.

```text
Request
  ├── Command (Write)   →  CommandHandler  →  Repository  →  PostgreSQL (Write)
  └── Query  (Read)     →  QueryHandler    →  Repository  →  PostgreSQL (Read)
```

### Commands vs. Queries

|                | Command                | Query               |
| -------------- | ---------------------- | ------------------- |
| **Purpose**    | Change state           | Read state          |
| **Return**     | None, ID, or DTO       | DTO with data       |
| **Example**    | `CreateTaskCommand`    | `GetTasksQuery`     |

### Benefits

* Clear responsibilities – each handler does exactly one thing
* Read and write models can scale independently

## Mediator Pattern (MediatR)

A central **mediator** receives commands and queries and automatically forwards
them to the appropriate handler. Endpoints and handlers do not know each other
directly – they communicate only via the mediator.

```text
Endpoint (Minimal API)
    |
  IMediator (MediatR)
    |
    ├── CreateTaskCommand    →  CreateTaskHandler
    ├── AssignUserCommand     →  AssignUserHandler
    └── GetTaskQuery          →  GetTaskHandler
```

### Benefits

* **Decoupling** – Senders and handlers don't know about each other — the mediator sits in between, reducing direct dependencies between components.
* **Single Responsibility** – Each request/command/query gets its own handler class, keeping logic focused and easy to find.
* **Testability** – Handlers are plain classes with no framework coupling — easy to unit test in isolation.
* **Reduced Endpoint Bloat** – Endpoints become thin dispatchers instead of containing business logic.

## Vertical Slice Architecture

Code is organized by **feature** instead of technical layers. Each feature in
`Features/` contains everything it needs – command/query, handler, and the
related DTOs.

```text
Features/
  ├── Tasks/      ── Commands/, Queries/
  ├── Project/    ── Commands/, Queries/
  ├── Epic/
  ├── Comment/
  └── Auth/       ── Commands/
```

### Benefits

* Everything related to a feature is in one place – easy to find and modify
* New feature = new folder, no need to touch existing code
* Features are isolated – fewer unintended dependencies
* Scales well for parallel work

---

## Supporting Patterns

### DTO Pattern

Handlers return `record` DTOs (e.g., `TaskItemDto`) instead of domain entities, so
internal models never leak across the API boundary.

### Dependency Injection

All collaborators (repositories, services, `IMediator`, `AppDbContext`) are
injected via the constructor.

### EF Core Code-First & Seeding

The database schema is defined in code. Migrations live in `Migrations/`, entity
type configurations live as `IEntityTypeConfiguration` classes in
`Data/Configurations/` (the `AppDbContext` applies them), and
`DbSeeder.MigrateAndSeedAsync` applies migrations and seeds initial data on
startup.

### JWT Authentication

Authentication is stateless and token-based. `JwtTokenService` issues tokens,
passwords are hashed via `IPasswordHasher<User>`, and JWT bearer validation is
configured as middleware in `Program.cs`.

### .NET Aspire Orchestration

The `MiniJiraAspire.AppHost` project orchestrates the server, frontend, and
PostgreSQL database for local development.

### Chatbot / AI Assistant

The in-app AI assistant lives in its own folder (`Chatbot/`) and is a
**deliberate exception** to the Endpoint → MediatR → Feature flow described
above. `ChatbotEndpoints` is a thicker minimal-API handler that injects the
repositories directly rather than dispatching commands/queries through MediatR —
it orchestrates an LLM rather than performing a single write/read use case, so a
self-contained module is the simpler fit. It still goes through the repository
ports, so it never touches the database directly, and the endpoint requires
authorization.

It talks to a **local LM Studio** instance (an OpenAI-compatible chat-completions
API) via two ports:

```text
ChatbotEndpoints  (POST /api/chats, requires auth)
    |
    ├── ILmStudioChatClient        ← Port (LLM access)
    |       └── LmStudioChatClient  ← Adapter (HttpClient → LM Studio)
    |
    └── IMiniJiraKnowledgeProvider ← Port (static app knowledge)
            └── MiniJiraKnowledgeProvider ← Adapter (reads Knowledge/*.txt)
```

Request flow:

1. **Classify intent** — the question is sent to the LLM, which returns a
   structured `ChatbotIntent` (intent name plus optional project/epic/search/
   status/priority slots).
2. **`general_help`** → retrieve relevant text from `Knowledge/*.txt` and let the
   LLM answer using only that context (a lightweight RAG approach).
3. **Live-data intents** (`get_my_tasks`, `summarize_project`, `list_project_epics`,
   …) → query the repositories, build a plain-text data context, and let the LLM
   phrase the answer.

Visibility is enforced before any data reaches the model: admins see all
projects, other users only their own/member projects (and the epics within
them). Configuration (`BaseUrl`, `Model`, `MaxTokens`, `Enabled`) is bound from
the `LmStudio` settings section via `ChatbotOptions`; if LM Studio is
unreachable the endpoint returns `503`.
