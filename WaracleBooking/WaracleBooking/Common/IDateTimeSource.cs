namespace WaracleBooking.Common;

public interface IDateTimeSource
{
    DateTime UtcNow { get; } 
}