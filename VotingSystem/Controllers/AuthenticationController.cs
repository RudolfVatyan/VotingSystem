using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Web3.Accounts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VotingSystem.Data;
using VotingSystem.Models;

namespace VotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly VotingContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(VotingContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/authentication/register
        [Authorize(Roles = "admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Role))
                return BadRequest("Username, Password, and Role are required.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
                return BadRequest("Username already taken.");
            var account = new Account(Guid.NewGuid().ToString());  // Generates a new private key
            var walletAddress = account.Address;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role,
                WalletAddress = walletAddress,  
                HasVoted = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{user.Username}' with role '{user.Role}' registered successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate user credentials
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }
            if (string.IsNullOrEmpty(user.WalletAddress))
            {
                // Generate a new wallet address if it doesn't exist
                var account = new Nethereum.Web3.Accounts.Account(Guid.NewGuid().ToString());
                user.WalletAddress = account.Address;
                await _context.SaveChangesAsync();
                Console.WriteLine($"New wallet address generated for user {user.Username}: {user.WalletAddress}");
            }

            // Create claims based on the user (in real apps, this would come from a database)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("WalletAddress", user.WalletAddress)
            };

            var secretKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(500, "JWT Secret Key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Token = tokenString,Role = user.Role });
        }

        // GET: api/authentication/protected-endpoint
        [Authorize]
        [HttpGet("protected-endpoint")]
        public IActionResult GetProtectedData()
        {
            return Ok("This is protected data!");
        }

        // GET: api/authentication/admin-data (only accessible by users with 'Admin' role)
        [Authorize(Roles = "admin")]
        [HttpGet("admin-data")]
        public IActionResult GetAdminData()
        {
            return Ok("This is admin data!");
        }

        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Role { get; set; }
        }

        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
    }
}
