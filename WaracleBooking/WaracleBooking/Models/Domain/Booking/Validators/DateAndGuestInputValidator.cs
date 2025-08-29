namespace WaracleBooking.Models.Domain.Booking.Validators;

public static class DateAndGuestInputValidator
{
    public static string? Validate(DateOnly from, DateOnly to, int numberOfGuests)
    {
        if (numberOfGuests < 1)
            return "Invalid number of guests specified, must be at least 1.";

        if (from < DateOnly.FromDateTime(DateTime.Today))
            return "We all wish we could travel back in time, need to save up for McLaren.";

        if (!(to > from))
            return "`To` date must be after `From` date.";

        return null;
    }
}