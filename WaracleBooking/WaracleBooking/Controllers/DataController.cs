using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Filters;
using WaracleBooking.Persistence.Repositories.Interfaces;

namespace WaracleBooking.Controllers;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/data")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public class DataController (IDataRepository dataRepository) : ControllerBase
{
    /// <summary>
    /// Seeds the database with initial data for hotels, rooms and bookings.
    /// </summary>
    /// <returns>A <see cref="CreatedResult"/> indicating the seeding operation was successful.</returns>
    [HttpPost]
    public async Task<IActionResult> SeedData()
    {
        await dataRepository.SeedData();
        return Created();
    }
    
    /// <summary>
    /// Clears all hotel, room and booking data from the database.
    /// </summary>
    /// <returns>An <see cref="OkResult"/> indicating the reset operation was successful.</returns>
    [HttpDelete]
    public async Task<IActionResult> ResetData()
    {
        await dataRepository.ClearData();
        return Ok();
    }
}