using System.Linq.Expressions;
using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Persistence.Repositories.Interfaces;

public interface IHotelRepository
{
    Task<List<Hotel>> FilterByAsync(Expression<Func<Hotel, bool>> predicate, CancellationToken cancellationToken = default);
}