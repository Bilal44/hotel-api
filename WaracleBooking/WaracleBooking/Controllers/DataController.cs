using System.Net.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WaracleBooking.Filters;
using WaracleBooking.Persistence.Repositories.Interfaces;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace WaracleBooking.Controllers;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/data")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public class DataController : ControllerBase
{
    private readonly IDataRepository _dataRepository;

    public DataController(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    /// <summary>
    /// Seeds the database with initial data for hotels, rooms and bookings.
    /// </summary>
    /// <returns>A <see cref="NoContent"/> indicating the seeding operation was successful.</returns>
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> SeedData()
    {
        await _dataRepository.SeedData();
        return NoContent();
    }
    
    /// <summary>
    /// Clears all hotel, room and booking data from the database.
    /// </summary>
    /// <returns>An <see cref="OkResult"/> indicating the reset operation was successful.</returns>
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status500InternalServerError)]
    [HttpDelete]
    public async Task<IActionResult> ResetData()
    {
        await _dataRepository.ClearData();
        return Ok();
    }
}