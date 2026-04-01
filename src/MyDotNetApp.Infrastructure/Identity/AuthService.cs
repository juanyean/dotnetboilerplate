using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyDotNetApp.Application.DTOs.Auth;
using MyDotNetApp.Application.Interfaces;
using MyDotNetApp.Domain.Common;

namespace MyDotNetApp.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public AuthService(UserManager<ApplicationUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Result.Failure<LoginResponseDto>("Invalid credentials.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Result.Failure<LoginResponseDto>("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtService.GenerateToken(user, roles);

        return Result.Success(new LoginResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserName = user.UserName!,
            Roles = roles,
            ExpiresAt = expiresAt
        });
    }

    public async Task<Result> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        if (request.Password != request.ConfirmPassword)
            return Result.Failure("Passwords do not match.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result.Failure("Email is already in use.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");
        return Result.Success();
    }
}
