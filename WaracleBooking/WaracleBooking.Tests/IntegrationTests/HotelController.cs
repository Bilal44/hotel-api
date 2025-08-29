using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using WaracleBooking.Models.Domain;
using WaracleBooking.Tests.IntegrationTests.TestHelpers;

namespace WaracleBooking.Tests.IntegrationTests;

public class HotelController : IClassFixture<TestDbFactory>
{
    private readonly TestDbFactory _factory;
    private readonly HttpClient _client;

    public HotelController(TestDbFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHotelByName_WithValidName_CaseInsensitive_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("api/hotels?name=WaRaC");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hotels = JsonConvert.DeserializeObject<List<Hotel>>(content);
        hotels.Should().NotBeNull();
        hotels.Should().ContainSingle(h => h.Name == "Waracle Hotel");
    }

    [Fact]
    public async Task GetHotelByName_WithInvalidName_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("api/hotels?name=NonExistentHotel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hotels = JsonConvert.DeserializeObject<List<Hotel>>(content);
        hotels.Should().NotBeNull();
        hotels.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 3)]
    public async Task GetAvailableRooms_WithValidRequest_ReturnsOk(int hotelId, int expectedRoomCount)
    {
        // Arrange
        var from = new DateTime(2025, 10, 02);
        var to = new DateTime(2025, 10, 04);
        var numberOfGuests = 2;

        // Act
        var response = await _client.GetAsync($"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var rooms = JsonConvert.DeserializeObject<List<Room>>(content);
        rooms.Should().NotBeNull();
        rooms.Should().HaveCount(expectedRoomCount);
        rooms.Should().OnlyContain(r => r.Capacity >= numberOfGuests);
    }

    [Fact]
    public async Task GetAvailableRooms_WithInvalidHotelId_ReturnsEmptyList()
    {
        // Arrange
        var hotelId = 999;
        var from = new DateTime(2025, 9, 23);
        var to = new DateTime(2025, 9, 24);
        var numberOfGuests = 2;

        // Act
        var response = await _client.GetAsync($"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var rooms = JsonConvert.DeserializeObject<List<Room>>(content);
        rooms.Should().NotBeNull();
        rooms.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAvailableRooms_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var hotelId = 1;
        var from = new DateTime(2025, 9, 25);
        var to = new DateTime(2025, 9, 24);
        var numberOfGuests = 2;

        // Act
        var response = await _client.GetAsync(
            $"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
