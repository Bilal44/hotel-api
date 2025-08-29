using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Filters;
using WaracleBooking.Mapping;
using WaracleBooking.Models.Domain;
using WaracleBooking.Models.Domain.Booking.Validators;
using WaracleBooking.Services.Interfaces;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace WaracleBooking.Controllers;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/hotels")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public class HotelController(IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Retrieves hotels that fully or partially match the specified name.
    /// The name matching is case-insensitive.
    /// </summary>
    /// <param name="name">Optional: The name or partial name of the hotel.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of matching hotels.</returns>
    /// <remarks>
    /// If <paramref name="name"/> is null or whitespace, the method returns all hotels.
    /// </remarks>
    [ProducesResponseType(typeof(List<Hotel>), Status200OK)]
    [ProducesResponseType(Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetHotelByName(
        [FromQuery] string? name,
        CancellationToken cancellationToken) =>
          Ok(BookingMapper.MapToHotels(await bookingService.GetHotelsByNameAsync(name, cancellationToken)));
    
    /// <summary>
    /// Retrieves available rooms for a hotel within a specified date range and guest count.
    /// </summary>
    /// <param name="id">The ID of the hotel.</param>
    /// <param name="from">Start date of the booking.</param>
    /// <param name="to">End date of the booking.</param>
    /// <param name="numberOfGuests">Number of guests to accommodate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of available rooms.</returns>
    [ProducesResponseType(typeof(List<Room>), Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    [ProducesResponseType(Status500InternalServerError)]
    [HttpGet("{id:int}/rooms")]
    public async Task<IActionResult> GetAvailableRooms(
        [FromRoute] int id,
        DateOnly from,
        DateOnly to,
        int numberOfGuests,
        CancellationToken cancellationToken)
    {
        var error = DateAndGuestInputValidator.Validate(from, to, numberOfGuests);
        if (error is not null)
            return BadRequest(error);
        
        return Ok(BookingMapper.MapToRooms(
            await bookingService.GetAvailableRoomsAsync(id, from, to, numberOfGuests, cancellationToken)));
    }
}