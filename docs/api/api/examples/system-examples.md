# System Examples

Examples for managing Cellario systems through the API.

## Overview

Systems represent isolated laboratory environments within Cellario. Each system has its own configuration, resources, and can be independently started, stopped, or paused. The API provides comprehensive system management capabilities via the `/systems` endpoints.

## Prerequisites

- Authenticated API session (see [Basic Operations](basic-operations.md))
- Appropriate system permissions (some operations require SystemConfiguration permission)

## List All Systems

### Endpoint

```
GET /systems
```

### Basic Request

```bash
curl -X GET "http://localhost:8444/systems" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$systems = Invoke-RestMethod -Uri "http://localhost:8444/systems" -Headers $headers

Write-Host "Found $($systems.Count) systems:"
foreach ($system in $systems) {
    Write-Host "  - $($system.name) (State: $($system.state))"
    if ($system.description) {
        Write-Host "    Description: $($system.description)"
    }
    if ($system.hostName) {
        Write-Host "    Host: $($system.hostName)"
    }
}
```

### Python Example

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
response = requests.get('http://localhost:8444/systems', headers=headers)
systems = response.json()

print(f"Found {len(systems)} systems:")
for system in systems:
    print(f"  - {system['name']} (State: {system['state']})")
    if system.get('description'):
        print(f"    Description: {system['description']}")
    if system.get('hostName'):
        print(f"    Host: {system['hostName']}")
```

## Get Current Active System

### Endpoint

```
GET /systems/current
```

The active system is the one chosen when the CellarioScheduler application is started.

### Basic Request

```bash
curl -X GET "http://localhost:8444/systems/current" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$currentSystem = Invoke-RestMethod -Uri "http://localhost:8444/systems/current" -Headers $headers

Write-Host "Active System: $($currentSystem.name)"
Write-Host "State: $($currentSystem.state)"
Write-Host "Host: $($currentSystem.hostName)"
Write-Host "Switch Address: $($currentSystem.switchAddress)"
Write-Host "Simulation Speed: $($currentSystem.simulationSpeed)"
```

## System Response Structure

Based on the API schema, system responses include:

```json
{
  "name": "string",
  "description": "string",
  "state": "Running",
  "switchAddress": "string",
  "hostName": "string",
  "simulationSpeed": 1,
  "assetId": "string"
}
```

### System States

Systems can be in one of these states:

- `UnInitialized` - System not yet initialized
- `Initializing` - System is starting up
- `Initialized` - System ready but not running
- `Running` - System actively processing
- `Pausing` - System is stopping operations
- `Paused` - System stopped but ready to resume
- `InError` - System encountered an error

## Create New System

### Endpoint

```
POST /systems
```

**Required Permission:** `SystemConfiguration`

### Request Structure

```json
{
  "name": "string",
  "description": "string",
  "switchAddress": "string",
  "hostName": "string"
}
```

### Basic Request

```bash
curl -X POST "http://localhost:8444/systems" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "TestSystem",
    "description": "Test laboratory system",
    "switchAddress": "192.168.1.100",
    "hostName": "lab-server-01"
  }'
```

### PowerShell Example

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$systemData = @{
    name = "TestSystem"
    description = "Test laboratory system"
    switchAddress = "192.168.1.100"
    hostName = "lab-server-01"
} | ConvertTo-Json

$newSystem = Invoke-RestMethod -Uri "http://localhost:8444/systems" `
    -Method POST -Headers $headers -Body $systemData

Write-Host "Created system: $($newSystem.name) with state $($newSystem.state)"
```

### Python Example

```python
import requests
import json

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

system_data = {
    "name": "TestSystem",
    "description": "Test laboratory system",
    "switchAddress": "192.168.1.100",
    "hostName": "lab-server-01"
}

response = requests.post(
    'http://localhost:8444/systems',
    headers=headers,
    json=system_data
)

new_system = response.json()
print(f"Created system: {new_system['name']} with state {new_system['state']}")
```

## Control System State

### Endpoint

```
PUT /systems/current
```

**Required Permission:** One of `SystemStart`, `SystemPause`, or `SystemStop` (each action is additionally checked against its matching permission - `Start` requires `SystemStart`, `Pause` requires `SystemPause`, `Stop` requires `SystemStop`).

> **Response:** This endpoint returns `202 Accepted` with an **empty body**. It does not return the system object. To observe the resulting state, re-fetch `GET /systems/current` after issuing the action (see [Monitor System State Changes](#monitor-system-state-changes)).

### Available Actions

- `Start` - Start the system for processing
- `Pause` - Pause system operations
- `Stop` - Stop the system completely
- `None` - No action

### Start System

```bash
curl -X PUT "http://localhost:8444/systems/current" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "Start",
    "doResourceIdentificationOnStart": true
  }'
```

### PowerShell System Control

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

# Start system with resource identification
$startAction = @{
    action = "Start"
    doResourceIdentificationOnStart = $true
} | ConvertTo-Json

# Returns 202 Accepted with an empty body
Invoke-RestMethod -Uri "http://localhost:8444/systems/current" `
    -Method PUT -Headers $headers -Body $startAction

Write-Host "System start command accepted (202)"

# Re-fetch to observe the resulting state
$current = Invoke-RestMethod -Uri "http://localhost:8444/systems/current" -Headers $headers
Write-Host "Current state: $($current.state)"

# Pause system
$pauseAction = @{
    action = "Pause"
    doResourceIdentificationOnStart = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8444/systems/current" `
    -Method PUT -Headers $headers -Body $pauseAction

Write-Host "System pause command accepted (202)"
```

### Python System Control

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

# Start system
start_action = {
    "action": "Start",
    "doResourceIdentificationOnStart": True
}

# Returns 202 Accepted with an empty body
response = requests.put(
    'http://localhost:8444/systems/current',
    headers=headers,
    json=start_action
)
print(f"System start accepted: {response.status_code}")

# Re-fetch to observe the resulting state
current = requests.get('http://localhost:8444/systems/current', headers=headers).json()
print(f"Current state: {current['state']}")

# Stop system
stop_action = {
    "action": "Stop",
    "doResourceIdentificationOnStart": False
}

response = requests.put(
    'http://localhost:8444/systems/current',
    headers=headers,
    json=stop_action
)

print(f"System stop accepted: {response.status_code}")
```

## Delete System

### Endpoint

```
DELETE /systems/{name}
```

**Required Permission:** `SystemConfiguration`

### Basic Request

```bash
curl -X DELETE "http://localhost:8444/systems/TestSystem" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### PowerShell Example

```powershell
$headers = @{ Authorization = "Bearer $token" }
$systemName = "TestSystem"

Invoke-RestMethod -Uri "http://localhost:8444/systems/$systemName" `
    -Method DELETE -Headers $headers

Write-Host "System '$systemName' deleted successfully"
```

## Rename System

### Endpoint

```
POST /systems/rename
```

**Required Permission:** `SystemConfiguration`

### Request Structure

```json
{
  "oldName": "string",
  "newName": "string"
}
```

### Basic Request

```bash
curl -X POST "http://localhost:8444/systems/rename" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "oldName": "OldSystemName",
    "newName": "NewSystemName"
  }'
```

### PowerShell Example

```powershell
$headers = @{
    Authorization = "Bearer $token"
    'Content-Type' = "application/json"
}

$renameData = @{
    oldName = "OldSystemName"
    newName = "NewSystemName"
} | ConvertTo-Json

$renamedSystem = Invoke-RestMethod -Uri "http://localhost:8444/systems/rename" `
    -Method POST -Headers $headers -Body $renameData

Write-Host "System renamed to: $($renamedSystem.name)"
```

## Monitor System State Changes

### PowerShell Monitoring Loop

```powershell
$headers = @{ Authorization = "Bearer $token" }

do {
    $currentSystem = Invoke-RestMethod -Uri "http://localhost:8444/systems/current" -Headers $headers
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - System: $($currentSystem.name), State: $($currentSystem.state)"

    # Check if system reached desired state
    if ($currentSystem.state -eq "Running") {
        Write-Host "System is now running!"
        break
    }

    Start-Sleep -Seconds 5
} while ($true)
```

### Python State Monitoring

```python
import requests
import time
from datetime import datetime

headers = {'Authorization': f'Bearer {token}'}

while True:
    response = requests.get('http://localhost:8444/systems/current', headers=headers)
    system = response.json()

    current_time = datetime.now().strftime('%H:%M:%S')
    print(f"{current_time} - System: {system['name']}, State: {system['state']}")

    # Check if system reached desired state
    if system['state'] == 'Running':
        print("System is now running!")
        break

    time.sleep(5)
```

## Error Scenarios

These endpoints use the standard FastEndpoints error shapes.

### System Not Found

`404 Not Found` responses contain a body with only a message:

```json
{
  "message": "System not found"
}
```

### Invalid System Action

Sending an unsupported action to `PUT /systems/current` produces a `400 Bad Request` FastEndpoints validation error with an errors dictionary (not a flat `message`):

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "generalErrors": ["Invalid action"]
  }
}
```

### Unauthorized Access

Requests without a valid bearer token return `401 Unauthorized` (no body).

### Permission Denied

Issuing a control action without the matching permission (`SystemStart`, `SystemPause`, or `SystemStop`) returns `403 Forbidden` (no body).

## Best Practices

1. **Monitor State Changes**: Always verify system state after control operations
2. **Resource Identification**: Use `doResourceIdentificationOnStart: true` when starting systems to ensure proper device detection
3. **Graceful Shutdown**: Pause systems before stopping to allow operations to complete
4. **Error Handling**: Implement proper error handling for state transition failures
5. **Permissions**: Ensure users have appropriate SystemConfiguration permissions for management operations
6. **Active System**: Remember that only the current active system can be controlled via `/systems/current`

## Related Documentation

- [OpenAPI Endpoints](../openapi-endpoints.md) - Complete API reference
- [Events Examples](events-examples.md) - Monitor system state change events
- [Resource Examples](resource-examples.md) - System resources and allocation
