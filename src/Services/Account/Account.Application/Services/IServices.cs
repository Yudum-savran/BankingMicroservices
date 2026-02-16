using Account.Domain.Common;

namespace Account.Application.Services;

/// <summary>
/// Event Bus interface for RabbitMQ integration
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : IDomainEvent;
}

/// <summary>
/// Cache Service interface for Redis integration
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// Elasticsearch service for logging and search
/// </summary>
public interface IElasticsearchService
{
    Task IndexAsync<T>(string indexName, T document) where T : class;
    Task<List<T>> SearchAsync<T>(string indexName, string query) where T : class;
}
