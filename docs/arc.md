# Architecture Documentation

## Overview

Our architecture combines the following patterns:

| Pattern                    | Purpose                                                |
| -------------------------- | ------------------------------------------------------ |
| **Onion Architecture** | Domain at the center, external systems are replaceable |
| **Repository Pattern**     | Abstract database access via interfaces                |
| **CQRS**                   | Clearly separate read and write operations             |
| **Mediator (MediatR)**     | Loosely couple commands & queries                      |
| **Vertical Slices**        | Organize code by feature instead of technical layers   |

---

## Onion Architecture (Ports & Adapters)

The domain logic sits at the center. All external systems – database, APIs, external services – communicate with the domain via clearly defined **ports** (interfaces). Concrete implementations are **adapters**.

```text
         ─────────────────────────────────────────────────
                            [ Domain ]
                    Entities, Events, Value Objects
         ─────────────────────────────────────────────────
              |                       |                  |
      [ IIssueRepository ]   [ IEmailService ]   [ IGithubService ]
              |                       |                  |
       [ EF Core + PG ]       [ SMTP Adapter ]    [ HTTP Adapter ]
```

### Benefits

* Domain is completely independent of frameworks and external systems
* Adapters (e.g., database) can be replaced without changing the domain
* Highly testable – ports can be easily mocked

### Repository Pattern

The Repository Pattern is the **adapter between the domain and the database**. The application layer only knows the interface (port), never the concrete implementation.

```text
Application
    |
IIssueRepository    ← Port (interface in Application)
    |
IssueRepository     ← Adapter (EF Core implementation in Infrastructure)
    |
 PostgreSQL
```

#### Benefits

* Business logic is independent of database technology
* Repository can be replaced with an in-memory mock for testing
* Fits into hexagonal architecture as an adapter

## CQRS (Command Query Responsibility Segregation)

CQRS separates **write operations (commands)** from **read operations (queries)**. Each operation has its own handler.

```text
Request
  ├── Command (Write)   →  CommandHandler  →  Repository  →  PostgreSQL (Write)
  └── Query  (Read)     →  QueryHandler    →  DbContext   →  PostgreSQL (Read)
```

### Commands vs. Queries

|                | Command                | Query               |
| -------------- | ---------------------- | ------------------- |
| **Purpose**    | Change state           | Read state          |
| **Return**     | None or ID only        | DTO with data       |

### Benefits

* Clear responsibilities – each handler does exactly one thing
* Read and write models can scale independently

## Mediator Pattern (MediatR)

A central **mediator** receives commands and queries and automatically forwards them to the appropriate handler. Controllers and handlers do not know each other directly – they communicate only via the mediator.

```text
Controller
    |
  ISender (MediatR)
    |
    ├── CreateIssueCommand   →  CreateIssueHandler
    ├── AssignIssueCommand   →  AssignIssueHandler
    └── GetIssueByIdQuery    →  GetIssueByIdHandler
```

### Benefits

* **Decoupling** – Senders and handlers don't know about each other — the mediator sits in between, reducing direct dependencies between components.
* **Single Responsibility** – Each request/command/query gets its own handler class, keeping logic focused and easy to find.
* **Testability** – Handlers are plain classes with no framework coupling — easy to unit test in isolation.
* **Reduced Controller Bloat** – Controllers become thin dispatchers (`mediator.Send(command)`) instead of containing business logic.


## 5. Vertical Slice Architecture

Code is organized by **feature** instead of technical layers. Each feature contains everything it needs – command/query, handler, validator, and DTO – in a single folder.

### Benefits

* Everything related to a feature is in one place – easy to find and modify
* New feature = new folder, no need to touch existing code
* Features are isolated – fewer unintended dependencies
* Scales well for parallel work
