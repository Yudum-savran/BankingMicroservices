using System.Text;
using System.Text.Json;
using Account.Application.Services;
using Account.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Account.Infrastructure.MessageBus;

/// <summary>
/// RabbitMQ Event Bus implementation
/// Publishes domain events to message broker
/// </summary>
public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private const string ExchangeName = "banking_events";

    public RabbitMQEventBus(IConfiguration configuration, ILogger<RabbitMQEventBus> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to RabbitMQ");
            throw;
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IDomainEvent
    {
        try
        {
            var eventName = @event.GetType().Name;
            var routingKey = $"account.{eventName.ToLower()}";

            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = eventName;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Event published to RabbitMQ. EventType: {EventType}, EventId: {EventId}, RoutingKey: {RoutingKey}",
                eventName,
                @event.EventId,
                routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to RabbitMQ. EventType: {EventType}", 
                @event.GetType().Name);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : IDomainEvent
    {
        var eventName = typeof(T).Name;
        var queueName = $"account_service_{eventName}";
        var routingKey = $"account.{eventName.ToLower()}";

        try
        {
            // Declare queue
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queue to exchange
            _channel.QueueBind(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: routingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (@event != null)
                    {
                        await handler(@event);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        
                        _logger.LogInformation(
                            "Event processed. EventType: {EventType}, MessageId: {MessageId}",
                            eventName,
                            ea.BasicProperties.MessageId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event. EventType: {EventType}", eventName);
                    // Reject and requeue
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "Subscribed to event. EventType: {EventType}, Queue: {Queue}",
                eventName,
                queueName);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to event. EventType: {EventType}", eventName);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("RabbitMQ connection closed");
    }
}
