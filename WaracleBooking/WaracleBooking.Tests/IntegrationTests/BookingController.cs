using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json;
using WaracleBooking.Models.Domain.Booking;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Models.Domain.Booking.ResponseModels;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Tests.IntegrationTests.TestHelpers;
using BookingStatus = WaracleBooking.Persistence.Entities.Enums.BookingStatus;

namespace WaracleBooking.Tests.IntegrationTests;

public class BookingController(TestDbFactory factory) : IClassFixture<TestDbFactory>
{
    private readonly TestDbFactory _factory = factory;
    private readonly BookingDbContext _dbContext = factory.BookingDbContext;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateBooking_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var requestModel = CreateValidBookingRequest();
        var request = JsonContent.Create(requestModel);
        
        // Act
        var response = await _client.PostAsync("api/bookings", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var booking = JsonConvert.DeserializeObject<BookingResponse>(content);
        booking.Should().NotBeNull();
        booking.Id.Should().NotBeEmpty();
        booking.GuestNames.Should().Be(requestModel.GuestNames);

        _dbContext.Bookings.Should().HaveCount(4);
        var dbBooking = _dbContext.Bookings.Last();
        dbBooking.GuestNames.Should().Be(requestModel.GuestNames);
        dbBooking.Status.Should().Be(BookingStatus.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidBookingRequests))]
    public async Task CreateBooking_WithInvalidRequest_ReturnsBadRequest(
        BookingRequest requestModel,
        string expectedErrorMessage)
    {
        // Arrange
        var request = JsonContent.Create(requestModel);

        // Act
        var response = await _client.PostAsync("api/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var bookingResponse = JsonConvert.DeserializeObject<BookingValidationResult>(content);
        bookingResponse.Should().NotBeNull();
        bookingResponse.Success.Should().BeFalse();
        bookingResponse.ErrorMessage.Should().Be(expectedErrorMessage);

        _dbContext.Bookings.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetBooking_WithValidId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync($"api/bookings/{TestConstants.BookingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var bookingResponse = JsonConvert.DeserializeObject<BookingResponse>(content);
        bookingResponse.Should().NotBeNull();
        bookingResponse.Id.Should().Be(TestConstants.BookingId);
    }

    [Fact]
    public async Task GetBooking_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"api/bookings/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private BookingRequest CreateValidBookingRequest() => new()
    {
        GuestNames = "John Doe",
        NumberOfGuests = 2,
        RoomId = 4,
        From = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
        To = DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
    };
    
    public static IEnumerable<object[]> InvalidBookingRequests()
    {
        var baseRequest = new BookingRequest
        {
            RoomId = 6,
            GuestNames = "John Doe",
            From = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
            To = DateOnly.FromDateTime(DateTime.Now.AddDays(13)),
            NumberOfGuests = 2
        };

        yield return new object[]
        {
            baseRequest with { From = DateOnly.FromDateTime(DateTime.Now), To = DateOnly.FromDateTime(DateTime.Now) },
            "`To` date must be after `From` date."
        };

        yield return new object[]
        {
            baseRequest with { From = DateOnly.FromDateTime(DateTime.Now).AddDays(-1) },
            "We all wish we could travel back in time, need to save up for McLaren."
        };

        yield return new object[]
        {
            baseRequest with { NumberOfGuests = 0 },
            "Invalid number of guests specified, must be at least 1."
        };

        yield return new object[]
        {
            baseRequest with { RoomId = 999 },
            "Room not found."
        };

        yield return new object[]
        {
            baseRequest with { NumberOfGuests = 10 },
            "Room can only accommodate 1 guest."
        };

        yield return new object[]
        {
            baseRequest with { From = DateOnly.FromDateTime(DateTime.Now), To = DateOnly.FromDateTime(DateTime.Now.AddDays(5)), },
            "Room is unavailable for the selected dates."
        };
    }
}
