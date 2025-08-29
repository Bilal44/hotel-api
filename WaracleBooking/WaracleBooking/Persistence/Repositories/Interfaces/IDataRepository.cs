namespace WaracleBooking.Persistence.Repositories.Interfaces;

public interface IDataRepository
{
    Task SeedData();
    Task ClearData();
}