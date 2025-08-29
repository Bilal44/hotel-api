using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WaracleBooking.Persistence.Entities.Enums;

namespace WaracleBooking.Persistence.Entities;

public class Room
{
    [Key]
    public int Id { get; set; }
    public RoomType Type { get; set; }
    public int Capacity { get; set; }
    public ICollection<Booking> Bookings { get; set; }
    
    [ForeignKey(nameof(Hotel))]
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
}