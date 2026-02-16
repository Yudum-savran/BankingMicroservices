using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Application.EventHandlers;
using Transaction.Application.Events;

namespace Transaction.Infrastructure.MessageBus;

public class RabbitMQEventBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMQEventBus(
        string hostName,
        string userName,
        string password,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("banking_events", ExchangeType.Topic, durable: true);
    }

    public void SubscribeToAccountEvents()
    {
        SubscribeToEvent("account.moneydeposited", HandleMoneyDepositedEvent);
        SubscribeToEvent("account.moneywithdrawn", HandleMoneyWithdrawnEvent);
        SubscribeToEvent("account.moneytransferred", HandleMoneyTransferredEvent);
    }

    private void SubscribeToEvent(string routingKey, Func<string, Task> handler)
    {
        var queueName = $"transaction_service_{routingKey}";
        
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, "banking_events", routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                await handler(message);
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {RoutingKey}", routingKey);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queueName, false, consumer);
        _logger.LogInformation("Subscribed to event: {RoutingKey}", routingKey);
    }

    private async Task HandleMoneyDepositedEvent(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AccountEventHandler>();
        var @event = JsonSerializer.Deserialize<MoneyDepositedEvent>(message);
        if (@event != null)
            await handler.HandleMoneyDepositedEvent(@event);
    }

    private async Task HandleMoneyWithdrawnEvent(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AccountEventHandler>();
        var @event = JsonSerializer.Deserialize<MoneyWithdrawnEvent>(message);
        if (@event != null)
            await handler.HandleMoneyWithdrawnEvent(@event);
    }

    private async Task HandleMoneyTransferredEvent(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AccountEventHandler>();
        var @event = JsonSerializer.Deserialize<MoneyTransferredEvent>(message);
        if (@event != null)
            await handler.HandleMoneyTransferredEvent(@event);
    }
}
