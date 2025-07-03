using GradProject.DTOs;
using GradProject.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GradProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<(IdentityResult result, string? token, User user)> Register(RegisterDTO model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            string? token = null;
            if (result.Succeeded)
            {
                token = GenerateJwtToken(user);
            }

            return (result, token, user);
        }

        public async Task<(bool success, string? token, User? user)> Login(LoginDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return (false, null, null);
            }

            var token = GenerateJwtToken(user);
            return (true, token, user);
        }

        public async Task<User?> GetUserProfile(int userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<(IdentityResult result, User? user)> UpdateUserProfile(int userId, UpdateProfileDTO model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "User not found" }), null);
            }

            user.Email = model.Email;
            user.UserName = model.Username;

            var result = await _userManager.UpdateAsync(user);
            return (result, user);
        }

        public async Task<IdentityResult> ChangePassword(int userId, ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            return await _userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!); // Added null-forgiving operator
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT key is not configured properly.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!), // Added null-forgiving operator
            };

            var expireHours = _configuration.GetValue<double>("Jwt:ExpireHours", 1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}