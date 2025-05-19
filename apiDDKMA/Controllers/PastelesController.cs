using Microsoft.AspNetCore.Mvc;
using apiDDKMA.Models;
using apiDDKMA.Services;

[ApiController]
[Route("api/[controller]")]
public class PastelesController : ControllerBase
{
    private readonly PastelService _pastelService;

    public PastelesController(PastelService pastelService)
    {
        _pastelService = pastelService;
    }

    // Endpoints simplificados para la vista
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PastelDto>>> GetAllSimple()
    {
        var pasteles = await _pastelService.GetAllPastelesSimpleAsync();
        return Ok(pasteles);
    }

    [HttpGet("destacados")]
    public async Task<ActionResult<IEnumerable<PastelDto>>> GetDestacadosSimple()
    {
        var pasteles = await _pastelService.GetPastelesDestacadosSimpleAsync();
        return Ok(pasteles);
    }

    // Endpoints completos para administración
    [HttpGet("admin")]
    public async Task<ActionResult<IEnumerable<Pastel>>> GetAllAdmin()
    {
        var pasteles = await _pastelService.GetAllPastelesAsync();
        return Ok(pasteles);
    }

    [HttpGet("admin/{id}")]
    public async Task<ActionResult<Pastel>> GetByIdAdmin(int id)
    {
        var pastel = await _pastelService.GetPastelByIdAsync(id);
        if (pastel == null) return NotFound();
        return Ok(pastel);
    }

    [HttpPost("admin")]
    public async Task<ActionResult<Pastel>> Create([FromBody] Pastel pastel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var nuevoPastel = await _pastelService.CreatePastelAsync(pastel);
        return CreatedAtAction(nameof(GetByIdAdmin), new { id = nuevoPastel.Id }, nuevoPastel);
    }

    [HttpPut("admin/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Pastel pastel)
    {
        if (id != pastel.Id)
        {
            return BadRequest("ID no coincide");
        }

        var result = await _pastelService.UpdatePastelAsync(pastel);
        if (!result) return NotFound();

        return NoContent();
    }

    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _pastelService.DeletePastelAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }
}