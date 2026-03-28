using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Client.Models.Auth;

public class SignInRequest
{
    [Required, EmailAddress]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class SignInResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("enable_mfa")]
    public bool EnableMfa { get; set; }

    [JsonPropertyName("mfa_id")]
    public string MfaId { get; set; } = "";

    [JsonPropertyName("mfa_type")]
    public string MfaType { get; set; } = "";
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}

public class ResetPasswordRequest
{
    public string Code { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}

public class SetPasswordRequest
{
    public string Code { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}

public class MfaVerifyRequest
{
    public string MfaId { get; set; } = "";
    public string MfaType { get; set; } = "";
    public string Code { get; set; } = "";
}
