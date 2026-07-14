# Integration Examples

Complete workflow examples showing how to integrate with the Cellario API using different programming languages and tools.

## PowerShell Integration

### Complete Workflow Script

```powershell
# Cellario API Integration Example
# This script demonstrates a complete workflow including authentication and basic operations

param(
    [string]$BaseUrl = "http://localhost:8444",
    [string]$Username = "admin",
    [string]$Password = "password"
)

function Write-Status {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] ERROR: $Message" -ForegroundColor Red
}

try {
    Write-Status "Starting Cellario API integration..."
    
    # Step 1: Test connectivity
    Write-Status "Testing API connectivity..."
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get
    Write-Host "Health Status: $($healthResponse.status)"
    
    # Step 2: Authenticate
    Write-Status "Authenticating..."
    $authBody = @{
        username = $Username
        password = $Password
    }
    
    $authResponse = Invoke-RestMethod -Uri "$BaseUrl/token" `
        -Method POST `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $authBody
    
    $headers = @{ Authorization = "Bearer $($authResponse.access_token)" }
    Write-Status "Authentication successful. Token expires in $($authResponse.expires_in) seconds."
    
    # Step 3: Get protocols
    Write-Status "Fetching protocols..."
    $protocols = Invoke-RestMethod -Uri "$BaseUrl/protocols" -Headers $headers
    Write-Host "Found $($protocols.Count) protocols:"
    
    foreach ($protocol in $protocols | Select-Object -First 5) {
        Write-Host "  - $($protocol.name) (ID: $($protocol.id))"
    }
    
    # Step 4: Get systems (if available)
    Write-Status "Fetching systems..."
    try {
        $systems = Invoke-RestMethod -Uri "$BaseUrl/systems" -Headers $headers
        Write-Host "Found $($systems.Count) systems"
    }
    catch {
        Write-Host "Systems endpoint not available or no systems configured"
    }
    
    # Step 5: Get orders (if available)  
    Write-Status "Fetching recent orders..."
    try {
        $orders = Invoke-RestMethod -Uri "$BaseUrl/orders" -Headers $headers
        Write-Host "Found $($orders.Count) orders"
        
        if ($orders.Count -gt 0) {
            $recentOrder = $orders | Select-Object -First 1
            Write-Host "Most recent order: $($recentOrder.protocolName) (State: $($recentOrder.state))"
        }
    }
    catch {
        Write-Host "Orders endpoint not available or no orders found"
    }
    
    Write-Status "Integration test completed successfully!"
}
catch {
    Write-Error-Custom "Integration failed: $($_.Exception.Message)"
    
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "Authentication failed. Please check username and password."
    }
    elseif ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "API endpoint not found. Check the base URL."
    }
    else {
        Write-Host "Response: $($_.Exception.Response)"
    }
    exit 1
}
```

### PowerShell Class-Based Approach

```powershell
class CellarioClient {
    [string]$BaseUrl
    [string]$AccessToken
    [hashtable]$Headers

    CellarioClient([string]$baseUrl) {
        $this.BaseUrl = $baseUrl
        $this.Headers = @{}
    }

    [bool] Authenticate([string]$username, [string]$password) {
        try {
            $body = @{
                username = $username
                password = $password
            }

            $response = Invoke-RestMethod -Uri "$($this.BaseUrl)/token" `
                -Method POST `
                -ContentType "application/x-www-form-urlencoded" `
                -Body $body

            $this.AccessToken = $response.access_token
            $this.Headers = @{ Authorization = "Bearer $($this.AccessToken)" }
            return $true
        }
        catch {
            Write-Error "Authentication failed: $($_.Exception.Message)"
            return $false
        }
    }

    [object] GetProtocols() {
        return Invoke-RestMethod -Uri "$($this.BaseUrl)/protocols" -Headers $this.Headers
    }

    [object] GetOrders() {
        return Invoke-RestMethod -Uri "$($this.BaseUrl)/orders" -Headers $this.Headers
    }

    [object] GetHealth() {
        return Invoke-RestMethod -Uri "$($this.BaseUrl)/health" -Method Get
    }
}

# Usage
$client = [CellarioClient]::new("http://localhost:8444")
if ($client.Authenticate("admin", "password")) {
    $protocols = $client.GetProtocols()
    Write-Host "Found $($protocols.Count) protocols"
}
```

## Python Integration

### Complete Workflow Class

```python
import requests
import json
from datetime import datetime, timedelta
from typing import Optional, Dict, Any, List

class CellarioAPI:
    """
    Cellario API client for Python integration
    """
    
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.token = None
        self.token_expires = None
        self.session = requests.Session()

    def _log(self, message: str):
        """Simple logging helper"""
        print(f"[{datetime.now().strftime('%H:%M:%S')}] {message}")

    def authenticate(self, username: str, password: str) -> bool:
        """
        Authenticate and store access token
        """
        try:
            self._log("Authenticating...")
            
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
                self.token_expires = datetime.now() + timedelta(seconds=expires_in)
                
                # Set default authorization header
                self.session.headers.update({
                    'Authorization': f'Bearer {self.token}'
                })
                
                self._log(f"Authentication successful. Token expires at {self.token_expires}")
                return True
            else:
                self._log(f"Authentication failed: {response.status_code} - {response.text}")
                return False
                
        except requests.RequestException as e:
            self._log(f"Authentication error: {e}")
            return False

    def is_token_valid(self) -> bool:
        """Check if current token is still valid"""
        if not self.token or not self.token_expires:
            return False
        return datetime.now() < self.token_expires

    def _make_request(self, method: str, endpoint: str, **kwargs) -> requests.Response:
        """
        Make authenticated request with automatic token validation
        """
        if not self.is_token_valid():
            raise Exception("Token is invalid or expired. Please authenticate first.")
            
        url = f"{self.base_url}{endpoint}"
        response = self.session.request(method, url, **kwargs)
        
        if response.status_code == 401:
            raise Exception("Request failed with 401 Unauthorized. Token may be expired.")
            
        return response

    def get_health(self) -> Dict[str, Any]:
        """Get API health status"""
        response = self.session.get(f'{self.base_url}/health')
        response.raise_for_status()
        return response.json()

    def get_protocols(self) -> List[Dict[str, Any]]:
        """Get all protocols"""
        response = self._make_request('GET', '/protocols')
        response.raise_for_status()
        return response.json()

    def get_orders(self) -> List[Dict[str, Any]]:
        """Get all orders"""
        response = self._make_request('GET', '/orders')
        response.raise_for_status()
        return response.json()

    def get_systems(self) -> List[Dict[str, Any]]:
        """Get all systems"""
        response = self._make_request('GET', '/systems')
        response.raise_for_status()
        return response.json()

    def create_order(self, order_data: Dict[str, Any]) -> Dict[str, Any]:
        """Create a new order"""
        response = self._make_request(
            'POST', 
            '/orders',
            json=order_data,
            headers={'Content-Type': 'application/json'}
        )
        response.raise_for_status()
        return response.json()

def main():
    """
    Example usage of the CellarioAPI class
    """
    # Initialize client
    api = CellarioAPI('http://localhost:8444')
    
    try:
        # Test connectivity
        health = api.get_health()
        print(f"API Health: {health['status']}")
        
        # Authenticate
        if not api.authenticate('admin', 'password'):
            print("Authentication failed!")
            return
        
        # Get protocols
        protocols = api.get_protocols()
        print(f"Found {len(protocols)} protocols:")
        for protocol in protocols[:5]:  # Show first 5
            print(f"  - {protocol.get('name', 'Unknown')} (ID: {protocol.get('id', 'N/A')})")
        
        # Get orders
        try:
            orders = api.get_orders()
            print(f"Found {len(orders)} orders")
            if orders:
                latest_order = orders[0]
                print(f"Latest order: {latest_order.get('protocolName', 'Unknown')} "
                      f"(State: {latest_order.get('state', 'Unknown')})")
        except Exception as e:
            print(f"Could not fetch orders: {e}")
        
        # Get systems
        try:
            systems = api.get_systems()
            print(f"Found {len(systems)} systems")
        except Exception as e:
            print(f"Could not fetch systems: {e}")
            
    except Exception as e:
        print(f"Integration test failed: {e}")

if __name__ == "__main__":
    main()
```

### Python Async Example

```python
import asyncio
import aiohttp
from typing import Dict, Any, List

class AsyncCellarioAPI:
    """
    Async version of Cellario API client
    """
    
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.token = None

    async def authenticate(self, session: aiohttp.ClientSession, username: str, password: str) -> bool:
        """Authenticate async"""
        data = {
            'username': username,
            'password': password
        }
        
        async with session.post(
            f'{self.base_url}/token',
            data=data,
            headers={'Content-Type': 'application/x-www-form-urlencoded'}
        ) as response:
            if response.status == 200:
                token_data = await response.json()
                self.token = token_data['access_token']
                return True
            return False

    def _get_headers(self) -> Dict[str, str]:
        """Get authorization headers"""
        if not self.token:
            raise Exception("Not authenticated")
        return {'Authorization': f'Bearer {self.token}'}

    async def get_protocols(self, session: aiohttp.ClientSession) -> List[Dict[str, Any]]:
        """Get protocols async"""
        async with session.get(
            f'{self.base_url}/protocols',
            headers=self._get_headers()
        ) as response:
            response.raise_for_status()
            return await response.json()

async def async_example():
    """Example of async usage"""
    api = AsyncCellarioAPI('http://localhost:8444')
    
    async with aiohttp.ClientSession() as session:
        if await api.authenticate(session, 'admin', 'password'):
            protocols = await api.get_protocols(session)
            print(f"Found {len(protocols)} protocols")

# Run async example
# asyncio.run(async_example())
```

## C# Integration

### Complete Workflow Class

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;

public class CellarioApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string _accessToken;
    private DateTime _tokenExpires;

    public CellarioApiClient(string baseUrl, HttpClient httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();
    }

    private void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        try
        {
            Log("Authenticating...");
            
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
                
                _accessToken = tokenData["access_token"].ToString();
                var expiresIn = Convert.ToInt32(tokenData.GetValueOrDefault("expires_in", 3600));
                _tokenExpires = DateTime.Now.AddSeconds(expiresIn);
                
                // Set default authorization header
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                
                Log($"Authentication successful. Token expires at {_tokenExpires}");
                return true;
            }
            
            Log($"Authentication failed: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Log($"Authentication error: {ex.Message}");
            return false;
        }
    }

    public bool IsTokenValid => 
        !string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpires;

    private async Task<HttpResponseMessage> MakeRequestAsync(HttpMethod method, string endpoint, 
        HttpContent content = null)
    {
        if (!IsTokenValid)
            throw new InvalidOperationException("Token is invalid or expired. Please authenticate first.");

        var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
        if (content != null)
            request.Content = content;

        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Request failed with 401 Unauthorized. Token may be expired.");
            
        return response;
    }

    public async Task<Dictionary<string, object>> GetHealthAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/health");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

    public async Task<List<Dictionary<string, object>>> GetProtocolsAsync()
    {
        var response = await MakeRequestAsync(HttpMethod.Get, "/protocols");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
    }

    public async Task<List<Dictionary<string, object>>> GetOrdersAsync()
    {
        var response = await MakeRequestAsync(HttpMethod.Get, "/orders");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
    }

    public async Task<Dictionary<string, object>> CreateOrderAsync(object orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await MakeRequestAsync(HttpMethod.Post, "/orders", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Example usage program
public class Program
{
    public static async Task Main(string[] args)
    {
        var client = new CellarioApiClient("http://localhost:8444");
        
        try
        {
            // Test connectivity
            var health = await client.GetHealthAsync();
            Console.WriteLine($"API Health: {health["status"]}");
            
            // Authenticate
            if (!await client.AuthenticateAsync("admin", "password"))
            {
                Console.WriteLine("Authentication failed!");
                return;
            }
            
            // Get protocols
            var protocols = await client.GetProtocolsAsync();
            Console.WriteLine($"Found {protocols.Count} protocols:");
            
            foreach (var protocol in protocols.Take(5))
            {
                var name = protocol.GetValueOrDefault("name", "Unknown");
                var id = protocol.GetValueOrDefault("id", "N/A");
                Console.WriteLine($"  - {name} (ID: {id})");
            }
            
            // Get orders
            try
            {
                var orders = await client.GetOrdersAsync();
                Console.WriteLine($"Found {orders.Count} orders");
                
                if (orders.Any())
                {
                    var latestOrder = orders.First();
                    var protocolName = latestOrder.GetValueOrDefault("protocolName", "Unknown");
                    var orderState = latestOrder.GetValueOrDefault("state", "Unknown");
                    Console.WriteLine($"Latest order: {protocolName} (State: {orderState})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch orders: {ex.Message}");
            }
            
            Console.WriteLine("Integration test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Integration test failed: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}
```

## Best Practices

### Error Handling

Always implement comprehensive error handling:

1. **Network errors** (connection timeouts, DNS issues)
2. **Authentication errors** (invalid credentials, expired tokens)
3. **API errors** (4xx/5xx HTTP status codes)
4. **Data validation errors** (malformed requests)

### Performance Considerations

1. **Reuse HTTP clients** to leverage connection pooling
2. **Cache authentication tokens** until they expire
3. **Implement exponential backoff** for retries
4. **Use async/await** patterns for non-blocking operations

### Security

1. **Never log access tokens** in plain text
2. **Use secure credential storage** (environment variables, secure vaults)
3. **Validate SSL certificates** in production
4. **Implement proper timeout values** to prevent hanging requests

## Next Steps

- Review [Error Handling](error-handling.md) patterns
- Explore specific operation examples (protocols, orders, devices)
- Consider using the [Cellario Client SDK](../../client-sdk/README.md) for production applications

## Related Documentation

- [Basic Operations](basic-operations.md) - Authentication and connectivity
- [Error Handling](error-handling.md) - Error scenarios and solutions
- [Client SDK](../../client-sdk/README.md) - Recommended production approach