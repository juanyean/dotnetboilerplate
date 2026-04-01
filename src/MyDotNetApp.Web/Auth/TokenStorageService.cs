using Microsoft.JSInterop;

namespace MyDotNetApp.Web.Auth;

public class TokenStorageService
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "authToken";

    public TokenStorageService(IJSRuntime js) => _js = js;

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }
        catch { }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch { }
    }
}
