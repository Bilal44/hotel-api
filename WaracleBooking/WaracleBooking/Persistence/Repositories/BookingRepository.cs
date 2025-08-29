using Microsoft.EntityFrameworkCore;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Persistence.Repositories;

public class BookingRepository(BookingDbContext dbContext) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await dbContext.Bookings
            .SingleOrDefaultAsync(b => b.Id == id);
    }

    public async Task AddAsync(Booking booking)
    {
        await dbContext.Bookings.AddAsync(booking);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Booking booking)
    {
        dbContext.Bookings.Update(booking);
        await dbContext.SaveChangesAsync();
    }
}