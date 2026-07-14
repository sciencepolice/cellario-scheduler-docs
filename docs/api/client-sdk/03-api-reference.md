# API Reference

This reference documents the **real** methods exposed by the generated `Cellario.Client` SDK. Every method, request type, response type, and enum listed here was verified against the generated source (`CellarioClient.cs`, `GeneratedClientClasses.cs`, `DesignerClientExtended.cs`).

Conventions used throughout:

- All operations are asynchronous and end in `Async`. Each method also has a synchronous twin (same name without `Async`) and accepts a trailing `CancellationToken`.
- Methods that "return nothing" return `Task`; collection methods return `Task<IList<T>>` directly (there is **no** paged wrapper / `.Items` envelope).
- Optional parameters are shown with their defaults.

## Creating a Client

```csharp
using Cellario.Client;

// Host + port + scheme
var client = new CellarioClient("localhost", port: 8444, useHttps: false);

// Or supply your own HttpClient (custom handlers, base address, etc.)
var client = new CellarioClient(httpClient);
```

### Authentication

```csharp
// Bearer token (recommended) - performs a login round-trip
await client.AuthenticateBearer("username", "password");

// Synchronous bearer login
client.AuthenticateBearerSync("username", "password");

// HTTP Basic auth
client.AuthenticateBasic("username", "password");

// Remove the Authorization header
client.ClearAuthentication();
```

## Available API Clients

`CellarioClient` exposes these sub-clients as properties:

| Property             | Type                       | Purpose                                            |
| -------------------- | -------------------------- | -------------------------------------------------- |
| `SystemConfig`       | `SystemConfigClient`       | Resources, operations, operation parameter types   |
| `Labware`            | `LabwareTypesClient`       | Labware type definitions                           |
| `Protocols`          | `ProtocolsClient`          | Read protocols, parameters, events, plate protocols |
| `Designer`           | `DesignerClient`           | Create/update/delete/validate protocol definitions |
| `Operations`         | `OperationsClient`         | Device operations queue                            |
| `Orders`             | `OrdersClient`             | Create orders, control execution, samples, params  |
| `Resources`          | `ResourcesClient`          | System resources, drivers, positions, device actions |
| `Subscribers`        | `SubscribersClient`        | Event subscriptions                                |
| `Systems`            | `SystemsClient`            | System info and state                              |
| `Settings`           | `SettingsClient`           | System settings                                    |
| `Version`            | `VersionClient`            | Service version information                        |
| `WellTransfers`      | `WellTransfersClient`      | Well-to-well transfer records                      |
| `DispenseMaps`       | `DispenseMapsClient`       | Dispense maps                                      |
| `Inventory`          | `InventoryClient`          | Inventory items, positions, scans                  |
| `Scripts`            | `ScriptsClient`            | Scripts                                            |
| `Templates`          | `TemplatesClient`          | Order templates, simulations, data packages        |
| `General`            | `GeneralClient`            | Files, peripherals                                 |
| `Users`              | `UsersClient`              | Users and roles                                    |
| `Login`              | `TokenClient`              | Login / token issuance                             |
| `Events`             | `EventsClient`             | Event querying                                     |
| `GlobalEventScripts` | `GlobalEventScriptsClient` | System-wide event scripts                          |

> The `Designer` sub-client is the editing counterpart to the read-only `Protocols` sub-client. Use `Protocols` to read, `Designer` to create/update/delete/validate.

## Orders (`client.Orders`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `CreateOrderAsync` | `(CreateOrderRequest createOrderRequest)` | `OrderResponse` |
| `CreateOrderWithTransferListAsync` | `(CreateOrderByTransferListRequest request)` | `OrderResponse` |
| `GetOrdersAsync` | `(bool? searchAllOrders, int? protocolId, bool? includeDetails, OrderState? state, bool? includeFailedOperations)` | `IList<OrderResponse>` |
| `GetOrderAsync` | `(int orderId, bool? includeDetails, bool? includeFailedOperations)` | `OrderResponse` |
| `IssueOrderActionAsync` | `(int orderId, IssueOrderActionRequest request)` | `OrderResponse` |
| `UpdateOrderDetailsAsync` | `(int orderId, UpdateOrderDetailsRequest request)` | `OrderResponse` |
| `DeleteOrderAsync` | `(int orderId)` | `Task` |
| `ValidateOrderAsync` | `(string orderId)` | `ValidateOrderResponse` |
| `EstimateOrderAsync` | `(string orderId)` | `EstimateOrderResponse` |
| `SimulateOrderAsync` | `(string orderId)` | `TimeSpan` |
| `QueueSimulationAsync` | `(QueueSimulationRequest request)` | `SimulationResultResponse` |
| `GetSimulationResultAsync` | `(string requestId)` | `SimulationResultResponse` |
| `GetSimulationResultsAsync` | `()` | `IList<SimulationResultResponse>` |
| `CancelSimulationAsync` | `(string requestId)` | `Task` |
| `CreateDataPackageAsync` | `(int orderId)` | `BaseEvent` |
| `CreateOrderParametersAsync` | `(int orderId, IList<RunOrderParameterRequest> newParameters)` | `IList<RunOrderParameterResponse>` |
| `GetOrderParametersAsync` | `(int orderId)` | `IList<RunOrderParameterResponse>` |
| `UpdateOrderParametersAsync` | `(int orderId, IList<UpdateOrderParameterRequest> newParameters)` | `IList<RunOrderParameterResponse>` |
| `DeleteOrderParameterAsync` | `(int orderId, int parameterId)` | `RunOrderParameterResponse` |
| `CreateOrderOutputAsync` | `(int orderId, OrderOutputRequest output)` | `ThreadOutputResponse` |
| `GetOrderOutputsByOrderAsync` | `(int orderId)` | `IList<ThreadOutputResponse>` |
| `CreateOrderSampleAsync` | `(int orderId, SingleOrderSampleRequest orderSample)` | `OrderSampleResponse` |
| `CreateOrderSamplesAsync` | `(int orderId, IList<PlateSetRequest> plateSets)` | `IList<OrderSampleResponse>` |
| `GetOrderSamplesByOrderAsync` | `(int orderId)` | `IList<OrderSampleResponse>` |
| `GetOrderSampleByOrderAsync` | `(int orderId, int orderSampleId)` | `OrderSampleResponse` |
| `GetOrderSampleByIdAsync` | `(int orderSampleId)` | `OrderSampleResponse` |
| `GetOrderSamplesAsync` | `(IList<int> orderSampleIds, IList<int> runOrderIds)` | `IList<OrderSampleResponse>` |
| `UpdateOrderSamplesAsync` | `(int orderId, IList<OrderSampleRequest> orderSamples)` | `IList<OrderSampleResponse>` |
| `DeleteOrderSampleAsync` | `(int orderSampleId)` | `Task` |
| `GetCurrentSampleOperationsAsync` | `(int orderId, bool? filterFinishedSamples, bool? includeValidSampleActions)` | `IList<SampleOperationResponse>` |
| `GetSampleOperationsAsync` | `(IList<int> orderIds, bool? current)` | `IList<SampleOperationResponse>` |
| `IssueSampleOperationActionAsync` | `(int orderId, int sampleOperationId, SampleAction action)` | `Task` |

> **Order control is done through `IssueOrderActionAsync`.** There are no `StartOrderAsync` / `PauseOrderAsync` / `ResumeOrderAsync` / `CancelOrderAsync` / `SearchOrdersAsync` methods.

### Creating an order

`CreateOrderRequest` does **not** have `ProtocolId` / `Name` / `Priority` / `StartMode` / `Notes` at the top level. The protocol and plates are specified through the nested `Order` (`OrderSetRequest`) property, or via `Template` (`TemplateSetRequest`) when ordering from a template (do not set both).

```csharp
var order = await client.Orders.CreateOrderAsync(new CreateOrderRequest
{
    Description = "Batch Process #123",
    User = "labuser",
    EmailRecipient = "alerts@company.com",
    InventoryScan = false,
    ClearStorage = false,
    ShouldBeValidated = false,
    CreateDefaultParameters = true,
    Order = new OrderSetRequest
    {
        ProtocolId = protocolId,
        PlateSets = new List<PlateSetRequest>
        {
            // describe the plates for this order...
        }
    }
});

Console.WriteLine($"Created order {order.Id} ({order.ProtocolName}), state={order.State}");
```

### Controlling an order

```csharp
// Start
await client.Orders.IssueOrderActionAsync(order.Id,
    new IssueOrderActionRequest { Action = OrderAction.Start });

// Pause
await client.Orders.IssueOrderActionAsync(order.Id,
    new IssueOrderActionRequest { Action = OrderAction.Pause });

// Cancel
await client.Orders.IssueOrderActionAsync(order.Id,
    new IssueOrderActionRequest { Action = OrderAction.Cancel });
```

`OrderAction` values: `Start`, `Pause`, `Cancel`, `Complete`, `Schedule`, `Validate`, `Invalidate`, `BatchPause`, `ExpressStart`, `None`.

### Listing orders

```csharp
var running = await client.Orders.GetOrdersAsync(
    searchAllOrders: true,
    state: OrderState.Started,
    includeDetails: true);
```

### `OrderResponse`

Key properties: `Id` (int), `State` (`OrderState`), `ProtocolId` (int), `ProtocolName` (string), `ProtocolVersion` (int), `Description`, `StartTime`/`StartTimeUtc`/`EndTime`/`EndTimeUtc` (`DateTimeOffset?`), `CreatedBy`, `CreatedDate`, `InventoryScan`, `EmailRecipients`, `System`, `OrderSamples`, `Parameters`, `ThreadOutputs`.

> There is **no** `Name` property and **no** `OrderDetailsResponse` / `OrderSummary` type. Use `ProtocolName` / `Description`.

### `OrderState` enum

`Created`, `Submitted`, `Started`, `Finished`, `Removed`, `Pausing`, `Paused`, `Scanning`, `Scripting`, `Starting`, `Canceled` (note the single-L spelling).

## Protocols (`client.Protocols`)

Read-only access to protocols and their components.

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetProtocolsAsync` | `(bool? includeDetails, IList<string> groupName, IList<string> protocolName, bool? latestVersionOnly, bool? validatedOnly, bool? includeImages, bool? includeLastUsedInfo)` | `IList<ProtocolResponse>` |
| `GetProtocolByIdAsync` | `(int protocolId, bool? includeDetails)` | `ProtocolResponse` |
| `GetProtocolRatiosAsync` | `(int protocolId, bool excludeOptionalThreads)` | `IList<PlateProtocolRatioResponse>` |
| `CreateProtocolParametersAsync` | `(int protocolId, IList<ProtocolParameterRequest> protocolParameters)` | `IList<ProtocolParameterResponse>` |
| `GetProtocolParametersByProtocolIdAsync` | `(int protocolId)` | `IList<ProtocolParameterResponse>` |
| `CreateProtocolEventAsync` | `(int protocolId, ProtocolEventRequest protocolEvent)` | `ProtocolEventResponse` |
| `GetProtocolEventsAsync` | `(int protocolId)` | `IList<ProtocolEventResponse>` |
| `GetPlateProtocolByIdAsync` | `(int plateProtocolId)` | `PlateProtocolResponse` |
| `GetPlateProtocolByProtocolIdAsync` | `(int protocolId, int plateProtocolId)` | `PlateProtocolResponse` |
| `GetPlateProtocolsAsync` | `(IList<int> plateProtocolIds, IList<int> protocolIds, string tag)` | `IList<PlateProtocolResponse>` |
| `GetPlateProtocolsByProtocolIdAsync` | `(int protocolId, string tag)` | `IList<PlateProtocolResponse>` |
| `GetProtocolStepByPlateProtocolIdAsync` | `(int plateProtocolId, int protocolStepId)` | `ProtocolStepResponse` |
| `GetProtocolStepByProtocolAndPlateProtocolIdAsync` | `(int protocolId, int plateProtocolId, int protocolStepId)` | `ProtocolStepResponse` |
| `GetProtocolStepsByPlateProtocolIdAsync` | `(int plateProtocolId)` | `IList<ProtocolStepResponse>` |
| `GetProtocolStepsByProtocolAndPlateProtocolIdAsync` | `(int protocolId, int plateProtocolId)` | `IList<ProtocolStepResponse>` |

```csharp
// List protocols (returns the list directly - no .Items wrapper)
IList<ProtocolResponse> protocols = await client.Protocols.GetProtocolsAsync();

// Filter by group / name / validation status
var assays = await client.Protocols.GetProtocolsAsync(
    groupName: new[] { "Assays" },
    validatedOnly: true);

// Single protocol with full details
var protocol = await client.Protocols.GetProtocolByIdAsync(protocolId, includeDetails: true);
```

> There is **no** `GetProtocolSummaryAsync` and **no** `ProtocolSummary` type. `GetProtocolAsync(int)` lives on `Designer`, not `Protocols` (see below).

## Protocol Designer (`client.Designer`)

Create, edit, delete, and validate protocol definitions.

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `CreateProtocolAsync` | `(CreateProtocolRequest createProtocolRequest)` | `ProtocolDtoResponse` |
| `GetProtocolAsync` | `(int id, bool? includeDetails)` | `ProtocolDtoResponse` |
| `UpdateProtocolAsync` | `(int id, ProtocolDtoRequest protocolDtoRequest, bool? createNewVersion)` | `ProtocolDtoResponse` |
| `DeleteProtocolAsync` | `(int id)` | `Task` |
| `SetProtocolValidatedAsync` | `(int id, SetProtocolValidatedRequest request)` | `ProtocolDtoResponse` |

`DesignerClient` also provides a static helper for round-trip editing:

| Helper | Signature | Returns |
| ------ | --------- | ------- |
| `DesignerClient.MapResponseToRequest` | `(ProtocolDtoResponse response)` | `ProtocolDtoRequest` |

```csharp
// Fetch a protocol definition for editing
ProtocolDtoResponse current = await client.Designer.GetProtocolAsync(protocolId, includeDetails: true);

// Convert response -> request, edit, then save
ProtocolDtoRequest edit = DesignerClient.MapResponseToRequest(current);
edit.Notes = "Updated via SDK";
await client.Designer.UpdateProtocolAsync(protocolId, edit, createNewVersion: true);

// Mark validated
await client.Designer.SetProtocolValidatedAsync(protocolId,
    new SetProtocolValidatedRequest { /* ... */ });
```

## Systems (`client.Systems`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetSystemsAsync` | `()` | `IList<SystemInfoResponse>` |
| `GetSystemByNameAsync` | `(string name)` | `SystemInfoResponse` |
| `GetCurrentSystemAsync` | `()` | `SystemInfoResponse` |
| `CreateSystemAsync` | `(CreateSystemInfoRequest request)` | `SystemInfoResponse` |
| `UpdateSystemAsync` | `(string name, UpdateSystemInfoRequest request)` | `SystemInfoResponse` |
| `RenameSystemAsync` | `(RenameSystemInfoRequest request)` | `SystemInfoResponse` |
| `DeleteSystemAsync` | `(string name)` | `Task` |
| `UpdateCurrentSystemStateAsync` | `(SystemActionSet systemActionSet)` | `Task` |

```csharp
var systems = await client.Systems.GetSystemsAsync();
var current = await client.Systems.GetCurrentSystemAsync();
Console.WriteLine($"{current.Name} ({current.HostName}) is {current.State}");
```

> There is **no** `GetSystemAsync(int)` and **no** `GetSystemStatusAsync`. Systems are addressed **by name**, not by id. `SystemInfoResponse` has `Name`, `Description`, `State`, `SwitchAddress`, `HostName`, `SimulationSpeed`, `AssetId` (there is **no** `Id` property).

### `SystemState` enum

`UnInitialized`, `Initializing`, `Initialized`, `Running`, `Pausing`, `Paused`, `InError` (there is no `Error` or `Offline`).

## Resources (`client.Resources`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetSystemResourcesAsync` | `(bool? includeDetails, string resourceType, bool? filterBySystem)` | `IList<SystemResourceResponse>` |
| `GetSystemResourceAsync` | `(string resourceName, bool? includeDetails, bool? filterBySystem)` | `SystemResourceResponse` |
| `GetDriversAsync` | `()` | `IList<DriverResponse>` |
| `GetDriverAsync` | `(string resourceName)` | `DriverResponse` |
| `GetResourceInventoryAsync` | `(string resourceName)` | `IList<OrderSampleResponse>` |
| `GetResourcePositionByIdAsync` | `(int resourcePositionId)` | `ResourcePositionResponse` |
| `GetResourcePositionByResourceAsync` | `(string resourceName, int resourcePositionId)` | `ResourcePositionResponse` |
| `GetResourcePositionsAsync` | `(IList<string> resources, IList<int> positionIds)` | `IList<ResourcePositionResponse>` |
| `GetResourcePositionsByResourceAsync` | `(string resourceName)` | `IList<ResourcePositionResponse>` |
| `IssueDeviceActionAsync` | `(string resourceName, DeviceActionSetRequest deviceActionSet)` | `Task` |

```csharp
var resources = await client.Resources.GetSystemResourcesAsync(includeDetails: true);
var device = await client.Resources.GetSystemResourceAsync("Incubator1", includeDetails: true);
```

> There is **no** `GetResourcesAsync(systemId)`, `GetResourcesByTypeAsync`, `GetResourceAvailabilityAsync`, or `ResourceType` enum. Filter by resource type using the `resourceType` string parameter on `GetSystemResourcesAsync`. (Resource *definitions* are managed via `client.SystemConfig`.)

## System Configuration (`client.SystemConfig`)

Manages resource definitions, resource types, operations, and operation parameter types.

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetResourcesAsync` | `()` | `IList<ResourceResponse>` |
| `GetResourceAsync` | `(string name)` | `ResourceResponse` |
| `CreateResourceAsync` | `(CreateResourceRequest request)` | `ResourceResponse` |
| `CopyResourceAsync` | `(CopyResourceRequest request)` | `ResourceResponse` |
| `UpdateResourceAsync` | `(string name, UpdateResourceRequest request)` | `ResourceResponse` |
| `RenameResourceAsync` | `(RenameResourceRequest request)` | `ResourceResponse` |
| `DeleteResourceAsync` | `(string name)` | `Task` |
| `ImportResourceAsync` | `(FileParameter hrbPackageFile, ImportResourceDetails details)` | `ResourceResponse` |
| `GetResourceTypesAsync` | `()` | `IList<ResourceTypeResponse>` |
| `GetResourceTypeAsync` | `(string name)` | `ResourceTypeResponse` |
| `CreateResourceTypeAsync` | `(CreateResourceTypeRequest request)` | `ResourceTypeResponse` |
| `UpdateResourceTypeAsync` | `(string name, UpdateResourceTypeRequest request)` | `ResourceTypeResponse` |
| `DeleteResourceTypeAsync` | `(string name)` | `Task` |
| `GetOperationsAsync` | `()` | `IList<OperationResponse>` |
| `GetOperationAsync` | `(string id)` | `OperationResponse` |
| `CreateOperationAsync` | `(FileParameter imageFile, OperationDetails details)` | `OperationResponse` |
| `UpdateOperationAsync` | `(string id, UpdateOperationRequest request)` | `OperationResponse` |
| `UpdateOperationImageAsync` | `(string id, FileParameter imageFile)` | `OperationResponse` |
| `DeleteOperationAsync` | `(string id)` | `Task` |
| `GetOperationParameterTypesAsync` | `()` | `IList<OperationParameterTypeResponse>` |
| `GetOperationParameterTypeAsync` | `(string id)` | `OperationParameterTypeResponse` |
| `CreateOperationParameterTypeAsync` | `(OperationParameterTypeRequest request)` | `OperationParameterTypeResponse` |
| `UpdateOperationParameterTypeAsync` | `(string id, OperationParameterTypeRequest request)` | `OperationParameterTypeResponse` |
| `DeleteOperationParameterTypeAsync` | `(string id)` | `Task` |

## Operations (`client.Operations`)

Device operation queue.

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `QueueOperationAsync` | `(DeviceOperationRequest request)` | `DeviceOperationResponse` |
| `GetDeviceOperationsAsync` | `()` | `IList<DeviceOperationResponse>` |
| `GetDeviceOperationAsync` | `(string id)` | `DeviceOperationResponse` |
| `CancelDeviceOperationAsync` | `(string id)` | `Task` |

## Inventory (`client.Inventory`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetInventoryAsync` | `(bool? includeEmpty, IList<string> resource)` | `IList<InventoryItemResponse>` |
| `GetInventoryByPositionAsync` | `(int positionId)` | `InventoryItemResponse` |
| `UpdateInventoryAsync` | `(IList<UpdateInventoryRequestItem> updateInventoryRequest)` | `IList<InventoryItemResponse>` |
| `InventoryScanAsync` | `(InventoryScanRequest request)` | `InventoryScanResponse` |
| `GetScansAsync` | `()` | `IList<InventoryScanResponse>` |
| `GetScanAsync` | `(Guid id)` | `InventoryScanResponse` |
| `CancelScanAsync` | `(Guid id)` | `Task` |

```csharp
var items = await client.Inventory.GetInventoryAsync(includeEmpty: false);
var atPosition = await client.Inventory.GetInventoryByPositionAsync(positionId);
```

> There is **no** `SearchInventoryAsync`, `CreateInventoryAsync`, `UpdateInventoryLocationAsync`, or `RemoveInventoryAsync`. Updates go through `UpdateInventoryAsync` with a list of `UpdateInventoryRequestItem`.

## Labware Types (`client.Labware`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetLabwareTypesAsync` | `()` | `IList<LabwareTypeResponse>` |
| `GetLabwareTypeAsync` | `(string id)` | `LabwareTypeResponse` |
| `CreateLabwareTypesAsync` | `(IList<LabwareTypeRequest> request)` | `IList<LabwareTypeResponse>` |
| `UpdateLabwareTypeAsync` | `(string id, LabwareTypeRequest request)` | `LabwareTypeResponse` |
| `DeleteLabwareTypeAsync` | `(string id)` | `Task` |

> The list method is plural: `GetLabwareTypesAsync()`.

## Users (`client.Users`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetUsersAsync` | `()` | `IList<UserResponse>` |
| `GetUserByIdAsync` | `(int id)` | `UserResponse` |
| `CreateUserAsync` | `(CreateUserRequest request)` | `UserResponse` |
| `UpdateUserAsync` | `(int id, UpdateUserDetails details)` | `UserResponse` |
| `DeleteUserAsync` | `(int id)` | `Task` |
| `GetUserRoleAsync` | `(int id)` | `RoleResponse` |
| `GetUserActionsAsync` | `()` | `IList<UserActionResponse>` |
| `GetRolesAsync` | `()` | `IList<RoleResponse>` |
| `GetRoleByIdAsync` | `(int id)` | `RoleResponse` |
| `CreateRoleAsync` | `(CreateRoleRequest request)` | `RoleResponse` |
| `UpdateRoleAsync` | `(int id, UpdateRoleDetails details)` | `RoleResponse` |
| `DeleteRoleAsync` | `(int id)` | `Task` |

```csharp
var users = await client.Users.GetUsersAsync();

await client.Users.CreateUserAsync(new CreateUserRequest
{
    UserName = "newuser",
    FirstName = "John",
    LastName = "Doe",
    Email = "user@company.com",
    UserRoleId = roleId,
    Password = "..."
});
```

> There is **no** `GetCurrentUserAsync`. `UserResponse` exposes `UserName` (capital N) and `UserRoleId` (a single role id, not a `Roles` collection).

## Settings (`client.Settings`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetSystemSettingsAsync` | `(string settingName, string systemName)` | `IList<SystemSettingResponse>` |
| `GetSystemSettingAsync` | `(string id)` | `SystemSettingResponse` |
| `CreateSystemSettingAsync` | `(SystemSettingRequest request)` | `SystemSettingResponse` |
| `UpdateSystemSettingAsync` | `(string id, SystemSettingRequest request)` | `SystemSettingResponse` |
| `DeleteSystemSettingAsync` | `(string id)` | `Task` |

```csharp
var settings = await client.Settings.GetSystemSettingsAsync();
var setting = await client.Settings.GetSystemSettingAsync(settingId);
await client.Settings.UpdateSystemSettingAsync(settingId, new SystemSettingRequest { /* ... */ });
```

> Methods are `*SystemSetting(s)*`, not `GetSettingsAsync` / `UpdateSettingAsync`.

## Scripts (`client.Scripts`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetScriptsAsync` | `(string groupName)` | `IList<ScriptResponse>` |
| `GetScriptAsync` | `(int id)` | `ScriptResponse` |
| `CreateScriptAsync` | `(ScriptRequest request)` | `ScriptResponse` |
| `DeleteScriptAsync` | `(int id)` | `Task` |

> There is **no** `ExecuteScriptAsync`. Scripts run as part of protocol steps / events.

## Global Event Scripts (`client.GlobalEventScripts`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetGlobalEventScriptsAsync` | `()` | `IList<GlobalEventScriptResponse>` |
| `GetGlobalEventScriptAsync` | `(int id)` | `GlobalEventScriptResponse` |
| `CreateGlobalEventScriptAsync` | `(GlobalEventScriptRequest request)` | `GlobalEventScriptResponse` |
| `UpdateGlobalEventScriptAsync` | `(string id, GlobalEventScriptRequest request)` | `GlobalEventScriptResponse` |
| `DeleteGlobalEventScriptAsync` | `(int id)` | `Task` |

## Templates (`client.Templates`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetOrderTemplatesAsync` | `(int? protocolId)` | `IList<OrderTemplateResponse>` |

> To create an order from a template, call `client.Orders.CreateOrderAsync` with the `Template` (`TemplateSetRequest`) property set. There is **no** `CreateOrderFromTemplateAsync`.
>
> Simulation and data-package methods (`QueueSimulationAsync`, `GetSimulationResultAsync`, `GetSimulationResultsAsync`, `CancelSimulationAsync`, `CreateDataPackageAsync`) live on `client.Orders`, **not** `client.Templates`.

## Events (`client.Events`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetEventsAsync` | `(DateTimeOffset? startDate, DateTimeOffset? endDate, EventType? type, string state, int? protocolId, int? orderId, ... many optional filters ..., int? page, int? pageSize)` | `IList<BaseEvent>` |
| `CreateGenericEventAsync` | `(object document)` | `Task` |
| `ReleaseEventsAsync` | `(int eventId)` | `EmptyResponse` |

```csharp
var events = await client.Events.GetEventsAsync(
    startDate: DateTimeOffset.UtcNow.AddHours(-1),
    endDate: DateTimeOffset.UtcNow,
    type: EventType.Order);

// Events for a specific order
var orderEvents = await client.Events.GetEventsAsync(orderId: orderId);
```

> The filter parameters are `startDate` / `endDate` / `type` (an `EventType?`), not `startTime` / `endTime` / `eventType`. `GetEventsAsync` accepts many additional optional filters (barcode, operation, resources, plate protocol, paging, etc.).

### `EventType` enum

`All`, `Generic`, `System`, `Order`, `Operation`, `Driver`, `Data`, `Inventory`, `Audit`, `Deadlock`.

## Well Transfers (`client.WellTransfers`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `CreateWellTransfersAsync` | `(IList<WellTransferRequest> request)` | `IList<WellTransferResponse>` |
| `GetWellTransfersAsync` | `(IList<int> orderIds, IList<int> orderSampleIds, WellTransferFilter? type)` | `IList<WellTransferResponse>` |
| `UpdateWellTransferAsync` | `(int wellTransferId, WellTransferUpdate details)` | `WellTransferResponse` |

## Dispense Maps (`client.DispenseMaps`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `CreateDispenseMapsAsync` | `(IList<DispenseMapRequest> request)` | `IList<DispenseMapResponse>` |
| `GetDispenseMapsAsync` | `(IList<int> orderIds, IList<int> orderSampleIds)` | `IList<DispenseMapResponse>` |

## Subscribers (`client.Subscribers`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetSubscribersAsync` | `()` | `IList<SubscriberResponse>` |
| `GetSubscriberAsync` | `(int id)` | `SubscriberResponse` |
| `CreateSubscriberAsync` | `(SubscriberRequest request)` | `SubscriberResponse` |
| `DeleteSubscriberAsync` | `(int id)` | `Task` |

## General (`client.General`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetFilesAsync` | `(string directory)` | `IList<string>` |
| `GetFileAsync` | `(string file)` | `FileResponse` |
| `GetPeripheralsAsync` | `(string name, string type, bool? value, string location)` | `IList<PeripheralResponse>` |

> There is **no** `GetHealthAsync` and **no** `CreateDataPackageAsync` here. Data packages are created via `client.Orders.CreateDataPackageAsync(orderId)`.

## Version (`client.Version`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `GetVersionAsync` | `()` | `ServiceVersion` |

```csharp
ServiceVersion version = await client.Version.GetVersionAsync();
Console.WriteLine($"API {version.ApiVersion}, Cellario {version.CellarioVersion}, assembly {version.AssemblyVersion}");
```

> `ServiceVersion` exposes `ApiVersion`, `AssemblyVersion`, `CellarioVersion`, and `HookVersions` (`IList<HookVersion>`). There is **no** single `Version` property.

## Login (`client.Login`)

| Method | Signature | Returns |
| ------ | --------- | ------- |
| `LoginAsync` | `(LoginRequest request)` | `LoginResponse` |

In most cases you do not call this directly - use `AuthenticateBearer` / `AuthenticateBearerSync` on the client, which call `LoginAsync` and set the `Authorization` header for you. `LoginResponse` exposes `Access_token`, `Token_type`, `Expires_in`, `Refresh_token`, and `Scope`.

## Error Handling

All sub-clients throw `ApiException` for non-success HTTP responses. `ApiException` exposes `int StatusCode` and `string Response` (the raw response body), plus a `Headers` dictionary.

```csharp
try
{
    var protocol = await client.Protocols.GetProtocolByIdAsync(invalidId);
}
catch (ApiException ex)
{
    switch (ex.StatusCode)
    {
        case 400:
            Console.WriteLine($"Bad Request: {ex.Response}");
            break;
        case 401:
            Console.WriteLine("Unauthorized - check authentication");
            break;
        case 403:
            Console.WriteLine("Forbidden - insufficient permissions");
            break;
        case 404:
            Console.WriteLine("Not Found - resource does not exist");
            break;
        case 500:
            Console.WriteLine($"Server Error: {ex.Response}");
            break;
    }
}
```

## Async and Sync Patterns

Every operation has an async and a synchronous form, and the async form accepts a cancellation token:

```csharp
// Async (recommended)
var protocols = await client.Protocols.GetProtocolsAsync();

// Synchronous twin (drop the "Async" suffix)
var protocolsSync = client.Protocols.GetProtocols();

// Async with cancellation
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var withTimeout = await client.Protocols.GetProtocolsAsync(cancellationToken: cts.Token);
```

## Next Steps

- [See Integration Examples →](04-examples-and-best-practices.md)
- [Troubleshooting Guide →](05-troubleshooting.md)
