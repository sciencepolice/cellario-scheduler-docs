# Inventory Examples

Examples for managing laboratory inventory through the Cellario API.

## Overview

Inventory operations provide tracking and management of labware and barcodes across storage and nest positions. Inventory is read and updated via the `/inventory` endpoints, and scans (which discover the contents of storage devices) are managed via the `/inventory/scans` endpoints.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- The `ResourceStorage` permission for inventory and scan operations (scans for an order additionally require `OrderScan`)

## Get Inventory

### Endpoint

```
GET /inventory
```

**Required Permission:** `ResourceStorage`

### Query Parameters

- `includeEmpty` (boolean, default: false) - Include empty positions in the results
- `resource` (string, repeatable) - Limit results to one or more resources by name (e.g. `?resource=Storage%201&resource=Storage%202`)

### Basic Request

```bash
curl -X GET http://localhost:8444/inventory \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$inventory = Invoke-RestMethod -Uri "http://localhost:8444/inventory" -Headers $headers

Write-Host "Found $($inventory.Count) inventory items:"
foreach ($item in $inventory) {
    Write-Host "  - Barcode: $($item.barcode), Labware: $($item.labwareType) @ $($item.location.resourceName)"
}
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
params = {'includeEmpty': 'false', 'resource': ['Storage 1', 'Storage 2']}
response = requests.get('http://localhost:8444/inventory', headers=headers, params=params)
inventory = response.json()

print(f"Found {len(inventory)} inventory items:")
for item in inventory:
    print(f"  - {item.get('barcode')} ({item.get('labwareType')}) @ {item['location']['resourceName']}")
```

### Inventory Item Response Structure

Each item is an `InventoryItemResponse`:

```json
{
  "barcode": "PLATE0001",
  "labwareType": "96-Well Plate",
  "location": {
    "positionId": 42,
    "resourceName": "Storage 1",
    "column": 1,
    "position": 3,
    "locationType": "Serial",
    "system": "MySystem"
  },
  "itemType": "LabwareAndBarcode",
  "orderSampleId": 1001,
  "isActive": true
}
```

Field notes:

- `barcode` - The labware barcode, if known
- `labwareType` - The labware type, if known
- `location` - Where the item lives (`positionId`, `resourceName`, `column`, `position`, `locationType`, `system`)
- `itemType` - Derived: `Empty`, `LabwareOnly`, `BarcodeOnly`, or `LabwareAndBarcode`
- `orderSampleId` - The order sample currently occupying the position, if any
- `isActive` - Whether the inventory record is active

## Update Inventory

### Endpoint

```
PUT /inventory
```

**Required Permission:** `ResourceStorage`

Updates one or more inventory positions. The request body is an array of items, each identifying the position by `positionId` and the labware/barcode to set. Returns the updated inventory items.

### Set Barcode and Labware for a Position

```bash
curl -X PUT "http://localhost:8444/inventory" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '[
    { "positionId": 42, "barcode": "PLATE0001", "labwareType": "96-Well Plate" }
  ]'
```

### PowerShell Example

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$body = @(
    @{ positionId = 42; barcode = "PLATE0001"; labwareType = "96-Well Plate" }
) | ConvertTo-Json

$updated = Invoke-RestMethod -Uri "http://localhost:8444/inventory" `
    -Method PUT -Headers $headers -Body $body

Write-Host "Updated $($updated.Count) position(s)"
```

To clear a position, send the position with empty/null `barcode` and `labwareType`.

## Barcode and Scan Operations

Scans discover the contents (barcodes/labware) of a resource or order. Scans are asynchronous: start a scan, then poll for its status using the returned `id`.

### Start a Scan

```
POST /inventory/scans
```

**Required Permission:** `ResourceStorage` (a scan with `requestType` of `Order` additionally requires `OrderScan`)

The body is an `InventoryScanRequest`. Set `requestType` to `Full`, `Partial`, or `Order`:

- `Full` - Scan an entire resource (requires `resource`)
- `Partial` - Scan specific positions of a resource (requires `resource` and `scanPositions`)
- `Order` - Scan inventory for an order (requires a valid `orderId`)

> Note: `persistInventory` is accepted but ignored and will be removed in the future.

#### Full Scan of a Resource

```bash
curl -X POST "http://localhost:8444/inventory/scans" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "requestType": "Full",
    "resource": "Storage 1"
  }'
```

#### Partial Scan (PowerShell)

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$scan = @{
    requestType = "Partial"
    resource = "Storage 1"
    scanPositions = @(
        @{ col = 1; row = 1 },
        @{ col = 1; row = 2 }
    )
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:8444/inventory/scans" `
    -Method POST -Headers $headers -Body $scan

Write-Host "Scan started with id $($result.id), status $($result.status)"
```

#### Scan for an Order (Python)

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

scan = {
    "requestType": "Order",
    "orderId": 1234
}

response = requests.post('http://localhost:8444/inventory/scans', headers=headers, json=scan)
result = response.json()
print(f"Scan id: {result['id']}, status: {result['status']}")
```

#### Scan Response Structure

```json
{
  "id": "3f1c0c2e-2a4b-4d2a-9b1e-0f2a8e3a1b22",
  "request": { "requestType": "Full", "resource": "Storage 1" },
  "status": "Queued",
  "statusMessage": null,
  "results": {
    "inventoryResults": [
      {
        "inventoryLocation": { "resourceName": "Storage 1", "column": 1, "position": 1 },
        "barcode": "PLATE0001"
      }
    ]
  }
}
```

### List Scans

```
GET /inventory/scans
```

**Required Permission:** `ResourceStorage`

Returns all known scan operations.

```bash
curl -X GET "http://localhost:8444/inventory/scans" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Get a Single Scan

```
GET /inventory/scans/{id}
```

**Required Permission:** `ResourceStorage`

Poll this endpoint with the `id` returned from starting a scan to track its progress. Returns `404 Not Found` if the scan id is unknown.

```powershell
$headers = @{ Authorization = "Bearer $token" }
$scanId = "3f1c0c2e-2a4b-4d2a-9b1e-0f2a8e3a1b22"
$scan = Invoke-RestMethod -Uri "http://localhost:8444/inventory/scans/$scanId" -Headers $headers
Write-Host "Status: $($scan.status) - $($scan.statusMessage)"
```

### Cancel a Scan

```
DELETE /inventory/scans/{id}
```

**Required Permission:** `ResourceStorage`

Cancels an in-progress scan. Returns `202 Accepted` with an empty body (also `202` if the id is unknown).

```bash
curl -X DELETE "http://localhost:8444/inventory/scans/3f1c0c2e-2a4b-4d2a-9b1e-0f2a8e3a1b22" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Error Scenarios

These endpoints use the standard FastEndpoints error shapes.

### Validation Error

A scan request missing required fields returns `400 Bad Request` with an errors dictionary:

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "generalErrors": ["Resource must be specified!"]
  }
}
```

### Scan Not Found

`GET /inventory/scans/{id}` for an unknown id returns `404 Not Found` with a body containing only a message.

### Permission Denied

Requests without the required permission return `403 Forbidden` (no body).

## Best Practices

1. **Poll for Scan Completion**: Scans are asynchronous - start a scan, then poll `GET /inventory/scans/{id}` using the returned `id`.
2. **Scope Reads**: Use the `resource` query parameter on `GET /inventory` to limit results to the storage devices you care about.
3. **Update by Position**: `PUT /inventory` targets positions by `positionId`; fetch inventory first to discover position ids.
4. **Audit Trail**: Inventory updates and scans are recorded in the Cellario audit trail automatically.

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Resource Examples](resource-examples.md) - Storage resources and positions
- [Order Examples](order-examples.md) - Inventory usage in orders
