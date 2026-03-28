using System.Net.Http.Json;
using Client.Models.IAM;
using Client.Models.Profile;
using Microsoft.Extensions.Configuration;

namespace Client.Services;

public interface IUserService
{
    Task<PagedResult<IamUser>> GetUsersAsync(int page, int pageSize, string? email = null, string? name = null);
    Task<UserProfile?> GetCurrentProfileAsync();
    Task InviteUserAsync(AddUserRequest request);
    Task SendPasswordResetAsync(string email);
    Task ResendActivationAsync(string email);
    Task UpdateProfileAsync(string userId, string firstName, string? lastName, string? phoneNumber);
    Task ChangePasswordAsync(string currentPassword, string newPassword);
}

public class UserService(HttpClient http, IConfiguration config) : IUserService
{
    private string ProjectKey => config["ApiClient:XBlocksKey"] ?? "";

    public async Task<PagedResult<IamUser>> GetUsersAsync(int page, int pageSize, string? email = null, string? name = null)
    {
        var body = new
        {
            page = page + 1,
            pageSize,
            sort = new { property = "createdDate", isDescending = true },
            filter = new { email = email ?? "", name = name ?? "" },
            projectKey = ProjectKey
        };
        return await http.PostAsJsonAsync("/idp/v1/Iam/GetUsers", body)
                   .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<PagedResult<IamUser>>())
                   .Unwrap() ?? new PagedResult<IamUser>();
    }

    public async Task<UserProfile?> GetCurrentProfileAsync() =>
        await http.GetFromJsonAsync<UserProfile>("/idp/v1/Iam/GetAccount");

    public async Task InviteUserAsync(AddUserRequest request)
    {
        var body = new
        {
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName ?? "",
            phoneNumber = request.PhoneNumber ?? "",
            language = "en",
            userPassType = "Plain",
            password = "",
            mfaEnabled = false,
            allowedLogInType = new[] { "Email" },
            projectKey = ProjectKey
        };
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/Create", body);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendPasswordResetAsync(string email)
    {
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/Recover", new { email, projectKey = ProjectKey });
        response.EnsureSuccessStatusCode();
    }

    public async Task ResendActivationAsync(string email)
    {
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/ResendActivation", new { email, projectKey = ProjectKey });
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateProfileAsync(string userId, string firstName, string? lastName, string? phoneNumber)
    {
        var body = new { userId, firstName, lastName = lastName ?? "", phoneNumber = phoneNumber ?? "", projectKey = ProjectKey };
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/Update", body);
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var body = new { oldPassword = currentPassword, newPassword, projectKey = ProjectKey };
        var response = await http.PostAsJsonAsync("/idp/v1/Iam/ChangePassword", body);
        response.EnsureSuccessStatusCode();
    }
}

