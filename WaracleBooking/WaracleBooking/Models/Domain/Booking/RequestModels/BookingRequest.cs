namespace WaracleBooking.Models.Domain.Booking.RequestModels;

public record BookingRequest
{
    public int RoomId { get; init; }
    public string GuestNames { get; init; }
    public int NumberOfGuests { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
}