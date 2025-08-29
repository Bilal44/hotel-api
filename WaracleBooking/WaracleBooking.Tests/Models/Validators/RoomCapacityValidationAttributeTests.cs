using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using WaracleBooking.Models.Domain;
using WaracleBooking.Models.Domain.Enums;
using WaracleBooking.Models.Validators;

namespace WaracleBooking.Tests.Models.Validators;

public class RoomCapacityValidationAttributeTests
{
    private readonly RoomCapacityValidationAttribute _attribute = new RoomCapacityValidationAttribute();
    
    [Theory]
    [InlineData(RoomType.Single, 1)]
    [InlineData(RoomType.Double, 2)]
    [InlineData(RoomType.Deluxe, 2)]
    public void Room_WithCorrectCapacity_PassesValidation(
        RoomType type,
        int capacity)
    {
        // Arrange
        var room = new Room { Type = type, Capacity = capacity };
        var validationContext = new ValidationContext(room);

        // Act
        var result = _attribute.GetValidationResult(capacity, validationContext);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }
    
    [Fact]
    public void NullValue_PassesValidation()
    {
        // Arrange
        var room = new Room { Type = RoomType.Single, Capacity = 1 };
        var validationContext = new ValidationContext(room);

        // Act
        var result = _attribute.GetValidationResult(null, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(RoomType.Single, 0, "Single rooms must have a capacity of 1.")]
    [InlineData(RoomType.Single, 2, "Single rooms must have a capacity of 1.")]
    [InlineData(RoomType.Double, 1, "Double rooms must have a capacity of 2.")]
    [InlineData(RoomType.Double, 3, "Double rooms must have a capacity of 2.")]
    [InlineData(RoomType.Deluxe, 1, "Deluxe rooms must have a capacity of 2.")]
    [InlineData(RoomType.Deluxe, 5, "Deluxe rooms must have a capacity of 2.")]
    public void Room_WithIncorrectCapacity_ReturnsError(
        RoomType type,
        int capacity,
        string errorMessage)
    {
        // Arrange
        var room = new Room { Type = type, Capacity = capacity };
        var validationContext = new ValidationContext(room);

        // Act
        var result = _attribute.GetValidationResult(room.Capacity, validationContext);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be(errorMessage);
    }
}