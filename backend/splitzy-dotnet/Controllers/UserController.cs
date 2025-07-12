using Microsoft.AspNetCore.Mvc;
using splitzy_dotnet.Models;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAll()
        {
            return Ok(_context.Users);
        }
    }
}
