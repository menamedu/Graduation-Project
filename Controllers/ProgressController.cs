using Microsoft.AspNetCore.Mvc;
using GradProject.Models.Entities;
using System.Security.Claims;
using GradProject.DTOs;
using Microsoft.AspNetCore.Authorization;
using GradProject.Repositories;


namespace GradProject.Controllers
{
    /// <summary>
    /// Controller for managing user progress in labs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance.</param>
        public ProgressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets the progress of the currently authenticated user across all labs.
        /// </summary>
        /// <returns>A list of progress DTOs for the user.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ProgressDto>>> GetProgress()
        {
                    // 1. Grab the raw claim value
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized();

            // 2. Parse it to an int
            if (!int.TryParse(userIdString, out var userid))
                return BadRequest("Invalid user ID in token.");


            var userId = userid;
            var progress = await _unitOfWork.UserLabs.Find(uv => uv.UserId == userId);
            
            var progressDtos = progress.Select(uv => new ProgressDto {
                    LabName = uv.Lab?.Name,
                    Date    = uv.AssignedAt,
                    Score   = uv.Status == LabStatus.Completed ? 100 : 0,
                    Status  = uv.Status == LabStatus.Completed 
                                ? LabStatus.Completed  
                                : LabStatus.NotCompleted
                }).ToList();

            return Ok(progressDtos);
        }
    }
}