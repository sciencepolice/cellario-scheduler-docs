# Events Examples

Practical examples for receiving Cellario events. Cellario exposes two delivery mechanisms:

1. **Webhook subscribers** — Cellario POSTs each event to an HTTP URL you host. This is the primary, configurable integration path and supports rich filtering. Managed via the `/subscribers` endpoints.
2. **SignalR hub** — a real-time WebSocket hub at `/hubs/cellario` that broadcasts a fixed set of order/system notifications to every connected client. This is what the Cellario web UI uses to stay live.

Use webhooks for external system integration; use SignalR for live, in-browser UI updates.

All REST examples use the base URL `http://localhost:8444`. Note there is **no `/api` prefix** on these routes.

## Prerequisites

- An authenticated API session and a bearer token (see [Basic Operations](basic-operations.md)).
- For webhooks: a publicly reachable HTTP endpoint that accepts `POST` and returns a `2xx`.
- See the [Events API reference](../events.md) for the full event-type catalog and payload schemas, and the [C# client primer](../../Cellario_Client_Event_Subscription_Primer.md) for SDK usage.

---

## Webhook Subscribers

### The subscriber model

A subscriber is created with these fields (camelCase JSON):

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user` | string | Yes | A friendly name for the subscription. |
| `url` | string | Yes | The URL Cellario will POST each matching event to. Must be reachable from the Cellario server. |
| `invertFilterBehavior` | bool | No | If `true`, you receive every event that does **not** match the filters. Default `false`. |
| `filters` | array | No | Filter objects describing the events you want. Omit/empty to receive all events. |
| `customHeaders` | array | No | Extra headers added to every outbound notification (e.g. an auth token your endpoint validates). |

Each item in `filters` is a `FilterItem`:

| Field | Type | Required | Description | Examples |
|-------|------|----------|-------------|----------|
| `filterType` | EventType | **Yes** | The event type to match. | `Order`, `Operation`, `Driver`, `System`, `Data`, `Inventory`, `Audit`, `Generic`, `Deadlock`, `All` |
| `state` | string | No | A specific state within that type. | `Started`, `Finished`, `Failed`, `InError` |
| `operation` | string | No | A specific operation name. | `LiquidTransfer`, `Move`, `Read` |
| `context` | string | No | The context in which the event occurs. | `Order`, `Operation`, `Protocol` |

Each item in `customHeaders` is a `Parameter` (`name`, `value`, optional `description`).

**Matching semantics**

- No filters → you receive every event.
- A filter with only `filterType` → all events of that type.
- Adding `state` / `operation` / `context` narrows within that type.
- Multiple filters are OR'd together — an event is delivered if it matches **any** filter.
- `invertFilterBehavior: true` flips the logic to "everything except the filters".

> Webhook filtering is by `filterType` (an `EventType` value) plus optional `state`. There are no composite event-name strings like `OrderStatusChanged` — those do not exist.

### Create a subscriber

`POST /subscribers`

```bash
curl -X POST http://localhost:8444/subscribers \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "user": "LIMS Integration",
    "url": "https://your-server.com/webhooks/cellario",
    "invertFilterBehavior": false,
    "filters": [
      { "filterType": "Order", "state": "Finished" },
      { "filterType": "Order", "state": "Canceled" },
      { "filterType": "System", "state": "InError" }
    ],
    "customHeaders": [
      { "name": "X-Api-Key", "value": "super-secret-value" }
    ]
  }'
```

**Response** (`201 Created`):

```json
{
  "id": 7,
  "user": "LIMS Integration",
  "url": "https://your-server.com/webhooks/cellario",
  "invertFilterBehavior": false,
  "filters": [
    { "filterType": "Order", "state": "Finished", "operation": "", "context": "" },
    { "filterType": "Order", "state": "Canceled", "operation": "", "context": "" },
    { "filterType": "System", "state": "InError", "operation": "", "context": "" }
  ],
  "customHeaders": [
    { "name": "X-Api-Key", "value": "super-secret-value", "description": null }
  ]
}
```

The `id` is an integer, supplied on creation. Use it to fetch or delete the subscription.

#### PowerShell

```powershell
$headers = @{
    Authorization  = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    user                 = "LIMS Integration"
    url                  = "https://your-server.com/webhooks/cellario"
    invertFilterBehavior = $false
    filters              = @(
        @{ filterType = "Order";  state = "Finished" }
        @{ filterType = "System"; state = "InError"  }
    )
    customHeaders        = @(
        @{ name = "X-Api-Key"; value = "super-secret-value" }
    )
} | ConvertTo-Json -Depth 5

$subscription = Invoke-RestMethod -Uri "http://localhost:8444/subscribers" `
    -Method POST -Headers $headers -Body $body

Write-Host "Created subscription with ID: $($subscription.id)"
```

### List subscribers

`GET /subscribers`

```bash
curl -X GET http://localhost:8444/subscribers \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Returns an array of `SubscriberResponse` objects (the create-response shape above).

### Get one subscriber

`GET /subscribers/{id}`

```bash
curl -X GET http://localhost:8444/subscribers/7 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Delete a subscriber

`DELETE /subscribers/{id}`

```bash
curl -X DELETE http://localhost:8444/subscribers/7 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

> There is **no update (PUT/PATCH) endpoint** for subscribers. To change a subscription's URL or filters, delete it and create a new one.

---

## Receiving Webhook Events

Cellario sends **one event per `POST`** (not a batch), with `Content-Type: application/json`, any `customHeaders` you registered, and a ~60-second timeout. Return a `2xx` once you've durably accepted the event. Non-`2xx`/timeout marks the event for retry, so make your handler **idempotent** — deduplicate on the event `id`.

Every event shares the `BaseEvent` fields (`id`, `date`, `description`, `version`, `systemName`, `systemAssetId`, `eventType`, `additionalInfo`) and adds type-specific fields. The `eventType` field is the discriminator. See [events.md](../events.md) for every payload.

### Example payloads

These mirror the real event DTOs. Order events carry the order identifiers at the **top level** (`orderId`, `state` — note the field is `state`, not `status`).

**Order finished** (`eventType: "Order"`):

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

**System in error** (`eventType: "System"`):

```json
{
  "name": "system",
  "state": "InError",
  "id": 2051,
  "date": "2026-03-26T18:36:41.737549+00:00",
  "description": "",
  "version": "3.0",
  "systemName": "system",
  "systemAssetId": "36ed6f74-ab1b-4493-b5fa-5ab21f5b372f",
  "eventType": "System",
  "additionalInfo": null
}
```

**Driver/device ready** (`eventType: "Driver"`):

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

(Operation, Data, Inventory, and Audit payloads are documented in [events.md](../events.md).)

### Webhook endpoint — ASP.NET Core

Switch on the `eventType` discriminator. For strongly-typed deserialization with the Cellario SDK's polymorphic converter, see the [C# client primer](../../Cellario_Client_Event_Subscription_Primer.md).

```csharp
[ApiController]
[Route("webhooks")]
public class CellarioWebhookController : ControllerBase
{
    private readonly ILogger<CellarioWebhookController> _logger;

    public CellarioWebhookController(ILogger<CellarioWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("cellario")]
    public IActionResult HandleEvent([FromBody] JsonElement evt)
    {
        if (Request.Headers["X-Api-Key"] != "super-secret-value")
        {
            return Unauthorized();
        }

        var eventType = evt.GetProperty("eventType").GetString();
        switch (eventType)
        {
            case "Order":
                var orderId = evt.GetProperty("orderId").GetInt32();
                var state = evt.GetProperty("state").GetString();
                _logger.LogInformation("Order {OrderId} -> {State}", orderId, state);
                break;

            case "System":
                _logger.LogInformation("System state -> {State}",
                    evt.GetProperty("state").GetString());
                break;

            default:
                _logger.LogWarning("Unhandled event type: {EventType}", eventType);
                break;
        }

        return Ok();
    }
}
```

### Webhook endpoint — Python Flask

```python
from flask import Flask, request, jsonify
import logging

app = Flask(__name__)
logger = logging.getLogger(__name__)

API_KEY = "super-secret-value"

@app.route('/webhooks/cellario', methods=['POST'])
def handle_cellario_event():
    if request.headers.get('X-Api-Key') != API_KEY:
        return jsonify({'error': 'unauthorized'}), 401

    event = request.get_json()
    event_type = event.get('eventType')

    if event_type == 'Order':
        logger.info("Order %s -> %s", event.get('orderId'), event.get('state'))
    elif event_type == 'System':
        logger.info("System state -> %s", event.get('state'))
    elif event_type == 'Driver':
        logger.info("Device %s -> %s", event.get('resourceName'), event.get('state'))
    else:
        logger.warning("Unhandled event type: %s", event_type)

    # Return 2xx only once the event is durably accepted; retries can redeliver.
    return jsonify({'status': 'ok'}), 200

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
```

---

## SignalR Hub (real-time, in-browser)

The hub lives at `/hubs/cellario`. It is **broadcast-only**: the server pushes notifications to all connected clients. There are **no client-invokable methods** — you do not call `invoke(...)` to subscribe; you simply connect and register handlers for the server-to-client methods below.

### Server-to-client methods

| Method | Message shape (camelCase over the wire) |
|--------|------------------------------------------|
| `OrderEvent` | `{ orderId, state }` where `state` is `Created` \| `Updated` \| `Deleted` |
| `OrderFailedEvent` | `{ orderId, errorMessage }` |
| `OrderProgressEvent` | `{ orderId, percentComplete }` |
| `OrderSimulatingEvent` | `{ orderId, isSimulating, estimatedDurationSeconds }` |
| `SimulationProgressEvent` | `{ requestId, orderId, status, progressPercent }` |
| `SystemStateEvent` | `{ systemName }` |

> Note these hub messages are lightweight notifications (mostly just IDs and state), distinct from the full webhook event DTOs. They tell the UI *what changed* so it can re-fetch.

### JavaScript / TypeScript client

```javascript
import { HubConnectionBuilder, HttpTransportType } from "@microsoft/signalr";

const connection = new HubConnectionBuilder()
  .withUrl("http://localhost:8444/hubs/cellario", {
    skipNegotiation: true,
    transport: HttpTransportType.WebSockets,
  })
  .withAutomaticReconnect()
  .build();

connection.on("OrderEvent", (message) => {
  // message: { orderId, state }  (state: Created | Updated | Deleted)
  console.log(`Order ${message.orderId} -> ${message.state}`);
});

connection.on("OrderFailedEvent", (message) => {
  // message: { orderId, errorMessage }
  console.error(`Order ${message.orderId} failed: ${message.errorMessage}`);
});

connection.on("OrderProgressEvent", (message) => {
  // message: { orderId, percentComplete }
  console.log(`Order ${message.orderId}: ${message.percentComplete}%`);
});

connection.on("OrderSimulatingEvent", (message) => {
  // message: { orderId, isSimulating, estimatedDurationSeconds }
  console.log(`Order ${message.orderId} simulating: ${message.isSimulating}`);
});

connection.on("SimulationProgressEvent", (message) => {
  // message: { requestId, orderId, status, progressPercent }
  console.log(`Simulation ${message.orderId}: ${message.progressPercent}%`);
});

connection.on("SystemStateEvent", (message) => {
  // message: { systemName } — re-fetch system state on receipt
  console.log(`System state changed: ${message.systemName}`);
});

connection
  .start()
  .then(() => console.log("Connected to Cellario hub"))
  .catch((err) => console.error("Hub connection failed:", err));
```

---

## Best Practices

1. **Webhook security**: prefer HTTPS URLs and validate a shared-secret `customHeaders` value on each request.
2. **Idempotency**: deduplicate on the event `id`; retries can redeliver an event.
3. **Respond quickly**: the webhook timeout is ~60 seconds. Acknowledge with a `2xx`, then process out of band.
4. **Filter narrowly**: subscribe only to the `filterType`/`state` combinations you need to reduce noise.
5. **No update endpoint**: to change a subscription, delete it and create a new one.

## Troubleshooting

- **No webhook events arriving**: verify the `filters` match the events you expect, that your `url` is reachable from the Cellario server, and that you return a `2xx`. Check `GET /subscribers/{id}` to confirm the stored filters.
- **401 from your endpoint loop**: confirm the `customHeaders` you registered match what your handler validates.
- **SignalR won't connect**: confirm the path is `/hubs/cellario` and that WebSocket transport is permitted by any proxy in between. Do not attempt to `invoke` a subscribe method — the hub has none.

## Related Documentation

- [Events API reference](../events.md) — full event-type catalog and payload schemas.
- [Cellario.Client Event Subscription Primer](../../Cellario_Client_Event_Subscription_Primer.md) — strongly-typed C# SDK usage and polymorphic deserialization.
- [Integration Examples](integration-examples.md) — complete workflow examples.
