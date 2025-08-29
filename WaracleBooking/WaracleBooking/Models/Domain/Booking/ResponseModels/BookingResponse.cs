using System.Text.Json.Serialization;
using WaracleBooking.Models.Domain.Enums;

namespace WaracleBooking.Models.Domain.Booking.ResponseModels;

public record BookingResponse
{
    public Guid Id { get; init; }
    public string GuestNames { get; init; }
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public int NumberOfGuests { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; init; }
}