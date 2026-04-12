# Architecture Documentation

## Overview

Our architecture combines the following patterns:

| Pattern                    | Purpose                                                |
| -------------------------- | ------------------------------------------------------ |
| **Hexagonal Architecture** | Domain at the center, external systems are replaceable |
| **Repository Pattern**     | Abstract database access via interfaces                |
| **CQRS**                   | Clearly separate read and write operations             |
| **Mediator (MediatR)**     | Loosely couple commands & queries                      |
| **Vertical Slices**        | Organize code by feature instead of technical layers   |

---

## 1. Hexagonal Architecture (Ports & Adapters)

### Core Idea

The domain logic sits at the center. All external systems – database, APIs, external services – communicate with the domain via clearly defined **ports** (interfaces). Concrete implementations are **adapters**.

```
  [ REST API / Controller ]   [ GitHub Webhook ]   [ SignalR ]
              |                       |                  |
         ─────────────────────────────────────────────────
                          [ Application Layer ]
                     Commands / Queries / Ports
         ─────────────────────────────────────────────────
                            [ Domain ]
                    Entities, Events, Value Objects
         ─────────────────────────────────────────────────
              |                       |                  |
      [ IIssueRepository ]   [ IEmailService ]   [ IGithubService ]
              |                       |                  |
       [ EF Core + PG ]        [ SMTP Adapter ]   [ HTTP Adapter ]
```

### Layers

| Layer              | Content                                                    | Dependencies         |
| ------------------ | ---------------------------------------------------------- | -------------------- |
| **Domain**         | Entities, Value Objects, Domain Events                     | None                 |
| **Application**    | Features (Commands, Queries, Handlers), Ports (Interfaces) | Only Domain          |
| **Infrastructure** | Repository implementations, EF Core, external services     | Application + Domain |
| **Presentation**   | Minimal controllers, middleware, routing                   | Application          |

### Benefits

* Domain is completely independent of frameworks and external systems
* Adapters (e.g., database) can be replaced without changing the domain
* Highly testable – ports can be easily mocked

---

## 2. Repository Pattern

### Core Idea

The Repository Pattern is the **adapter between the domain and the database**. The application layer only knows the interface (port), never the concrete implementation.

```
Application
    |
IIssueRepository    ← Port (interface in Application)
    |
IssueRepository     ← Adapter (EF Core implementation in Infrastructure)
    |
 PostgreSQL
```

### Example (C#)

```csharp
// Port – located in Application/Ports/
public interface IIssueRepository
{
    Task<Issue?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Issue>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Issue issue, CancellationToken ct = default);
    Task UpdateAsync(Issue issue, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

// Adapter – located in Infrastructure/Persistence/
public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _context;

    public IssueRepository(AppDbContext context) => _context = context;

    public async Task<Issue?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Issues.FindAsync([id], ct);

    public async Task AddAsync(Issue issue, CancellationToken ct = default)
    {
        await _context.Issues.AddAsync(issue, ct);
        await _context.SaveChangesAsync(ct);
    }
}
```

### Benefits

* Business logic is independent of database technology
* Repository can be replaced with an in-memory mock for testing
* Fits perfectly into hexagonal architecture as an adapter

---

## 3. CQRS (Command Query Responsibility Segregation)

### Core Idea

CQRS separates **write operations (commands)** from **read operations (queries)**. Each operation has its own handler.

```
Request
  ├── Command (Write)   →  CommandHandler  →  Repository  →  PostgreSQL (Write)
  └── Query  (Read)     →  QueryHandler    →  DbContext   →  PostgreSQL (Read)
```

### Commands vs. Queries

|                | Command                | Query               |
| -------------- | ---------------------- | ------------------- |
| **Purpose**    | Change state           | Read state          |
| **Return**     | None or ID only        | DTO with data       |
| **Validation** | Yes (FluentValidation) | Minimal             |
| **Example**    | `CreateIssueCommand`   | `GetIssueByIdQuery` |

### Example (C#)

```csharp
// Command
public record CreateIssueCommand(string Title, string Description, string AssigneeId)
    : IRequest<Guid>;

// Query
public record GetIssueByIdQuery(Guid IssueId)
    : IRequest<IssueDto>;

// Query Result (DTO)
public record IssueDto(Guid Id, string Title, string Status, string AssigneeId);
```

### Benefits

* Read path can be optimized independently (e.g., Dapper instead of EF Core)
* Clear responsibilities – each handler does exactly one thing
* Read and write models can scale independently

---

## 4. Mediator Pattern (MediatR)

### Core Idea

A central **mediator** receives commands and queries and automatically forwards them to the appropriate handler. Controllers and handlers do not know each other directly – they communicate only via the mediator.

```
Controller
    |
  ISender (MediatR)
    |
    ├── CreateIssueCommand   →  CreateIssueHandler
    ├── AssignIssueCommand   →  AssignIssueHandler
    └── GetIssueByIdQuery    →  GetIssueByIdHandler
```

### Pipeline Behaviors

MediatR supports **pipeline behaviors** – middleware for every request. This cleanly handles cross-cutting concerns:

```
Request → [ValidationBehavior] → [LoggingBehavior] → Handler → Response
```

```csharp
// Validation Behavior – executed automatically for every command
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Controller Example

```csharp
[ApiController]
[Route("api/issues")]
public class IssueController : ControllerBase
{
    private readonly ISender _sender;

    public IssueController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateIssueCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetIssueByIdQuery(id), ct));
}
```

---

## 5. Vertical Slice Architecture

### Core Idea

Code is organized by **feature** instead of technical layers. Each feature contains everything it needs – command/query, handler, validator, and DTO – in a single folder.

Combined with MediatR, the structure looks like this:

```
src/
├── Domain/                         ← Shared domain (entities, events)
│   ├── Entities/
│   │   ├── Issue.cs
│   │   └── Sprint.cs
│   └── Events/
│       └── IssueCreatedEvent.cs
│
├── Application/
│   ├── Ports/                      ← Hexagonal ports (interfaces)
│   │   ├── IIssueRepository.cs
│   │   └── IEmailService.cs
│   │
│   └── Features/                   ← Vertical slices
│       ├── Issues/
│       │   ├── CreateIssue/
│       │   │   ├── CreateIssueCommand.cs
│       │   │   ├── CreateIssueHandler.cs
│       │   │   └── CreateIssueValidator.cs
│       │   ├── AssignIssue/
│       │   │   ├── AssignIssueCommand.cs
│       │   │   └── AssignIssueHandler.cs
│       │   └── GetIssueById/
│       │       ├── GetIssueByIdQuery.cs
│       │       ├── GetIssueByIdHandler.cs
│       │       └── IssueDto.cs
│       └── Sprints/
│           └── CreateSprint/
│               ├── CreateSprintCommand.cs
│               └── CreateSprintHandler.cs
│
├── Infrastructure/                 ← Hexagonal adapters
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── IssueRepository.cs      ← Adapter for IIssueRepository
│   │   └── Migrations/
│   └── ExternalServices/
│       └── EmailService.cs         ← Adapter for IEmailService
│
└── Presentation/
    └── Controllers/
        ├── IssueController.cs      ← Thin controller, only ISender.Send()
        └── SprintController.cs
```

### Complete Feature Example: `CreateIssue`

```csharp
// CreateIssueCommand.cs
public record CreateIssueCommand(string Title, string Description, string AssigneeId)
    : IRequest<Guid>;

// CreateIssueValidator.cs
public class CreateIssueValidator : AbstractValidator<CreateIssueCommand>
{
    public CreateIssueValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.AssigneeId).NotEmpty();
    }
}

// CreateIssueHandler.cs
public class CreateIssueHandler : IRequestHandler<CreateIssueCommand, Guid>
{
    private readonly IIssueRepository _repository;  // Port from Application/Ports/

    public CreateIssueHandler(IIssueRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateIssueCommand command, CancellationToken ct)
    {
        var issue = new Issue(command.Title, command.Description, command.AssigneeId);
        await _repository.AddAsync(issue, ct);
        return issue.Id;
    }
}
```

### Benefits

* Everything related to a feature is in one place – easy to find and modify
* New feature = new folder, no need to touch existing code
* Features are isolated – fewer unintended dependencies
* Scales well for teams (each team works on its own slice)

---

## How All Patterns Work Together

```
HTTP Request
     |
     ▼
[ Controller ]           → Thin, only ISender.Send()
     |
     ▼
[ MediatR Pipeline ]     → ValidationBehavior → LoggingBehavior
     |
     ▼
[ Feature Handler ]      → Vertical slice (command or query)
     |
     ▼
[ Port / Interface ]     → IIssueRepository (hexagonal architecture)
     |
     ▼
[ Repository Adapter ]   → EF Core implementation
     |
     ▼
[ PostgreSQL ]
```

### Dependency Rules

```
Presentation  →  Application  →  Domain
Infrastructure  →  Application  →  Domain

❌ Domain must NOT know anything external  
❌ Application must NOT know Infrastructure  
✅ Infrastructure implements the ports from Application  
```
