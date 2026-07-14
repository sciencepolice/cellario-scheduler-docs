# Authentication

## Overview

Cellario Scheduler can optionally be configured to support authentication. If no authentication is configured, clients can submit requests to Cellario Scheduler without providing any credentials. When authentication is enabled, the following steps must be taken to successfully submit requests.

To authenticate clients, Cellario Scheduler uses simple username and password authentication. A successful response will generate an access token to be used when making any other requests to the API. The default token lifetime is one (1) day but is configurable by administrators via the `UserManagement.ApiSessionTimeout` setting (in seconds). A value of `-1` disables expiration.

## Authentication Flow

### 1. Authenticate and Generate Bearer Token

**POST** `/token`

Authenticate client and generate bearer token.

#### Request Headers

```
Content-Type: application/x-www-form-urlencoded
```

#### Request Body Parameters

| Parameter  | Type   | Required | Description                          |
| ---------- | ------ | -------- | ------------------------------------ |
| `username` | String | Yes      | The user name of the service account |
| `password` | String | Yes      | The password of the service account  |

#### Example Request

```bash
curl -X POST http://localhost:8444/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=your_username&password=your_password"
```

```powershell
$body = @{
    username = "your_username"
    password = "your_password"
}

$response = Invoke-RestMethod -Uri "http://localhost:8444/token" `
    -Method POST `
    -ContentType "application/x-www-form-urlencoded" `
    -Body $body
```

#### Response Body

```json
{
  "access_token": "string", // The bearer token to be used in the Authorization header
  "token_type": "bearer", // The token type
  "expires_in": 86400 // The time until the bearer token expires in seconds
}
```

#### Response Codes

- **200** - Authentication successful
- **400** - Authentication not successful

### 2. Using the Access Token

Once you have an access token, include it in the Authorization header for all API requests:

```bash
curl -X GET http://localhost:8444/protocols \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

```powershell
$headers = @{
    Authorization = "Bearer YOUR_ACCESS_TOKEN"
}

$response = Invoke-RestMethod -Uri "http://localhost:8444/protocols" `
    -Method GET `
    -Headers $headers
```

## Security Best Practices

### Token Management

- Store access tokens securely and never expose them in client-side code
- Handle token expiration gracefully by catching 401 responses and re-authenticating

### Error Handling

```bash
# Example with error handling
response=$(curl -s -w "%{http_code}" -X GET http://localhost:8444/protocols \
  -H "Authorization: Bearer $ACCESS_TOKEN")

http_code="${response: -3}"
if [ "$http_code" -eq 401 ]; then
    echo "Token expired, re-authenticating..."
    # Re-authentication logic here
fi
```

### Security Considerations

- Use HTTPS in production environments
- Implement proper session management
- Regularly rotate credentials

## Related Documentation

- [Overview →](overview.md)
- [Events →](events.md)
- [Examples →](examples-overview.md)
