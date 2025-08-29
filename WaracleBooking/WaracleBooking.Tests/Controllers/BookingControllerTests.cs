using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Controllers;
using WaracleBooking.Mapping;
using WaracleBooking.Models.Domain.Booking;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Services.Interfaces;
using BookingEntity = WaracleBooking.Persistence.Entities.Booking;

namespace WaracleBooking.Tests.Controllers
{
    public class BookingControllerTests
    {
        private readonly IBookingService _bookingService;
        private readonly BookingController _controller;

        public BookingControllerTests()
        {
            _bookingService = A.Fake<IBookingService>();
            _controller = new BookingController(_bookingService);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        }

        [Fact]
        public async Task GetBooking_WithValidId_ReturnsOkWithBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var expectedResponse = new BookingEntity { Id = Guid.NewGuid(), RoomId = 1, NumberOfGuests = 1 };

            A.CallTo(() => _bookingService.GetBookingByIdAsync(bookingId))
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetBooking(bookingId);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(BookingMapper.MapToBooking(expectedResponse));
        }
        
        [Fact]
        public async Task GetBooking_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
        
            A.CallTo(() => _bookingService.GetBookingByIdAsync(bookingId))
                .Returns((BookingEntity)null!);
        
            // Act
            var result = await _controller.GetBooking(bookingId);
        
            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateBooking_WithValidRequest_ReturnsCreatedWithBooking()
        {
            // Arrange
            var request = new BookingRequest
            {
                RoomId = 1,
                GuestNames = "guest",
                From = DateOnly.FromDateTime(DateTime.Now),
                To = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                NumberOfGuests = 1
            };
            var bookingResult = new BookingValidationResult { Success = true, Booking = new BookingEntity { Id = Guid.NewGuid() } };
        
            A.CallTo(() => _bookingService.CreateBookingAsync(request))
                .Returns(bookingResult);
        
            // Act
            var result = await _controller.CreateBooking(request);
        
            // Assert
            result.Should().BeOfType<CreatedResult>()
                .Which.Value.Should().BeEquivalentTo(BookingMapper.MapToBooking(bookingResult.Booking));;
        }
        
        [Fact]
        public async Task CreateBooking_WithInvalidData_ReturnsBadRequestWithResult()
        {
            // Arrange
            var request = new BookingRequest();
            var bookingResult = new BookingValidationResult { Success = false, ErrorMessage = "Validation failed" };
        
            A.CallTo(() => _bookingService.CreateBookingAsync(request))
                .Returns(bookingResult);
        
            // Act
            var result = await _controller.CreateBooking(request);
        
            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(bookingResult);
        }
    }
}