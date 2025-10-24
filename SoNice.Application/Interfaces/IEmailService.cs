namespace SoNice.Application.Interfaces;

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email verification
    /// </summary>
    /// <param name="toEmail">Recipient email</param>
    /// <param name="userName">User name</param>
    /// <param name="verifyLink">Verification link</param>
    /// <returns>Task representing the async operation</returns>
    Task SendVerificationEmailAsync(string toEmail, string userName, string verifyLink);

    /// <summary>
    /// Send OTP email
    /// </summary>
    /// <param name="toEmail">Recipient email</param>
    /// <param name="userName">User name</param>
    /// <param name="otp">OTP code</param>
    /// <returns>Task representing the async operation</returns>
    Task SendOtpEmailAsync(string toEmail, string userName, string otp);
}
