# Protocol Examples

Examples for working with protocols in the Cellario API based on the actual `/protocols` endpoint.

## Overview

Protocols define the steps and procedures for laboratory automation workflows. The Cellario API provides a `/protocols` endpoint for fetching protocol information.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- `ProtocolView` permission for protocol operations

## List All Protocols

### Endpoint

```
GET /protocols
```

### Query Parameters

- `includeDetails` (boolean, optional) - Include PlateProtocols, Parameters, Properties, and ProtocolSteps
- `groupName` (array, optional) - Filter by protocol group names
- `protocolName` (array, optional) - Filter by specific protocol names
- `latestVersionOnly` (boolean, optional) - Return only the latest version of each protocol
- `validatedOnly` (boolean, optional) - Return only validated protocols

### cURL Example

```bash
curl -X GET "http://localhost:8444/protocols" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$protocols = Invoke-RestMethod -Uri "http://localhost:8444/protocols" -Headers $headers

Write-Host "Found $($protocols.Count) protocols:"
foreach ($protocol in $protocols) {
    Write-Host "  - $($protocol.name) (ID: $($protocol.id)) - Group: $($protocol.group)"
}
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
response = requests.get('http://localhost:8444/protocols', headers=headers)
protocols = response.json()

print(f"Found {len(protocols)} protocols:")
for protocol in protocols:
    print(f"  - {protocol['name']} (ID: {protocol['id']}) - Group: {protocol.get('group', 'N/A')}")
```

### C# Example

```csharp
var response = await httpClient.GetAsync("/protocols");
response.EnsureSuccessStatusCode();

var json = await response.Content.ReadAsStringAsync();
var protocols = JsonSerializer.Deserialize<List<ProtocolResponse>>(json);

Console.WriteLine($"Found {protocols.Count} protocols:");
foreach (var protocol in protocols)
{
    Console.WriteLine($"  - {protocol.Name} (ID: {protocol.Id}) - Group: {protocol.Group}");
}
```

## Fetch Protocols with Details

### Request with Details

```bash
curl -X GET "http://localhost:8444/protocols?includeDetails=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Note**: Including details can result in large payloads. It's not recommended to fetch all protocols with details.

### Filter by Group

```bash
curl -X GET "http://localhost:8444/protocols?groupName=SeqPrep&groupName=Assay" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Filter by Protocol Name and Version

```bash
curl -X GET "http://localhost:8444/protocols?protocolName=PCR%20Setup&latestVersionOnly=true&validatedOnly=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## PowerShell Advanced Filtering

```powershell
$headers = @{ Authorization = "Bearer $token" }

# Get only latest validated protocols from specific groups
$params = @{
    Uri = "http://localhost:8444/protocols"
    Headers = $headers
    Body = @{
        groupName = @("SeqPrep", "Assay")
        latestVersionOnly = $true
        validatedOnly = $true
    }
    Method = "GET"
}

$protocols = Invoke-RestMethod @params
Write-Host "Found $($protocols.Count) validated protocols"

foreach ($protocol in $protocols) {
    Write-Host "Protocol: $($protocol.name)"
    Write-Host "  Version: $($protocol.version)"
    Write-Host "  Validated: $($protocol.validated)"
    Write-Host "  Group: $($protocol.group)"
    Write-Host ""
}
```

## Response Structure

Based on the OpenAPI schema, protocol responses include:

```json
{
  "id": 207,
  "name": "string",
  "group": "string",
  "version": "string",
  "validated": true,
  "notes": "string",
  "createdBy": "string",
  "createdDate": "2024-03-26T10:00:00Z",
  "modifiedBy": "string",
  "modifiedDate": "2024-03-26T12:00:00Z"
}
```

When `includeDetails=true` is specified, additional fields are included:

- `plateProtocols` - Array of plate protocol configurations. Each plate protocol includes its own nested `protocolSteps` array (operations/steps for that thread) - `protocolSteps` is not a top-level field of the protocol itself.
- `parameters` - Array of protocol parameters
- `properties` - Array of protocol properties

## Error Scenarios

### Unauthorized Access

A `401 Unauthorized` status is returned with an empty body when the request has no valid authentication token.

### Forbidden (Missing ProtocolView Permission)

A `403 Forbidden` status is returned with an empty body when the caller lacks the `ProtocolView` permission.

## Best Practices

1. **Use Filtering**: Apply filters (`groupName`, `protocolName`, `validatedOnly`) to reduce response size
2. **Avoid Details for Bulk Queries**: Only use `includeDetails=true` for specific protocols
3. **Version Management**: Use `latestVersionOnly=true` when you only need current versions
4. **Validation Status**: Use `validatedOnly=true` for production workflows
5. **Handle Empty Results**: Check for empty arrays when filters return no matches

## Related Operations

- [Order Examples](order-examples.md) - Creating orders from protocols (uses protocol IDs)
- [System Examples](system-examples.md) - System-specific protocol configurations

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Basic Operations](basic-operations.md) - Authentication and setup
- [Error Handling](error-handling.md) - Common error scenarios
