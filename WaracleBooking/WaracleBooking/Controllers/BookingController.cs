using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Filters;
using WaracleBooking.Mapping;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Services.Interfaces;

namespace WaracleBooking.Controllers;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/bookings")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Retrieves a booking by its unique identifier.
    /// </summary>
    /// <param name="id">The unique <see cref="Guid"/> of the booking to retrieve.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing the booking if found; otherwise, a <see cref="NotFoundResult"/>.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking is null)
            return NotFound();

        return Ok(BookingMapper.MapToBooking(booking));
    }
    
    /// <summary>
    /// Creates a new booking based on the provided request data.
    /// </summary>
    /// <param name="request">The booking request containing room ID, guest details and date range.</param>
    /// <returns>
    /// Returns a <see cref="CreatedResult"/> with the newly created booking if successful; 
    /// otherwise, a <see cref="BadRequestObjectResult"/> with appropriate validation error.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateBooking(BookingRequest request)
    {
        var bookingResult = await _bookingService.CreateBookingAsync(request);
        
        if (!bookingResult.Success)
            return BadRequest(bookingResult);
        
        return Created(
            $"{Request.GetDisplayUrl()}/{bookingResult.Booking.Id}",
            BookingMapper.MapToBooking(bookingResult.Booking));
    }
}