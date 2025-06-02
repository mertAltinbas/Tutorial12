using Microsoft.AspNetCore.Mvc;
using Tutorial12.DTOs;
using Tutorial12.Exceptions;
using Tutorial12.Services;

namespace Tutorial12.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly IDbService _dbService;

    public TripsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var trips = await _dbService.GetTripAsync(page, pageSize);
            return Ok(trips);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", detail = ex.Message });
        }
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClientFromTrip(int idClient)
    {
        try
        {
            await _dbService.DeleteClientAsync(idClient);
            return Ok(new { message = "Client successfully removed." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occured.", detail = ex.Message });
        }
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AssignClientToTripDTO dto)
    {
        try
        {
            await _dbService.AssignClientToTripAsync(idTrip, dto);
            return Ok(new { message = "Client succesfully assigned a trip" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurd.", detail = ex.Message });
        }
    }
}