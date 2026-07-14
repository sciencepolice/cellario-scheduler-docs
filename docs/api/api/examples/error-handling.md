# Error Handling Examples

This guide covers common error scenarios when using the Cellario API and provides practical solutions for handling them gracefully.

## Authentication Errors

### Invalid Credentials (401 Unauthorized)

#### cURL Example

```bash
# Capture HTTP status code along with response
response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST http://localhost:8444/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=wrongpassword")

# Extract status code
http_code=$(echo $response | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
body=$(echo $response | sed -e 's/HTTPSTATUS\:.*//g')

if [ "$http_code" -ne 200 ]; then
    echo "Authentication failed with status: $http_code"
    echo "Response: $body"
    exit 1
fi

echo "Authentication successful"
```

#### PowerShell Example

```powershell
function Get-AccessToken {
    param(
        [string]$BaseUrl,
        [string]$Username,
        [string]$Password
    )

    try {
        $body = @{
            username = $Username
            password = $Password
        }

        $response = Invoke-RestMethod -Uri "$BaseUrl/token" `
            -Method POST `
            -ContentType "application/x-www-form-urlencoded" `
            -Body $body

        return @{
            Success = $true
            Token = $response.access_token
            ExpiresIn = $response.expires_in
        }
    }
    catch {
        $statusCode = $null
        $errorMessage = $_.Exception.Message

        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        return @{
            Success = $false
            StatusCode = $statusCode
            ErrorMessage = $errorMessage
        }
    }
}

# Usage
$authResult = Get-AccessToken -BaseUrl "http://localhost:8444" -Username "admin" -Password "wrongpassword"

if (-not $authResult.Success) {
    switch ($authResult.StatusCode) {
        401 {
            Write-Host "Authentication failed: Invalid username or password" -ForegroundColor Red
        }
        404 {
            Write-Host "Authentication endpoint not found. Check the base URL." -ForegroundColor Red
        }
        500 {
            Write-Host "Server error during authentication. Please try again later." -ForegroundColor Red
        }
        $null {
            Write-Host "Network error: $($authResult.ErrorMessage)" -ForegroundColor Red
        }
        default {
            Write-Host "Unexpected error: $($authResult.StatusCode) - $($authResult.ErrorMessage)" -ForegroundColor Red
        }
    }
    exit 1
}

Write-Host "Authentication successful!" -ForegroundColor Green
```

#### Python Example

```python
import requests
from requests.exceptions import RequestException, ConnectionError, Timeout

class AuthenticationError(Exception):
    """Custom exception for authentication errors"""
    def __init__(self, message, status_code=None):
        super().__init__(message)
        self.status_code = status_code

def authenticate_with_error_handling(base_url, username, password):
    """
    Authenticate with comprehensive error handling
    """
    try:
        data = {
            'username': username,
            'password': password
        }

        response = requests.post(
            f'{base_url}/token',
            data=data,
            headers={'Content-Type': 'application/x-www-form-urlencoded'},
            timeout=30  # 30 second timeout
        )

        if response.status_code == 200:
            return response.json()['access_token']
        elif response.status_code == 401:
            raise AuthenticationError("Invalid username or password", 401)
        elif response.status_code == 404:
            raise AuthenticationError("Authentication endpoint not found. Check the base URL.", 404)
        elif response.status_code >= 500:
            raise AuthenticationError(f"Server error: {response.status_code}", response.status_code)
        else:
            raise AuthenticationError(f"Unexpected error: {response.status_code} - {response.text}", response.status_code)

    except ConnectionError:
        raise AuthenticationError("Could not connect to the API server. Check the URL and network connection.")
    except Timeout:
        raise AuthenticationError("Authentication request timed out. The server may be overloaded.")
    except RequestException as e:
        raise AuthenticationError(f"Network error during authentication: {e}")

# Usage
try:
    token = authenticate_with_error_handling('http://localhost:8444', 'admin', 'wrongpassword')
    print("Authentication successful!")
except AuthenticationError as e:
    print(f"Authentication failed: {e}")
    if hasattr(e, 'status_code') and e.status_code:
        print(f"Status code: {e.status_code}")
```

#### C# Example

```csharp
using System.Net;
using System.Net.Http;
using System.Text.Json;

public class AuthenticationException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public AuthenticationException(string message, HttpStatusCode? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class AuthenticationHelper
{
    private readonly HttpClient _httpClient;

    public AuthenticationHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> AuthenticateAsync(string baseUrl, string username, string password)
    {
        try
        {
            var authData = new Dictionary<string, string>
            {
                {"username", username},
                {"password", password}
            };

            var content = new FormUrlEncodedContent(authData);

            using var response = await _httpClient.PostAsync($"{baseUrl}/token", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
                return tokenData["access_token"].ToString();
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Invalid username or password", response.StatusCode);
                case HttpStatusCode.NotFound:
                    throw new AuthenticationException("Authentication endpoint not found. Check the base URL.", response.StatusCode);
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                    throw new AuthenticationException($"Server error: {response.StatusCode}", response.StatusCode);
                default:
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationException($"Unexpected error: {response.StatusCode} - {errorContent}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new AuthenticationException($"Network error during authentication: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new AuthenticationException("Authentication request timed out. The server may be overloaded.");
        }
        catch (AuthenticationException)
        {
            throw; // Re-throw authentication exceptions
        }
        catch (Exception ex)
        {
            throw new AuthenticationException($"Unexpected error during authentication: {ex.Message}");
        }
    }
}

// Usage
var auth = new AuthenticationHelper(new HttpClient());
try
{
    string token = await auth.AuthenticateAsync("http://localhost:8444", "admin", "wrongpassword");
    Console.WriteLine("Authentication successful!");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
    if (ex.StatusCode.HasValue)
    {
        Console.WriteLine($"Status code: {ex.StatusCode}");
    }
}
```

## Token Expiration Handling

### Automatic Token Refresh

#### PowerShell Example

```powershell
class CellarioClient {
    [string]$BaseUrl
    [string]$Username
    [string]$Password
    [string]$AccessToken
    [datetime]$TokenExpires
    [hashtable]$Headers

    CellarioClient([string]$baseUrl, [string]$username, [string]$password) {
        $this.BaseUrl = $baseUrl
        $this.Username = $username
        $this.Password = $password
        $this.Headers = @{}
    }

    [bool] IsTokenValid() {
        if (-not $this.AccessToken -or -not $this.TokenExpires) {
            return $false
        }
        # Add 60 second buffer to avoid edge cases
        return (Get-Date).AddSeconds(60) -lt $this.TokenExpires
    }

    [void] EnsureAuthenticated() {
        if (-not $this.IsTokenValid()) {
            $this.Authenticate()
        }
    }

    [void] Authenticate() {
        try {
            $body = @{
                username = $this.Username
                password = $this.Password
            }

            $response = Invoke-RestMethod -Uri "$($this.BaseUrl)/token" `
                -Method POST `
                -ContentType "application/x-www-form-urlencoded" `
                -Body $body

            $this.AccessToken = $response.access_token
            $this.TokenExpires = (Get-Date).AddSeconds($response.expires_in)
            $this.Headers = @{ Authorization = "Bearer $($this.AccessToken)" }

            Write-Host "Token refreshed. Expires at: $($this.TokenExpires)"
        }
        catch {
            throw "Authentication failed: $($_.Exception.Message)"
        }
    }

    [object] InvokeApiRequest([string]$method, [string]$endpoint, [object]$body = $null) {
        $this.EnsureAuthenticated()

        $params = @{
            Uri = "$($this.BaseUrl)$endpoint"
            Method = $method
            Headers = $this.Headers
        }

        if ($body) {
            $params.Body = ($body | ConvertTo-Json)
            $params.ContentType = "application/json"
        }

        try {
            return Invoke-RestMethod @params
        }
        catch {
            if ($_.Exception.Response.StatusCode -eq 401) {
                Write-Host "Token expired during request, retrying..."
                $this.Authenticate()
                $params.Headers = $this.Headers
                return Invoke-RestMethod @params
            }
            throw
        }
    }

    [object] GetProtocols() {
        return $this.InvokeApiRequest("GET", "/protocols")
    }
}

# Usage
$client = [CellarioClient]::new("http://localhost:8444", "admin", "password")
$protocols = $client.GetProtocols()
Write-Host "Found $($protocols.Count) protocols"
```

#### Python Example

```python
import requests
from datetime import datetime, timedelta
from functools import wraps

class TokenExpiredError(Exception):
    """Raised when an API token has expired"""
    pass

def auto_reauthenticate(func):
    """Decorator to automatically refresh tokens on 401 errors"""
    @wraps(func)
    def wrapper(self, *args, **kwargs):
        # Ensure we have a valid token before making the request
        if not self.is_token_valid():
            self.authenticate(self.username, self.password)

        try:
            return func(self, *args, **kwargs)
        except requests.HTTPError as e:
            if e.response.status_code == 401:
                # Token expired during request, refresh and retry
                print("Token expired during request, refreshing...")
                self.authenticate(self.username, self.password)
                return func(self, *args, **kwargs)
            raise
    return wrapper

class CellarioAPI:
    def __init__(self, base_url):
        self.base_url = base_url.rstrip('/')
        self.username = None
        self.password = None
        self.token = None
        self.token_expires = None
        self.session = requests.Session()

    def authenticate(self, username, password):
        """Authenticate and store credentials for auto-refresh"""
        self.username = username
        self.password = password

        data = {
            'username': username,
            'password': password
        }

        response = self.session.post(
            f'{self.base_url}/token',
            data=data,
            headers={'Content-Type': 'application/x-www-form-urlencoded'}
        )

        if response.status_code == 200:
            token_data = response.json()
            self.token = token_data['access_token']
            expires_in = token_data.get('expires_in', 3600)
            # Subtract 60 seconds buffer to avoid edge cases
            self.token_expires = datetime.now() + timedelta(seconds=expires_in - 60)

            self.session.headers.update({
                'Authorization': f'Bearer {self.token}'
            })

            print(f"Token obtained. Expires at: {self.token_expires}")
            return True
        else:
            raise Exception(f"Authentication failed: {response.status_code}")

    def is_token_valid(self):
        """Check if current token is still valid"""
        if not self.token or not self.token_expires:
            return False
        return datetime.now() < self.token_expires

    @auto_reauthenticate
    def _make_authenticated_request(self, method, endpoint, **kwargs):
        """Make an authenticated request with automatic token refresh"""
        response = self.session.request(method, f"{self.base_url}{endpoint}", **kwargs)
        response.raise_for_status()
        return response

    def get_protocols(self):
        """Get protocols with automatic token management"""
        response = self._make_authenticated_request('GET', '/protocols')
        return response.json()

    def get_orders(self):
        """Get orders with automatic token management"""
        response = self._make_authenticated_request('GET', '/orders')
        return response.json()

# Usage
api = CellarioAPI('http://localhost:8444')
api.authenticate('admin', 'password')

# These calls will automatically refresh tokens if needed
protocols = api.get_protocols()
print(f"Found {len(protocols)} protocols")

orders = api.get_orders()  # Will reuse token or refresh if expired
print(f"Found {len(orders)} orders")
```

## Network and Connectivity Errors

### Connection Timeout Handling

#### PowerShell with Retry Logic

```powershell
function Invoke-ApiRequestWithRetry {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [object]$Body = $null,
        [int]$MaxRetries = 3,
        [int]$RetryDelaySeconds = 5
    )

    $attempt = 0
    $lastError = $null

    while ($attempt -lt $MaxRetries) {
        $attempt++

        try {
            $params = @{
                Uri = $Uri
                Method = $Method
                Headers = $Headers
                TimeoutSec = 30
            }

            if ($Body) {
                $params.Body = ($Body | ConvertTo-Json)
                $params.ContentType = "application/json"
            }

            Write-Host "Attempt $attempt of $MaxRetries..."
            $response = Invoke-RestMethod @params
            Write-Host "Request successful!" -ForegroundColor Green
            return $response
        }
        catch [System.Net.WebException] {
            $lastError = $_.Exception

            if ($_.Exception.Status -eq [System.Net.WebExceptionStatus]::Timeout) {
                Write-Warning "Request timed out (attempt $attempt of $MaxRetries)"
            }
            elseif ($_.Exception.Status -eq [System.Net.WebExceptionStatus]::ConnectFailure) {
                Write-Warning "Connection failed (attempt $attempt of $MaxRetries)"
            }
            else {
                Write-Warning "Network error: $($_.Exception.Status) (attempt $attempt of $MaxRetries)"
            }

            if ($attempt -lt $MaxRetries) {
                Write-Host "Waiting $RetryDelaySeconds seconds before retry..."
                Start-Sleep -Seconds $RetryDelaySeconds
            }
        }
        catch {
            # Non-network error, don't retry
            Write-Error "Non-network error: $($_.Exception.Message)"
            throw
        }
    }

    throw "Request failed after $MaxRetries attempts. Last error: $($lastError.Message)"
}

# Usage
try {
    $protocols = Invoke-ApiRequestWithRetry -Uri "http://localhost:8444/protocols" -Headers $headers
    Write-Host "Found $($protocols.Count) protocols"
}
catch {
    Write-Error "Failed to get protocols: $($_.Exception.Message)"
}
```

## API Response Validation

### Validate Response Structure

#### Python Example

```python
import requests
from typing import Dict, Any, List, Optional

class ApiResponseError(Exception):
    """Raised when API response is invalid or unexpected"""
    pass

class CellarioAPIValidator:
    """API client with response validation"""

    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.session = requests.Session()

    def _validate_protocol(self, protocol: Dict[str, Any]) -> bool:
        """Validate protocol object structure"""
        required_fields = ['id', 'name']
        return all(field in protocol for field in required_fields)

    def _validate_order(self, order: Dict[str, Any]) -> bool:
        """Validate order object structure"""
        required_fields = ['id', 'state']
        return all(field in order for field in required_fields)

    def get_protocols_with_validation(self) -> List[Dict[str, Any]]:
        """Get protocols with response validation"""
        try:
            response = self.session.get(f'{self.base_url}/protocols')
            response.raise_for_status()

            data = response.json()

            # Validate response is a list
            if not isinstance(data, list):
                raise ApiResponseError(f"Expected list of protocols, got {type(data).__name__}")

            # Validate each protocol object
            invalid_protocols = []
            for i, protocol in enumerate(data):
                if not self._validate_protocol(protocol):
                    invalid_protocols.append(i)

            if invalid_protocols:
                raise ApiResponseError(f"Invalid protocol objects at indices: {invalid_protocols}")

            return data

        except requests.RequestException as e:
            raise ApiResponseError(f"Failed to fetch protocols: {e}")
        except (ValueError, KeyError) as e:
            raise ApiResponseError(f"Invalid JSON response: {e}")

    def get_orders_with_validation(self) -> List[Dict[str, Any]]:
        """Get orders with response validation"""
        try:
            response = self.session.get(f'{self.base_url}/orders')
            response.raise_for_status()

            data = response.json()

            if not isinstance(data, list):
                raise ApiResponseError(f"Expected list of orders, got {type(data).__name__}")

            # Validate each order object
            for i, order in enumerate(data):
                if not self._validate_order(order):
                    missing_fields = [field for field in ['id', 'state']
                                    if field not in order]
                    raise ApiResponseError(f"Order at index {i} missing fields: {missing_fields}")

            return data

        except requests.RequestException as e:
            raise ApiResponseError(f"Failed to fetch orders: {e}")

# Usage with validation
validator = CellarioAPIValidator('http://localhost:8444')

try:
    protocols = validator.get_protocols_with_validation()
    print(f"Successfully validated {len(protocols)} protocols")

    orders = validator.get_orders_with_validation()
    print(f"Successfully validated {len(orders)} orders")

except ApiResponseError as e:
    print(f"API Response Error: {e}")
except Exception as e:
    print(f"Unexpected Error: {e}")
```

## Best Practices Summary

### 1. Implement Comprehensive Error Handling

```python
# Good: Handle specific error types
try:
    response = api.get_protocols()
except AuthenticationError:
    # Re-authenticate and retry
    pass
except ConnectionError:
    # Check network connectivity
    pass
except TimeoutError:
    # Implement retry logic
    pass
except ApiResponseError:
    # Validate response format
    pass

# Bad: Catch-all error handling
try:
    response = api.get_protocols()
except Exception:
    print("Something went wrong")  # Not helpful
```

### 2. Use Exponential Backoff for Retries

```python
import time
import random

def retry_with_exponential_backoff(func, max_retries=3, base_delay=1):
    for attempt in range(max_retries):
        try:
            return func()
        except retriable_error:
            if attempt == max_retries - 1:
                raise

            # Exponential backoff with jitter
            delay = base_delay * (2 ** attempt) + random.uniform(0, 1)
            time.sleep(delay)
```

### 3. Validate Inputs and Outputs

```python
# Validate inputs before sending requests
def create_order(protocol_id, name):
    if not protocol_id or not isinstance(protocol_id, str):
        raise ValueError("protocol_id must be a non-empty string")

    if not name or len(name.strip()) == 0:
        raise ValueError("name must be a non-empty string")

    # Make API call...

# Validate outputs after receiving responses
def validate_api_response(response_data, expected_type):
    if not isinstance(response_data, expected_type):
        raise ApiResponseError(f"Expected {expected_type}, got {type(response_data)}")
```

### 4. Log Errors Appropriately

```python
import logging

logger = logging.getLogger(__name__)

try:
    response = api.get_protocols()
except AuthenticationError as e:
    logger.warning(f"Authentication failed: {e}")
    # Don't log the actual credentials/tokens
except Exception as e:
    logger.error(f"Unexpected error in get_protocols: {e}", exc_info=True)
    raise
```

## Related Documentation

- [Basic Operations](basic-operations.md) - Authentication and connectivity
- [Integration Examples](integration-examples.md) - Complete workflow examples
- [API Authentication](../authentication.md) - Detailed authentication methods
