using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class EpicRepository(AppDbContext db) : IEpicRepository
{
    public async Task<List<EpicDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = db.Epics.AsNoTracking().AsQueryable();
        

        return await query
            .OrderBy(e => e.Name)
            .Select(e => new EpicDto(
                e.Id,
                e.Name,
                e.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<EpicDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Include + ThenInclude lets students inspect how EF loads a graph in one query.
        var epic = await db.Epics
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (epic is null)
        {
            throw new Exception("Epic get by id");
        }

        return new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty);
    }

    public async Task<EpicDto> CreateAsync(CreateEpicRequest request, CancellationToken cancellationToken = default)
    {
        var epic = new Epic
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Epics.Add(epic);
        await db.SaveChangesAsync(cancellationToken);

        return new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty);
    }

    public async Task UpdateAsync(int id, UpdateEpicRequest request, CancellationToken cancellationToken = default)
    {
        var evt = await db.Epics.FindAsync([id], cancellationToken);

        if (evt is null)
        {
            throw new Exception("Event update");
        }

        evt.Name = request.Name;
        evt.Description = request.Description;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var evt = await db.Epics.FindAsync([id], cancellationToken);

        if (evt is null)
        {
            throw new Exception("Epic delete");
        }

        db.Epics.Remove(evt);
        await db.SaveChangesAsync(cancellationToken);
    }
    
}