# Cellario API Overview

## Welcome

Cellario Scheduler provides a RESTful API for interacting with your Cellario systems. From creating and starting orders to checking for errors, the API gives you the tools you need to create integrations with your lab automation.

## Technical Overview

Cellario has a RESTful Web API that is hosted and runs within the Cellario desktop application. Each instance of Cellario will have its own instance of the API. Multiple Cellario instances can share a single Cellario Database, allowing you to view information about multiple systems from a single endpoint.

### Data Scope Behavior

- **Protocols**: When requesting protocols, you will get all protocols in the database, not just the protocols that are valid to run on the system you are currently communicating with
- **Orders**: When requesting orders, you will only see the orders that are valid for the Cellario instance that you are currently connected to
- **Resources**: System-specific resources are scoped to the connected instance

## Base URL

The API is accessible at:

```
http://{cellario-server}:8444
```

Where `{cellario-server}` is the IP address or hostname of the machine running Cellario Scheduler.

## OpenAPI Specification

The complete OpenAPI JSON specification is available for download:

- **Interactive Documentation**: `http://localhost:8444/docs`
- **JSON Specification**: `http://localhost:8444/docs/v1/swagger.json`

You can use this specification with tools like:

- Online Swagger Editor to view the API documentation
- OpenAPI Generator to auto-generate clients in your language of choice
- Postman for API testing and collection management

## Content Types

The API accepts and returns JSON data:

```
Content-Type: application/json
Accept: application/json
```

## Getting Started

### Accessing API Documentation

**Method 1: Direct Browser Access**
Navigate directly to the interactive documentation:

```
http://localhost:8444/docs
```

**Method 2: From Cellario Desktop Application**

1. Open Cellario on your Cellario PC
2. Once opened, Cellario will automatically launch the API services
3. Click the "Gear" icon in the top right of Cellario Scheduler
4. Select "Help" > "API Documentation"
5. Your default browser will launch and display the Cellario API documentation

**Additional Documentation Formats**

- **ReDoc Documentation**: From the Swagger page, you can access a ReDoc version which provides detailed schema documentation with a clean, readable interface (though without the interactive "Try it out" functionality)
- **OpenAPI JSON**: Download the complete specification at `http://localhost:8444/docs/v1/swagger.json` for use with other tools

### Making Your First API Call

#### Basic Health Check

Start with a simple health check to verify connectivity:

```bash
curl -X GET http://localhost:8444/health
```

```powershell
Invoke-RestMethod -Uri "http://localhost:8444/health" -Method Get
```

#### Basic GET Request - List Resources

1. In Swagger UI, scroll down to the **Resources** section
2. Click on `GET /resources`
3. Select **Try it out!**
4. View the response containing all configured resources
5. Celebrate your first Cellario API request!

#### GET Request with Parameters

Many endpoints provide filtering capabilities through parameters:

1. Look at the `GET /resources` endpoint in Swagger UI
2. Note the **Parameters** section with fillable values
3. Try the `resourceType` parameter:
   - Select the box next to `resourceType`
   - Input a configured resource type name (for example `BMG.NepheloStar` — resource types are user-defined, so the valid values depend on your system configuration)
   - Select **Try it out!**
4. Notice the filtered response contains only matching resources

### Example Response Format

This endpoint returns a plain JSON array of resource objects. There is no pagination envelope and no `items`/`totalCount` wrapper:

```json
[
  {
    "name": "StorageDevice1",
    "resourceType": "BMG.NepheloStar",
    "state": "Ready"
  }
]
```

## Client SDK

HighRes Biosolutions provides a comprehensive C# client SDK for working with the Cellario API. For developers using .NET applications, we recommend using the [Cellario Client SDK](../client-sdk/) which provides:

- Strongly-typed request/response models
- Built-in authentication handling
- Async/await support
- Comprehensive error handling
- IntelliSense support in Visual Studio

## Common Use Cases

### Integration Scenarios

- **LIMS Integration**: Connect laboratory information management systems
- **Automated Workflows**: Trigger protocols based on external events
- **System Monitoring**: Monitor order status and system health
- **Custom Applications**: Build specialized interfaces for specific workflows

### API Capabilities

- Create and manage protocols
- Submit and monitor orders
- Check system and device status
- Manage inventory and resources
- Subscribe to real-time events
- Access system configuration

## Authentication

Authentication is optional but recommended for production deployments. When enabled, the API uses simple username/password authentication: you POST your credentials to `/token` and receive a JWT bearer token in response. This is not a full OAuth 2.0 implementation — the `/token` endpoint only accepts `username` and `password` form fields and does not use a `grant_type` parameter. See the [Authentication Guide](authentication.md) for complete details.

## Rate Limiting

Currently, no rate limiting is enforced, but this may change in future versions based on system performance requirements.

## HTTPS Support

The API supports both HTTP and HTTPS connections. For production deployments, HTTPS is recommended using proper SSL certificates.

## Next Steps

- **New to REST APIs?** Start with our [Examples](examples.md) section
- **Need Authentication?** Review the [Authentication Guide](authentication.md)
- **Want Real-time Updates?** Check out [Events](events.md)
- **Using .NET?** See the [Client SDK Documentation](../client-sdk/)
- **Complete Reference?** Browse [OpenAPI Endpoints](openapi-endpoints.md)

## Additional Resources

- **Interactive API Explorer**: `http://localhost:8444/docs`
- **OpenAPI Specification**: `http://localhost:8444/docs/v1/swagger.json`
- **Health Monitoring**: `http://localhost:8444/health`
- **API Version**: `http://localhost:8444/version`
