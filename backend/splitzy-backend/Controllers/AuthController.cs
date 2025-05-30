using MediatR;
using Microsoft.AspNetCore.Mvc;
using splitzy_backend.Command;
using splitzy_backend.DTOs;

namespace splitzy_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO dto)
        {
            var success = await _mediator.Send(new LoginUserCommand(dto));
            if (!success)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { message = "Login successful" });
        }
    }
}
