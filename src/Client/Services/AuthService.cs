using System.Net.Http.Json;
using Blazored.LocalStorage;
using Client.Models.Auth;
using Microsoft.Extensions.Configuration;

namespace Client.Services;

public interface IAuthService
{
    Task<SignInResponse> SignInAsync(string username, string password);
    Task<SignInResponse> VerifyMfaAsync(MfaVerifyRequest request);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task SetPasswordAsync(SetPasswordRequest request);
    Task SignOutAsync();
    Task<string?> GetAccessTokenAsync();
}

public class AuthService(HttpClient http, ILocalStorageService localStorage, IConfiguration config) : IAuthService
{
    private string ProjectKey => config["ProjectKey"] ?? config["ApiClient:XBlocksKey"] ?? "";

    public async Task<SignInResponse> SignInAsync(string username, string password)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = password
        };

        var response = await http.PostAsync("/idp/v1/Authentication/Token", new FormUrlEncodedContent(payload));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SignInResponse>() ?? new SignInResponse();

        if (!result.EnableMfa && !string.IsNullOrWhiteSpace(result.AccessToken))
        {
            await localStorage.SetItemAsStringAsync("accessToken", result.AccessToken);
            await localStorage.SetItemAsStringAsync("refreshToken", result.RefreshToken ?? "");
        }

        return result;
    }

    public async Task<SignInResponse> VerifyMfaAsync(MfaVerifyRequest request)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "mfa_code",
            ["mfa_id"] = request.MfaId,
            ["mfa_type"] = request.MfaType,
            ["otp"] = request.Code
        };

        var response = await http.PostAsync("/idp/v1/Authentication/Token", new FormUrlEncodedContent(payload));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SignInResponse>() ?? new SignInResponse();
        if (!string.IsNullOrWhiteSpace(result.AccessToken))
        {
            await localStorage.SetItemAsStringAsync("accessToken", result.AccessToken);
            await localStorage.SetItemAsStringAsync("refreshToken", result.RefreshToken ?? "");
        }

        return result;
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/Recover", new { email, projectKey = ProjectKey });
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var body = new { code = request.Code, newPassword = request.Password, projectKey = ProjectKey };
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/ResetPassword", body);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetPasswordAsync(SetPasswordRequest request)
    {
        var body = new { code = request.Code, password = request.Password, projectKey = ProjectKey };
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/Activate", body);
        response.EnsureSuccessStatusCode();
    }

    public async Task SignOutAsync()
    {
        try
        {
            var refreshToken = await localStorage.GetItemAsStringAsync("refreshToken");
            await http.PostAsJsonAsync("/idp/v1/Authentication/Logout", new { refreshToken });
        }
        finally
        {
            await localStorage.RemoveItemAsync("accessToken");
            await localStorage.RemoveItemAsync("refreshToken");
        }
    }

    public Task<string?> GetAccessTokenAsync() => localStorage.GetItemAsStringAsync("accessToken").AsTask();
}
