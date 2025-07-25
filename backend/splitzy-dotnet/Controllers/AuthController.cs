﻿using Microsoft.AspNetCore.Mvc;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using splitzy_dotnet.Services;
using splitzy_dotnet.Services.Interfaces;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IJWTService _jWTService;

        public AuthController(AppDbContext context, IConfiguration configuration, IJWTService jWTService)
        {
            _context = context;
            _configuration = configuration;
            _jWTService = jWTService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO user)
        {
            var loginUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (loginUser == null)
            {
                return Unauthorized("User Not Found");
            }

            if (loginUser.PasswordHash != user.Password)
            {
                return Unauthorized("Password is Wrong");
            }

            var token = _jWTService.GenerateToken(loginUser.UserId);

            return Ok(new
            {
                Message = "Login Succesfully",
                Id = loginUser.UserId,
                Token = token
            });
        }
    }
}
