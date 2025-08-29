using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WaracleBooking.Persistence.Entities.Enums;

namespace WaracleBooking.Persistence.Entities;

public class Booking
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string GuestNames { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int NumberOfGuests { get; set; }
    public BookingStatus Status { get; set; }
    
    [ForeignKey(nameof(Room))]
    public int RoomId { get; set; }
    public Room Room { get; set; }
}