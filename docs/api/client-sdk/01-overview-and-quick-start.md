# Cellario Client SDK - Overview & Quick Start

## Overview

The **Cellario Client SDK** (`Cellario.Client.dll`) is a .NET 8.0 library that provides programmatic access to the Cellario Scheduler API. It offers a strongly-typed, async-friendly interface for integrating with Cellario's laboratory automation platform from external applications.

The client is auto-generated from Cellario's OpenAPI specification using NSwag, ensuring it stays synchronized with the latest API features and changes.

## Features

- **Strongly Typed**: All API endpoints, request/response models, and enums are strongly typed
- **Async Support**: Full async/await support for all operations
- **Authentication**: Built-in support for Basic and Bearer token authentication
- **Comprehensive Coverage**: Access to all Cellario API endpoints including:
  - Protocols and Protocol Design
  - Orders and Order Management
  - Systems and Resources
  - Inventory and Labware
  - Users and Settings
  - Events and Notifications
  - Scripts and Templates

## Installation

### NuGet Package

Add the Cellario.Client NuGet package to your project:

```xml
<PackageReference Include="Cellario.Client" Version="4.5.0" />
```

### Manual DLL Reference

The `Cellario.Client.dll` is included with your Cellario installation and can be found in the Cellario install folder (typically `C:\Program Files\HighRes Biosolutions\Cellario\`).

Reference the DLL directly in your project:

```xml
<Reference Include="Cellario.Client">
  <HintPath>C:\Program Files\HighRes Biosolutions\Cellario\Cellario.Client.dll</HintPath>
</Reference>
```

Or copy the DLL to your project and reference it locally:

```xml
<Reference Include="Cellario.Client">
  <HintPath>libs\Cellario.Client.dll</HintPath>
</Reference>
```

## Quick Start

### 1. Basic Client Setup

```csharp
using Cellario.Client;

// Create client with default port (8444)
var client = new CellarioClient("localhost");

// Or specify custom port and HTTPS
var client = new CellarioClient("cellario.company.com", 443, useHttps: true);

// Or use existing HttpClient
var httpClient = new HttpClient();
var client = new CellarioClient(httpClient);
```

### 2. Authentication

#### Bearer Token Authentication (Recommended)

```csharp
// Async authentication
await client.AuthenticateBearer("username", "password");

// Synchronous authentication
client.AuthenticateBearerSync("username", "password");
```

### 3. Making API Calls

```csharp
// Get all protocols
var protocols = await client.Protocols.GetProtocolsAsync();

// Get specific protocol
var protocol = await client.Protocols.GetProtocolAsync(protocolId);

// Create new order
var orderRequest = new CreateOrderRequest
{
    Description = "My Test Order",
    Order = new OrderSetRequest
    {
        ProtocolId = protocolId,
        // ... plate sets and other properties
    }
};
var newOrder = await client.Orders.CreateOrderAsync(orderRequest);

// Get system information
var systems = await client.Systems.GetSystemsAsync();
```

## Next Steps

- [Learn about Authentication →](02-authentication.md)
- [Explore API Reference →](03-api-reference.md)
- [See Examples →](04-examples-and-best-practices.md)
