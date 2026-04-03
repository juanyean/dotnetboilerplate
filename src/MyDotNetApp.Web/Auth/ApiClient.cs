using System.Net.Http.Headers;

namespace MyDotNetApp.Web.Auth;

/// <summary>
/// Scoped service that creates an authenticated HttpClient by reading the JWT
/// from localStorage at call time — within the active Blazor circuit where
/// IJSRuntime is available.
/// </summary>
public class ApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStorageService _tokenStorage;

    public ApiClient(IHttpClientFactory factory, TokenStorageService tokenStorage)
    {
        _factory = factory;
        _tokenStorage = tokenStorage;
    }

    public async Task<HttpClient> CreateAsync()
    {
        var client = _factory.CreateClient("API");
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
