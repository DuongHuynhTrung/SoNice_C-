using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoNice.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SoNice.Application.Services;

/// <summary>
/// Email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _smtpHost = configuration["Email:SmtpHost"] ?? throw new ArgumentNullException("Email:SmtpHost");
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:SmtpUsername"] ?? throw new ArgumentNullException("Email:SmtpUsername");
        _smtpPassword = configuration["Email:SmtpPassword"] ?? throw new ArgumentNullException("Email:SmtpPassword");
        _fromEmail = configuration["Email:FromEmail"] ?? throw new ArgumentNullException("Email:FromEmail");
        _fromName = configuration["Email:FromName"] ?? "SoNice";
    }

    public async Task SendVerificationEmailAsync(string toEmail, string userName, string verifyLink)
    {
        try
        {
            var subject = "Xác thực tài khoản Sonice";
            var htmlBody = GetVerificationEmailTemplate(userName, verifyLink);

            await SendEmailAsync(toEmail, subject, htmlBody);
            _logger.LogInformation($"Verification email sent to {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send verification email to {toEmail}");
            throw;
        }
    }

    public async Task SendOtpEmailAsync(string toEmail, string userName, string otp)
    {
        try
        {
            var subject = "Mã OTP đặt lại mật khẩu Sonice";
            var htmlBody = GetOtpEmailTemplate(userName, otp);

            await SendEmailAsync(toEmail, subject, htmlBody);
            _logger.LogInformation($"OTP email sent to {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
            throw;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

        using var message = new MailMessage();
        message.From = new MailAddress(_fromEmail, _fromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        await client.SendMailAsync(message);
    }

    private static string GetVerificationEmailTemplate(string userName, string verifyLink)
    {
        return $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html lang=""en"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html charset=UTF-8"" />
</head>
<body style=""background-color:#fff;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Oxygen-Sans,Ubuntu,Cantarell,Helvetica Neue,sans-serif"">
    <div style=""width:50vw; margin: 0 auto"">
        <div style=""width: 100%; height: 200px; margin: 0 auto;"">
            <img src=""https://firebasestorage.googleapis.com/v0/b/website-for-mc.appspot.com/o/ChatGPT%20Image%20Sep%2017%2C%202025%2C%2011_04_48%20PM.png?alt=media&token=e564789f-735b-4e8f-baf5-564dc17319b3""
                style=""width: auto;height:200px;object-fit: cover; margin-left: 38%;"">
        </div>
        <table style=""padding:0 40px"" align=""center"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation""
            width=""100%"">
            <tbody>
                <tr>
                    <td>
                        <hr
                            style=""width:100%;border:none;border-top:1px solid black;border-color:black;margin:20px 0"" />
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;margin-bottom: 25px;"">
                            Xin chào
                            <a style=""font-size:16px;line-height:22px;margin:16px 0;font-weight: bold;"">{userName},</a>
                        </p>
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;text-align: justify"">
                            Chúng tôi thấy bạn đang gửi yêu cầu xác thực địa chỉ email này để tạo tài khoản trên
                            <spans style=""font-weight: bold;"">Sò Nice</spans>.
                        </p>
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;text-align: justify"">
                            Để xác nhận email đăng ký, bạn vui lòng click vào nút ""Xác thực"".
                        </p>
                        <button style=""background-color:#007bff;border:none;padding:10px 20px;
               text-align:center;font-size:16px;cursor:pointer;border-radius:5px"">
                            <a href=""{verifyLink}"" target=""_blank""
                                style=""color:white;text-decoration:none;display:inline-block;width:100%;height:100%"">
                                Xác thực
                            </a>
                        </button>

                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;text-align: justify"">
                            Trong trường hợp nút ""Xác thực"" không hoạt động, bạn hãy copy link dưới và dán vào trình
                            duyệt.
                        </p>
                        <a href=""{verifyLink}"" target=""_blank"">
                            {verifyLink}
                        </a>
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;text-align: justify"">
                            Nếu đây không phải yêu cầu xác thực của bạn, bạn có thể bỏ qua email này. Có thể một ai đó
                            đã gõ nhầm địa chỉ email của bạn.
                        </p>
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043;text-align: justify"">
                            Chúng tôi xin chân thành cảm ơn.
                        </p>
                        <p style=""font-size:14px;line-height:22px;margin:16px 0;color:#3c4043"">Trân trọng,</p>
                        <p
                            style=""font-weight:bold;font-size:16px;line-height:22px;margin:16px 0px 0px 0px;color:#3c4043"">
                            Sò Nice</p>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</body>
</html>";
    }

    private static string GetOtpEmailTemplate(string userName, string otp)
    {
        return $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<meta http-equiv=""Content-Type"" content=""text/html charset=UTF-8"" />
<html lang=""en"">
<body style=""background-color:#ffffff;font-family:HelveticaNeue,Helvetica,Arial,sans-serif"">
    <table align=""center"" role=""presentation"" cellSpacing=""0"" cellPadding=""0"" border=""0"" width=""100%""
        style=""max-width:37.5em;background-color:#ffffff;border:1px solid #eee;border-radius:5px;box-shadow:0 5px 10px rgba(20,50,70,.2);margin-top:20px;width:360px;margin:0 auto;padding:68px 0 68px"">
        <div>
            <tr style=""width:100%"">
                <td>
                    <img alt=""Sò Nice"" src=""https://firebasestorage.googleapis.com/v0/b/website-for-mc.appspot.com/o/ChatGPT%20Image%20Sep%2017%2C%202025%2C%2011_04_48%20PM.png?alt=media&token=e564789f-735b-4e8f-baf5-564dc17319b3""
                        width=""200"" height=""auto""
                        style=""display:block;outline:none;border:none;text-decoration:none;margin:0 auto"" />
                    <p
                        style=""font-size:11px;line-height:16px;margin:16px 8px 8px 8px;color:#0a85ea;font-weight:700;font-family:HelveticaNeue,Helvetica,Arial,sans-serif;height:16px;letter-spacing:0;text-transform:uppercase;text-align:center"">
                        Xác thực Email</p>
                    <h1
                        style=""color:#000;display:inline-block;font-family:HelveticaNeue-Medium,Helvetica,Arial,sans-serif;font-size:20px;font-weight:500;line-height:24px;margin-bottom:0;margin-top:0;text-align:center"">
                        Đây là mã OTP để hoàn thành việc xác thực email của bạn
                    </h1>
                    <table
                        style=""background:rgba(0,0,0,.05);border-radius:4px;margin:16px auto 14px;vertical-align:middle;width:280px""
                        align=""center"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" width=""100%"">
                        <tbody>
                            <tr>
                                <td>
                                    <p
                                        style=""font-size:32px;line-height:40px;margin:0 auto;color:#000;display:inline-block;font-family:HelveticaNeue-Bold;font-weight:700;letter-spacing:6px;padding-bottom:8px;padding-top:8px;width:100%;text-align:center"">
                                        {otp}</p>
                                </td>
                            </tr>
                        </tbody>
                    </table>

                    <p
                        style=""font-size:15px;line-height:23px;margin:0;color:#444;font-family:HelveticaNeue,Helvetica,Arial,sans-serif;letter-spacing:0;padding:0 40px;text-align:center"">
                        Liên hệ <a target=""_blank"" style=""color:#444;text-decoration:underline""
                            href=""mailto:sonicexinchao@gmail.com"">sonicexinchao@gmail.com</a> hoặc thông qua số điện thoại
                        <span style=""text-decoration: #0a85ea;"">0825 911 088</span> nếu bạn không yêu cầu
                        chuyện này!
                    </p>
                </td>
            </tr>
        </div>
    </table>
</body>
</html>";
    }
}
