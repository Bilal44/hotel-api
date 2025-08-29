using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Persistence.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly BookingDbContext _dbContext;

    public HotelRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Hotel>> FilterByAsync(
        Expression<Func<Hotel, bool>> predicate,
        CancellationToken cancellationToken) =>
        await _dbContext.Hotels
            .Where(predicate)
            .Include(h => h.Rooms)
            .ToListAsync(cancellationToken);
}