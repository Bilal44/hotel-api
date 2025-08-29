using System.ComponentModel.DataAnnotations;

namespace WaracleBooking.Models.Domain.Booking.RequestModels;

public record BookingRequest
{
    [Required(AllowEmptyStrings=false)]
    public string GuestNames { get; init; }
    public int NumberOfGuests { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public int RoomId { get; init; }
}