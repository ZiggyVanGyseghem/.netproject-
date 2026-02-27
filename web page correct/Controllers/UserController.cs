using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_page_correct.school;
using web_page_correct.Models;

namespace web_page_correct.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly SchoolContext _schoolContext;
        public UsersController(ILogger<UsersController> logger, SchoolContext schoolContext)
        {
            _logger = logger;
            _schoolContext = schoolContext;
        }

        [HttpPost]
        [Route("Users")]
        public async Task<ActionResult<User>> CreateUser(UserDto userDto)
        {
            var userDbo = userDto.ToDbo();
            await _schoolContext.AddAsync(userDbo);
            await _schoolContext.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetUser),
                new { id = userDbo.Id },
                UserDto.FromDbo(userDbo)
            );
        }
        [HttpGet]
        [Route("Users/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var userDbo = await _schoolContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (userDbo == null)
            {
                return NotFound();
            }
            return UserDto.FromDbo(userDbo);
        }
        [HttpGet]
        [Route("children/count")]
        public async Task<ActionResult<int>> GetNumberOfChildren([FromQuery(Name = "class-id")] int? classid, [FromQuery(Name = "allergen-id")] int? allergenId)
        {
            IQueryable<Child> children = _schoolContext.Children;
            if (classid != null)
            {
                children = children.Where(c => c.ClassId == classid);
            }
            if (allergenId != null)
            {
                children = children.Where(c => c.Allergens.Select(a => a.Id).Contains((int)allergenId));
            }
            return await children.CountAsync();
        }
    }
}