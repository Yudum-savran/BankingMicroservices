using Microsoft.AspNetCore.Mvc;
using Notification.API.Services;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationsController> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    /// <summary>
    /// Send a test email
    /// </summary>
    [HttpPost("email")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
        return Ok(new { message = "Email sent successfully" });
    }

    /// <summary>
    /// Send a test SMS
    /// </summary>
    [HttpPost("sms")]
    public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
    {
        await _smsService.SendSmsAsync(request.PhoneNumber, request.Message);
        return Ok(new { message = "SMS sent successfully" });
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "Notification Service" });
    }
}

public record SendEmailRequest(string To, string Subject, string Body);
public record SendSmsRequest(string PhoneNumber, string Message);
