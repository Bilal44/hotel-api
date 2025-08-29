using System.ComponentModel.DataAnnotations;

namespace WaracleBooking.Persistence.Entities;

public class Hotel
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    
    [MaxLength(6)]
    public ICollection<Room> Rooms { get; set; }
}