using System.Net.Http.Headers;

namespace MyDotNetApp.Web.Auth;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;

    public ApiAuthHandler(TokenStorageService tokenStorage)
        => _tokenStorage = tokenStorage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
