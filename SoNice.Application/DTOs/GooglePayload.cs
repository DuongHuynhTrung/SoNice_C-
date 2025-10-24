namespace SoNice.Application.DTOs;

/// <summary>
/// Google OAuth payload
/// </summary>
public class GooglePayload
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Sub { get; set; } = string.Empty;
    public string Iss { get; set; } = string.Empty;
    public string Aud { get; set; } = string.Empty;
    public long Exp { get; set; }
    public long Iat { get; set; }
}
