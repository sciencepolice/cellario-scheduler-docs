# Order Examples

Examples for creating and managing laboratory orders through the Cellario API.

## Overview

Orders represent laboratory work requests that execute protocols with specific parameters and samples. Orders go through various states from creation to completion and can be controlled via the API. This section shows comprehensive order management using real API endpoints.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- Access to protocols for order creation (see [Protocol Examples](protocol-examples.md))
- Appropriate permissions for order operations (OrderCreate, OrderStart, OrderCancel, etc.)

## List Orders

### Endpoint

```
GET /orders
```

### Query Parameters

- `searchAllOrders` (boolean, optional) - Search across all systems
- `protocolId` (integer, optional) - Filter by specific protocol ID
- `includeDetails` (boolean, default: false) - Include detailed order information
- `state` (string, optional) - Filter by order state
- `includeFailedOperations` (boolean, default: false) - Include failed operation details

### Basic Request

```bash
curl -X GET "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Filter Orders by State

```bash
curl -X GET "http://localhost:8444/orders?state=Started&includeDetails=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }

# Get all orders
$orders = Invoke-RestMethod -Uri "http://localhost:8444/orders" -Headers $headers

Write-Host "Found $($orders.Count) orders:"
foreach ($order in $orders) {
    Write-Host "  - Order $($order.id): $($order.protocolName) (State: $($order.state))"
    if ($order.description) {
        Write-Host "    Description: $($order.description)"
    }
    Write-Host "    Created: $($order.createdDate) by $($order.createdBy)"
}

# Filter running orders with details
$runningOrders = Invoke-RestMethod -Uri "http://localhost:8444/orders" -Headers $headers -Body @{
    state = "Started"
    includeDetails = $true
} -Method GET

Write-Host "Running orders: $($runningOrders.Count)"
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}

# Get all orders
response = requests.get('http://localhost:8444/orders', headers=headers)
orders = response.json()

print(f"Found {len(orders)} orders:")
for order in orders:
    print(f"  - Order {order['id']}: {order['protocolName']} (State: {order['state']})")
    if order.get('description'):
        print(f"    Description: {order['description']}")
    print(f"    Created: {order['createdDate']} by {order['createdBy']}")

# Filter by protocol and state
params = {
    'protocolId': 123,
    'state': 'Started',
    'includeDetails': True
}

response = requests.get('http://localhost:8444/orders', headers=headers, params=params)
filtered_orders = response.json()
print(f"Filtered orders: {len(filtered_orders)}")
```

## Get Individual Order

### Endpoint

```
GET /orders/{orderId}
```

### Query Parameters

- `includeDetails` (boolean, default: false) - Include samples, parameters, and outputs
- `includeFailedOperations` (boolean, default: false) - Include failed operation details

### Basic Request

```bash
curl -X GET "http://localhost:8444/orders/123?includeDetails=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$orderId = 123

$order = Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" -Headers $headers -Body @{
    includeDetails = $true
    includeFailedOperations = $true
} -Method GET

Write-Host "Order Details:"
Write-Host "  ID: $($order.id)"
Write-Host "  Protocol: $($order.protocolName) (v$($order.protocolVersion))"
Write-Host "  State: $($order.state)"
Write-Host "  Description: $($order.description)"
Write-Host "  Created: $($order.createdDate) by $($order.createdBy)"

if ($order.startTime) {
    Write-Host "  Started: $($order.startTime)"
}
if ($order.endTime) {
    Write-Host "  Ended: $($order.endTime)"
}

Write-Host "  Samples: $($order.orderSamples.Count)"
Write-Host "  Parameters: $($order.parameters.Count)"
Write-Host "  Email Recipients: $($order.emailRecipients)"
Write-Host "  Inventory Scan: $($order.inventoryScan)"
```

## Order Response Structure

Based on the API schema, order responses include:

```json
{
  "id": 123,
  "state": "Started",
  "protocolId": 456,
  "protocolName": "Sample Prep Protocol",
  "protocolVersion": 1,
  "description": "Sample preparation for batch A",
  "startTime": "2024-03-26T10:00:00Z",
  "startTimeUtc": "2024-03-26T10:00:00Z",
  "endTime": null,
  "endTimeUtc": null,
  "createdBy": "admin",
  "createdDate": "2024-03-26T09:30:00Z",
  "inventoryScan": true,
  "emailRecipients": "lab@company.com",
  "scheduleDetail": null,
  "system": "Lab System 1",
  "orderSamples": [],
  "parameters": []
}
```

### Order States

Orders progress through these states:

- `Created` - Order created but not submitted
- `Submitted` - Order submitted for processing
- `Starting` - Order is being started
- `Started` - Order is actively running
- `Scanning` - Performing inventory scan
- `Scripting` - Running scripts
- `Pausing` - Order is being paused
- `Paused` - Order execution paused
- `Finished` - Order completed successfully
- `Canceled` - Order was canceled
- `Removed` - Order was removed from system

## Get Protocol ID for Order Creation

### Prerequisites for Order Creation

Before creating orders, you must obtain the protocol ID from available protocols. Orders require valid protocol IDs to define what laboratory procedure will be executed.

### Endpoint

```
GET /protocols
```

### Get Available Protocols

```bash
curl -X GET "http://localhost:8444/protocols" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example - Get Protocol ID

```powershell
$headers = @{ Authorization = "Bearer $token" }

# Get all protocols
$protocols = Invoke-RestMethod -Uri "http://localhost:8444/protocols" -Headers $headers

Write-Host "Available Protocols:"
foreach ($protocol in $protocols) {
    Write-Host "  - ID: $($protocol.id) - $($protocol.name) (v$($protocol.version))"
    if ($protocol.description) {
        Write-Host "    Description: $($protocol.description)"
    }
}

# Select a protocol for order creation
$selectedProtocol = $protocols | Where-Object { $_.name -like "*Sample Prep*" } | Select-Object -First 1
if ($selectedProtocol) {
    Write-Host "Selected Protocol ID: $($selectedProtocol.id) for order creation"
    $protocolId = $selectedProtocol.id
} else {
    Write-Host "No suitable protocol found"
}
```

### Python Example - Get Protocol ID

```python
import requests

headers = {'Authorization': f'Bearer {token}'}

# Get all protocols
response = requests.get('http://localhost:8444/protocols', headers=headers)
protocols = response.json()

print("Available Protocols:")
for protocol in protocols:
    print(f"  - ID: {protocol['id']} - {protocol['name']} (v{protocol['version']})")
    if protocol.get('description'):
        print(f"    Description: {protocol['description']}")

# Select a protocol for order creation
sample_prep_protocols = [p for p in protocols if 'Sample Prep' in p['name']]
if sample_prep_protocols:
    selected_protocol = sample_prep_protocols[0]
    protocol_id = selected_protocol['id']
    print(f"Selected Protocol ID: {protocol_id} for order creation")
else:
    print("No suitable protocol found")
```

### Filter Protocols by Category

```bash
curl -X GET "http://localhost:8444/protocols?category=SamplePrep&active=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

For complete protocol management examples, see [Protocol Examples](protocol-examples.md).

## Order Creation Models Overview

The Cellario API supports three distinct order creation patterns, each optimized for different laboratory workflows:

### 1. Template-Based Orders

- **Use case**: Standardized workflows with consistent plate requirements
- **Key feature**: Batch scaling with `batchCount` multiplier
- **Best for**: Routine protocols where you want to run multiple copies
- **Quantity control**: Global batch count with per-thread overrides

### 2. Direct Protocol Orders

- **Use case**: Custom workflows requiring precise plate specification
- **Key feature**: Explicit plate set configuration without templates
- **Best for**: One-off experiments or highly customized setups
- **Quantity control**: Must specify exact quantities per protocol thread

### 3. Transfer List Orders

- **Use case**: Complex well-to-well transfer operations
- **Key feature**: Precise source-to-destination mapping with volumes
- **Best for**: Sample cherry-picking, reformatting, or complex transfers
- **Quantity control**: Driven by transfer list specifications

### Plate Quantity Specification Methods

Within template and direct protocol orders, you can specify plate quantities using three methods with clear precedence:

**Priority Order**: `Barcodes` → `ExplicitCount` → `BatchCount` (template only)

1. **Explicit Barcodes**: Specify exact plates by barcode
2. **Explicit Count**: Specify number of plates (system assigns barcodes)
3. **Batch Count**: Template-driven scaling (templates only)

## Create New Order

### Endpoint

```
POST /orders
```

**Required Permission:** `OrderCreate`

### Request Structure

You can create orders in two ways:

#### Option 1: By Protocol (Manual Order)

```json
{
  "description": "string",
  "user": "string",
  "emailRecipient": "string",
  "inventoryScan": true,
  "clearStorage": false,
  "shouldBeValidated": true,
  "createDefaultParameters": true,
  "order": {
    "protocolId": 123,
    "plateSets": [
      {
        "plateProtocolId": 5051,
        "labwareType": "Golden Plate",
        "placementProcess": "Available",
        "explicitCount": 3
      }
    ],
    "scheduleDetail": null
  }
}
```

#### Option 2: By Template with Plate Sets

```json
{
  "description": "string",
  "user": "string",
  "emailRecipient": "string",
  "inventoryScan": true,
  "clearStorage": true,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "template": {
    "templateId": 2544,
    "batchCount": 1,
    "excludeOptionalThreads": true,
    "plateSets": [
      {
        "plateProtocolId": 5051,
        "labwareType": "Golden Plate",
        "placementProcess": "Available",
        "explicitCount": 3
      },
      {
        "plateProtocolId": 5052,
        "labwareType": "384PP_DMSO2",
        "placementProcess": "Available",
        "explicitCount": 1
      }
    ]
  }
}
```

### Plate Set Configuration

**Important:** Plate sets require `plateProtocolId` values that must be obtained from the selected protocol's details.

#### Plate Set Fields:

- **plateProtocolId**: ID from the protocol's plate protocol definitions (required)
- **labwareType**: Type of labware/plate to use
- **placementProcess**: Resource assignment strategy ("Available", "Fixed", "FixedAndIncrement")
- **explicitCount**: Number of plates for this thread (optional)
- **barcodes**: Array of specific plate barcodes (optional)
- **resourcePositionId**: Explicit resource position ID for placement (optional)
- **outputResourcePositionId**: Starting output position for samples (optional)
- **outputEndingResourcePositionId**: Ending output position (requires outputResourcePositionId)

#### Resource Assignment Options

The `placementProcess` field controls how plates are assigned to physical positions in the laboratory system:

**Available** (Default):
- System automatically finds available positions starting from the first available slot
- If `resourcePositionId` is provided, starts searching from that position
- If no `resourcePositionId` specified, uses the first available position in assigned resources
- Best for most standard workflows

**Fixed**:
- **Requires** `resourcePositionId` to be specified
- All samples in the plate set use the exact same resource position
- Useful when multiple samples share one physical location

**FixedAndIncrement**:
- **Requires** `resourcePositionId` as starting position
- Each subsequent sample increments to the next position (follows column-then-row ordering)
- Useful for sequential placement starting from a specific position

#### Resource Assignment Examples

**Automatic Assignment (No Position Required)**:
```json
{
  "plateProtocolId": 5051,
  "labwareType": "Golden Plate",
  "placementProcess": "Available",
  "explicitCount": 3
  // System finds available positions automatically
}
```

**Starting Position with Auto-Increment**:
```json
{
  "plateProtocolId": 5051,
  "labwareType": "Golden Plate", 
  "placementProcess": "FixedAndIncrement",
  "resourcePositionId": 12345,  // Starting position
  "explicitCount": 3            // Places at 12345, 12346, 12347
}
```

**Explicit Fixed Position**:
```json
{
  "plateProtocolId": 5051,
  "labwareType": "Golden Plate",
  "placementProcess": "Fixed",
  "resourcePositionId": 12345,  // All samples use this position
  "explicitCount": 3
}
```

**Output Position Control**:
```json
{
  "plateProtocolId": 5051,
  "labwareType": "Golden Plate",
  "placementProcess": "Available",
  "barcodes": ["PLATE001", "PLATE002"],
  "outputResourcePositionId": 67890,      // Starting output position
  "outputEndingResourcePositionId": 67891 // Ending output position
}
```

#### Plate Quantity Precedence Rules:

**Priority Order**: `Barcodes` > `ExplicitCount` > Template `BatchCount`

1. **Explicit Barcodes** (highest priority): When `barcodes` array is provided, the exact number and identity of plates is determined by the barcode list
2. **Explicit Count** (middle priority): When `explicitCount` is specified with correct `plateProtocolId`, it overrides the template's `batchCount` for that specific plate thread
3. **Batch Count** (lowest priority): When neither barcodes nor explicit count are specified, uses the template's `batchCount` value

#### Example: Explicit Count Override

Template with `BatchCount: 1` but different explicit counts per thread:

```json
{
  "template": {
    "templateId": 2544,
    "batchCount": 1,
    "plateSets": [
      {
        "plateProtocolId": 5051,
        "labwareType": "Golden Plate",
        "explicitCount": 3 // Creates 3 assay plates
      },
      {
        "plateProtocolId": 5052,
        "labwareType": "384PP_DMSO2",
        "explicitCount": 1 // Creates 1 source plate
      }
    ]
  }
}
```

**Result**: 3 assay plates + 1 source plate (explicit counts override `batchCount: 1`)

Changing the second thread to `explicitCount: 2`:

**Result**: 3 assay plates + 2 source plates

#### Getting Resource Position IDs

To use explicit resource positions, you first need to get available position IDs:

**Get All Positions for a Resource**:
```bash
curl -X GET "http://localhost:8444/resources/Plate_Hotel_1/resourcePositions" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Get Specific Position Details**:
```bash
curl -X GET "http://localhost:8444/resourcePositions/12345" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**PowerShell Example - Find Available Positions**:
```powershell
$headers = @{ Authorization = "Bearer $token" }

# Get all positions for a specific resource
$positions = Invoke-RestMethod -Uri "http://localhost:8444/resources/Plate_Hotel_1/resourcePositions" -Headers $headers

Write-Host "Available positions for Plate_Hotel_1:"
foreach ($position in $positions) {
    Write-Host "  Position ID: $($position.id) - Location: Row $($position.row), Col $($position.column)"
    if ([string]::IsNullOrEmpty($position.plateBarcode)) {
        Write-Host "    Status: Available"
    } else {
        Write-Host "    Status: Occupied by $($position.plateBarcode)"
    }
}

# Find first available position
$availablePosition = $positions | Where-Object { [string]::IsNullOrEmpty($_.plateBarcode) } | Select-Object -First 1
if ($availablePosition) {
    Write-Host "First available position: $($availablePosition.id)"
}
```

**Python Example - Resource Position Lookup**:
```python
import requests

headers = {'Authorization': f'Bearer {token}'}

# Get available positions
response = requests.get('http://localhost:8444/resources/Plate_Hotel_1/resourcePositions', headers=headers)
positions = response.json()

print("Available positions:")
for position in positions:
    barcode = position.get('plateBarcode')
    status = "Available" if not barcode else f"Occupied by {barcode}"
    print(f"  Position {position['id']}: Row {position.get('row', 'N/A')}, Col {position.get('column', 'N/A')} - {status}")

# Find available positions
available_positions = [p for p in positions if not p.get('plateBarcode')]
if available_positions:
    print(f"First available position ID: {available_positions[0]['id']}")
```

### Create Order by Protocol

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Sample preparation batch A",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "clearStorage": false,
    "shouldBeValidated": true,
    "createDefaultParameters": true,
    "order": {
      "protocolId": 123,
      "plateSets": [],
      "scheduleDetail": null
    }
  }'
```

### Complete PowerShell Workflow - Protocol to Order with Plate Sets

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

# Step 1: Get available protocols
$protocols = Invoke-RestMethod -Uri "http://localhost:8444/protocols" -Headers $headers

# Step 2: Select a protocol and get its details including plate protocols
$selectedProtocol = $protocols | Where-Object { $_.name -like "*Sample Prep*" } | Select-Object -First 1

if (-not $selectedProtocol) {
    Write-Error "No suitable protocol found"
    return
}

Write-Host "Using Protocol: $($selectedProtocol.name) (ID: $($selectedProtocol.id))"

# Step 3: Get protocol details to find plate protocol IDs
$protocolDetails = Invoke-RestMethod -Uri "http://localhost:8444/protocols/$($selectedProtocol.id)" -Headers $headers

# Extract plate protocol IDs (assuming protocol has plate protocols defined)
$plateProtocols = $protocolDetails.plateProtocols
if ($plateProtocols.Count -eq 0) {
    Write-Warning "Protocol has no plate protocols defined"
}

# Step 4: Create order with plate sets using actual plate protocol IDs
$orderData = @{
    description = "Sample preparation batch A with plate sets"
    user = "admin"
    emailRecipient = "lab@company.com"
    inventoryScan = $true
    clearStorage = $false
    shouldBeValidated = $true
    createDefaultParameters = $true
    order = @{
        protocolId = $selectedProtocol.id
        plateSets = @(
            @{
                plateProtocolId = $plateProtocols[0].id  # Use actual plate protocol ID
                labwareType = "Golden Plate"
                placementProcess = "Available"
                explicitCount = 3  # Override batch count for this plate type
            }
        )
        scheduleDetail = $null
    }
} | ConvertTo-Json -Depth 4

$newOrder = Invoke-RestMethod -Uri "http://localhost:8444/orders" `
    -Method POST -Headers $headers -Body $orderData

Write-Host "Created order with ID: $($newOrder.id)"
Write-Host "State: $($newOrder.state)"
Write-Host "Protocol: $($newOrder.protocolName)"
Write-Host "Plate Sets: $($newOrder.orderSamples.Count) configured"
```

### Complete Python Workflow - Protocol to Order with Plate Sets

```python
import requests
import json

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

# Step 1: Get available protocols
response = requests.get('http://localhost:8444/protocols', headers=headers)
protocols = response.json()

# Step 2: Select a protocol and get its details
sample_prep_protocols = [p for p in protocols if 'Sample Prep' in p['name']]

if not sample_prep_protocols:
    raise Exception("No suitable protocol found")

selected_protocol = sample_prep_protocols[0]
print(f"Using Protocol: {selected_protocol['name']} (ID: {selected_protocol['id']})")

# Step 3: Get protocol details to find plate protocol IDs
response = requests.get(f"http://localhost:8444/protocols/{selected_protocol['id']}", headers=headers)
protocol_details = response.json()

plate_protocols = protocol_details.get('plateProtocols', [])
if not plate_protocols:
    print("Warning: Protocol has no plate protocols defined")

# Step 4: Create order with plate sets using actual plate protocol IDs
order_data = {
    "description": "Sample preparation batch A with plate sets",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": True,
    "clearStorage": False,
    "shouldBeValidated": True,
    "createDefaultParameters": True,
    "order": {
        "protocolId": selected_protocol['id'],
        "plateSets": [
            {
                "plateProtocolId": plate_protocols[0]['id'],  # Use actual plate protocol ID
                "labwareType": "Golden Plate",
                "placementProcess": "Available",
                "explicitCount": 3  # Override batch count for this plate type
            }
        ] if plate_protocols else [],
        "scheduleDetail": None
    }
}

response = requests.post(
    'http://localhost:8444/orders',
    headers=headers,
    json=order_data
)

new_order = response.json()
print(f"Created order with ID: {new_order['id']}")
print(f"State: {new_order['state']}")
print(f"Protocol: {new_order['protocolName']}")
print(f"Plate Sets: {len(new_order.get('orderSamples', []))} configured")
```

### Template-Based Order with Multiple Plate Sets

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Multi-plate batch processing",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "clearStorage": true,
    "shouldBeValidated": false,
    "createDefaultParameters": true,
    "template": {
      "templateId": 2544,
      "batchCount": 1,
      "excludeOptionalThreads": true,
      "plateSets": [
        {
          "plateProtocolId": 5051,
          "labwareType": "Golden Plate",
          "placementProcess": "Available",
          "explicitCount": 3
        },
        {
          "plateProtocolId": 5052,
          "labwareType": "384PP_DMSO2",
          "placementProcess": "Available",
          "explicitCount": 1
        }
      ]
    }
  }'
```

### Order with Explicit Barcodes (Highest Priority)

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Order with specific barcoded plates",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "template": {
      "templateId": 2544,
      "batchCount": 2,
      "plateSets": [
        {
          "plateProtocolId": 5051,
          "labwareType": "Golden Plate",
          "barcodes": ["ASSAY001", "ASSAY002", "ASSAY003"]
        },
        {
          "plateProtocolId": 5052,
          "labwareType": "384PP_DMSO2",
          "explicitCount": 2
        }
      ]
    }
  }'
```

**Result**:

- 3 assay plates with specific barcodes (barcodes override `batchCount: 2`)
- 2 source plates (explicit count overrides `batchCount: 2`)

### PowerShell Barcode-Based Order

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$barcodeOrderData = @{
    description = "Specific plates by barcode"
    user = "admin"
    emailRecipient = "lab@company.com"
    inventoryScan = $true
    template = @{
        templateId = 2544
        batchCount = 1  # Will be overridden by barcodes
        plateSets = @(
            @{
                plateProtocolId = 5051
                labwareType = "Golden Plate"
                barcodes = @("PLATE001", "PLATE002")  # Exact plates
            },
            @{
                plateProtocolId = 5052
                labwareType = "384PP_DMSO2"
                explicitCount = 1  # Numeric count
            }
        )
    }
} | ConvertTo-Json -Depth 4

$newOrder = Invoke-RestMethod -Uri "http://localhost:8444/orders" `
    -Method POST -Headers $headers -Body $barcodeOrderData

Write-Host "Created order with barcoded plates:"
Write-Host "  Order ID: $($newOrder.id)"
Write-Host "  Assay plates: 2 (from barcodes)"
Write-Host "  Source plates: 1 (from explicit count)"
```

### Order with Resource Positioning

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Order with explicit resource positioning",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "template": {
      "templateId": 2544,
      "batchCount": 1,
      "plateSets": [
        {
          "plateProtocolId": 5051,
          "labwareType": "Golden Plate",
          "placementProcess": "FixedAndIncrement",
          "resourcePositionId": 12345,
          "explicitCount": 3,
          "outputResourcePositionId": 67890,
          "outputEndingResourcePositionId": 67892
        },
        {
          "plateProtocolId": 5052,
          "labwareType": "384PP_DMSO2",
          "placementProcess": "Fixed",
          "resourcePositionId": 54321,
          "explicitCount": 1
        }
      ]
    }
  }'
```

**Result**:
- 3 assay plates placed at positions 12345, 12346, 12347 (incremental from starting position)
- Output samples placed at positions 67890-67892
- 1 source plate at fixed position 54321

### PowerShell Resource Positioning Workflow

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

# Step 1: Find available positions
$positions = Invoke-RestMethod -Uri "http://localhost:8444/resources/Plate_Hotel_1/resourcePositions" -Headers $headers
$availablePositions = $positions | Where-Object { $_.isEmpty -eq $true }

if ($availablePositions.Count -lt 3) {
    Write-Error "Not enough available positions. Need 3, found $($availablePositions.Count)"
    return
}

$startingPosition = $availablePositions[0].id
$fixedPosition = $availablePositions[2].id

Write-Host "Using starting position: $startingPosition"
Write-Host "Using fixed position: $fixedPosition"

# Step 2: Create order with explicit positioning
$positionedOrderData = @{
    description = "Order with calculated resource positions"
    user = "admin"
    emailRecipient = "lab@company.com"
    inventoryScan = $true
    template = @{
        templateId = 2544
        batchCount = 1
        plateSets = @(
            @{
                plateProtocolId = 5051
                labwareType = "Golden Plate"
                placementProcess = "FixedAndIncrement"
                resourcePositionId = $startingPosition
                explicitCount = 2
            },
            @{
                plateProtocolId = 5052
                labwareType = "384PP_DMSO2"
                placementProcess = "Fixed"
                resourcePositionId = $fixedPosition
                explicitCount = 1
            }
        )
    }
} | ConvertTo-Json -Depth 4

$newOrder = Invoke-RestMethod -Uri "http://localhost:8444/orders" `
    -Method POST -Headers $headers -Body $positionedOrderData

Write-Host "Created order with explicit positioning:"
Write-Host "  Order ID: $($newOrder.id)"
Write-Host "  Assay plates: 2 (starting at position $startingPosition)"
Write-Host "  Source plate: 1 (fixed at position $fixedPosition)"
```

## Order Creation Model Comparison

| Model | Use Case | Quantity Control | Barcode Support | Resource Positioning | Complexity |
|-------|----------|------------------|-----------------|---------------------|------------|
| **Template** | Standard workflows | Batch scaling + overrides | Full (highest priority) | Available, Fixed, FixedAndIncrement | Low-Medium |
| **Direct Protocol** | Custom setups | Explicit per thread | Full support | Available, Fixed, FixedAndIncrement | Medium |
| **Transfer List** | Well-level transfers | Transfer-driven | Required for all plates | Automatic from transfer specs | High |

### Resource Positioning Summary

**All order types support flexible resource positioning**:
- **Available**: System finds positions automatically (no resourcePositionId required)
- **Fixed**: All samples use same position (requires resourcePositionId)  
- **FixedAndIncrement**: Start at position, increment for each sample (requires resourcePositionId)
- **Output positioning**: Control where final samples are placed with outputResourcePositionId

### When to Use Each Model

**Template Orders**:

- Routine protocols with consistent requirements
- Need to scale entire workflows (batch count)
- Want standardized configurations with occasional overrides

**Direct Protocol Orders**:

- One-off experiments
- Highly customized plate configurations
- Don't have suitable templates available

**Transfer List Orders**:

- Sample cherry-picking or reformatting
- Complex well-to-well volume transfers
- Precise source-to-destination mapping required
- Integration with liquid handling systems

## Control Order Execution

### Endpoint

```
PUT /orders/{orderId}
```

**Required Permissions:** `OrderStart`, `OrderCancel`, `OrderPause`, `OrderComplete`, `OrderEdit`, `OrderSchedule` (depending on action)

### Available Actions

- `Start` - Start order execution
- `ExpressStart` - Start order with express priority
- `Pause` - Pause order execution
- `Cancel` - Cancel the order
- `Complete` - Mark order as complete
- `Schedule` - Schedule the order for later execution
- `Validate` - Validate order parameters
- `Invalidate` - Invalidate the order
- `BatchPause` - Pause batch processing
- `None` - No action (status check)

### Start Order

```bash
curl -X PUT "http://localhost:8444/orders/123" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "Start",
    "schedule": null
  }'
```

### PowerShell Order Control

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$orderId = 123

# Start order
$startAction = @{
    action = "Start"
    schedule = $null
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" `
    -Method PUT -Headers $headers -Body $startAction

Write-Host "Order $orderId started. New state: $($result.state)"

# Pause order
$pauseAction = @{
    action = "Pause"
    schedule = $null
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" `
    -Method PUT -Headers $headers -Body $pauseAction

Write-Host "Order $orderId paused. New state: $($result.state)"

# Cancel order
$cancelAction = @{
    action = "Cancel"
    schedule = $null
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" `
    -Method PUT -Headers $headers -Body $cancelAction

Write-Host "Order $orderId canceled. New state: $($result.state)"
```

### Python Order Control

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

order_id = 123

# Start order
start_action = {
    "action": "Start",
    "schedule": None
}

response = requests.put(
    f'http://localhost:8444/orders/{order_id}',
    headers=headers,
    json=start_action
)

result = response.json()
print(f"Order {order_id} started. New state: {result['state']}")

# Pause order
pause_action = {
    "action": "Pause",
    "schedule": None
}

response = requests.put(
    f'http://localhost:8444/orders/{order_id}',
    headers=headers,
    json=pause_action
)

result = response.json()
print(f"Order {order_id} paused. New state: {result['state']}")
```

## Monitor Order Progress

### PowerShell Monitoring Loop

```powershell
$headers = @{ Authorization = "Bearer $token" }
$orderId = 123

do {
    $order = Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" -Headers $headers

    $timestamp = Get-Date -Format 'HH:mm:ss'
    Write-Host "$timestamp - Order $($order.id): State = $($order.state)"

    if ($order.startTime -and $order.state -eq "Started") {
        $elapsed = (Get-Date) - [DateTime]::Parse($order.startTime)
        Write-Host "  Running for: $($elapsed.ToString('hh\:mm\:ss'))"
    }

    # Check for completion
    if ($order.state -in @("Finished", "Canceled", "Removed")) {
        Write-Host "Order completed with state: $($order.state)"
        if ($order.endTime) {
            Write-Host "Ended at: $($order.endTime)"
        }
        break
    }

    Start-Sleep -Seconds 10
} while ($true)
```

### Python Progress Monitoring

```python
import requests
import time
from datetime import datetime

headers = {'Authorization': f'Bearer {token}'}
order_id = 123

while True:
    response = requests.get(f'http://localhost:8444/orders/{order_id}', headers=headers)
    order = response.json()

    current_time = datetime.now().strftime('%H:%M:%S')
    print(f"{current_time} - Order {order['id']}: State = {order['state']}")

    if order.get('startTime') and order['state'] == 'Started':
        start_time = datetime.fromisoformat(order['startTime'].replace('Z', '+00:00'))
        elapsed = datetime.now() - start_time.replace(tzinfo=None)
        print(f"  Running for: {elapsed}")

    # Check for completion
    if order['state'] in ['Finished', 'Canceled', 'Removed']:
        print(f"Order completed with state: {order['state']}")
        if order.get('endTime'):
            print(f"Ended at: {order['endTime']}")
        break

    time.sleep(10)
```

## Delete Order

### Endpoint

```
DELETE /orders/{orderId}
```

**Required Permission:** `OrderDelete`

### Basic Request

```bash
curl -X DELETE "http://localhost:8444/orders/123" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$orderId = 123

Invoke-RestMethod -Uri "http://localhost:8444/orders/$orderId" `
    -Method DELETE -Headers $headers

Write-Host "Order $orderId deleted successfully"
```

## Quantity Specification Examples

### Batch Count Only (Template Default)

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Batch processing from template",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "template": {
      "templateId": 456,
      "batchCount": 3
    }
  }'
```

**Behavior**: All plate threads in template will be multiplied by `batchCount: 3`

### Mixed Quantity Specification

```bash
curl -X POST "http://localhost:8444/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Mixed quantity specification",
    "user": "admin",
    "emailRecipient": "lab@company.com",
    "inventoryScan": true,
    "template": {
      "templateId": 456,
      "batchCount": 2,
      "plateSets": [
        {
          "plateProtocolId": 5051,
          "barcodes": ["SPECIFIC1", "SPECIFIC2", "SPECIFIC3"]
        },
        {
          "plateProtocolId": 5052,
          "explicitCount": 4
        }
      ]
    }
  }'
```

**Result**:

- Thread 5051: 3 plates (from barcodes, overrides `batchCount: 2`)
- Thread 5052: 4 plates (from explicit count, overrides `batchCount: 2`)
- Other threads: 2 plates each (uses `batchCount: 2`)

### PowerShell Template Order

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$templateOrderData = @{
    description = "Template order with quantity overrides"
    user = "admin"
    emailRecipient = "lab@company.com"
    inventoryScan = $true
    template = @{
        templateId = 456
        batchCount = 2
        plateSets = @(
            @{
                plateProtocolId = 5051
                labwareType = "Assay Plate"
                barcodes = @("ASSAY001", "ASSAY002", "ASSAY003")
            },
            @{
                plateProtocolId = 5052
                labwareType = "Source Plate"
                explicitCount = 1
            }
        )
    }
} | ConvertTo-Json -Depth 4

$newOrder = Invoke-RestMethod -Uri "http://localhost:8444/orders" `
    -Method POST -Headers $headers -Body $templateOrderData

Write-Host "Created template order:"
Write-Host "  Order ID: $($newOrder.id)"
Write-Host "  Assay plates: 3 (from barcodes)"
Write-Host "  Source plates: 1 (from explicit count)"
Write-Host "  Other threads: 2 each (from batch count)"
```

## Error Scenarios

The API returns two error response shapes:

- **Not found (404)** returns only a `message` field.
- **Validation and other errors (400 / 422 / 500)** return a `statusCode`, a `message`, and an optional `errors` map (field name to array of error strings).

### Order Not Found (404)

```json
{
  "message": "Order not found."
}
```

### Validation / Bad Request (400)

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "generalErrors": [
      "Invalid Order request. Please provide Template or Order."
    ]
  }
}
```

### Permission Denied (403)

A `403 Forbidden` status is returned with an empty body when the caller lacks the
required permission for the requested order action.

## Best Practices

1. **State Management**: Always check order state before issuing control actions
2. **Progress Monitoring**: Use polling instead of continuous requests for status updates
3. **Error Handling**: Implement proper error handling for all order operations
4. **Validation**: Use `shouldBeValidated: true` to catch issues early
5. **Email Notifications**: Set `emailRecipient` for important order completion notifications
6. **Inventory Scanning**: Enable `inventoryScan` when sample tracking is required
7. **Default Parameters**: Use `createDefaultParameters: true` to populate protocol defaults
8. **Permissions**: Ensure users have appropriate OrderCreate, OrderStart, etc. permissions

## Related Documentation

- [Protocol Examples](protocol-examples.md) - Create and manage protocols used in orders
- [Events Examples](events-examples.md) - Monitor order state change events
- [System Examples](system-examples.md) - Ensure system is running before starting orders
- [Driver Examples](driver-examples.md) - Managing drivers during order execution
- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference

## Order Progress Monitoring

### Check Progress

_To be populated with order progress monitoring examples_

### Get Execution Log

_To be populated with order execution log retrieval examples_

## Sample Management

### Add Samples to Order

_To be populated with sample assignment examples_

### Update Sample Information

_To be populated with sample update examples_

## Order Parameters

### Set Order Parameters

_To be populated with parameter setting examples_

### Override Protocol Parameters

_To be populated with parameter override examples_

## Common Workflows

### Complete Order Creation Workflow

```powershell
# 1. Get available protocols
$protocols = Invoke-RestMethod -Uri "$baseUrl/protocols" -Headers $headers

# 2. Select protocol and create order
$selectedProtocol = $protocols[0]
$orderData = @{
    description = "Automated Order $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    user        = "admin"
    order       = @{
        protocolId = $selectedProtocol.id
        plateSets  = @()
    }
}

$newOrder = Invoke-RestMethod -Uri "$baseUrl/orders" `
    -Method POST -Headers $headers -Body ($orderData | ConvertTo-Json -Depth 4) -ContentType "application/json"

# 3. Monitor order state
do {
    Start-Sleep -Seconds 5
    $orderStatus = Invoke-RestMethod -Uri "$baseUrl/orders/$($newOrder.id)" -Headers $headers
    Write-Host "Order state: $($orderStatus.state)"
} while ($orderStatus.state -eq "Started")

Write-Host "Order completed with final state: $($orderStatus.state)"
```
