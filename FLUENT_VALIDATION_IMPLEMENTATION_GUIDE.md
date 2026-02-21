# ?? FluentValidation + Error Handling Implementation Guide

**Tarih:** 2024  
**Versiyon:** 1.0  
**Framework:** .NET 8 + FluentValidation 11.9.2  

---

## ?? Yap?lan De?i?iklikler Özeti

### ? Yeni Dosyalar

1. **Validators (Application Layer)**
   - `Account.Application/Validators/CreateAccountCommandValidator.cs`
   - `Account.Application/Validators/DepositMoneyCommandValidator.cs`
   - `Account.Application/Validators/WithdrawMoneyCommandValidator.cs`
   - `Account.Application/Validators/TransferMoneyCommandValidator.cs`
   - `Account.Application/Validators/AccountQueryValidators.cs`

2. **Exception Classes (Domain Layer)**
   - `Account.Domain/Exceptions/ApplicationExceptions.cs`
     - `ApplicationException` (base)
     - `ValidationException`
     - `DomainException`
     - `NotFoundException`
     - `DuplicateException`
     - `InvalidOperationException`
     - `UnauthorizedException`
     - `ForbiddenException`

3. **Error Handling (API Layer)**
   - `Account.API/Middleware/GlobalExceptionHandlingMiddleware.cs`
   - `ErrorResponse` model (standardized error format)

4. **MediatR Pipeline (Application Layer)**
   - `Account.Application/Behaviors/ValidationBehavior.cs`
   - Otomatik validasyon logic'i MediatR pipeline'?nda

### ? Güncellenmi? Dosyalar

1. **Account.API/Program.cs**
   - FluentValidation registration
   - ValidationBehavior MediatR pipeline'?na eklendi
   - GlobalExceptionHandlingMiddleware middleware'?na eklendi

2. **Account.API/Controllers/AccountsController.cs**
   - Basitle?tirildi (validation/error handling art?k middleware'da)
   - ProducesResponseType attributes eklendi
   - Error response types dokümante edildi

3. **Account.Application/Commands/Handlers/CreateAccountCommandHandler.cs**
   - Domain exceptions kullanacak ?ekilde güncellendi
   - Internal error details expose edilmiyor

4. **Account.API/Account.API.csproj**
   - FluentValidation NuGet paketleri eklendi

---

## ?? MIMARÎ AKI?

```
Client Request
    ?
[API Controller] - Minimal logic, sadece MediatR'a gönder
    ?
[MediatR.Send(Command/Query)]
    ?
[ValidationBehavior Pipeline] ? FluentValidation validators çal???r
    ?? If Valid ? Handler'a git
    ?? If Invalid ? ValidationException throw et
    ?
[Command/Query Handler] - ?? mant??? çal???r
    ?? Domain logic
    ?? Database i?lemleri
    ?? Event publishing
    ?
[Response return] veya [Exception throw]
    ?
[GlobalExceptionHandlingMiddleware] - Tüm exceptions yakala ve handle et
    ?? ValidationException ? 400 BadRequest
    ?? NotFoundException ? 404 NotFound
    ?? DomainException ? 400 BadRequest
    ?? UnauthorizedException ? 401 Unauthorized
    ?? [Unhandled] ? 500 InternalServerError (details expose edilmez)
    ?
[Standardized ErrorResponse] ? Client'a JSON response
```

---

## ?? VALIDATORS (FluentValidation)

### 1. CreateAccountCommandValidator

```csharp
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty).WithMessage("Customer ID must be a valid GUID");

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Account type must be a valid type");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3, 3).WithMessage("Currency must be a 3-letter code")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be uppercase");

        RuleFor(x => x.DailyWithdrawLimit)
            .GreaterThan(0).WithMessage("Daily withdraw limit must be > 0");
    }
}
```

**Kontrol Edilen Alanlar:**
- ? CustomerId: Empty check + Guid validity
- ? AccountType: Enum validation
- ? Currency: Format + length + pattern
- ? DailyWithdrawLimit: Positive number

### 2. DepositMoneyCommandValidator / WithdrawMoneyCommandValidator

```csharp
RuleFor(x => x.Amount)
    .GreaterThan(0).WithMessage("Amount must be > 0")
    .LessThanOrEqualTo(decimal.MaxValue).WithMessage("Amount is too large")
    .ScalePrecision(2, 18).WithMessage("Amount must have at most 2 decimal places");

RuleFor(x => x.Description)
    .NotEmpty().WithMessage("Description is required")
    .MaximumLength(500).WithMessage("Description max 500 chars");
```

### 3. TransferMoneyCommandValidator

```csharp
RuleFor(x => new { x.SourceAccountId, x.TargetAccountId })
    .Must(x => x.SourceAccountId != x.TargetAccountId)
    .WithMessage("Source and target accounts must be different");
```

### 4. Query Validators

```csharp
// GetAccountByIdQuery
RuleFor(x => x.AccountId).NotEmpty().Must(x => x != Guid.Empty);

// GetAccountByNumberQuery
RuleFor(x => x.AccountNumber)
    .NotEmpty()
    .Matches(@"^TR\d{2}\d{5}0\d{16}$")
    .WithMessage("Account number must be in valid Turkish format");
```

---

## ??? EXCEPTION HANDLING

### Exception Hierarchy (Domain Layer'da)

```
ApplicationException (Base)
??? ValidationException      ? 400 BadRequest
??? DomainException          ? 400 BadRequest (business rule violation)
??? NotFoundException        ? 404 NotFound
??? DuplicateException       ? 409 Conflict
??? InvalidOperationException ? 400 BadRequest
??? UnauthorizedException    ? 401 Unauthorized
??? ForbiddenException       ? 403 Forbidden
```

### Middleware (GlobalExceptionHandlingMiddleware)

```csharp
public class GlobalExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Switch statement handles all exception types
            // Returns standardized ErrorResponse
        }
    }
}
```

### ErrorResponse Model

```csharp
public class ErrorResponse
{
    public string Code { get; set; }                    // Machine-readable code
    public string Message { get; set; }                 // Safe message for client
    public Dictionary<string, string[]>? Errors { get; set; }  // Validation errors only
    public DateTime Timestamp { get; set; }             // When error occurred
    public string? TraceId { get; set; }                // For debugging
}
```

**Örnek Responses:**

**Validation Error (400):**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Amount": ["Deposit amount must be greater than 0"],
    "Description": ["Description is required"]
  },
  "timestamp": "2024-01-20T10:30:00Z"
}
```

**Not Found (404):**
```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "Account with ID '00000000-0000-0000-0000-000000000000' was not found",
  "timestamp": "2024-01-20T10:30:00Z"
}
```

**Domain Error (400):**
```json
{
  "code": "INVALID_OPERATION",
  "message": "Insufficient balance",
  "timestamp": "2024-01-20T10:30:00Z"
}
```

**Unhandled Error (500):**
```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred. Please try again later.",
  "timestamp": "2024-01-20T10:30:00Z"
}
```

---

## ?? MEDIATR VALIDATION PIPELINE

### ValidationBehavior (Pipeline Behavior)

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Assembly'de bu request type için validators ara
        if (!_validators.Any())
            return await next();  // Validator yok, ilerle

        // 2. Tüm validators'? paralel çal??t?r
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(...)));

        // 3. Hatalar? topla
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // 4. Hata varsa, ValidationException throw et
        if (failures.Any())
            throw new ValidationException(errors);

        // 5. Validation passed, handler'a git
        return await next();
    }
}
```

### Registration (Program.cs)

```csharp
// FluentValidation validators'? register et
builder.Services.AddValidatorsFromAssemblyContaining(
    typeof(Account.Application.Validators.CreateAccountCommandValidator));

// MediatR'a ValidationBehavior'? pipeline'a ekle
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommandHandler).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

---

## ?? KULLANIM ÖRNEKLERI

### 1. Valid Request (CreateAccount)

**Request:**
```bash
POST /api/accounts
Authorization: Bearer {token}
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountType": 1,
  "currency": "TRY",
  "dailyWithdrawLimit": 10000
}
```

**Response (201 Created):**
```json
{
  "accountId": "123e4567-e89b-12d3-a456-426614174000",
  "accountNumber": "TR26000010000000000001",
  "success": true,
  "message": "Account created successfully"
}
```

---

### 2. Invalid Request (Validation Fails)

**Request (negative amount):**
```bash
POST /api/accounts/123e4567-e89b-12d3-a456-426614174000/deposit
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": -100,
  "description": ""
}
```

**Response (400 BadRequest):**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Amount": ["Deposit amount must be greater than 0"],
    "Description": ["Description is required"]
  },
  "timestamp": "2024-01-20T10:35:22Z"
}
```

---

### 3. Domain Rule Violation

**Request (withdraw more than daily limit):**
```bash
POST /api/accounts/123e4567-e89b-12d3-a456-426614174000/withdraw
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 50000,
  "description": "Large withdrawal"
}
```

**Response (400 BadRequest):**
```json
{
  "code": "INVALID_OPERATION",
  "message": "Daily withdrawal limit exceeded",
  "timestamp": "2024-01-20T10:37:15Z"
}
```

---

### 4. Resource Not Found

**Request (non-existent account):**
```bash
GET /api/accounts/00000000-0000-0000-0000-000000000000
Authorization: Bearer {token}
```

**Response (404 NotFound):**
```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "Account with ID '00000000-0000-0000-0000-000000000000' was not found",
  "timestamp": "2024-01-20T10:39:50Z"
}
```

---

### 5. Unauthorized/Unhandled Error

**Request (no token):**
```bash
GET /api/accounts/123e4567-e89b-12d3-a456-426614174000
```

**Response (401 Unauthorized):**
```json
{
  "code": "UNAUTHORIZED",
  "message": "Unauthorized",
  "timestamp": "2024-01-20T10:41:05Z"
}
```

---

## ?? TEST ETME (Manual)

### 1. Postman Collections

**Invalid Amount:**
```
POST http://localhost:5001/api/accounts/{accountId}/deposit
{
  "amount": -50,
  "description": "Test"
}

Expected: 400 with validation errors
```

**Valid Request:**
```
POST http://localhost:5001/api/accounts
{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountType": 1,
  "currency": "TRY",
  "dailyWithdrawLimit": 10000
}

Expected: 201 Created with account details
```

---

## ?? VALIDATION RULES MATRIS

| Field | Validator | Rules |
|-------|-----------|-------|
| **CustomerId** | Required | Not empty, valid GUID |
| **AccountType** | Enum | 1=Checking, 2=Savings, 3=Business |
| **Currency** | String | Exactly 3 chars, uppercase, letters only |
| **DailyWithdrawLimit** | Decimal | > 0, ? MaxValue |
| **Amount** | Decimal | > 0, 2 decimal places max |
| **Description** | String | Not empty, ? 500 chars |
| **SourceAccountId** | GUID | Not empty, ? TargetAccountId |
| **TargetAccountId** | GUID | Not empty, ? SourceAccountId |
| **AccountNumber** | String | Format: TR + 26 digits |

---

## ?? GÜVENLIK NOTLARI

### ? Ne Expose Edilir?
- Validation error messages (user-friendly)
- NotFoundException messages (sadece resource not found)
- Business rule violations (domain exceptions)

### ? Ne EXPOSE ED?LMEZ?
- Database error details
- Stack traces
- Internal exception messages
- System information

### Örnek
```csharp
catch (DbUpdateException dbEx)
{
    // ? YANLI?: throw new Exception($"Database error: {dbEx.InnerException.Message}");
    
    // ? DO?RU:
    _logger.LogError(dbEx, "Database error");
    throw new DomainException("Failed to create account", "DB_ERROR");
}
```

---

## ??? TROUBLESHOOTING

### Problem: Validation çal??m?yor

**Çözüm:**
```csharp
// 1. Validator'?n assembly'sinin do?ru oldu?unu kontrol et
builder.Services.AddValidatorsFromAssemblyContaining(
    typeof(CreateAccountCommandValidator));  // ? Do?ru validator class

// 2. ValidationBehavior'?n MediatR pipeline'?nda kay?tl? oldu?unu kontrol et
cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
```

### Problem: Middleware exception catch etmiyor

**Çözüm:**
```csharp
// Middleware'? FIRST olarak register et
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();  // ? Early!

// Sonra di?erleri
app.UseCors("AllowAll");
app.UseSerilogRequestLogging();
```

### Problem: Ambiguous ValidationException reference

**Çözüm:**
```csharp
// Alias kullan
using AppValidationException = Account.Domain.Exceptions.ValidationException;

// Veya fully qualified name
throw new Account.Domain.Exceptions.ValidationException(errors);
```

---

## ?? NEXT STEPS

### Phase 2: Correlation ID Ekle
```csharp
// Middleware'da request'e correlation ID ekle
context.Request.Headers.TryGetValue("X-Correlation-ID", 
    out var correlationId);
// All logs'a ekle
```

### Phase 3: Logging Context Enrich
```csharp
using (LogContext.PushProperty("CorrelationId", correlationId))
using (LogContext.PushProperty("UserId", userId))
{
    // All logs automatically include these properties
}
```

### Phase 4: Custom Validators
```csharp
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IAccountRepository _repository;
    
    public CreateAccountCommandValidator(IAccountRepository repository)
    {
        _repository = repository;
        
        // Async rule - DB query
        RuleFor(x => x.CustomerId)
            .MustAsync(async (id, ct) => 
            {
                var exists = await _repository.CustomerExistsAsync(id, ct);
                return exists;
            })
            .WithMessage("Customer does not exist");
    }
}
```

---

## ?? KAYNAKLAR

- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [MediatR Behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors)
- [ASP.NET Exception Handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [Clean Code Exception Handling](https://www.geeksforgeeks.org/best-practices-for-exception-handling-in-java/)

---

## ? VERIFICATION CHECKLIST

- ? Validators all fields
- ? ValidationBehavior runs before handlers
- ? GlobalExceptionHandlingMiddleware catches all exceptions
- ? Internal errors not exposed to clients
- ? ErrorResponse format consistent
- ? HTTP status codes correct
- ? Logging includes error details
- ? Build successful
- ? No compilation errors

**Implementation Date:** 2024  
**Status:** ? COMPLETE  
**Test Status:** ?? MANUAL TESTING REQUIRED

