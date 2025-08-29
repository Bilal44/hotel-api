using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly BookingDbContext _dbContext;

    public RoomRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _dbContext.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
    
    public async Task<List<Room>> FilterByAsync(
        Expression<Func<Room, bool>> predicate,
        CancellationToken cancellationToken) =>
        await _dbContext.Rooms
            .Where(predicate)
            .Include(h => h.Bookings)
            .ToListAsync(cancellationToken);
}