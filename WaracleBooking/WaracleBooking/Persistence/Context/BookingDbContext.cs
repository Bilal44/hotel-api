using Microsoft.EntityFrameworkCore;
using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Persistence.Context;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
}