using MyDotNetApp.Application.DTOs.Auth;
using MyDotNetApp.Domain.Common;

namespace MyDotNetApp.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<Result> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
}
