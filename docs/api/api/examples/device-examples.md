# Device Examples

Examples for controlling and monitoring laboratory devices (resources) through the Cellario API.

## Overview

In Cellario, a physical instrument is represented as a **resource**. Device control - connecting, initializing, enabling, simulating, aborting, and executing operations - is performed against a resource by name via the `/resources/{resourceName}` endpoint. Device state and details are read from the resource endpoints.

For the full resource response shape and listing/filtering, see [Resource Examples](resource-examples.md). For driver details (supported operations, connection parameters), see [Driver Examples](driver-examples.md).

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- Appropriate resource permissions (see per-action notes below)

## Issue a Device Action

### Endpoint

```
PUT /resources/{resourceName}
```

Issues an action to the device associated with the named resource. The endpoint returns `202 Accepted` with an empty body once the action has been queued; it does not return an operation result.

> Note: For executing operations this endpoint behaves like `POST /operations` (Queue Operation) but does not return the request ID. The Queue Operation endpoint is recommended when you need to track the operation result.

### Available Actions

The request body is a `DeviceActionSet`. The `action` field accepts:

- `Connect` - Connect to the device
- `Initialize` - Initialize the device
- `ExecuteOperation` - Run a named operation (provide `operation` and `parameters`)
- `Abort` - Abort the current device activity
- `Enable` - Enable the resource for automation
- `Disable` - Disable the resource
- `Simulate` - Put the resource into simulation
- `Unsimulate` - Take the resource out of simulation
- `None` - No action

### Required Permissions

Permissions are checked per action (any one of the following grants access to the matching actions):

- `ResourceDriver` - `Connect`, `Initialize`, `ExecuteOperation`, `Abort`
- `ResourceSimulate` - `Simulate`, `Unsimulate`
- `ResourceEnable` - `Enable`, `Disable`

### Request Structure

```json
{
  "action": "ExecuteOperation",
  "operation": "Aspirate",
  "parameters": [
    { "name": "Volume", "value": "100" }
  ],
  "dependentResources": []
}
```

`operation` and `parameters` are only required for `ExecuteOperation`. For simple actions like `Enable`, only `action` is needed.

### Connect / Initialize a Device

```bash
curl -X PUT "http://localhost:8444/resources/Liquid%20Handler%201" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "action": "Connect" }'
```

### Enable a Device (PowerShell)

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$body = @{ action = "Enable" } | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8444/resources/Liquid Handler 1" `
    -Method PUT -Headers $headers -Body $body

Write-Host "Enable action accepted (202)"
```

### Execute an Operation (Python)

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

action = {
    "action": "ExecuteOperation",
    "operation": "Aspirate",
    "parameters": [
        {"name": "Volume", "value": "100"}
    ]
}

response = requests.put(
    'http://localhost:8444/resources/Liquid Handler 1',
    headers=headers,
    json=action
)

# 202 Accepted with an empty body
print(f"Status: {response.status_code}")
```

### Simulate / Unsimulate a Device

```bash
curl -X PUT "http://localhost:8444/resources/Liquid%20Handler%201" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "action": "Simulate" }'
```

## Monitor Device State

Device state is exposed on the resource. Fetch a single resource to read its current state:

```
GET /resources/{resourceName}
```

```powershell
$headers = @{ Authorization = "Bearer $token" }
$resource = Invoke-RestMethod -Uri "http://localhost:8444/resources/Liquid Handler 1" -Headers $headers

Write-Host "Resource: $($resource.name)"
Write-Host "  State: $($resource.state)"
Write-Host "  Enabled: $($resource.isEnabled), Simulated: $($resource.isSimulated)"
if ($resource.errorMessage) {
    Write-Host "  Error: $($resource.errorMessage)"
}
```

The `state` field reflects the current device state and `errorMessage` is populated when the device is in an error state. See [Resource Examples](resource-examples.md) for the full response shape.

## Scan a Resource (Deprecated)

A synchronous resource scan can be issued via the deprecated scan endpoint. Prefer the asynchronous [inventory scan endpoints](inventory-examples.md) for new integrations.

### Endpoint

```
POST /service/resources?resourceName={resourceName}
```

`resourceName` is supplied as a query parameter; the scan details are supplied in the body. This call blocks until the scan finishes. `PersistInventory` and `OrderId` in the body are ignored by this endpoint.

### Request

```bash
curl -X POST "http://localhost:8444/service/resources?resourceName=Storage%201" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "requestType": "Full" }'
```

For a partial scan, set `requestType` to `Partial` and include `scanPositions`:

```json
{
  "requestType": "Partial",
  "scanPositions": [
    { "col": 1, "row": 1 }
  ]
}
```

### Response

```json
{
  "stateTransitions": [],
  "dataProduced": [],
  "logsProduced": [],
  "errorMessage": null
}
```

## Error Scenarios

These endpoints use the standard FastEndpoints error shapes.

### Validation Error

An invalid request (for example an unknown action) returns `400 Bad Request` with an errors dictionary:

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "generalErrors": ["Invalid device action"]
  }
}
```

### Resource Not Found

```json
{
  "message": "Resource Liquid Handler 1 not found"
}
```

### Permission Denied

Issuing an action without the required permission returns `403 Forbidden` (no body).

## Best Practices

1. **Use the right permission**: Each action requires a specific permission (`ResourceDriver`, `ResourceSimulate`, or `ResourceEnable`).
2. **Treat the action as fire-and-forget**: `PUT /resources/{resourceName}` returns `202 Accepted` with no body. Re-read the resource (or use [Operations](operations-examples.md)) to observe results.
3. **Prefer Queue Operation for tracked work**: Use `POST /operations` when you need the request ID to follow up on an operation.
4. **Check state after acting**: Poll `GET /resources/{resourceName}` to confirm the device reached the expected state.

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Resource Examples](resource-examples.md) - List and inspect resources
- [Driver Examples](driver-examples.md) - Driver details and supported operations
- [Operations Examples](operations-examples.md) - Queue and track device operations
- [Inventory Examples](inventory-examples.md) - Inventory scans
