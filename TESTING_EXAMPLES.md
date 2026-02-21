# ?? TESTING ÖRNEKLERI - cURL & Postman

**Tarih:** 2024  
**Framework:** .NET 8 + FluentValidation  

---

## ?? ÖN KO?ULLAR

1. Account Service çal???yor: `http://localhost:5001`
2. Auth Service çal???yor: `http://localhost:5004`
3. JWT token elde etmi? durumda

---

## ?? ADIM 1: JWT TOKEN ELDE ET

### Auth Service'den Token Al

**cURL:**
```bash
curl -X POST http://localhost:5004/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456...",
  "expiresAt": "2024-02-03T15:30:00Z",
  "message": "Login successful"
}
```

**Token'u kaydet:**
```bash
export TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## ? TEST 1: VALID REQUEST - Account Olu?tur

### cURL

```bash
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountType": 1,
    "currency": "TRY",
    "dailyWithdrawLimit": 10000
  }'
```

### Expected Response (201 Created)

```json
{
  "accountId": "123e4567-e89b-12d3-a456-426614174000",
  "accountNumber": "TR26000010000000000001",
  "success": true,
  "message": "Account created successfully"
}
```

### Postman

```
Method: POST
URL: http://localhost:5001/api/accounts
Headers:
  - Content-Type: application/json
  - Authorization: Bearer {TOKEN}

Body (JSON):
{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountType": 1,
  "currency": "TRY",
  "dailyWithdrawLimit": 10000
}
```

---

## ? TEST 2: VALIDATION ERROR - Invalid CustomerId (Empty)

### cURL

```bash
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "00000000-0000-0000-0000-000000000000",
    "accountType": 1,
    "currency": "TRY",
    "dailyWithdrawLimit": 10000
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "CustomerId": ["Customer ID must be a valid GUID"]
  },
  "timestamp": "2024-01-20T10:30:00Z"
}
```

---

## ? TEST 3: VALIDATION ERROR - Invalid Currency

### cURL

```bash
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountType": 1,
    "currency": "INVALID",
    "dailyWithdrawLimit": 10000
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Currency": [
      "Currency must be a 3-letter code (e.g., TRY, USD)",
      "Currency must be uppercase 3-letter code"
    ]
  },
  "timestamp": "2024-01-20T10:31:15Z"
}
```

---

## ? TEST 4: VALIDATION ERROR - Negative DailyWithdrawLimit

### cURL

```bash
curl -X POST http://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountType": 1,
    "currency": "TRY",
    "dailyWithdrawLimit": -500
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "DailyWithdrawLimit": [
      "Daily withdraw limit must be greater than 0"
    ]
  },
  "timestamp": "2024-01-20T10:32:30Z"
}
```

---

## ? TEST 5: DEPOSIT - Valid Request

Önce account ID'yi test 1'den al.

### cURL

```bash
ACCOUNT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X POST http://localhost:5001/api/accounts/$ACCOUNT_ID/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "amount": 1000.50,
    "description": "Initial deposit"
  }'
```

### Expected Response (200 OK)

```json
{
  "accountId": "123e4567-e89b-12d3-a456-426614174000",
  "amount": 1000.50,
  "newBalance": 1000.50,
  "success": true,
  "message": "Deposit successful"
}
```

---

## ? TEST 6: DEPOSIT - Negative Amount

### cURL

```bash
ACCOUNT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X POST http://localhost:5001/api/accounts/$ACCOUNT_ID/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "amount": -500,
    "description": "Test"
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Amount": ["Deposit amount must be greater than 0"]
  },
  "timestamp": "2024-01-20T10:35:22Z"
}
```

---

## ? TEST 7: DEPOSIT - Missing Description

### cURL

```bash
ACCOUNT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X POST http://localhost:5001/api/accounts/$ACCOUNT_ID/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "amount": 500.00
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Description": ["Description is required"]
  },
  "timestamp": "2024-01-20T10:36:45Z"
}
```

---

## ? TEST 8: DEPOSIT - Decimal Precision

### cURL

```bash
ACCOUNT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X POST http://localhost:5001/api/accounts/$ACCOUNT_ID/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "amount": 100.999,
    "description": "Test with 3 decimals"
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "Amount": ["Amount must have at most 2 decimal places"]
  },
  "timestamp": "2024-01-20T10:37:50Z"
}
```

---

## ? TEST 9: TRANSFER - Valid Request

Önce iki account olu?tur (test 1'i 2 kez çal??t?r).

### cURL

```bash
SOURCE_ACCOUNT="123e4567-e89b-12d3-a456-426614174000"
TARGET_ACCOUNT="223e4567-e89b-12d3-a456-426614174001"

curl -X POST http://localhost:5001/api/accounts/$SOURCE_ACCOUNT/transfer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "targetAccountId": "'$TARGET_ACCOUNT'",
    "amount": 500.00,
    "description": "Transfer to friend"
  }'
```

### Expected Response (200 OK)

```json
{
  "success": true,
  "message": "Transfer successful",
  "amount": 500.00,
  "sourceAccountNewBalance": 500.50,
  "timestamp": "2024-01-20T10:40:00Z"
}
```

---

## ? TEST 10: TRANSFER - Same Account

### cURL

```bash
ACCOUNT="123e4567-e89b-12d3-a456-426614174000"

curl -X POST http://localhost:5001/api/accounts/$ACCOUNT/transfer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "targetAccountId": "'$ACCOUNT'",
    "amount": 100.00,
    "description": "Transfer to self"
  }'
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "TargetAccountId": [
      "Source and target accounts must be different"
    ]
  },
  "timestamp": "2024-01-20T10:41:30Z"
}
```

---

## ? TEST 11: GET ACCOUNT - Not Found

### cURL

```bash
curl -X GET http://localhost:5001/api/accounts/00000000-0000-0000-0000-000000000000 \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Response (404 NotFound)

```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "Account with ID '00000000-0000-0000-0000-000000000000' was not found",
  "timestamp": "2024-01-20T10:42:15Z"
}
```

---

## ? TEST 12: GET ACCOUNT BY NUMBER - Invalid Format

### cURL

```bash
curl -X GET http://localhost:5001/api/accounts/number/INVALID \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Response (400 BadRequest)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": {
    "AccountNumber": [
      "Account number must be in valid Turkish format (TR + 26 digits)"
    ]
  },
  "timestamp": "2024-01-20T10:43:22Z"
}
```

---

## ? TEST 13: Unauthorized - No Token

### cURL

```bash
curl -X GET http://localhost:5001/api/accounts/123e4567-e89b-12d3-a456-426614174000
```

### Expected Response (401 Unauthorized)

```json
{
  "code": "UNAUTHORIZED",
  "message": "Authorization header was not provided. Invalid token.",
  "timestamp": "2024-01-20T10:44:05Z"
}
```

---

## ? TEST 14: Unauthorized - Invalid Token

### cURL

```bash
curl -X GET http://localhost:5001/api/accounts/123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer invalid_token_here"
```

### Expected Response (401 Unauthorized)

```json
{
  "code": "UNAUTHORIZED",
  "message": "Invalid token.",
  "timestamp": "2024-01-20T10:45:10Z"
}
```

---

## ?? POSTMAN COLLECTION (JSON)

```json
{
  "info": {
    "name": "Account Service - Validation Tests",
    "version": "1.0"
  },
  "item": [
    {
      "name": "1. Login",
      "request": {
        "method": "POST",
        "url": "http://localhost:5004/api/auth/login",
        "header": [
          {"key": "Content-Type", "value": "application/json"}
        ],
        "body": {
          "raw": "{\"email\":\"test@example.com\",\"password\":\"Test123!\"}"
        }
      }
    },
    {
      "name": "2. Create Account (Valid)",
      "request": {
        "method": "POST",
        "url": "http://localhost:5001/api/accounts",
        "header": [
          {"key": "Content-Type", "value": "application/json"},
          {"key": "Authorization", "value": "Bearer {{token}}"}
        ],
        "body": {
          "raw": "{\"customerId\":\"550e8400-e29b-41d4-a716-446655440000\",\"accountType\":1,\"currency\":\"TRY\",\"dailyWithdrawLimit\":10000}"
        }
      }
    },
    {
      "name": "3. Create Account (Invalid Currency)",
      "request": {
        "method": "POST",
        "url": "http://localhost:5001/api/accounts",
        "header": [
          {"key": "Content-Type", "value": "application/json"},
          {"key": "Authorization", "value": "Bearer {{token}}"}
        ],
        "body": {
          "raw": "{\"customerId\":\"550e8400-e29b-41d4-a716-446655440000\",\"accountType\":1,\"currency\":\"INVALID\",\"dailyWithdrawLimit\":10000}"
        }
      }
    }
  ]
}
```

---

## ?? SUMMARY TABLE

| Test # | Endpoint | Method | Expected Status | Validation |
|--------|----------|--------|-----------------|------------|
| 1 | /api/accounts | POST | 201 | Valid |
| 2 | /api/accounts | POST | 400 | Invalid CustomerId |
| 3 | /api/accounts | POST | 400 | Invalid Currency |
| 4 | /api/accounts | POST | 400 | Negative Limit |
| 5 | /api/accounts/{id}/deposit | POST | 200 | Valid |
| 6 | /api/accounts/{id}/deposit | POST | 400 | Negative Amount |
| 7 | /api/accounts/{id}/deposit | POST | 400 | Missing Description |
| 8 | /api/accounts/{id}/deposit | POST | 400 | Decimal Precision |
| 9 | /api/accounts/{id}/transfer | POST | 200 | Valid |
| 10 | /api/accounts/{id}/transfer | POST | 400 | Same Account |
| 11 | /api/accounts/{id} | GET | 404 | Not Found |
| 12 | /api/accounts/number/{num} | GET | 400 | Invalid Format |
| 13 | /api/accounts/{id} | GET | 401 | No Token |
| 14 | /api/accounts/{id} | GET | 401 | Invalid Token |

---

## ?? TESTING WORKFLOW

```bash
# 1. Ba?la
export BASE_URL="http://localhost:5001"
export AUTH_URL="http://localhost:5004"

# 2. Token al
TOKEN=$(curl -s -X POST $AUTH_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

echo "Token: $TOKEN"

# 3. Account olu?tur (valid)
curl -X POST $BASE_URL/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"customerId":"550e8400-e29b-41d4-a716-446655440000","accountType":1,"currency":"TRY","dailyWithdrawLimit":10000}'

# 4. Invalid test
curl -X POST $BASE_URL/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"customerId":"550e8400-e29b-41d4-a716-446655440000","accountType":1,"currency":"INVALID","dailyWithdrawLimit":10000}'
```

---

## ? KEY POINTS

1. **Validation happens automatically** - No need for manual checks
2. **Error messages are clear** - Users know exactly what's wrong
3. **Internal errors are hidden** - Security maintained
4. **Status codes are correct** - 400 for validation, 404 for not found, etc.
5. **All responses follow same format** - Consistent API

---

**Happy Testing! ??**

