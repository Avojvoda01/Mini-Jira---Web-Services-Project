using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Commands;

public record DeleteUserCommand(string UserId) : IRequest<bool>;

public class DeleteUserHandler(IUserRepository repository) : IRequestHandler<DeleteUserCommand, bool>
{
    public Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.UserId, ct);
}
