using System.Text.Json.Serialization;
using WaracleBooking.Models.Domain.Enums;
using WaracleBooking.Models.Validators;

namespace WaracleBooking.Models.Domain;

[RoomCapacityValidation]
public record Room
{
    public int Id { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoomType Type { get; init; }
    public int Capacity { get; init; }
}