using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic.Commands;

public record DeleteEpicCommand(Guid Id) : IRequest;

public class DeleteEpicHandler(IEpicRepository repository) : IRequestHandler<DeleteEpicCommand>
{
    public Task Handle(DeleteEpicCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.Id, ct);
}
