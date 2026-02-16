using System.Text.Json;
using Account.Application.Services;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Account.Infrastructure.Logging;

/// <summary>
/// Elasticsearch Service for logging and searching
/// </summary>
public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(IConfiguration configuration, ILogger<ElasticsearchService> logger)
    {
        _logger = logger;

        var uri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex("banking-logs");

        _client = new ElasticsearchClient(settings);

        _logger.LogInformation("Elasticsearch client initialized. Uri: {Uri}", uri);
    }

    public async Task IndexAsync<T>(string indexName, T document) where T : class
    {
        try
        {
            var response = await _client.IndexAsync(document, indexName);

            if (response.IsValidResponse)
            {
                _logger.LogDebug("Document indexed to Elasticsearch. Index: {Index}, Id: {Id}", 
                    indexName, response.Id);
            }
            else
            {
                _logger.LogError("Error indexing document to Elasticsearch. Index: {Index}, Error: {Error}",
                    indexName, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document to Elasticsearch. Index: {Index}", indexName);
        }
    }

    public async Task<List<T>> SearchAsync<T>(string indexName, string query) where T : class
    {
        try
        {
            var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(query)
                    )
                )
                .Size(100)
            );

            if (response.IsValidResponse)
            {
                _logger.LogDebug("Search executed on Elasticsearch. Index: {Index}, Results: {Count}",
                    indexName, response.Documents.Count);

                return response.Documents.ToList();
            }
            else
            {
                _logger.LogError("Error searching Elasticsearch. Index: {Index}, Error: {Error}",
                    indexName, response.DebugInformation);
                return new List<T>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Elasticsearch. Index: {Index}, Query: {Query}",
                indexName, query);
            return new List<T>();
        }
    }

    /// <summary>
    /// Log account activity to Elasticsearch
    /// </summary>
    public async Task LogAccountActivityAsync(AccountActivityLog log)
    {
        await IndexAsync("account-activities", log);
    }

    /// <summary>
    /// Search account activities
    /// </summary>
    public async Task<List<AccountActivityLog>> SearchAccountActivitiesAsync(
        Guid accountId,
        DateTime? from = null,
        DateTime? to = null)
    {
        var query = $"accountId:{accountId}";
        
        if (from.HasValue)
            query += $" AND timestamp:>={from.Value:yyyy-MM-ddTHH:mm:ss}";
        
        if (to.HasValue)
            query += $" AND timestamp:<={to.Value:yyyy-MM-ddTHH:mm:ss}";

        return await SearchAsync<AccountActivityLog>("account-activities", query);
    }
}

/// <summary>
/// Account activity log model for Elasticsearch
/// </summary>
public class AccountActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
