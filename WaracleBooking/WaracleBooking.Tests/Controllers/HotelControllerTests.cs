using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Controllers;
using WaracleBooking.Services.Interfaces;
using WaracleBooking.Models.Domain;
using HotelModel = WaracleBooking.Models.Domain.Hotel;
using HotelEntity = WaracleBooking.Persistence.Entities.Hotel;
using RoomEntity = WaracleBooking.Persistence.Entities.Room;

namespace WaracleBooking.Tests.Controllers
{
    public class HotelControllerTests
    {
        private readonly IBookingService _bookingService;
        private readonly HotelController _controller;

        public HotelControllerTests()
        {
            _bookingService = A.Fake<IBookingService>();
            _controller = new HotelController(_bookingService);
        }

        [Fact]
        public async Task GetHotelByName_WithValidName_ReturnsOkWithHotels()
        {
            // Arrange
            var hotelName = "Test Hotel";
            var expectedResponse = new List<HotelEntity>
            {
                new HotelEntity { Id = 1, Name = "Test Hotel 1" },
                new HotelEntity { Id = 2, Name = "Test Hotel 2" }
            };

            A.CallTo(() => _bookingService.GetHotelsByNameAsync(hotelName, A<CancellationToken>._))
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetHotelByName("Test Hotel", CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<HotelModel>>()
                .Which.Should().HaveSameCount(expectedResponse);
        }

        [Fact]
        public async Task GetHotelByName_WithEmptyName_ReturnsOkWithAllHotels()
        {
            // Arrange
            var hotelName = "     ";
            var expectedResponse = new List<HotelEntity>
            {
                new HotelEntity { Id = 1, Name = "Test Hotel 1" },
                new HotelEntity { Id = 2, Name = "Test Hotel 2" },
                new HotelEntity { Id = 3, Name = "Test Hotel 3" }
            };

            A.CallTo(() => _bookingService.GetHotelsByNameAsync(hotelName, A<CancellationToken>._))
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetHotelByName(hotelName, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<HotelModel>>()
                .Which.Should().HaveSameCount(expectedResponse);
        }

        [Fact]
        public async Task GetHotelByName_WithInvalidName_ReturnsOkWithNoHotels()
        {
            // Arrange
            var hotelName = "No";

            A.CallTo(() => _bookingService.GetHotelsByNameAsync(hotelName, A<CancellationToken>._))
                .Returns(new List<HotelEntity>());

            // Act
            var result = await _controller.GetHotelByName(hotelName, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<HotelModel>>()
                .Which.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableRooms_WithValidParameters_ReturnsAvailableRooms()
        {
            // Arrange
            var hotelId = 1;
            var fromDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3));
            var toDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var numberOfGuests = 2;
            var expectedRooms = new List<RoomEntity>
            {
                new RoomEntity { Id = 1, Capacity = 2 },
                new RoomEntity { Id = 2, Capacity = 2 }
            };

            A.CallTo(() =>
                    _bookingService.GetAvailableRoomsAsync(hotelId, fromDate, toDate, numberOfGuests,
                        A<CancellationToken>._))
                .Returns(expectedRooms);

            // Act
            var result =
                await _controller.GetAvailableRooms(hotelId, fromDate, toDate, numberOfGuests, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<Room>>()
                .Which.Should().HaveSameCount(expectedRooms);
        }
        
        [Fact]
        public async Task GetAvailableRooms_WithInvalidGuestCount_ReturnsBadRequest()
        {
            // Arrange
            var hotelId = 1;
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
            var toDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var numberOfGuests = 0;

            // Act
            var result =
                await _controller.GetAvailableRooms(hotelId, fromDate, toDate, numberOfGuests, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Invalid number of guests specified, must be at least 1.");
        }

        [Theory]
        [MemberData(nameof(InvalidDateRanges))]
        public async Task GetAvailableRooms_WithInvalidDateRange_ReturnsBadRequest(
            DateOnly fromDate,
            DateOnly toDate,
            string errorMessage)
        {
            // Arrange
            var hotelId = 1;
            var numberOfGuests = 2;

            // Act
            var result =
                await _controller.GetAvailableRooms(hotelId, fromDate, toDate, numberOfGuests, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(errorMessage);
        }

        public static IEnumerable<object[]> InvalidDateRanges =>
            new List<object[]>
            {
                new object[]
                {
                    DateOnly.FromDateTime(DateTime.Now).AddDays(5),
                    DateOnly.FromDateTime(DateTime.Now).AddDays(4),
                    "`To` date must be after `From` date."
                },
                new object[]
                {
                    DateOnly.FromDateTime(DateTime.Now),
                    DateOnly.FromDateTime(DateTime.Now),
                    "`To` date must be after `From` date."
                },
                new object[]
                {
                    DateOnly.FromDateTime(DateTime.Now).AddDays(-1),
                    DateOnly.FromDateTime(DateTime.Now).AddDays(4),
                    "We all wish we could travel back in time, need to save up for McLaren."
                }
            };
    }
}