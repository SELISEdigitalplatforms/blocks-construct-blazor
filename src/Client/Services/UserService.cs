using System.Net.Http.Json;
using System.Text.Json;
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

public class UserService(HttpClient http, RuntimeClientConfig runtimeConfig) : IUserService
{
    private string ProjectKey => runtimeConfig.XBlocksKey;

    public async Task<PagedResult<IamUser>> GetUsersAsync(int page, int pageSize, string? email = null, string? name = null)
    {
        var body = new
        {
            page,
            pageSize,
            sort = new { property = "createdDate", isDescending = true },
            filter = new { email = email ?? "", name = name ?? "" },
            projectKey = ProjectKey
        };

        var response = await http.PostAsJsonAsync("/idp/v1/Iam/GetUsers", body);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResult<IamUser>>()
            ?? new PagedResult<IamUser>();
    }

    public async Task<UserProfile?> GetCurrentProfileAsync()
    {
        var response = await http.GetAsync("/idp/v1/Iam/GetAccount");
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var root = document.RootElement;
        var payload = root;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyIgnoreCase(root, "data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
            {
                payload = dataElement;
            }
            else if (TryGetPropertyIgnoreCase(root, "result", out var resultElement) && resultElement.ValueKind == JsonValueKind.Object)
            {
                payload = resultElement;
            }
            else if (TryGetPropertyIgnoreCase(root, "account", out var accountElement) && accountElement.ValueKind == JsonValueKind.Object)
            {
                payload = accountElement;
            }
        }

        return MapAccount(payload);
    }

    private static UserProfile? MapAccount(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var firstName = ReadString(payload, "firstName");
        var lastName = ReadString(payload, "lastName");
        var displayName = ReadString(payload, "fullName", "name", "displayName", "userName", "username");

        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(displayName))
        {
            firstName = displayName;
        }

        var roles = new List<string>();
        if (TryGetPropertyIgnoreCase(payload, "roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in rolesElement.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    roles.Add(value);
                }
            }
        }

        if (roles.Count == 0
            && TryGetPropertyIgnoreCase(payload, "memberships", out var membershipsElement)
            && membershipsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var membership in membershipsElement.EnumerateArray())
            {
                if (!TryGetPropertyIgnoreCase(membership, "roles", out var memberRoles)
                    || memberRoles.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var role in memberRoles.EnumerateArray())
                {
                    var value = role.GetString();
                    if (!string.IsNullOrWhiteSpace(value) && !roles.Contains(value, StringComparer.OrdinalIgnoreCase))
                    {
                        roles.Add(value);
                    }
                }
            }
        }

        return new UserProfile
        {
            ItemId = ReadString(payload, "itemId"),
            FirstName = firstName,
            LastName = lastName,
            Email = ReadString(payload, "email", "userName", "username"),
            PhoneNumber = ReadString(payload, "phoneNumber"),
            Roles = roles,
            ProfileImageUrl = ReadString(payload, "profileImageUrl"),
            CreatedDate = ReadDateTime(payload, "createdDate"),
            LastLoggedInTime = ReadNullableDateTime(payload, "lastLoggedInTime"),
            MfaEnabled = ReadBoolean(payload, "mfaEnabled")
        };
    }

    private static string ReadString(JsonElement source, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetPropertyIgnoreCase(source, propertyName, out var property)
                || property.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            var value = property.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ReadString(JsonElement source, string propertyName)
    {
        return ReadString(source, [propertyName]);
    }

    private static DateTime ReadDateTime(JsonElement source, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(source, propertyName, out var property))
        {
            return DateTime.MinValue;
        }

        return property.ValueKind == JsonValueKind.String && property.TryGetDateTime(out var value)
            ? value
            : DateTime.MinValue;
    }

    private static DateTime? ReadNullableDateTime(JsonElement source, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(source, propertyName, out var property)
            || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String && property.TryGetDateTime(out var value)
            ? value
            : null;
    }

    private static bool ReadBoolean(JsonElement source, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(source, propertyName, out var property)
            || property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False)
        {
            return false;
        }

        return property.GetBoolean();
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement source, string propertyName, out JsonElement value)
    {
        if (source.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        if (source.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        foreach (var property in source.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

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

