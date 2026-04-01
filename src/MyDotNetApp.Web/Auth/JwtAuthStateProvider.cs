using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyDotNetApp.Web.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;
    private static readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(TokenStorageService tokenStorage)
        => _tokenStorage = tokenStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return _anonymous;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await _tokenStorage.RemoveTokenAsync();
                return _anonymous;
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task NotifyUserLoginAsync(string token)
    {
        await _tokenStorage.SetTokenAsync(token);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _tokenStorage.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }
}
