using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Client.Models.IAM;

public class IamUser
{
    [JsonPropertyName("userId")]
    public string ItemId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = [];

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonIgnore]
    public bool Active => Status.Equals("Active", StringComparison.OrdinalIgnoreCase);

    public bool IsVerified { get; set; }
    public bool MfaEnabled { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("lastLogin")]
    public DateTime? LastLoggedInTime { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class PagedResult<T>
{
    public List<T> Data { get; set; } = [];
    public int TotalCount { get; set; }
}

public class AddUserRequest
{
    [Required] public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "user";
}
