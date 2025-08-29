using FluentAssertions;
using WaracleBooking.Mapping;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Entities.Enums;
using BookingStatusModel = WaracleBooking.Models.Domain.Enums.BookingStatus;
using HotelModel = WaracleBooking.Models.Domain.Hotel;
using RoomModel = WaracleBooking.Models.Domain.Room;
using RoomTypeModel = WaracleBooking.Models.Domain.Enums.RoomType;

namespace WaracleBooking.Tests.Mapping
{
    public class BookingMapperTests
    {
        [Fact]
        public void MapToBooking_ValidBooking_ReturnsCorrectBookingResponse()
        {
            // Arrange
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                GuestNames = "John Doe",
                NumberOfGuests = 2,
                CheckIn = new DateOnly(2024, 1, 15),
                CheckOut = new DateOnly(2024, 1, 20),
                Status = BookingStatus.Success
            };

            // Act
            var result = BookingMapper.MapToBooking(booking);

            // Assert
            result.Id.Should().Be(booking.Id);
            result.GuestNames.Should().Be(booking.GuestNames);
            result.NumberOfGuests.Should().Be(booking.NumberOfGuests);
            result.CheckIn.Should().Be(booking.CheckIn); 
            result.CheckOut.Should().Be(booking.CheckOut);
            result.Status.Should().Be(BookingStatusModel.Confirmed);
        }

        [Fact]
        public void MapToHotels_ValidHotelList_ReturnsCorrectHotelModels()
        {
            // Arrange
            var hotels = new List<Hotel>
            {
                new Hotel { Id = 1, Name = "Waracle Hotel" },
                new Hotel { Id = 2, Name = "City Inn" }
            };

            // Act
            var result = BookingMapper.MapToHotels(hotels).ToList();

            // Assert
            result.Should().HaveCount(2);
            
            result.Should().BeEquivalentTo(new[]
            {
                new HotelModel { Id = 1, Name = "Waracle Hotel" },
                new HotelModel { Id = 2, Name = "City Inn" }
            });
        }

        [Fact]
        public void MapToRooms_ValidRoomList_ReturnsCorrectRoomModels()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room { Id = 1, Capacity = 1, Type = RoomType.Single },
                new Room { Id = 2, Capacity = 2, Type = RoomType.Double },
                new Room { Id = 3, Capacity = 4, Type = RoomType.Deluxe }
            };

            // Act
            var result = BookingMapper.MapToRooms(rooms).ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(new[]
            {
                new RoomModel { Id = 1, Capacity = 1, Type = RoomTypeModel.Single },
                new RoomModel { Id = 2, Capacity = 2, Type = RoomTypeModel.Double },
                new RoomModel { Id = 3, Capacity = 4, Type = RoomTypeModel.Deluxe }
            });
        }

        [Theory]
        [InlineData(BookingStatus.Success, BookingStatusModel.Confirmed)]
        [InlineData(BookingStatus.Pending, BookingStatusModel.Failed)]
        [InlineData(BookingStatus.Cancelled, BookingStatusModel.Failed)]
        [InlineData((BookingStatus)999, BookingStatusModel.Failed)]
        public void MapBookingStatus_MapsTo_CorrectResponseStatus(
            BookingStatus status,
            BookingStatusModel expectedStatus)
        {
            // Arrange
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                GuestNames = "John Doe",
                NumberOfGuests = 2,
                CheckIn = new DateOnly(2024, 1, 15),
                CheckOut = new DateOnly(2024, 1, 20),
                Status = status
            };

            // Act
            var result = BookingMapper.MapToBooking(booking);

            // Assert
            result.Status.Should().Be(expectedStatus);
        }
        
        [Fact]
        public void MapRoomType_UnsupportedType_ThrowsNotSupportedException()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room { Id = 1, Capacity = 1, Type = (RoomType)99 }
            };

            // Act
            var act = () => BookingMapper.MapToRooms(rooms).ToList();
            
            // Assert
            act.Should().Throw<NotSupportedException>()
                .WithMessage($"Unsupported room type: [{rooms[0].Type}]");
        }
    }
}