using WaracleBooking.Models.Domain.Booking;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Services.Interfaces;

public interface IBookingService
{
    Task<List<Hotel>> GetHotelsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Room>> GetAvailableRoomsAsync(
        int hotelId,
        DateOnly from,
        DateOnly to,
        int numberOfGuests,
        CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingByIdAsync(Guid id);
    Task<BookingValidationResult> CreateBookingAsync(BookingRequest request);
}