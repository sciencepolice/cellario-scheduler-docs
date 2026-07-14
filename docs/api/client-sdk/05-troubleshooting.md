# Troubleshooting Guide

## Common Issues and Solutions

### Connection Problems

#### "Connection Refused" or "Unable to connect"

**Symptoms:**

- `HttpRequestException`: No connection could be made
- Timeout errors during client creation

**Solutions:**

1. **Verify Cellario is running**

   There is no health endpoint. Use the version endpoint to confirm the API is reachable.

   ```bash
   # Check if API is accessible (Linux/macOS)
   curl http://localhost:8444/version
   ```

   ```powershell
   # Check if API is accessible (PowerShell)
   Invoke-RestMethod -Uri "http://localhost:8444/version" -Method Get
   ```

   Or from C#:

   ```csharp
   var client = new CellarioClient("localhost", 8444);
   var version = await client.Version.GetVersionAsync();
   Console.WriteLine($"API reachable. Version: {version.ApiVersion}");
   ```

2. **Check host and port configuration**

```csharp
  // Verify correct host/port
  var client = new CellarioClient("localhost", 8444);

  // For HTTPS (use actual server IP/hostname)
  var client = new CellarioClient("192.168.1.100", 8444, useHttps: true);
  // or
  var client = new CellarioClient("your-cellario-server", 8444, useHttps: true);
```

3. **Network connectivity**

- Ensure firewall allows connections
- Check if running on different machine/container
- Verify DNS resolution

#### "SSL/TLS Connection Issues"

**Symptoms:**

- Certificate validation errors
- TLS handshake failures

**Solutions:**

```csharp
// For development: bypass certificate validation (NOT for production)
var handler = new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var httpClient = new HttpClient(handler);
var client = new CellarioClient(httpClient);

// For production: ensure proper certificates
var client = new CellarioClient("cellario.company.com", 443, useHttps: true);
```

### Authentication Issues

#### "401 Unauthorized" Errors

**Symptoms:**

- Authentication fails immediately
- API calls return 401 after working previously

**Solutions:**

1. **Check credentials**

```csharp
  // Verify username/password
  try
  {
      await client.AuthenticateBearer("username", "password");
  }
  catch (ApiException ex) when (ex.StatusCode == 401)
  {
      Console.WriteLine("Invalid credentials");
  }
```

2. **Handle token expiration**

```csharp
  public async Task<T> ExecuteWithAuthRetryAsync<T>(Func<Task<T>> operation)
  {
      try
      {
          return await operation();
      }
      catch (ApiException ex) when (ex.StatusCode == 401)
      {
          // Re-authenticate and retry
          await client.AuthenticateBearer(username, password);
          return await operation();
      }
  }
```

3. **Check account status**

- Verify account is active in Cellario
- Ensure user has API access permissions
- Check if account is locked

#### "403 Forbidden" Errors

**Symptoms:**

- Authentication succeeds but API calls fail with 403
- Access denied to specific resources

**Solutions:**

1. **Check user accounts**

```csharp
  // List users to verify the account exists and is not locked.
  // (There is no "current user" endpoint; look the user up by name.)
  try
  {
      var users = await client.Users.GetUsersAsync();
      var user = users.FirstOrDefault(u => u.UserName == "username");
      if (user != null)
          Console.WriteLine($"User: {user.UserName}, Locked: {user.IsLocked}, RoleId: {user.UserRoleId}");
  }
  catch (ApiException ex)
  {
      Console.WriteLine($"Permission check failed: {ex.Message}");
  }
```

2. **Review role assignments**

- Ensure user has required roles in Cellario
- Check system-specific permissions
- Verify protocol/order access rights

### API Request Issues

#### "400 Bad Request" Validation Errors

**Symptoms:**

- Request validation fails
- Missing required parameters
- Invalid data formats

**Solutions:**

1. **Check request validation**

```csharp
  try
  {
      var order = await client.Orders.CreateOrderAsync(request);
  }
  catch (ApiException ex) when (ex.StatusCode == 400)
  {
      Console.WriteLine($"Validation error: {ex.Response}");
      // Parse the error response for specific field errors
  }
```

2. **Validate required fields**

```csharp
  var orderRequest = new CreateOrderRequest
  {
      Description = "Order description",
      User = "automation",
      Order = new OrderSetRequest
      {
          ProtocolId = protocolId, // Required
          PlateSets = new[]
          {
              new PlateSetRequest
              {
                  PlateProtocolId = plateProtocolId,
                  LabwareType = "96-well",
                  PlacementProcess = PlacementProcess.Available,
                  ExplicitCount = 1
              }
          }
      }
  };

  // Validate before sending.
  if (orderRequest.Order?.PlateSets == null || !orderRequest.Order.PlateSets.Any())
      throw new ArgumentException("At least one plate set is required");
```

#### "404 Not Found" Errors

**Symptoms:**

- Resource doesn't exist
- Incorrect IDs in requests

**Solutions:**

1. **Verify resource IDs**

```csharp
  // Check if protocol exists before using.
  try
  {
      var protocol = await client.Protocols.GetProtocolByIdAsync(protocolId);
  }
  catch (ApiException ex) when (ex.StatusCode == 404)
  {
      Console.WriteLine($"Protocol {protocolId} not found");
      return;
  }
```

2. **List available resources**

```csharp
  // Get available protocols to find correct ID.
  var protocols = await client.Protocols.GetProtocolsAsync();
  foreach (var p in protocols)
  {
      Console.WriteLine($"ID: {p.Id}, Name: {p.Name}");
  }
```

#### "500 Internal Server Error"

**Symptoms:**

- Server-side errors
- Unexpected API failures

**Solutions:**

1. **Check server logs**

- Review Cellario application logs
- Look for database connection issues
- Check for resource constraints

2. **Implement retry logic**

```csharp
  public async Task<T> ExecuteWithRetryAsync<T>(
      Func<Task<T>> operation,
      int maxRetries = 3)
  {
      for (int attempt = 1; attempt <= maxRetries; attempt++)
      {
          try
          {
              return await operation();
          }
          catch (ApiException ex) when (ex.StatusCode >= 500 && attempt < maxRetries)
          {
              var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
              await Task.Delay(delay);
          }
      }

      return await operation(); // Final attempt
  }
```

### Serialization Issues

#### JSON Deserialization Errors

**Symptoms:**

- `JsonException` during response parsing
- Unexpected null values
- Type conversion errors

**Solutions:**

1. **Check API version compatibility**

```csharp
  // Verify client and server versions match.
  var version = await client.Version.GetVersionAsync();
  Console.WriteLine($"API Version: {version.ApiVersion}, Cellario Version: {version.CellarioVersion}");
```

2. **Handle optional properties**

```csharp
  // Check for null values in responses.
  var protocols = await client.Protocols.GetProtocolsAsync();
  foreach (var protocol in protocols)
  {
      var notes = protocol.Notes ?? "No notes";
      var group = protocol.Group ?? "Default";
  }
```

3. **Regenerate client**

   ```bash
   # Update client with latest API schema
   cd Cellario.Client
   GenerateClient.bat
   ```

### Performance Issues

#### Slow API Responses

**Symptoms:**

- Requests taking longer than expected
- Timeout errors

**Solutions:**

1. **Increase timeout**

```csharp
  var httpClient = new HttpClient
  {
      Timeout = TimeSpan.FromMinutes(10) // Increase from default
  };
  var client = new CellarioClient(httpClient);
```

2. **Narrow the result set**

```csharp
  // GetProtocolsAsync returns the full list. Use the available filters to reduce
  // payload size, and skip heavy details unless you need them.
  var protocols = await client.Protocols.GetProtocolsAsync(
      includeDetails: false,
      groupName: new[] { "Assays" },
      latestVersionOnly: true,
      validatedOnly: true);
```

3. **Optimize batch operations**

```csharp
  // Process in smaller batches with delays.
  var protocolIds = GetLargeListOfIds();
  var semaphore = new SemaphoreSlim(5); // Limit concurrent requests.

  var protocols = await Task.WhenAll(protocolIds.Select(async id =>
  {
      await semaphore.WaitAsync();
      try
      {
          return await client.Protocols.GetProtocolByIdAsync(id);
      }
      finally
      {
          semaphore.Release();
      }
  }));
```

#### Memory Issues

**Symptoms:**

- High memory usage
- OutOfMemoryException

**Solutions:**

1. **Proper disposal**

```csharp
  // Dispose HttpClient when done
  using var httpClient = new HttpClient();
  var client = new CellarioClient(httpClient);
  // HttpClient disposed automatically
```

2. **Process large datasets in chunks**

```csharp
  // Instead of loading all data into memory
  await foreach (var protocolBatch in GetProtocolsInBatchesAsync())
  {
      ProcessBatch(protocolBatch);
      // Each batch is eligible for garbage collection
  }
```

## Debugging Tools

### HTTP Request/Response Logging

```csharp
public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
        : base(new HttpClientHandler())
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Log request
        _logger.LogInformation("HTTP {Method} {Uri}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            _logger.LogDebug("Request Body: {Content}", content);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Log response
        _logger.LogInformation("HTTP {StatusCode} {ReasonPhrase}",
            response.StatusCode, response.ReasonPhrase);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error Response: {Content}", errorContent);
        }

        return response;
    }
}

// Usage
var httpClient = new HttpClient(new LoggingHandler(logger));
var client = new CellarioClient(httpClient);
```

### Reachability Check

There is no dedicated health endpoint. Use the version endpoint as a lightweight reachability probe.

```csharp
public async Task<bool> IsApiReachableAsync()
{
    try
    {
        var version = await client.Version.GetVersionAsync();
        return !string.IsNullOrEmpty(version.ApiVersion);
    }
    catch
    {
        return false;
    }
}
```

### Connection Test Utility

```csharp
public class CellarioConnectionTester
{
    public async Task<ConnectionTestResult> TestConnectionAsync(
        string host,
        int port,
        bool useHttps,
        string username = null,
        string password = null)
    {
        var result = new ConnectionTestResult();

        try
        {
            // Test basic connectivity.
            var client = new CellarioClient(host, port, useHttps);
            var version = await client.Version.GetVersionAsync();
            result.CanConnect = true;
            result.ApiVersion = version.ApiVersion;

            // Test authentication if credentials provided.
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateBearer(username, password);
                // A successful authenticated call confirms the credentials work.
                var users = await client.Users.GetUsersAsync();
                result.CanAuthenticate = true;
                result.CurrentUser = username;
            }
        }
        catch (HttpRequestException ex)
        {
            result.Error = $"Connection failed: {ex.Message}";
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            result.Error = "Authentication failed: Invalid credentials";
        }
        catch (ApiException ex)
        {
            result.Error = $"API error: {ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex)
        {
            result.Error = $"Unexpected error: {ex.Message}";
        }

        return result;
    }
}

public class ConnectionTestResult
{
    public bool CanConnect { get; set; }
    public bool CanAuthenticate { get; set; }
    public string ApiVersion { get; set; }
    public string CurrentUser { get; set; }
    public string Error { get; set; }
}
```

## Version Compatibility

Ensure your client version matches your Cellario server version:

| Client Version | Cellario Version | .NET Version | Notes           |
| -------------- | ---------------- | ------------ | --------------- |
| 4.5.x          | 4.5.x            | .NET 8.0+    | Latest features |
| 4.4.x          | 4.4.x            | .NET 8.0+    | Stable release  |
| 4.3.x          | 4.3.x            | .NET 6.0+    | Legacy support  |

**Version Mismatch Issues:**

- New API endpoints not available in older clients
- Changed response models causing deserialization errors
- Deprecated endpoints removed in newer versions

**Solution:** Always regenerate the client when upgrading Cellario versions.

## Getting Support

### Information to Collect

When reporting issues, include:

1. **Client Information**

```csharp
  Console.WriteLine($"Client Version: {typeof(CellarioClient).Assembly.GetName().Version}");
```

2. **Server Information**

```csharp
  var version = await client.Version.GetVersionAsync();
  Console.WriteLine($"API Version: {version.ApiVersion}");
  Console.WriteLine($"Cellario Version: {version.CellarioVersion}");
  Console.WriteLine($"Assembly Version: {version.AssemblyVersion}");
```

3. **Error Details**

- Full exception stack trace
- HTTP status codes
- Request/response bodies (sanitized)

4. **Environment Details**

- .NET version
- Operating system
- Network configuration

### Support Channels

- **Documentation**: Check API docs at `http://your-cellario-server:8444/docs`
- **OpenAPI Spec**: `http://your-cellario-server:8444/docs/v1/swagger.json`
- **Support**: Contact HighRes Biosolutions support team

## Next Steps

- [Back to Overview →](01-overview-and-quick-start.md)
- [API Reference →](03-api-reference.md)
- [Examples →](04-examples-and-best-practices.md)
