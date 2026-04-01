using Microsoft.AspNetCore.Identity;
using MyDotNetApp.Application.DTOs.Users;
using MyDotNetApp.Application.Interfaces;
using MyDotNetApp.Domain.Common;

namespace MyDotNetApp.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = _userManager.Users.ToList();
        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(MapToDto(user, roles));
        }

        return Result.Success<IEnumerable<UserDto>>(dtos);
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure<UserDto>("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return Result.Success(MapToDto(user, roles));
    }

    public async Task<Result<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure<UserDto>("User not found.");

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.IsActive = dto.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure<UserDto>(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        // Update role if changed
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Result.Success(MapToDto(user, roles));
    }

    public async Task<Result> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure("User not found.");

        // Soft-delete by deactivating
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        IsActive = user.IsActive,
        Roles = roles,
        CreatedAt = user.CreatedAt
    };
}
