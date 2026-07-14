# Driver Examples

Examples for monitoring laboratory drivers through the Cellario API.

## Overview

The Cellario API provides access to driver information including connection parameters, supported operations, and state via the `/drivers` endpoints. A driver is the software component associated with a resource that knows how to communicate with a physical (or simulated) device.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- Appropriate driver permissions

## List All Drivers

### Endpoint

```
GET /drivers
```

Returns an array of drivers ordered by name.

### Basic Request

```bash
curl -X GET http://localhost:8444/drivers \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$drivers = Invoke-RestMethod -Uri "http://localhost:8444/drivers" -Headers $headers

Write-Host "Found $($drivers.Count) drivers:"
foreach ($driver in $drivers) {
    Write-Host "  - $($driver.name) (Type: $($driver.resourceType), State: $($driver.state))"
}
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
response = requests.get('http://localhost:8444/drivers', headers=headers)
drivers = response.json()

print(f"Found {len(drivers)} drivers:")
for driver in drivers:
    print(f"  - {driver['name']} (Type: {driver.get('resourceType', 'Unknown')})")
```

## Get a Single Driver

### Endpoint

```
GET /drivers/{resourceName}
```

Returns a single driver for the named resource. Returns `404 Not Found` if no driver exists for that resource.

### Basic Request

```bash
curl -X GET "http://localhost:8444/drivers/Liquid%20Handler%201" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$resourceName = "Liquid Handler 1"
$driver = Invoke-RestMethod -Uri "http://localhost:8444/drivers/$resourceName" -Headers $headers

Write-Host "Driver: $($driver.name)"
Write-Host "  Assembly: $($driver.assemblyName)"
Write-Host "  Enabled: $($driver.enabled), Simulated: $($driver.simulated)"
Write-Host "  Operations: $($driver.operations.Count)"
```

## Driver Response Structure

A driver response (`DriverResponse`) includes:

```json
{
  "name": "Liquid Handler 1",
  "resourceType": "Liquid Handler",
  "assemblyName": "Cellario.Drivers.ExampleDriver",
  "version": "1.0.0",
  "connectionParameters": [
    { "name": "Port", "value": "COM3" }
  ],
  "operations": [
    {
      "name": "Aspirate",
      "parameters": [
        {
          "name": "Volume",
          "value": "100",
          "lookup": []
        }
      ]
    }
  ],
  "state": "Idle",
  "enabled": true,
  "simulated": false
}
```

Field notes:

- `name` - The resource name the driver is associated with
- `resourceType` - The resource type (e.g. Liquid Handler, Robot)
- `assemblyName` - The driver assembly that implements the device communication
- `version` - The driver version, if available
- `connectionParameters` - Connection settings used to reach the device
- `operations` - The operations the driver supports, each with its own parameters
- `state` - The current device state (see [Device Examples](device-examples.md))
- `enabled` - Whether the driver/resource is enabled
- `simulated` - Whether the driver is running in simulation

## Error Scenarios

### Driver Not Found

A request for a driver that does not exist returns `404 Not Found` with a body containing only a message:

```json
{
  "message": "Driver Liquid Handler 1 not found."
}
```

### Unauthorized Access

Requests without a valid bearer token return `401 Unauthorized` (no body).

## Best Practices

1. **Check Driver State**: Monitor the `state` field for device availability and errors.
2. **Inspect Supported Operations**: Use the `operations` array to discover the operations a device supports before queuing one.
3. **Handle Missing Drivers**: A resource may not have a driver (`hasDriver` is `false` on the resource); expect `404` from `GET /drivers/{resourceName}` in that case.

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Device Examples](device-examples.md) - Control devices and issue actions
- [Operations Examples](operations-examples.md) - Driver operations during execution
- [Events Examples](events-examples.md) - Driver status events
