# ?? QUICK REFERENCE CARD

## ? HIZLI BA?LANGMI?

### Validator Nas?l Kullan?l?r?

```csharp
// 1. Create validator class
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Must be positive");
    }
}

// 2. Register in Program.cs
builder.Services.AddValidatorsFromAssembly(...);

// 3. Done! Validation runs automatically via MediatR pipeline
```

---

### Exception Nas?l Throw Edilir?

```csharp
// Validation Error
throw new ValidationException(new Dictionary<string, string[]>
{
    { "Amount", new[] { "Must be positive" } }
});

// Domain Error
throw new DomainException("Insufficient balance");

// Not Found
throw new NotFoundException("Account", accountId);

// Duplicate
throw new DuplicateException("Account");

// Unauthorized
throw new UnauthorizedException();

// Forbidden
throw new ForbiddenException();
```

---

### Error Response Örnekleri

**Validation Error (400):**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Amount": ["Must be positive"]
  },
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

**Not Found (404):**
```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "Account with ID '...' was not found",
  "timestamp": "2024-01-20T10:30:00Z"
}
```

---

## ?? COMMON VALIDATORS

### Amount
```csharp
RuleFor(x => x.Amount)
    .GreaterThan(0).WithMessage("Must be positive")
    .ScalePrecision(2, 18).WithMessage("Max 2 decimals");
```

### GUID
```csharp
RuleFor(x => x.Id)
    .NotEmpty().WithMessage("Required")
    .Must(x => x != Guid.Empty).WithMessage("Invalid GUID");
```

### String Length
```csharp
RuleFor(x => x.Description)
    .NotEmpty().WithMessage("Required")
    .MaximumLength(500).WithMessage("Max 500 chars");
```

### Enum
```csharp
RuleFor(x => x.AccountType)
    .IsInEnum().WithMessage("Invalid enum value");
```

### Pattern
```csharp
RuleFor(x => x.Currency)
    .Matches("^[A-Z]{3}$").WithMessage("3-letter code");
```

---

## ?? FILE LOCATIONS

| What | Where |
|------|-------|
| Validators | `Account.Application/Validators/` |
| Exceptions | `Account.Domain/Exceptions/` |
| Middleware | `Account.API/Middleware/` |
| Pipeline Behavior | `Account.Application/Behaviors/` |

---

## ?? QUICK TEST

```bash
# Valid
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"customerId":"550e8400-e29b-41d4-a716-446655440000","accountType":1,"currency":"TRY","dailyWithdrawLimit":10000}'

# Invalid (should fail validation)
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"customerId":"550e8400-e29b-41d4-a716-446655440000","accountType":1,"currency":"INVALID","dailyWithdrawLimit":-100}'
```

---

## ?? DOCUMENTATION FILES

| File | Purpose |
|------|---------|
| `FLUENT_VALIDATION_IMPLEMENTATION_GUIDE.md` | Full technical guide |
| `FLUENT_VALIDATION_SUMMARY.md` | Summary + checklist |
| `TESTING_EXAMPLES.md` | 14 test scenarios |
| `FILE_STRUCTURE_GUIDE.md` | Architecture overview |
| `IMPLEMENTATION_COMPLETE.md` | Completion summary |

---

## ? CHECKLIST FOR NEW FEATURE

- [ ] Create command/query
- [ ] Create validator
- [ ] Add validation rules
- [ ] Create exception (if needed)
- [ ] Create handler
- [ ] Test via API
- [ ] Update documentation

---

## ?? NAMESPACE IMPORTS

```csharp
// For validators
using FluentValidation;

// For exceptions
using Account.Domain.Exceptions;

// For middleware
using Account.API.Middleware;

// For behaviors
using Account.Application.Behaviors;
```

---

## ?? TROUBLESHOOTING

| Problem | Solution |
|---------|----------|
| Validation not running | Check MediatR registration in Program.cs |
| Exception not caught | Check middleware is registered early |
| Error message not shown | Check ErrorResponse in response |
| Build failing | Run `dotnet restore` |
| Compilation error | Check using statements |

---

## ?? DEPLOYMENT CHECKLIST

- [x] Build successful
- [x] No compilation errors
- [x] All tests passing
- [x] Documentation complete
- [x] Security hardened
- [ ] Deployed to staging
- [ ] Final testing done
- [ ] Production ready

---

## ?? REMEMBER

1. **Validators run automatically** - No manual validation needed
2. **Errors are handled globally** - No try-catch needed
3. **Internal errors are hidden** - Security first
4. **Logs have full details** - Debug in logs, not responses
5. **Keep it simple** - One validator per command/query

---

**Happy Coding! ??**

For more details, see:
- `FLUENT_VALIDATION_IMPLEMENTATION_GUIDE.md` (full reference)
- `TESTING_EXAMPLES.md` (14 test scenarios)
