# Basic API Operations

This guide covers fundamental Cellario API operations including connectivity testing and authentication.

## Health Check

Test API connectivity before starting any operations.

### cURL Example

```bash
curl -X GET http://localhost:8444/health
```

### PowerShell Example

```powershell
Invoke-RestMethod -Uri "http://localhost:8444/health" -Method Get
```

### Expected Response

```json
{
  "status": "Healthy",
  "components": [
    {
      "name": "Cellario Database",
      "status": "Healthy",
      "description": "",
      "exception": "",
      "duration": "00:00:00.1693596",
      "data": {},
      "tags": ["ready"]
    },
    {
      "name": "Events Database",
      "status": "Healthy",
      "description": "",
      "exception": "",
      "duration": "00:00:00.1857092",
      "data": {},
      "tags": ["ready"]
    }
  ],
  "duration": "00:00:00.1878735",
  "version": "4.5"
}
```

## Authentication

All API operations (except health checks) require authentication. Cellario uses simple username/password authentication to obtain a bearer access token (there is no `grant_type`, `client_id`, or `client_secret` field).

### Get Access Token

#### cURL Example

```bash
curl -X POST http://localhost:8444/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=yourpassword"
```

#### PowerShell Example

```powershell
$body = @{
    username = "admin"
    password = "yourpassword"
}

$response = Invoke-RestMethod -Uri "http://localhost:8444/token" `
    -Method POST `
    -ContentType "application/x-www-form-urlencoded" `
    -Body $body

$token = $response.access_token
Write-Host "Token: $token"
```

#### Python Example

```python
import requests

def get_access_token(base_url, username, password):
    data = {
        'username': username,
        'password': password
    }

    response = requests.post(
        f'{base_url}/token',
        data=data,
        headers={'Content-Type': 'application/x-www-form-urlencoded'}
    )

    if response.status_code == 200:
        return response.json()['access_token']
    else:
        raise Exception(f"Authentication failed: {response.status_code}")

# Usage
token = get_access_token('http://localhost:8444', 'admin', 'yourpassword')
print(f"Access token: {token}")
```

#### C# Example

```csharp
using System.Net.Http;
using System.Text.Json;

public class AuthenticationHelper
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuthenticationHelper(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<string> GetAccessTokenAsync(string username, string password)
    {
        var authData = new Dictionary<string, string>
        {
            {"username", username},
            {"password", password}
        };

        var content = new FormUrlEncodedContent(authData);
        var response = await _httpClient.PostAsync($"{_baseUrl}/token", content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
            return tokenData["access_token"].ToString();
        }

        throw new Exception($"Authentication failed: {response.StatusCode}");
    }
}

// Usage
var auth = new AuthenticationHelper("http://localhost:8444");
string token = await auth.GetAccessTokenAsync("admin", "yourpassword");
Console.WriteLine($"Access token: {token}");
```

### Token Response Format

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "bearer",
  "expires_in": 86400
}
```

## Using Access Tokens

Once you have an access token, include it in the `Authorization` header for all subsequent API requests.

### cURL with Token

```bash
# Store token in variable
TOKEN="your_access_token_here"

# Use token in requests
curl -X GET http://localhost:8444/protocols \
  -H "Authorization: Bearer $TOKEN"
```

### PowerShell with Token

```powershell
# Create headers object
$headers = @{
    Authorization = "Bearer $token"
}

# Use headers in requests
$protocols = Invoke-RestMethod -Uri "http://localhost:8444/protocols" -Headers $headers
```

### Python with Token

```python
headers = {
    'Authorization': f'Bearer {token}'
}

response = requests.get('http://localhost:8444/protocols', headers=headers)
data = response.json()
```

### C# with Token

```csharp
_httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

var response = await _httpClient.GetAsync($"{_baseUrl}/protocols");
string jsonResponse = await response.Content.ReadAsStringAsync();
```

## Token Management Best Practices

### Token Expiration

Access tokens expire after a set time (1 day by default, configurable via the `UserManagement.ApiSessionTimeout` setting; `-1` disables expiration). Store the `expires_in` value and refresh tokens before they expire.

### Secure Storage

- **Never log tokens** in plain text
- **Use secure storage** for tokens in production applications
- **Clear tokens** when authentication fails

### Error Handling

Always handle authentication errors gracefully:

```powershell
try {
    $response = Invoke-RestMethod -Uri "http://localhost:8444/protocols" -Headers $headers
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "Token expired or invalid. Please re-authenticate."
        # Re-authentication logic here
    }
    else {
        Write-Host "Request failed: $($_.Exception.Message)"
    }
}
```

## Next Steps

Once you can authenticate successfully, explore specific API operations:

- [Protocol Examples](protocol-examples.md) - Work with laboratory protocols
- [Order Examples](order-examples.md) - Create and manage orders
- [Driver Examples](driver-examples.md) - Manage laboratory drivers
- [Error Handling](error-handling.md) - Handle common error scenarios

## Related Documentation

- [Authentication](../authentication.md) - Detailed authentication documentation
- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Integration Examples](integration-examples.md) - Complete workflow examples
