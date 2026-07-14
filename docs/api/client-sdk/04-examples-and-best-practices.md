# Examples & Best Practices

## Common Usage Patterns

### Protocol Management Example

`GetProtocolsAsync` returns an `IList<ProtocolResponse>` directly (there is no paged wrapper). To filter
by group, pass the `groupName` parameter, which accepts a list of group names.

```csharp
public class ProtocolManager
{
    private readonly CellarioClient _client;

    public ProtocolManager(CellarioClient client)
    {
        _client = client;
    }

    public async Task<List<ProtocolResponse>> GetProtocolsByGroupAsync(string group)
    {
        var protocols = await _client.Protocols.GetProtocolsAsync(groupName: new[] { group });
        return protocols.ToList();
    }

    // Round-trip clone: read a full protocol definition, map it to a request, and apply
    // it to a freshly created protocol shell.
    public async Task<ProtocolDtoResponse> CloneProtocolAsync(
        int sourceProtocolId,
        string newName,
        string targetGroup = null)
    {
        // Read the full protocol definition (Designer returns ProtocolDtoResponse).
        var sourceProtocol = await _client.Designer.GetProtocolAsync(sourceProtocolId);

        // Map the response to a request object for editing.
        var protocolRequest = DesignerClient.MapResponseToRequest(sourceProtocol);
        protocolRequest.Name = newName;
        if (targetGroup != null)
            protocolRequest.Group = targetGroup;

        // Create the new protocol shell, then apply the mapped definition to it.
        var created = await _client.Designer.CreateProtocolAsync(new CreateProtocolRequest
        {
            Name = newName,
            Group = targetGroup ?? sourceProtocol.Group
        });

        return await _client.Designer.UpdateProtocolAsync(created.Id, protocolRequest);
    }
}
```

### Order Automation Example

Orders are created with a `CreateOrderRequest` whose `Order` property describes the order via an
`OrderSetRequest` (protocol id plus one or more plate sets). Order lifecycle actions (start, pause,
cancel) are issued through `IssueOrderActionAsync` with an `IssueOrderActionRequest` carrying an
`OrderAction`.

```csharp
public class OrderAutomation
{
    private readonly CellarioClient _client;
    private readonly ILogger<OrderAutomation> _logger;

    public async Task<int> CreateAndStartOrderAsync(
        int protocolId,
        string description,
        int plateProtocolId,
        string labwareType)
    {
        try
        {
            // Create the order from an explicit order definition.
            var orderRequest = new CreateOrderRequest
            {
                Description = description,
                User = "automation",
                CreateDefaultParameters = true,
                Order = new OrderSetRequest
                {
                    ProtocolId = protocolId,
                    PlateSets = new[]
                    {
                        new PlateSetRequest
                        {
                            PlateProtocolId = plateProtocolId,
                            LabwareType = labwareType,
                            PlacementProcess = PlacementProcess.Available,
                            ExplicitCount = 1
                        }
                    }
                }
            };

            var order = await _client.Orders.CreateOrderAsync(orderRequest);
            _logger.LogInformation($"Created order {order.Id} for protocol {order.ProtocolName}");

            // Start the order by issuing a Start action.
            await _client.Orders.IssueOrderActionAsync(order.Id, new IssueOrderActionRequest
            {
                Action = OrderAction.Start
            });
            _logger.LogInformation($"Started order {order.Id}");

            return order.Id;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, $"Failed to create/start order for protocol {protocolId}");
            throw;
        }
    }

    public async Task<OrderState> WaitForOrderCompletionAsync(
        int orderId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var order = await _client.Orders.GetOrderAsync(orderId);

            if (order.State == OrderState.Finished ||
                order.State == OrderState.Removed ||
                order.State == OrderState.Canceled)
            {
                return order.State;
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        throw new TimeoutException($"Order {orderId} did not complete within {timeout}");
    }
}
```

### System Monitoring Example

`GetSystemsAsync` returns an `IList<SystemInfoResponse>`. Each system's current state is available
directly on `SystemInfoResponse.State` (there is no separate status call). Systems are identified by
`Name`.

```csharp
public class SystemMonitor
{
    private readonly CellarioClient _client;
    private readonly Timer _monitoringTimer;

    public event Action<SystemStatusChangeEventArgs> SystemStatusChanged;

    public SystemMonitor(CellarioClient client)
    {
        _client = client;
        _monitoringTimer = new Timer(MonitorSystems, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private async void MonitorSystems(object state)
    {
        try
        {
            var systems = await _client.Systems.GetSystemsAsync();

            foreach (var system in systems)
            {
                // Check for issues using the state reported on the system itself.
                if (system.State == SystemState.InError)
                {
                    SystemStatusChanged?.Invoke(new SystemStatusChangeEventArgs
                    {
                        SystemName = system.Name,
                        Status = system.State,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log monitoring errors but don't throw.
            Console.WriteLine($"System monitoring error: {ex.Message}");
        }
    }
}

public class SystemStatusChangeEventArgs : EventArgs
{
    public string SystemName { get; set; }
    public SystemState Status { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Integration Examples

### Console Application

```csharp
using Cellario.Client;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration.
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        // Create and authenticate client.
        var client = new CellarioClient(
            config["Cellario:Host"],
            config.GetValue<int>("Cellario:Port")
        );

        await client.AuthenticateBearer(
            config["Cellario:Username"],
            config["Cellario:Password"]
        );

        // List available protocols.
        Console.WriteLine("Available Protocols:");
        var protocols = await client.Protocols.GetProtocolsAsync();
        foreach (var protocol in protocols.Take(10))
        {
            Console.WriteLine($"  {protocol.Id}: {protocol.Name} ({protocol.Group})");
        }

        // Show system status.
        Console.WriteLine("\nSystem Status:");
        var systems = await client.Systems.GetSystemsAsync();
        foreach (var system in systems)
        {
            Console.WriteLine($"  {system.Name}: {system.State}");
        }
    }
}
```

### ASP.NET Core Service

```csharp
public interface ICellarioService
{
    Task<List<ProtocolResponse>> GetProtocolsAsync(string group = null);
    Task<int> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse> GetOrderStatusAsync(int orderId);
}

public class CellarioService : ICellarioService
{
    private readonly CellarioClient _client;
    private readonly ILogger<CellarioService> _logger;
    private readonly SemaphoreSlim _authSemaphore = new(1, 1);
    private DateTime _lastAuthTime = DateTime.MinValue;
    private readonly TimeSpan _authTimeout = TimeSpan.FromHours(1);

    public CellarioService(CellarioClient client, ILogger<CellarioService> logger)
    {
        _client = client;
        _logger = logger;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (DateTime.UtcNow - _lastAuthTime < _authTimeout)
            return;

        await _authSemaphore.WaitAsync();
        try
        {
            if (DateTime.UtcNow - _lastAuthTime < _authTimeout)
                return;

            var username = Environment.GetEnvironmentVariable("CELLARIO_USERNAME");
            var password = Environment.GetEnvironmentVariable("CELLARIO_PASSWORD");

            await _client.AuthenticateBearer(username, password);
            _lastAuthTime = DateTime.UtcNow;

            _logger.LogInformation("Cellario client re-authenticated");
        }
        finally
        {
            _authSemaphore.Release();
        }
    }

    public async Task<List<ProtocolResponse>> GetProtocolsAsync(string group = null)
    {
        await EnsureAuthenticatedAsync();

        var groupNames = group != null ? new[] { group } : null;

        try
        {
            var protocols = await _client.Protocols.GetProtocolsAsync(groupName: groupNames);
            return protocols.ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            _lastAuthTime = DateTime.MinValue; // Force re-auth.
            await EnsureAuthenticatedAsync();
            var protocols = await _client.Protocols.GetProtocolsAsync(groupName: groupNames);
            return protocols.ToList();
        }
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        await EnsureAuthenticatedAsync();

        var order = await _client.Orders.CreateOrderAsync(request);
        _logger.LogInformation($"Created order {order.Id} for protocol {order.ProtocolName}");

        return order.Id;
    }

    public async Task<OrderResponse> GetOrderStatusAsync(int orderId)
    {
        await EnsureAuthenticatedAsync();
        return await _client.Orders.GetOrderAsync(orderId);
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs (ASP.NET Core 6+)
using Cellario.Client;

var builder = WebApplication.CreateBuilder(args);

// Register Cellario client.
builder.Services.AddSingleton<CellarioClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new CellarioClient(
        config["Cellario:Host"],
        config.GetValue<int>("Cellario:Port")
    );
});

// Register service.
builder.Services.AddScoped<ICellarioService, CellarioService>();

var app = builder.Build();

// Configure middleware.
app.MapControllers();

app.Run();
```

### Background Service for Order Processing

`GetOrdersAsync` returns an `IList<OrderResponse>` directly and accepts an optional `state` filter.
Auto-starting an order is done by issuing a `Start` action.

```csharp
public class OrderProcessingService : BackgroundService
{
    private readonly CellarioClient _client;
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OrderProcessingService(
        CellarioClient client,
        ILogger<OrderProcessingService> logger,
        IServiceProvider serviceProvider)
    {
        _client = client;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.AuthenticateBearer(
            Environment.GetEnvironmentVariable("CELLARIO_USERNAME"),
            Environment.GetEnvironmentVariable("CELLARIO_PASSWORD")
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrders(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in order processing service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task ProcessPendingOrders(CancellationToken cancellationToken)
    {
        // Get orders that are created but not yet started.
        var orders = await _client.Orders.GetOrdersAsync(state: OrderState.Created);

        foreach (var order in orders.Where(ShouldAutoStart))
        {
            try
            {
                await _client.Orders.IssueOrderActionAsync(order.Id, new IssueOrderActionRequest
                {
                    Action = OrderAction.Start
                });
                _logger.LogInformation($"Auto-started order {order.Id} ({order.ProtocolName})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to start order {order.Id}");
            }
        }
    }

    private bool ShouldAutoStart(OrderResponse order)
    {
        // Custom logic to determine if an order should be auto-started.
        return order.CreatedDate.HasValue &&
               order.CreatedDate.Value < DateTimeOffset.UtcNow.AddMinutes(-5);
    }
}
```

## Best Practices

### 1. Resource Management

`CellarioClient` can be constructed from an `HttpClient` you own. The client does not take ownership
of the `HttpClient`, so dispose the `HttpClient` yourself.

```csharp
// Own and dispose the underlying HttpClient.
public class CellarioClientManager : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly CellarioClient _client;

    public CellarioClientManager()
    {
        _httpClient = new HttpClient();
        _client = new CellarioClient(_httpClient);
    }

    public CellarioClient Client => _client;

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
```

### 2. Error Handling Patterns

```csharp
public async Task<TResult> ExecuteWithRetryAsync<TResult>(
    Func<Task<TResult>> operation,
    int maxRetries = 3,
    TimeSpan delay = default)
{
    delay = delay == default ? TimeSpan.FromSeconds(1) : delay;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (ApiException ex) when (ex.StatusCode == 401 && attempt < maxRetries)
        {
            // Re-authenticate and retry.
            await _client.AuthenticateBearer(_username, _password);
            await Task.Delay(delay);
        }
        catch (HttpRequestException ex) when (attempt < maxRetries)
        {
            // Network error, retry with delay.
            _logger.LogWarning($"Network error on attempt {attempt}: {ex.Message}");
            await Task.Delay(delay * attempt); // Exponential backoff.
        }
    }

    // Final attempt without catching exceptions.
    return await operation();
}
```

### 3. Batch Operations

```csharp
public async Task<List<TResult>> ProcessBatchAsync<TInput, TResult>(
    IEnumerable<TInput> items,
    Func<TInput, Task<TResult>> processor,
    int batchSize = 10,
    TimeSpan delay = default)
{
    delay = delay == default ? TimeSpan.FromMilliseconds(100) : delay;
    var results = new List<TResult>();

    var batches = items
        .Select((item, index) => new { item, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.item));

    foreach (var batch in batches)
    {
        var batchTasks = batch.Select(processor);
        var batchResults = await Task.WhenAll(batchTasks);
        results.AddRange(batchResults);

        // Small delay between batches to avoid overwhelming the server.
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }
    }

    return results;
}

// Usage example.
var protocolIds = new[] { 1, 2, 3, 4, 5 };
var protocols = await ProcessBatchAsync(
    protocolIds,
    id => client.Protocols.GetProtocolByIdAsync(id),
    batchSize: 3
);
```

### 4. Configuration Management

```csharp
public class CellarioOptions
{
    public const string SectionName = "Cellario";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8444;
    public bool UseHttps { get; set; } = false;
    public string Username { get; set; }
    public string Password { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan AuthTimeout { get; set; } = TimeSpan.FromHours(1);
}

// Registration.
services.Configure<CellarioOptions>(configuration.GetSection(CellarioOptions.SectionName));

services.AddSingleton<CellarioClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<CellarioOptions>>().Value;
    var httpClient = new HttpClient { Timeout = options.Timeout };
    return new CellarioClient(httpClient);
});
```

### 5. Async Best Practices

```csharp
// Use ConfigureAwait(false) in library code.
var protocols = await client.Protocols.GetProtocolsAsync()
    .ConfigureAwait(false);

// Avoid async void except for event handlers.
public async Task ProcessOrdersAsync() // Good
public async void ProcessOrders()      // Avoid

// Use cancellation tokens.
public async Task<OrderResponse> WaitForOrderAsync(int orderId, CancellationToken cancellationToken)
{
    while (true)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var order = await client.Orders.GetOrderAsync(orderId);
        if (order.State == OrderState.Finished)
            return order;

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }
}
```

## Next Steps

- [Troubleshooting Guide →](05-troubleshooting.md)
- [API Reference →](03-api-reference.md)
