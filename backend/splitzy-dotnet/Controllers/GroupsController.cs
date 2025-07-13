using Microsoft.AspNetCore.Mvc;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllGroups")]
        public IActionResult GetAllGroups()
        {
            var groupsFromDB = _context.Groups.ToList();

            var groups = groupsFromDB.Select(g => new GroupDTO
            {
                GroupId = g.GroupId,
                Name = g.Name,
                CreatedAt = g.CreatedAt,
            });

            return Ok(groups);
        }
    }
}
