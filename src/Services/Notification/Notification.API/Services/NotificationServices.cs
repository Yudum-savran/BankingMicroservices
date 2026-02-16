using Microsoft.Extensions.Logging;

namespace Notification.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Mock implementation - gerçek SMTP entegrasyonu için MailKit kullanılabilir
        _logger.LogInformation("Sending email to {To}, Subject: {Subject}", to, subject);
        _logger.LogInformation("Email body: {Body}", body);
        
        // Simulated delay
        await Task.Delay(100);
        
        _logger.LogInformation("Email sent successfully to {To}", to);
    }
}

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Mock implementation
        _logger.LogInformation("Sending SMS to {PhoneNumber}, Message: {Message}", phoneNumber, message);
        
        await Task.Delay(100);
        
        _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
    }
}
