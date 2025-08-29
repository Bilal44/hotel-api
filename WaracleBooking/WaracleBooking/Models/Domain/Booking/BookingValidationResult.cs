namespace WaracleBooking.Models.Domain.Booking;

public record BookingValidationResult
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; }
    public Persistence.Entities.Booking Booking { get; init; }

    public static BookingValidationResult Fail(string message) =>
        new BookingValidationResult
        {
            Success = false,
            ErrorMessage = message
        };
    
    public static BookingValidationResult Pass(Persistence.Entities.Booking booking) =>
        new BookingValidationResult
        {
            Success = true,
            Booking = booking
        };
}