using System.Text.Json.Serialization;

namespace Client.Models.Profile;

public class UserProfile
{
    [JsonPropertyName("userId")]
    public string ItemId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoggedInTime { get; set; }
    public bool MfaEnabled { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}
