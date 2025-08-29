using System.ComponentModel.DataAnnotations;
using WaracleBooking.Models.Domain;
using WaracleBooking.Models.Domain.Enums;

namespace WaracleBooking.Models.Validators;

public class RoomCapacityValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        
        if (value is null)
        {
            // Not validating nulls, use [Required] if needed
            return ValidationResult.Success;
        }
        
        var room = (Room)validationContext.ObjectInstance;

        return room.Type switch
        {
            RoomType.Single when room.Capacity != 1 =>
                new ValidationResult("Single rooms must have a capacity of 1."),
            RoomType.Double or RoomType.Deluxe when room.Capacity != 2 =>
                new ValidationResult($"{room.Type.ToString()} rooms must have a capacity of 2."),
            _ => ValidationResult.Success
        };
    }
}