using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Persistence.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
}