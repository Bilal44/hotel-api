using System.Linq.Expressions;
using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Persistence.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id);
    Task<List<Room>> FilterByAsync(
        Expression<Func<Room, bool>> predicate,
        CancellationToken cancellationToken = default);
}