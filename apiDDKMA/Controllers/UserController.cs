using Microsoft.AspNetCore.Mvc;
using apiDDKMA.Models;
using apiDDKMA.Services;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // Endpoint existente
    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> Get(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="registerRequest">The user registration details</param>
    /// <returns>The newly created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the registration request is invalid</response>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Usuario))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Usuario>> Register([FromBody] RegisterRequest registerRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = await _userService.RegisterUserAsync(registerRequest);
            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Nuevo: Login
    [HttpPost("login")]
    public async Task<ActionResult<Usuario>> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await _userService.AuthenticateAsync(loginRequest);
        if (user == null) return Unauthorized("Credenciales incorrectas");

        // Oculta el hash de la contraseña en la respuesta
        user.PasswordHash = null;
        return Ok(user);
    }
}