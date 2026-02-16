using Account.Application.Services;
using Account.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Account.Application.Queries.Handlers;

/// <summary>
/// Query Handler with Redis caching for read optimization
/// CQRS pattern: Queries can be optimized independently
/// </summary>
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;

    public GetAccountByIdQueryHandler(
        IAccountRepository accountRepository,
        ICacheService cacheService,
        ILogger<GetAccountByIdQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<AccountDto?> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"account:{request.AccountId}";

        // Try to get from Redis cache first
        var cachedAccount = await _cacheService.GetAsync<AccountDto>(cacheKey);
        if (cachedAccount != null)
        {
            _logger.LogInformation("Account {AccountId} retrieved from cache", request.AccountId);
            return cachedAccount;
        }

        // If not in cache, get from database
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        
        if (account == null)
            return null;

        var accountDto = new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Balance = account.Balance.Amount,
            Currency = account.Currency,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt,
            LastTransactionDate = account.LastTransactionDate,
            DailyWithdrawLimit = account.DailyWithdrawLimit,
            DailyWithdrawnAmount = account.DailyWithdrawnAmount
        };

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, accountDto, TimeSpan.FromMinutes(5));

        _logger.LogInformation("Account {AccountId} retrieved from database and cached", request.AccountId);

        return accountDto;
    }
}

public class GetAccountsByCustomerIdQueryHandler 
    : IRequestHandler<GetAccountsByCustomerIdQuery, List<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAccountsByCustomerIdQueryHandler> _logger;

    public GetAccountsByCustomerIdQueryHandler(
        IAccountRepository accountRepository,
        ICacheService cacheService,
        ILogger<GetAccountsByCustomerIdQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<AccountDto>> Handle(
        GetAccountsByCustomerIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"customer:{request.CustomerId}:accounts";

        var cachedAccounts = await _cacheService.GetAsync<List<AccountDto>>(cacheKey);
        if (cachedAccounts != null)
        {
            _logger.LogInformation(
                "Accounts for customer {CustomerId} retrieved from cache", 
                request.CustomerId);
            return cachedAccounts;
        }

        var accounts = await _accountRepository.GetByCustomerIdAsync(
            request.CustomerId, 
            cancellationToken);

        var accountDtos = accounts.Select(account => new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Balance = account.Balance.Amount,
            Currency = account.Currency,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt,
            LastTransactionDate = account.LastTransactionDate,
            DailyWithdrawLimit = account.DailyWithdrawLimit,
            DailyWithdrawnAmount = account.DailyWithdrawnAmount
        }).ToList();

        await _cacheService.SetAsync(cacheKey, accountDtos, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Accounts for customer {CustomerId} retrieved from database and cached", 
            request.CustomerId);

        return accountDtos;
    }
}

public class GetAccountByNumberQueryHandler : IRequestHandler<GetAccountByNumberQuery, AccountDto?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetAccountByNumberQueryHandler> _logger;

    public GetAccountByNumberQueryHandler(
        IAccountRepository accountRepository,
        ICacheService cacheService,
        ILogger<GetAccountByNumberQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<AccountDto?> Handle(
        GetAccountByNumberQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"account:number:{request.AccountNumber}";

        var cachedAccount = await _cacheService.GetAsync<AccountDto>(cacheKey);
        if (cachedAccount != null)
        {
            return cachedAccount;
        }

        var account = await _accountRepository.GetByAccountNumberAsync(
            request.AccountNumber, 
            cancellationToken);
        
        if (account == null)
            return null;

        var accountDto = new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Balance = account.Balance.Amount,
            Currency = account.Currency,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt,
            LastTransactionDate = account.LastTransactionDate,
            DailyWithdrawLimit = account.DailyWithdrawLimit,
            DailyWithdrawnAmount = account.DailyWithdrawnAmount
        };

        await _cacheService.SetAsync(cacheKey, accountDto, TimeSpan.FromMinutes(5));

        return accountDto;
    }
}
