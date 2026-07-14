# Resource Examples

Examples for managing laboratory resources through the Cellario API.

## Overview

Resources represent shared laboratory assets such as instruments, storage devices, and utilities that need to be managed and scheduled. The API provides access via the `/resources` endpoint.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- Appropriate resource access permissions

## List Resources

### Endpoint

```
GET /resources
```

### Query Parameters

- `includeDetails` (boolean, default: false) - Include ResourcePositions for storage resources
- `resourceType` (string, optional) - Filter by specific resource type
- `filterBySystem` (boolean, default: true) - Filter to current active system only

### Basic Request

```bash
curl -X GET "http://localhost:8444/resources" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$resources = Invoke-RestMethod -Uri "http://localhost:8444/resources" -Headers $headers

Write-Host "Found $($resources.Count) resources:"
foreach ($resource in $resources) {
    Write-Host "  - $($resource.name) (Type: $($resource.resourceType))"
}
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
response = requests.get('http://localhost:8444/resources', headers=headers)
resources = response.json()

print(f"Found {len(resources)} resources:")
for resource in resources:
    print(f"  - {resource['name']} (Type: {resource.get('resourceType', 'Unknown')})")
```

## Filter Resources by Type

### Filter by Resource Type

```bash
curl -X GET "http://localhost:8444/resources?resourceType=Storage" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Include Detailed Information

```bash
curl -X GET "http://localhost:8444/resources?includeDetails=true&resourceType=Storage" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Advanced Filtering

```powershell
$headers = @{ Authorization = "Bearer $token" }

# Get storage resources with position details
$storageResources = Invoke-RestMethod -Uri "http://localhost:8444/resources" -Headers $headers -Body @{
    resourceType = "Storage"
    includeDetails = $true
} -Method GET

Write-Host "Found $($storageResources.Count) storage resources:"
foreach ($resource in $storageResources) {
    Write-Host "Resource: $($resource.name)"
    Write-Host "  Type: $($resource.resourceType)"
    if ($resource.resourcePositions) {
        Write-Host "  Positions: $($resource.resourcePositions.Count)"
    }
    Write-Host ""
}
```

## Resource Response Structure

A resource response (`SystemResourceResponse`) includes:

```json
{
  "name": "Storage 1",
  "state": "Idle",
  "errorMessage": "",
  "resourceType": "Storage",
  "associatedResource": null,
  "associatedCart": null,
  "isEnabled": true,
  "hasDriver": true,
  "isSimulated": false,
  "systemName": "MySystem",
  "resourcePositions": [],
  "properties": [],
  "instance": 1,
  "location": 0,
  "envelope": null,
  "assetId": null,
  "dock": null
}
```

Field notes:

- `name` - The resource name
- `state` - Current device state
- `errorMessage` - Populated when the resource is in an error state
- `resourceType` - The resource type (e.g. Storage, Liquid Handler, Robot)
- `associatedResource` / `associatedCart` - Parent resource / cart, if any
- `isEnabled` - Whether the resource is enabled and available for automation
- `hasDriver` - Whether the resource has a driver associated with it (boolean)
- `isSimulated` - Whether the resource is running in simulation
- `systemName` - The system the resource belongs to
- `resourcePositions` - Storage positions (populated when `includeDetails=true`)
- `properties` - Resource properties as `{ name, value }` pairs
- `instance`, `location`, `envelope` - Scheduler-internal placement details
- `assetId` - Optional cross-system identifier
- `dock` - Dock information when the resource is docked

When `includeDetails=true`, storage resources also include `resourcePositions` describing each storage location.

## Error Scenarios

These endpoints use the standard FastEndpoints error shapes.

### Resource Not Found

`GET /resources/{resourceName}` returns `404 Not Found` with a body containing only a message:

```json
{
  "message": "Resource Storage 1 not found"
}
```

### Unauthorized Access

Requests without a valid bearer token return `401 Unauthorized` (no body).

## Best Practices

1. **Filter Appropriately**: Use `resourceType` parameter to limit results to relevant resources
2. **Details When Needed**: Only use `includeDetails=true` when position information is required
3. **System Filtering**: Use `filterBySystem=false` only when cross-system resource info is needed
4. **Handle Empty Results**: Check for empty arrays when filters return no matches

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Order Examples](order-examples.md) - Resource allocation in orders
