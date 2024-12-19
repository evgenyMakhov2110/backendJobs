using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace JobApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("Попытка входа: Username={Username}, Password={Password}", dto.Username, dto.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Password == dto.Password);

            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден: Username={Username}", dto.Username);
                return Unauthorized("Неверный логин или пароль");
            }

            _logger.LogInformation("Пользователь найден: Username={Username}, Role={Role}", user.Username, user.Role);

            // Генерируем JWT токен
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(keyString))
            {
                _logger.LogError("JWT Key is not configured.");
                return StatusCode(500, "Internal server error");
            }

            _logger.LogInformation($"Jwt:Key = {keyString}");
            _logger.LogInformation($"Jwt:Key Length: {keyString.Length * 8} bits");

            var key = Encoding.ASCII.GetBytes(keyString);

            if (key.Length < 32)
            {
                _logger.LogError("JWT Key length is insufficient: {KeyLength} bits", key.Length * 8);
                return StatusCode(500, "Internal server error");
            }

            // Дополнительное логирование
            _logger.LogInformation($"JWT Key Length in AuthController: {key.Length * 8} bits");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("username", user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
        }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
