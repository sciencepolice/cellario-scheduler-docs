# OpenAPI Specification & Endpoints

## Overview

The Cellario API is fully documented using OpenAPI 3.0 specification. The interactive documentation and specification files are available directly from your Cellario server.

## Accessing API Documentation

### Interactive Documentation (Swagger UI)

Visit the interactive API documentation in your browser:

```
http://{cellario-server}:8444/docs
```

This provides:

- Interactive API explorer
- Request/response examples
- Schema definitions
- Authentication testing

### OpenAPI Specification Files

Download the complete OpenAPI specification:

```
http://{cellario-server}:8444/docs/v1/swagger.json
```

## Available Endpoints

_This section will be populated based on the current API routes defined in ApiRoutes.cs_

### Authentication

| Method | Endpoint | Description                       |
| ------ | -------- | --------------------------------- |
| POST   | `/token` | Authenticate and get bearer token |

### System Management

| Method | Endpoint   | Description                 |
| ------ | ---------- | --------------------------- |
| GET    | `/health`  | Health check endpoint       |
| GET    | `/ready`   | Readiness check             |
| GET    | `/live`    | Liveness check              |
| GET    | `/version` | Get API version information |

### Systems

_To be populated from current System endpoints_

### Protocols

_To be populated from current Protocol endpoints_

### Orders

_To be populated from current Order endpoints_

### Resources

_To be populated from current Resource endpoints_

### Devices & Operations

_To be populated from current Device/Operation endpoints_

### Labware

_To be populated from current Labware endpoints_

### Inventory

_To be populated from current Inventory endpoints_

### Events & Subscribers

_To be populated from current Event/Subscriber endpoints_

### Scripts

_To be populated from current Script endpoints_

### System Configuration

_To be populated from current SystemConfig endpoints_

### Users & Roles

_To be populated from current User/Role endpoints_

### Files & Data

_To be populated from current File/Data endpoints_

## Endpoint Categories

### Public Endpoints

Endpoints that don't require authentication:

- `/health`
- `/ready`
- `/live`
- `/version`
- `/docs`

### Authentication Required

All other endpoints require a valid bearer token in the Authorization header:

```
Authorization: Bearer {access_token}
```

### Permission-Based Access

Some endpoints require specific user permissions:

- System configuration endpoints require `SystemConfiguration` permission
- Order management may require `OrderCreate`, `OrderStart` permissions
- Protocol design may require `ProtocolEdit` permission

_Specific permission requirements to be documented per endpoint_

## Request/Response Patterns

### Standard Response Format

Successful responses return the requested resource object (or array) directly as the JSON body. There is no `success`/`data` wrapper envelope. For example, `GET /protocols` returns a JSON array of protocol objects, and `POST /token` returns the token object directly:

```json
{
  "access_token": "eyJhbGciOiJIUzI1Ni␣...",
  "token_type": "bearer",
  "expires_in": 86400
}
```

### Error Response Format

On a validation failure (HTTP 400), the API returns the standard FastEndpoints error shape: a `statusCode`, a top-level `message`, and an `errors` object that maps each offending field name to a list of error messages:

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "fieldName": [
      "Human readable error message"
    ]
  }
}
```

### Pagination

List endpoints such as `GET /protocols` and `GET /resources` are not paginated. They return a plain JSON array of all matching items, with no `skip`/`take` query parameters and no `items`/`totalCount` envelope. Use the endpoint-specific filter parameters (for example `groupName`, `protocolName`, `resourceType`) to narrow the results instead.

## Data Types & Schemas

### Common Data Types

_To be populated with actual schema definitions from the current API_

### Enumerations

_Common enums used throughout the API_

## Rate Limiting

_To be documented if rate limiting is implemented_

## Webhooks & Real-time Updates

### SignalR Hubs

_To be documented based on current SignalR implementation_

### Webhook Endpoints

_To be documented based on current webhook/event system_

## Client Code Generation

The OpenAPI specification can be used to generate client libraries in various languages:

```bash
# Generate C# client
openapi-generator generate -i http://localhost:8444/docs/v1/swagger.json -g csharp -o ./CellarioClient

# Generate Python client
openapi-generator generate -i http://localhost:8444/docs/v1/swagger.json -g python -o ./cellario-python-client

# Generate JavaScript/TypeScript client
openapi-generator generate -i http://localhost:8444/docs/v1/swagger.json -g typescript-axios -o ./cellario-js-client
```

## Testing with API Tools

### Using Postman

1. Import OpenAPI spec: `http://localhost:8444/docs/v1/swagger.json`
2. Set up authentication with bearer token
3. Test endpoints interactively

### Using curl

Basic examples:

```bash
# Health check
curl -X GET http://localhost:8444/health

# Authenticate
curl -X POST http://localhost:8444/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=password"

# List protocols (with authentication)
curl -X GET http://localhost:8444/protocols \
  -H "Authorization: Bearer {token}"
```

## Related Documentation

- [Overview →](overview.md)
- [Authentication →](authentication.md)
- [Examples →](examples.md)
- [Glossary →](glossary.md)
