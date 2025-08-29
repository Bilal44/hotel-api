using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using WaracleBooking.Models.Domain;
using WaracleBooking.Tests.IntegrationTests.TestHelpers;

namespace WaracleBooking.Tests.IntegrationTests;

public class HotelController(TestDbFactory factory) : IClassFixture<TestDbFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

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
    [MemberData(nameof(AvailableRoomValidRequests))]
    public async Task GetAvailableRooms_WithValidRequest_ReturnsOk(
        int hotelId,
        int numberOfGuests,
        DateOnly from,
        DateOnly to,
        int expectedRoomCount
    )
    {
        // Act
        var response =
            await _client.GetAsync(
                $"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

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
        var from = DateOnly.FromDateTime(DateTime.Today);
        var to = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var numberOfGuests = 2;

        // Act
        var response =
            await _client.GetAsync(
                $"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

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
        var from = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        var to = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var numberOfGuests = 2;

        // Act
        var response = await _client.GetAsync(
            $"api/hotels/{hotelId}/rooms?from={from:O}&to={to:O}&numberOfGuests={numberOfGuests}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static IEnumerable<object[]> AvailableRoomValidRequests =>
        new List<object[]>
        {
            // No overlap, all rooms eligible for a single guest
            new object[]
            {
                1,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                6
            },
            // No overlap, only 2 Double and 2 Deluxe eligible for 2 guests
            new object[] 
            {
                1,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                4
            },
            // Excluded room 6 (booked day 2-3), all other rooms eligible
            new object[]
            {
                1,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                5
            },
            // Excluded room 6 (booked day 2-3), other single room excluded too
            new object[]
            {
                1,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                4
            },
            // Room 6 check-out is on day 3, all rooms eligible
            new object[]
            {
                1,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
                6
            },
            // Room 6 check-out is on day 3, both single rooms excluded though
            new object[]
            {
                1,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
                4
            },
            // Room 6 check-in is on day 2, all rooms eligible
            new object[]
            {
                1,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                6
            },
            // Room 9 check-in is on day 10, only single rooms excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(6)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                4
            },
            // Excluded room 9 (booked day 10-18), two single rooms also excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(11)),
                3
            },
            // Excluded room 9 (booked day 10-18)
            new object[]
            {
                2,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(11)),
                5
            },
            // Excluded room 9 (booked day 10-18), two single rooms also excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(17)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                3
            },
            // Excluded room 10 (booked day 25-45), two single rooms also excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(18)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(26)),
                3
            },
            // Room 9 check-out = day 18, Room 10 check-in = day 25, only single rooms excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(18)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                4
            },
            // Room 9 check-out = day 18, Room 10 check-in = day 25, all rooms eligible
            new object[]
            {
                2,
                1,
                DateOnly.FromDateTime(DateTime.Now.AddDays(18)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(25)),
                6
            },
            // Room 9 and Room 10 are both booked, single rooms are also excluded
            new object[]
            {
                2,
                2,
                DateOnly.FromDateTime(DateTime.Now.AddDays(17)),
                DateOnly.FromDateTime(DateTime.Now.AddDays(26)),
                2
            }
        };
}