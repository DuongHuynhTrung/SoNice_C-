using MongoDB.Bson.Serialization.Attributes;
using SoNice.Domain.Enums;

namespace SoNice.Domain.Entities;

/// <summary>
/// User domain entity - matches Node.js User model exactly
/// </summary>
public class User : BaseEntity
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("password")]
    public string Password { get; set; } = string.Empty;

    [BsonElement("full_name")]
    public string? FullName { get; set; }

    [BsonElement("dob")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("gender")]
    public string? Gender { get; set; }

    [BsonElement("avatar_url")]
    public string? AvatarUrl { get; set; }

    [BsonElement("phone_number")]
    public string? PhoneNumber { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("is_verified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("role_name")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public UserRole RoleName { get; set; } = UserRole.Customer;

    [BsonElement("otp")]
    public int? Otp { get; set; }

    [BsonElement("otpExpired")]
    public DateTime? OtpExpired { get; set; }
}
