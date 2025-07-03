using GradProject.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using GradProject.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GradProject.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance.</param>
        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _unitOfWork.Users.GetAll());
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The user with the specified ID, or a 404 Not Found if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user object to create.</param>
        /// <returns>The newly created user.</returns>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            await _unitOfWork.Users.Add(user);
            await _unitOfWork.Complete();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="user">The updated user object.</param>
        /// <returns>No content, or a 400 Bad Request if the ID does not match.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id) return BadRequest();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.Complete();
            return NoContent();
        }

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content, or a 404 Not Found if the user is not found.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _unitOfWork.Users.GetById(id);
            if (user == null) return NotFound();
            _unitOfWork.Users.Delete(user);
            await _unitOfWork.Complete();
            return NoContent();
        }
    }
}