# Events API

The Cellario Scheduler provides a comprehensive event system for external system integration. Events are generated throughout the system lifecycle and can be consumed via webhook subscriptions to notify external systems of laboratory automation state changes.

## Event Types

The system supports nine concrete, emitted event types, each with specific payloads and use cases. The `EventType` enum also defines an `All` value, which is a filter wildcard used by subscriptions (see [Webhook Filtering](#webhook-filtering)) rather than a distinct emitted event.

### System Events (`EventType.System`)

Represent changes in the overall system state.

**States**: `Initialized`, `Running`, `Paused`, `UnInitialized`, `Initializing`, `Pausing`, `InError`

```json
{
  "name": "system",
  "state": "Initialized",
  "id": 2051,
  "date": "2026-03-26T18:36:41.737549+00:00",
  "description": "ON",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "System",
  "additionalInfo": null
}
```

### Order Events (`EventType.Order`)

Track the lifecycle of laboratory orders from creation to completion.

**States**: `Created`, `Submitted`, `Started`, `Finished`, `Removed`, `Pausing`, `Paused`, `Scanning`, `Scripting`, `Starting`, `Canceled`

```json
{
  "orderId": 285,
  "orderAssetId": "9de49e1d-ffd1-4c64-8f6c-2a7f49eed829",
  "state": "Finished",
  "protocolId": 37,
  "protocolName": "NGS Prep 1 - PostAmp 1 - PCR1 Clean-Up",
  "protocolAssetId": "b66f2dee-d194-4f3d-81a4-5b2a5508f591",
  "id": 1621,
  "date": "2026-03-03T16:34:44.531938+00:00",
  "description": "",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "Order",
  "additionalInfo": null
}
```

### Operation Events (`EventType.Operation`)

Monitor individual operations within orders, providing detailed tracking of each step.

**States**: `None`, `Started`, `Finished`, `Accept`, `Repeat`, `Failed`

```json
{
  "name": "Load",
  "barcode": "96 PCR 1 Source Plate_5",
  "labwareType": "Generic 384",
  "labwareTypeAssetId": "becf599e-6179-4be9-bd1c-54fd40982870",
  "plateProtocolName": "96 PCR 1 Source Plate",
  "state": "Finished",
  "currentResource": "AmbiStore 1 Nest 1",
  "currentResourceAssetId": "7eb3564e-0aae-4f13-92cd-89f3f3da56ba",
  "destinationResource": "AmbiStore 1",
  "destinationResourceAssetId": "fbbd745f-950e-44d6-9383-b7137d565d63",
  "operationResource": "AmbiStore 1",
  "operationResourceAssetId": "fbbd745f-950e-44d6-9383-b7137d565d63",
  "plateProtocolId": 406,
  "sampleOperationId": 16411,
  "orderSampleId": 5336,
  "orderId": 285,
  "orderAssetId": "9de49e1d-ffd1-4c64-8f6c-2a7f49eed829",
  "operationResourceType": "HRB.HighResStore",
  "protocolStepId": 4650,
  "notes": "",
  "parameters": [
    {
      "name": "Column",
      "value": "16",
      "description": null
    },
    {
      "name": "NestId",
      "value": "1",
      "description": null
    },
    {
      "name": "OrderId",
      "value": "285",
      "description": null
    },
    {
      "name": "Plate Number",
      "value": "5",
      "description": null
    },
    {
      "name": "Plate Type",
      "value": "Generic 384",
      "description": null
    },
    {
      "name": "Plates",
      "value": "1,96 PCR 1 Source Plate_5,Generic 384",
      "description": null
    }
  ],
  "isManualTask": false,
  "isDeviceSimulated": true,
  "id": 1616,
  "date": "2026-03-03T16:34:43.767142+00:00",
  "description": "",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "Operation",
  "additionalInfo": null
}
```

### Driver Events (`EventType.Driver`)

Device driver status and communication events.

**States**: `Unavailable`, `Error`, `Idle`, `Connecting`, `Connected`, `Initializing`, `Ready`, `DoingOp`

```json
{
  "state": "Ready",
  "errorMessage": "",
  "resourceName": "Washer-Dispenser 6",
  "resourceAssetId": "b6f5bb02-cdd1-4264-aabe-eb833609f8bf",
  "isDeviceSimulated": true,
  "id": 2050,
  "date": "2026-03-26T18:36:41.513948+00:00",
  "description": null,
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "Driver",
  "additionalInfo": null
}
```

### Data Events (`EventType.Data`)

Capture and track data generated during protocol execution.

**Contexts**: `Order`, `Operation`

```json
{
  "name": "DataPackage",
  "context": "Order",
  "files": [
    {
      "path": "C:\\SystemData\\Cellario\\DataPackages\\2026.03.03.1634.285.zip",
      "checkSum": {
        "name": "md5",
        "format": "hexadecimal",
        "value": "f83cf6299cb6e90618f8bef87024dbae"
      }
    }
  ],
  "data": null,
  "order": {
    "id": 285,
    "orderAssetId": "9de49e1d-ffd1-4c64-8f6c-2a7f49eed829",
    "protocolId": 37,
    "protocolAssetId": "b66f2dee-d194-4f3d-81a4-5b2a5508f591",
    "protocolVersion": "1",
    "startTime": "2026-03-02T18:05:22",
    "startTimeUtc": "1970-01-01T00:00:00Z",
    "endTime": "0001-01-01T00:00:00",
    "endTimeUtc": "1970-01-01T00:00:00Z",
    "parameters": [],
    "properties": []
  },
  "operation": null,
  "id": 1623,
  "date": "2026-03-03T16:34:44.897417+00:00",
  "description": "",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "Data",
  "additionalInfo": null
}
```

### Inventory Events (`EventType.Inventory`)

Track labware movement and inventory changes.

_Example payload structure coming soon._

### Audit Events (`EventType.Audit`)

Compliance and audit trail events for regulatory requirements.

**Audit Contexts**: `Protocol`, `Order`, `Resource`, `Setting`, `Labware`
**States**: `Create`, `Read`, `Update`, `Delete`

```json
{
  "auditContext": "Order",
  "state": "Create",
  "metadata": {
    "orderId": 291,
    "orderAssetId": "e0428a87-e736-408b-8554-18f13f54b3bd",
    "protocolId": 95,
    "protocolName": "Looping and Protocol Parameters - Monitored Cell Growth",
    "protocolAssetId": "72253aa3-cb41-4859-993e-78f8fc9058bb"
  },
  "id": 1771,
  "date": "2026-03-23T22:46:24.800969+00:00",
  "description": "Order Audit Event",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "Audit",
  "additionalInfo": null
}
```

### Generic Events (`EventType.Generic`)

General-purpose events for custom integrations.

_Example payload structure coming soon._

### Deadlock Events (`EventType.Deadlock`)

System deadlock detection and resolution events.

_Example payload structure coming soon._

## Webhook Subscriptions

### Creating Subscriptions

Create webhook subscriptions to receive events at external HTTP endpoints:

```http
POST /subscribers
Content-Type: application/json

{
  "user": "External System Integration",
  "url": "https://your-server.com/webhooks/cellario",
  "invertFilterBehavior": false,
  "filters": [
    {
      "filterType": "Order",
      "state": "Finished"
    },
    {
      "filterType": "System",
      "state": "InError"
    }
  ],
  "customHeaders": [
    {
      "name": "Authorization",
      "value": "Bearer your-webhook-token"
    },
    {
      "name": "X-Source-System",
      "value": "Cellario"
    }
  ]
}
```

**Response** (201 Created):

```json
{
  "id": 42,
  "user": "External System Integration",
  "url": "https://your-server.com/webhooks/cellario",
  "invertFilterBehavior": false,
  "filters": [
    {
      "filterType": "Order",
      "state": "Finished"
    },
    {
      "filterType": "System",
      "state": "InError"
    }
  ],
  "customHeaders": [
    {
      "name": "Authorization",
      "value": "Bearer your-webhook-token"
    },
    {
      "name": "X-Source-System",
      "value": "Cellario"
    }
  ]
}
```

### Webhook Filtering

Subscribers support flexible filtering to receive only relevant events:

#### Event Type Filtering

Filter by specific event types:

- `System`, `Order`, `Operation`, `Driver`, `Data`, `Inventory`, `Audit`, `Generic`, `Deadlock`

#### State Filtering

Filter by event states (state values vary by event type):

- **System**: `Running`, `Paused`, `InError`, etc.
- **Order**: `Started`, `Finished`, `Submitted`, `Paused`, `Canceled`, etc.
- **Operation**: `Started`, `Finished`, `Failed`, etc.

#### Inverted Filtering

Set `invertFilterBehavior: true` to receive all events _except_ those matching the filters.

#### Multiple Filters

Multiple filters work as OR conditions - events matching any filter will be sent.

### Webhook Delivery

When events match subscriber filters:

1. **HTTP POST** request sent to the subscriber URL
2. **Custom headers** included as configured
3. **JSON payload** contains the full event object
4. **Retry logic** for failed deliveries
5. **Delivery status** tracked in the system

### Webhook Payload Example

```json
{
  "id": 125,
  "date": "2024-01-15T10:40:00Z",
  "eventType": "Operation",
  "description": "PCR operation finished",
  "systemName": "CellarioSystem-01",
  "systemAssetId": "SYS-001",
  "sampleOperationId": 5001,
  "name": "PCR Amplification",
  "state": "Finished",
  "orderId": 1001,
  "orderAssetId": "ORD-2024-001",
  "barcode": "PLATE001234",
  "currentResource": "PCR_Device_01"
}
```

### Managing Subscriptions

#### List Subscriptions

```http
GET /subscribers
```

#### Get Specific Subscription

```http
GET /subscribers/{id}
```

#### Delete Subscription

```http
DELETE /subscribers/{id}
```

## Event Generation Architecture

### Event Sources

Events are generated by several key system components:

#### SchedulerEventService

The primary event generator that monitors:

- Order state changes (submitted → started → finished)
- Operation progress and state transitions
- System state changes (startup, shutdown, errors)
- Device interactions and status updates

#### Protocol and Order Services

Generate audit events for:

- Protocol creation, modification, deletion
- Order creation, modification, execution control

#### Device Drivers

Generate driver events for:

- Device connections and disconnections
- Device status updates and errors
- Operation execution results

### Event Flow

1. **Event Generation**: Business logic generates ServiceEvent objects
2. **Event Channel**: Events queued in thread-safe channels for processing
3. **Event Storage**: Events persisted to ServiceEvents database table
4. **Webhook Delivery**: SubscriberEventPublisher sends to external webhooks
5. **Cloud Integration**: Optional publishing to cloud event systems

### Event Persistence

All events are stored in the `ServiceEvents` table with full audit trail:

- Event metadata and payload
- Creation timestamps and system information
- Related entity references (order, protocol, device)
- Event properties and results data
- Retry status for webhook deliveries

## Integration Examples

### REST API Webhook Management

```csharp
// Create webhook subscription
var subscriber = await client.Subscribers.CreateSubscriberAsync(new SubscriberRequest
{
    User = "LIMS Integration",
    Url = "https://lims.company.com/api/cellario-events",
    Filters = new[]
    {
        new FilterItem { FilterType = EventType.Order, State = "Finished" },
        new FilterItem { FilterType = EventType.Order, State = "Canceled" }
    },
    CustomHeaders = new[]
    {
        new Parameter { Name = "Authorization", Value = "Bearer lims-token" },
        new Parameter { Name = "X-Event-Source", Value = "Cellario" }
    }
});
```

### Webhook Endpoint Implementation

```csharp
[ApiController]
[Route("api/cellario-events")]
public class CellarioWebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleEvent([FromBody] BaseEvent cellarioEvent)
    {
        switch (cellarioEvent.EventType)
        {
            case EventType.Order when cellarioEvent is OrderEvent orderEvent:
                await ProcessOrderEvent(orderEvent);
                break;

            case EventType.Operation when cellarioEvent is OperationEvent opEvent:
                await ProcessOperationEvent(opEvent);
                break;

            case EventType.System when cellarioEvent is SystemEvent sysEvent:
                await ProcessSystemEvent(sysEvent);
                break;
        }

        return Ok();
    }
}
```

## Security Considerations

### Authentication

- Webhook subscriptions support custom authentication headers
- API endpoints protected by role-based access control

### Webhook Security

- Use HTTPS endpoints for webhook URLs
- Include authentication tokens in custom headers
- Validate webhook signatures if required
- Implement idempotency handling for duplicate events

### Event Filtering

- Filter events at the subscription level to minimize data exposure
- Use inverted filtering to exclude sensitive events
- Monitor subscription activity and delivery success rates

## Troubleshooting

### Common Issues

#### Missing Webhook Events

- Check subscriber filter configuration
- Verify webhook endpoint accessibility and response codes
- Review delivery retry logs in the system

#### Event Delivery Delays

- Monitor system performance during high event volumes
- Check database performance for event storage
- Review webhook endpoint response times

### Monitoring and Diagnostics

The system provides comprehensive monitoring:

- Event generation rates and volume metrics
- Webhook delivery success/failure rates
- Event storage performance and retention

## Related Documentation

- [Overview →](overview.md)
- [Authentication →](authentication.md)
- [Examples →](examples.md)
