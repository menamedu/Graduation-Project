using GradProject.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using GradProject.Repositories;
using GradProject.DTOs;


namespace GradProject.Controllers
{
    /// <summary>
    /// Controller for managing lab-related operations.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class LabController : ControllerBase
    {
        private readonly LabManager _labManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabController"/> class.
        /// </summary>
        /// <param name="labManager">The lab manager instance.</param>
        /// <param name="unitOfWork">The unit of work instance.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor instance.</param>
        public LabController(LabManager labManager, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _labManager = labManager;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a new lab for a given user and image.
        /// </summary>
        /// <param name="request">The lab creation request containing username and image name.</param>
        /// <returns>The URL of the created lab, or a bad request if validation fails or an error occurs.</returns>
        [HttpPost("createLab")]
        [Authorize]
        public async Task<IActionResult> CreateLab([FromBody] LabRequest request)
        {
            #region  //verifying
            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { success = false, message = "Username is required." });
            }

            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_-]+$"))
            {
                return BadRequest(new { success = false, message = "Username must contain only alphanumeric characters, underscores, or hyphens." });
            }

            if (string.IsNullOrEmpty(request.ImageName))
            {
                return BadRequest(new { success = false, message = "Image name is required." });
            }

            if (!Regex.IsMatch(request.ImageName, @"^[a-zA-Z0-9][a-zA-Z0-9_.-]+(/[a-zA-Z0-9][a-zA-Z0-9_.-]+)?$"))
            {
                return BadRequest(new { success = false, message = "Image name must be a valid Docker image name (alphanumeric, underscores, dots, hyphens, optional repository)." });
            }
            #endregion

            try
            {
                // Find the user by username
                var user = (await _unitOfWork.Users.Find(u => u.UserName == request.Username)).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new { success = false, message = "User not found." });
                }

                // Find the lab by image name
                var lab = (await _unitOfWork.Labs.Find(l => l.ImageName == request.ImageName)).FirstOrDefault();
                if (lab == null)
                {
                    return BadRequest(new { success = false, message = "Lab not found." });
                }

                // Check if UserLab already exists
                var existingUserLab = (await _unitOfWork.UserLabs
                    .Find(ul => ul.UserId == user.Id && ul.LabId == lab.Id)).FirstOrDefault();

                if (existingUserLab != null)
                {
                    // Lab already exists for this user, just create/return the lab URL
                    string existingLabUrl = await _labManager.CreateLabAsync(request.Username, request.ImageName);
                    return Ok(new { success = true, labUrl = existingLabUrl });
                }

                // Create new lab URL
                string labUrl = await _labManager.CreateLabAsync(request.Username, request.ImageName);

                // Create new UserLab entry
                var userLab = new UserLab
                {
                    UserId = user.Id,
                    LabId = lab.Id
                };

                await _unitOfWork.UserLabs.Add(userLab);
                await _unitOfWork.Complete();

                return Ok(new { success = true, labUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to create lab: {ex.Message}" });
            }
        }

        /// <summary>
        /// Extends the time for a lab for a given user.
        /// </summary>
        /// <param name="request">The lab request containing the username.</param>
        /// <returns>A success message, or a bad request if validation fails or an error occurs.</returns>
        [HttpPost("extendLabTime")]
        [Authorize]
        public async Task<IActionResult> ExtendLabTime([FromBody] LabRequest request)
        {
            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { success = false, message = "Username is required." });
            }

            try
            {
                await _labManager.ExtendLabTimeAsync(request.Username);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to extend lab time: {ex.Message}" });
            }
        }

        /// <summary>
        /// Submits a lab by setting its status to completed.
        /// </summary>
        /// <param name="flag">The flag associated with the lab.</param>
        /// <returns>A success message, or a bad request if the lab or user lab is not found.</returns>
        [HttpPost("SubmitLab")]
        [Authorize]
        public async Task<IActionResult> SubmitLab([FromBody] string flag)
        {
            // Get the lab id
            var lab = (await _unitOfWork.Labs.Find(e => e.Flag == flag)).FirstOrDefault();
            if (lab == null)
            {
                return BadRequest("Lab not found");
            }
            var labId = lab.Id;

            // Get userId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found.");
            }


            // Convert userId to int if necessary, assuming IdOfUser is of type int
            if (!int.TryParse(userId, out int IdOfUser))
            {
                return BadRequest("Invalid user ID format");
            }


            // Search for userLab
            var userLab = (await _unitOfWork.UserLabs.Find(e => e.LabId == labId && e.UserId == IdOfUser)).FirstOrDefault();
            if (userLab == null)
            {
                return BadRequest("User lab not found");
            }
            var foundUserLab = userLab;

            // Set the lab status to completed
            foundUserLab.Status = LabStatus.Completed;
            _unitOfWork.UserLabs.Update(foundUserLab);

            // Save to the database
            await _unitOfWork.Complete();

            // Return success
            return Ok("Lab submitted successfully");
        }

        /// <summary>
        /// Terminates a lab given its URL.
        /// </summary>
        /// <param name="labUrl">The URL of the lab to terminate.</param>
        /// <returns>A success message, or a bad request if the URL is invalid or an error occurs.</returns>
        [HttpDelete("terminateLab")]
        [Authorize]
        public async Task<IActionResult> TerminateLab([FromQuery] string labUrl)
        {
            if (string.IsNullOrEmpty(labUrl))
            {
                return BadRequest(new { success = false, message = "Lab URL is required." });
            }

            try
            {
                await _labManager.TerminateLabAsync(labUrl);
                return Ok(new { success = true, message = "Lab terminated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to terminate lab: {ex.Message}" });
            }
        }

    }
}