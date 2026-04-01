using MyDotNetApp.Application.DTOs.Users;
using MyDotNetApp.Domain.Common;

namespace MyDotNetApp.Application.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken ct = default);
    Task<Result<UserDto>> GetUserByIdAsync(string id, CancellationToken ct = default);
    Task<Result<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(string id, CancellationToken ct = default);
}
