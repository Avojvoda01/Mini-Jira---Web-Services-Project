Basierend auf deinen Use Cases habe ich dir eine saubere **C# API Definition** im Stil deiner Architektur (CQRS + MediatR + Vertical Slices) entworfen.
Ich halte die Controller bewusst **thin** und orientiere mich an REST + deinen Rollen/Use Cases.

---

# 🔌 API Definition (C# / ASP.NET Core)

## 1. Auth / Identity

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));
}
```

---

## 2. Dashboard

```csharp
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await _sender.Send(new GetDashboardQuery(), ct));
}
```

---

## 3. Tasks (Core Feature)

```csharp
[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ISender _sender;

    public TaskController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { TaskId = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new DeleteTaskCommand(id), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetTaskByIdQuery(id), ct));

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchTasksQuery query, CancellationToken ct)
        => Ok(await _sender.Send(query, ct));
}
```

### Task Actions (Status, Priority, Assignment)

```csharp
[HttpPatch("{id:guid}/status")]
public async Task<IActionResult> ChangeStatus(Guid id, ChangeTaskStatusCommand command, CancellationToken ct)
    => Ok(await _sender.Send(command with { TaskId = id }, ct));

[HttpPatch("{id:guid}/priority")]
public async Task<IActionResult> ChangePriority(Guid id, ChangeTaskPriorityCommand command, CancellationToken ct)
    => Ok(await _sender.Send(command with { TaskId = id }, ct));

[HttpPatch("{id:guid}/assign-user")]
public async Task<IActionResult> AssignUser(Guid id, AssignUserToTaskCommand command, CancellationToken ct)
    => Ok(await _sender.Send(command with { TaskId = id }, ct));

[HttpPatch("{id:guid}/assign-epic")]
public async Task<IActionResult> AssignEpic(Guid id, AssignEpicToTaskCommand command, CancellationToken ct)
    => Ok(await _sender.Send(command with { TaskId = id }, ct));
```

---

## 4. Comments

```csharp
[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
public class CommentController : ControllerBase
{
    private readonly ISender _sender;

    public CommentController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, CreateCommentCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { TaskId = taskId }, ct));

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> Update(Guid taskId, Guid commentId, UpdateCommentCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { TaskId = taskId, CommentId = commentId }, ct));

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid commentId, CancellationToken ct)
        => Ok(await _sender.Send(new DeleteCommentCommand(taskId, commentId), ct));
}
```

---

## 5. Epics

```csharp
[ApiController]
[Route("api/epics")]
public class EpicController : ControllerBase
{
    private readonly ISender _sender;

    public EpicController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateEpicCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateEpicCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { EpicId = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new DeleteEpicCommand(id), ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetAllEpicsQuery(), ct));
}
```

---

## 6. Projects (Manager)

```csharp
[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new DeleteProjectCommand(id), ct));
}
```

### Project Members

```csharp
[HttpPost("{projectId:guid}/members")]
public async Task<IActionResult> AssignMember(Guid projectId, AssignMemberCommand command, CancellationToken ct)
    => Ok(await _sender.Send(command with { ProjectId = projectId }, ct));

[HttpDelete("{projectId:guid}/members/{userId:guid}")]
public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId, CancellationToken ct)
    => Ok(await _sender.Send(new RemoveMemberCommand(projectId, userId), ct));
```

---

## 7. Admin (User & Roles)

```csharp
[ApiController]
[Route("api/admin/users")]
public class UserManagementController : ControllerBase
{
    private readonly ISender _sender;

    public UserManagementController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new DeleteUserCommand(id), ct));
}
```

### Roles

```csharp
[ApiController]
[Route("api/admin/roles")]
public class RoleController : ControllerBase
{
    private readonly ISender _sender;

    public RoleController(ISender sender) => _sender = sender;

    [HttpPatch("{userId:guid}")]
    public async Task<IActionResult> ChangeRole(Guid userId, ChangeUserRoleCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { UserId = userId }, ct));
}
```

---

# 🧠 Design-Highlights (wichtig für dich)

* **1 Endpoint = 1 Use Case → passt perfekt zu Vertical Slice**
* **Commands/Queries werden direkt gemappt → keine Service Layer nötig**
* **Routing spiegelt Business wider (Tasks, Epics, Projects)**
* **Nested Routes für Comments → klare Ownership (`/tasks/{id}/comments`)**
* **PATCH für partielle Änderungen (Status, Priority)**

---

# 🚀 Optional (wenn du noch besser werden willst)

Du könntest noch ergänzen:

* API Versioning (`/api/v1/...`)
* Authorization:

```csharp
[Authorize(Roles = "Admin")]
```

* ProblemDetails für Fehler
* Pagination bei `SearchTasksQuery`

---

Wenn du willst, kann ich dir als nächsten Schritt noch:

✅ die **Commands & Queries komplett definieren**
✅ oder eine **OpenAPI / Swagger Spec** daraus generieren
✅ oder eine **Clean Folder-Struktur für die Features** bauen

Sag einfach, was du brauchst 👍
