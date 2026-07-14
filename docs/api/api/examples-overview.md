# API Examples Overview

This section provides practical examples of common Cellario API operations organized by functional area. Each section includes examples using various tools and programming languages.

## Getting Started

Start with the basic operations to understand authentication and connectivity:

→ **[Basic Operations](basic-operations.md)** - Health checks and authentication  
→ **[Error Handling](error-handling.md)** - Common error scenarios and solutions

## API Operations by Domain

### Core Laboratory Operations

→ **[Protocol Examples](protocol-examples.md)** - Create, manage, and query protocols  
→ **[Order Examples](order-examples.md)** - Create and manage laboratory orders  
→ **[Device Examples](device-examples.md)** - Control and monitor laboratory devices

### System Management

→ **[System Examples](system-examples.md)** - System information and configuration  
→ **[Resource Examples](resource-examples.md)** - Manage laboratory resources  
→ **[Inventory Examples](inventory-examples.md)** - Track and manage inventory

### Real-time Features

→ **[Events Examples](events-examples.md)** - Subscribe to and handle real-time events

## Integration Guides

→ **[Integration Examples](integration-examples.md)** - Complete workflow examples in PowerShell, Python, and C#

## Tools and Languages Covered

Each example section includes code samples for:

- **cURL** - Command-line HTTP requests
- **PowerShell** - Windows automation scripts
- **Python** - Cross-platform scripting
- **C#** - .NET applications (without SDK)

For production integrations, consider using the [Cellario Client SDK](../client-sdk/README.md) for a more robust development experience.

## Prerequisites

Before running these examples:

1. **Running Cellario API** - Ensure Cellario is running with API enabled
2. **Authentication** - Have valid user credentials (username/password)
3. **Network Access** - Confirm connectivity to the API endpoint (default: `http://localhost:8444`)
4. **Tools** - Install required tools (cURL, PowerShell, Python, etc.)

## Common Base URL

All examples assume the API is running at `http://localhost:8444`. Update URLs as needed for your environment.

For HTTPS configurations, see [HTTPS Configuration Guide](../deployment/https-configuration.md).

## Related Documentation

- [API Overview](overview.md) - Understand the API architecture
- [Authentication](authentication.md) - Detailed authentication methods
- [OpenAPI Endpoints](openapi-endpoints.md) - Complete API reference
- [Events](events.md) - Real-time event system documentation
- [Client SDK](../client-sdk/README.md) - Recommended approach for production integrations
