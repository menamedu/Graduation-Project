using GradProject.DTOs;
using GradProject.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace GradProject.Services
{
    public interface IAuthService
    {
        Task<(IdentityResult result, string? token, User user)> Register(RegisterDTO model);
        Task<(bool success, string? token, User? user)> Login(LoginDTO model);
        Task<User?> GetUserProfile(int userId);
        Task<(IdentityResult result, User? user)> UpdateUserProfile(int userId, UpdateProfileDTO model);
        Task<IdentityResult> ChangePassword(int userId, ChangePasswordDto model);
    }
}