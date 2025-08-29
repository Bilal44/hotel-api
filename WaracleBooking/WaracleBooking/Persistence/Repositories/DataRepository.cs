using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Entities.Enums;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Persistence.Repositories;

public class DataRepository (BookingDbContext dbContext) : IDataRepository
{
    public async Task SeedData()
    {
        if (!dbContext.Hotels.Any())
        {
            dbContext.Hotels.AddRange(
                new Hotel { Id = 1, Name = "Waracle Hotel" },
                new Hotel { Id = 2, Name = "Grand Hotel" }
            );

            dbContext.Rooms.AddRange(
                new Room { Id = 1, Type = RoomType.Single, Capacity = 1, HotelId = 1 },
                new Room { Id = 2, Type = RoomType.Double, Capacity = 2, HotelId = 1 },
                new Room { Id = 3, Type = RoomType.Double, Capacity = 2, HotelId = 1 },
                new Room { Id = 4, Type = RoomType.Deluxe, Capacity = 4, HotelId = 1 },
                new Room { Id = 5, Type = RoomType.Deluxe, Capacity = 4, HotelId = 1 },
                new Room { Id = 6, Type = RoomType.Single, Capacity = 1, HotelId = 1 },
                new Room { Id = 7, Type = RoomType.Single, Capacity = 1, HotelId = 2 },
                new Room { Id = 8, Type = RoomType.Double, Capacity = 2, HotelId = 2 },
                new Room { Id = 9, Type = RoomType.Double, Capacity = 2, HotelId = 2 },
                new Room { Id = 10, Type = RoomType.Deluxe, Capacity = 4, HotelId = 2 },
                new Room { Id = 11, Type = RoomType.Deluxe, Capacity = 4, HotelId = 2 },
                new Room { Id = 12, Type = RoomType.Single, Capacity = 1, HotelId = 2 }
            );

            dbContext.Bookings.AddRange(
                new Booking
                {
                    Id = new Guid("d2f66cd0-9c8d-4c51-960e-c798c1f357fb"),
                    RoomId = 6,
                    GuestNames = "B Ahmad",
                    CheckIn = new DateOnly(2025, 9, 20),
                    CheckOut = new DateOnly(2025, 9, 22),
                    NumberOfGuests = 2
                },
                new Booking
                {
                    Id = new Guid("cb7aea26-61e9-4049-b7be-9867b2c24af4"),
                    RoomId = 9,
                    GuestNames = "B Ahmad and Guest 1",
                    CheckIn = new DateOnly(2025, 9, 25),
                    CheckOut = new DateOnly(2025, 9, 26),
                    NumberOfGuests = 2
                },
                new Booking
                {
                    Id = new Guid("aebbf84d-bd61-4a8a-b8d7-a360e3d9c39c"),
                    RoomId = 10,
                    GuestNames = "B Ahmad, Guest 1, Guest 2",
                    CheckIn = new DateOnly(2025, 9, 29),
                    CheckOut = new DateOnly(2025, 10, 20),
                    NumberOfGuests = 2
                }
            );
            
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ClearData()
    {
        dbContext.Bookings.RemoveRange(dbContext.Bookings);
        dbContext.Rooms.RemoveRange(dbContext.Rooms);
        dbContext.Hotels.RemoveRange(dbContext.Hotels);
        
        await dbContext.SaveChangesAsync();
    }
}