using Microsoft.AspNetCore.Mvc;
using apiDDKMA.Models;
using apiDDKMA.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CarritosController : ControllerBase
{
    private readonly CarritoService _carritoService;
    private readonly ILogger<CarritosController> _logger;

    public CarritosController(CarritoService carritoService, ILogger<CarritosController> logger)
    {
        _carritoService = carritoService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        //return 15;
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }

    /// <summary>
    /// Obtiene el carrito del usuario actual
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarritoDto))]
    public async Task<ActionResult<CarritoDto>> GetCarrito()
    {
        try
        {
            var userId = GetCurrentUserId();
            var carrito = await _carritoService.GetUserCartAsync(userId);
            return Ok(carrito);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener carrito");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CarritoItemDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CarritoItemDto>> AgregarAlCarrito([FromBody] AddToCartRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Eliminamos la verificación de autenticación
            var item = await _carritoService.AddToCartAsync(request);

            return CreatedAtAction(nameof(GetCarrito), new { userId = request.UserId }, item);
        }
        catch (Exception ex) when (ex.Message.Contains("no está disponible") || ex.Message.Contains("stock"))
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar al carrito. Request: {@Request}", request);
            return StatusCode(500, new
            {
                Message = "Error interno del servidor",
                Detail = ex.Message // Solo para desarrollo, quitar en producción
            });
        }
    }
    /// <summary>
    /// Actualiza la cantidad de un ítem en el carrito
    /// </summary>
    [HttpPut("items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarritoItemDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CarritoItemDto>> ActualizarCantidad(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            _logger.LogInformation("Updating cart item {ItemId} to quantity {NewQuantity} for user ", itemId, request.NewQuantity);
            if (!ModelState.IsValid || request.NewQuantity <= 0)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var item = await _carritoService.UpdateCartItemAsync(userId, itemId, request);
            _logger.LogInformation("Successfully updated cart item {ItemId}", itemId);
            return Ok(item);
        }
        catch (Exception ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex) when (ex.Message.Contains("stock") || ex.Message.Contains("disponible"))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Error de validación",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item {ItemId} with new quantity {NewQuantity}. Exception details: {ExceptionMessage}",
                itemId, request.NewQuantity, ex.ToString());
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error interno del servidor",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Elimina un ítem del carrito
    /// </summary>
    [HttpDelete("items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarDelCarrito(int itemId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _carritoService.RemoveFromCartAsync(userId, itemId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ítem del carrito");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error interno del servidor",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Vacía el carrito por completo
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VaciarCarrito()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _carritoService.ClearCartAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al vaciar carrito");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}