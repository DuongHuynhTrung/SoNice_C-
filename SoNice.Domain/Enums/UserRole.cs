namespace SoNice.Domain.Enums;

/// <summary>
/// User role enumeration - matches Node.js UserRoleEnum exactly
/// </summary>
public enum UserRole
{
    Customer = 0,
    Admin = 1
}

/// <summary>
/// User status enumeration - matches Node.js UserStatusEnum exactly
/// </summary>
public enum UserStatus
{
    Active = 0,
    Blocked = 1
}