using Notification.API.Services;
using Notification.API.Events;
using Microsoft.Extensions.Logging;

namespace Notification.API.EventHandlers;

public class AccountEventHandler
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<AccountEventHandler> _logger;

    public AccountEventHandler(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<AccountEventHandler> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task HandleAccountCreatedEvent(AccountCreatedEvent @event)
    {
        _logger.LogInformation("Processing AccountCreatedEvent for Account {AccountId}", @event.AccountId);

        var subject = "Welcome to Banking System!";
        var body = $@"
            <h1>Welcome!</h1>
            <p>Your account has been created successfully.</p>
            <p><strong>Account Number:</strong> {@event.AccountNumber}</p>
            <p><strong>Account Type:</strong> {@event.AccountType}</p>
            <p>Thank you for choosing our banking services.</p>
        ";

        await _emailService.SendEmailAsync("customer@example.com", subject, body);
    }

    public async Task HandleMoneyDepositedEvent(MoneyDepositedEvent @event)
    {
        _logger.LogInformation("Processing MoneyDepositedEvent for Account {AccountId}", @event.AccountId);

        var subject = "Money Deposited - Transaction Confirmation";
        var body = $@"
            <h2>Deposit Confirmation</h2>
            <p><strong>Amount:</strong> {@event.Amount:N2} TRY</p>
            <p><strong>New Balance:</strong> {@event.NewBalance:N2} TRY</p>
            <p><strong>Description:</strong> {@event.Description}</p>
            <p><strong>Date:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
        ";

        await _emailService.SendEmailAsync("customer@example.com", subject, body);
        await _smsService.SendSmsAsync("+90500000000", $"{@event.Amount:N2} TRY deposited. New balance: {@event.NewBalance:N2} TRY");
    }

    public async Task HandleMoneyWithdrawnEvent(MoneyWithdrawnEvent @event)
    {
        _logger.LogInformation("Processing MoneyWithdrawnEvent for Account {AccountId}", @event.AccountId);

        var subject = "Money Withdrawn - Transaction Confirmation";
        var body = $@"
            <h2>Withdrawal Confirmation</h2>
            <p><strong>Amount:</strong> {@event.Amount:N2} TRY</p>
            <p><strong>New Balance:</strong> {@event.NewBalance:N2} TRY</p>
            <p><strong>Description:</strong> {@event.Description}</p>
            <p><strong>Date:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
        ";

        await _emailService.SendEmailAsync("customer@example.com", subject, body);
        await _smsService.SendSmsAsync("+90500000000", $"{@event.Amount:N2} TRY withdrawn. New balance: {@event.NewBalance:N2} TRY");
    }

    public async Task HandleMoneyTransferredEvent(MoneyTransferredEvent @event)
    {
        _logger.LogInformation("Processing MoneyTransferredEvent from {SourceAccountId} to {TargetAccountId}",
            @event.SourceAccountId, @event.TargetAccountId);

        // Notification to sender
        var senderSubject = "Money Transfer Sent - Confirmation";
        var senderBody = $@"
            <h2>Transfer Sent</h2>
            <p><strong>Amount:</strong> {@event.Amount:N2} TRY</p>
            <p><strong>To Account:</strong> {@event.TargetAccountId}</p>
            <p><strong>New Balance:</strong> {@event.NewBalance:N2} TRY</p>
            <p><strong>Description:</strong> {@event.Description}</p>
            <p><strong>Date:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
        ";

        await _emailService.SendEmailAsync("sender@example.com", senderSubject, senderBody);
        await _smsService.SendSmsAsync("+90500000001", $"Transfer sent: {@event.Amount:N2} TRY. New balance: {@event.NewBalance:N2} TRY");

        // Notification to receiver (future implementation)
        _logger.LogInformation("Receiver notification would be sent here");
    }
}
