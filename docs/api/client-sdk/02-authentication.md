# Authentication & Configuration

## Authentication Methods

The Cellario Client SDK supports multiple authentication methods to connect to your Cellario server.

### Bearer Token Authentication (Recommended)

Bearer token authentication uses JWT tokens and is the preferred method for production applications.

```csharp
// Async authentication
await client.AuthenticateBearer("username", "password");

// Synchronous authentication
client.AuthenticateBearerSync("username", "password");
```

### Basic Authentication

Basic authentication sends credentials with each request. Use for simple scenarios or testing.

```csharp
client.AuthenticateBasic("username", "password");
```

### Clear Authentication

Remove authentication credentials from the client:

```csharp
client.ClearAuthentication();
```

## Authentication Management

### Token Expiry Handling

```csharp
// Re-authenticate on token expiry
try
{
    var result = await client.Protocols.GetProtocolsAsync();
}
catch (ApiException ex) when (ex.StatusCode == 401)
{
    await client.AuthenticateBearer(username, password);
    var result = await client.Protocols.GetProtocolsAsync();
}
```

### Persistent Authentication Service

```csharp
public class CellarioAuthService
{
    private readonly CellarioClient _client;
    private readonly string _username;
    private readonly string _password;
    private DateTime _lastAuth = DateTime.MinValue;
    private readonly TimeSpan _authTimeout = TimeSpan.FromHours(1);

    public async Task EnsureAuthenticatedAsync()
    {
        if (DateTime.UtcNow - _lastAuth > _authTimeout)
        {
            await _client.AuthenticateBearer(_username, _password);
            _lastAuth = DateTime.UtcNow;
        }
    }

    public async Task<T> ExecuteAuthenticatedAsync<T>(Func<Task<T>> operation)
    {
        await EnsureAuthenticatedAsync();
        try
        {
            return await operation();
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            await _client.AuthenticateBearer(_username, _password);
            _lastAuth = DateTime.UtcNow;
            return await operation();
        }
    }
}
```

## Advanced Configuration

### Custom HttpClient Configuration

```csharp
var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromMinutes(5);
httpClient.DefaultRequestHeaders.Add("X-Custom-Header", "value");

var client = new CellarioClient(httpClient);
```

### Connection Settings

```csharp
// HTTPS with custom port
var client = new CellarioClient("cellario.company.com", 443, useHttps: true);

// Custom base URL handling
var httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://cellario.company.com/api/")
};
var client = new CellarioClient(httpClient);
```

### JSON Serialization Configuration

The client uses System.Text.Json with custom converters for:

- Enum serialization as strings
- Polymorphic event deserialization
- Case-insensitive property matching
- Null value handling

### Environment-Based Configuration

```csharp
public class CellarioClientFactory
{
    public static CellarioClient CreateClient(IConfiguration config)
    {
        var host = config["Cellario:Host"] ?? "localhost";
        var port = config.GetValue<int?>("Cellario:Port") ?? 8444;
        var useHttps = config.GetValue<bool>("Cellario:UseHttps");

        return new CellarioClient(host, port, useHttps);
    }

    public static async Task<CellarioClient> CreateAuthenticatedClientAsync(
        IConfiguration config)
    {
        var client = CreateClient(config);

        var username = config["Cellario:Username"] ??
                      Environment.GetEnvironmentVariable("CELLARIO_USERNAME");
        var password = config["Cellario:Password"] ??
                      Environment.GetEnvironmentVariable("CELLARIO_PASSWORD");

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            await client.AuthenticateBearer(username, password);
        }

        return client;
    }
}
```

### Configuration File Example

```json
{
  "Cellario": {
    "Host": "cellario.company.com",
    "Port": 8444,
    "UseHttps": false,
    "Timeout": "00:05:00",
    "Username": "api-user",
    "Password": "api-password"
  }
}
```

## Security Best Practices

### 1. Credential Storage

- Never hardcode credentials in source code
- Use environment variables or secure configuration
- Consider Azure Key Vault or similar for production

### 2. Connection Security

- Use HTTPS in production environments
- Validate SSL certificates
- Consider client certificate authentication for high-security scenarios

### 3. Token Management

- Implement automatic re-authentication
- Handle authentication failures gracefully
- Log authentication events for auditing

## Troubleshooting Authentication

### Common Authentication Issues

1. **Invalid Credentials (401)**
   - Verify username/password
   - Check if account is active in Cellario
   - Ensure user has API access permissions

2. **Access Denied (403)**
   - User lacks required permissions
   - Check role assignments in Cellario
   - Verify system access rights

3. **Connection Refused**
   - Verify Cellario server is running
   - Check host/port configuration
   - Ensure network connectivity

4. **Token Expiry**
   - Implement automatic re-authentication
   - Monitor for 401 responses
   - Consider re-authentication patterns

### Debug Authentication Flow

```csharp
public class DebugAuthenticationHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log outgoing request
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
        if (request.Headers.Authorization != null)
        {
            Console.WriteLine($"Auth: {request.Headers.Authorization.Scheme}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Log response
        Console.WriteLine($"Response: {response.StatusCode}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Auth Error: {content}");
        }

        return response;
    }
}

// Usage
var httpClient = new HttpClient(new DebugAuthenticationHandler());
var client = new CellarioClient(httpClient);
```

## Next Steps

- [Explore API Reference →](03-api-reference.md)
- [See Integration Examples →](04-examples-and-best-practices.md)
