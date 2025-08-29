using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Entities.Enums;

namespace WaracleBooking.Tests.IntegrationTests.TestHelpers;

public class TestDbFactory : WebApplicationFactory<Program>
{
    public readonly BookingDbContext BookingDbContext;

    public TestDbFactory()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        BookingDbContext = new BookingDbContext(options);

        Seed();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BookingDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<BookingDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
        });

        return base.CreateHost(builder);
    }

    private void Seed()
    {
        BookingDbContext.Database.EnsureDeleted();
        BookingDbContext.Database.EnsureCreated();
        if (!BookingDbContext.Hotels.Any())
        {
            BookingDbContext.Hotels.AddRange(
                new Hotel { Id = 1, Name = "Waracle Hotel" },
                new Hotel { Id = 2, Name = "Grand Hotel" }
            );

            BookingDbContext.Rooms.AddRange(
                new Room { Id = 1, Type = RoomType.Single, Capacity = 1, HotelId = 1 },
                new Room { Id = 2, Type = RoomType.Double, Capacity = 2, HotelId = 1 },
                new Room { Id = 3, Type = RoomType.Double, Capacity = 2, HotelId = 1 },
                new Room { Id = 4, Type = RoomType.Deluxe, Capacity = 2, HotelId = 1 },
                new Room { Id = 5, Type = RoomType.Deluxe, Capacity = 2, HotelId = 1 },
                new Room { Id = 6, Type = RoomType.Single, Capacity = 1, HotelId = 1 },
                new Room { Id = 7, Type = RoomType.Single, Capacity = 1, HotelId = 2 },
                new Room { Id = 8, Type = RoomType.Double, Capacity = 2, HotelId = 2 },
                new Room { Id = 9, Type = RoomType.Double, Capacity = 2, HotelId = 2 },
                new Room { Id = 10, Type = RoomType.Deluxe, Capacity = 2, HotelId = 2 },
                new Room { Id = 11, Type = RoomType.Deluxe, Capacity = 2, HotelId = 2 },
                new Room { Id = 12, Type = RoomType.Single, Capacity = 1, HotelId = 2 }
            );

            BookingDbContext.Bookings.AddRange(
                new Booking
                {
                    Id = new Guid("d2f66cd0-9c8d-4c51-960e-c798c1f357fb"),
                    RoomId = 6,
                    GuestNames = "B Ahmad",
                    CreatedAt = DateTime.UtcNow,
                    CheckIn = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                    CheckOut = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                    NumberOfGuests = 1,
                    Status = BookingStatus.Success
                },
                new Booking
                {
                    Id = new Guid("cb7aea26-61e9-4049-b7be-9867b2c24af4"),
                    RoomId = 9,
                    GuestNames = "B Ahmad and Guest 1",
                    CreatedAt = DateTime.UtcNow.AddMinutes(2.4),
                    CheckIn = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                    CheckOut = DateOnly.FromDateTime(DateTime.Now.AddDays(18)),
                    NumberOfGuests = 2,
                    Status = BookingStatus.Success
                },
                new Booking
                {
                    Id = new Guid("aebbf84d-bd61-4a8a-b8d7-a360e3d9c39c"),
                    RoomId = 10,
                    GuestNames = "B Ahmad and Guest 2",
                    CreatedAt = DateTime.UtcNow.AddMinutes(7.2),
                    CheckIn = DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                    CheckOut = DateOnly.FromDateTime(DateTime.Now.AddDays(45)),
                    NumberOfGuests = 2,
                    Status = BookingStatus.Success
                }
            );
            
            BookingDbContext.SaveChangesAsync();
        }
    }
}