using GradProject.DTOs;
using GradProject.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradProject.Services; // Added this using statement


namespace GradProject.Controllers
{
    /// <summary>
    /// Controller for user authentication and profile management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service instance.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">The registration details.</param>
        /// <returns>A success message and JWT token if registration is successful, otherwise a bad request with errors.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, token, user) = await _authService.Register(model);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                Message = "User registered successfully!",
                Token = token,
                UserId = user.Id,
                Username = user.UserName
            });
        }

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="model">The login credentials.</param>
        /// <returns>A JWT token if login is successful, otherwise unauthorized.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var (success, token, user) = await _authService.Login(model);

            if (!success)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            return Ok(new
            {
                Token = token,
                UserId = user!.Id,
                Username = user.UserName!
            });
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        /// <returns>The user's profile information, or 404 Not Found if the user is not found.</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID format." });
            }

            var user = await _authService.GetUserProfile(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                user.Id,
                Username = user.UserName!,
                Email = user.Email!
            });
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        /// <param name="model">The updated profile details.</param>
        /// <returns>A success message and updated user details, or a bad request with errors.</returns>
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDTO model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID format." });
            }

            var (result, user) = await _authService.UpdateUserProfile(userId, model);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to update profile" });
            }

            return Ok(new
            {
                message = "Profile updated successfully",
                user.Id,
                Username = user.UserName,
                user.Email
            });
        }

        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="model">The old and new password details.</param>
        /// <returns>A success message, or a bad request with errors.</returns>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID format." });
            }

            var result = await _authService.ChangePassword(userId, model);

            if (!result.Succeeded)
            {
                var errors = string.Join(" - ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            return Ok(new { message = "Password changed successfully" });
        }
    }
}