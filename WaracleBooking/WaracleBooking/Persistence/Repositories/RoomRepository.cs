using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Persistence.Repositories;

public class RoomRepository(BookingDbContext dbContext) : IRoomRepository
{
    public async Task<Room?> GetByIdAsync(int id)
    {
        return await dbContext.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<List<Room>> FilterByAsync(
        Expression<Func<Room, bool>> predicate,
        CancellationToken cancellationToken) =>
        await dbContext.Rooms
            .Where(predicate)
            .Include(r => r.Bookings)
            .ToListAsync(cancellationToken);
}