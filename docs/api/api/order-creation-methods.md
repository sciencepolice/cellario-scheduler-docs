# Creating Orders via the API

This guide describes the different ways to POST an order to the Cellario Scheduler API. All orders are created through a single endpoint, but the shape of the request body determines how the order is built and how its plates (samples) are placed onto system resources.

- [Endpoint](#endpoint)
- [Choosing a creation method](#choosing-a-creation-method)
- [Common envelope fields](#common-envelope-fields)
- [Method 1: Manual order (explicit `Order`)](#method-1-manual-order-explicit-order)
  - [Generic "Available" placement](#generic-available-placement)
  - [Specific resource assignment (`Fixed` / `FixedAndIncrement`)](#specific-resource-assignment-fixed--fixedandincrement)
  - [Output resource selection](#output-resource-selection)
- [Method 2: Template order (`Template`)](#method-2-template-order-template)
- [Method 3: Transfer-list order](#method-3-transfer-list-order)
- [Scheduling an order](#scheduling-an-order)
- [Validation errors](#validation-errors)

## Endpoint

```http
POST /orders
Content-Type: application/json
```

Transfer-list orders use a dedicated route:

```http
POST /orders/transferlist
Content-Type: application/json
```

Both require the `OrderCreate` permission. On success the API returns `201 Created` with an `OrderResponse` body and a `Location` header pointing at `GET /orders/{orderId}`.

## Choosing a creation method

The `POST /orders` request body is an envelope with two mutually exclusive payload fields. Provide **exactly one**:

| Field      | Method         | Use when                                                                                                                          |
| ---------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `order`    | Manual order   | You know the protocol and want to specify each thread's plates, barcodes, and placement directly.                                 |
| `template` | Template order | You have a saved order template and want to scale it by batch count / primary plate quantity, with optional per-thread overrides. |

> Providing neither `order` nor `template` returns a validation error: _"Invalid Order request. Please provide Template or Order."_ If both are provided, `template` silently takes precedence and `order` is ignored.

Transfer-list orders are a third method that uses a separate endpoint (`POST /orders/transferlist`) and the `transferListSet` payload. See [Method 3](#method-3-transfer-list-order).

## Common envelope fields

These top-level fields apply to every `POST /orders` request regardless of method:

| Field                     | Type    | Description                                                                                                                                           |
| ------------------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `description`             | string? | User-provided description of the order.                                                                                                               |
| `user`                    | string? | Username creating the order. **On the request this field is `user`, not `createdBy`** — the response echoes it back as `createdBy`, but sending `createdBy` in the request body has no effect. |
| `emailRecipient`          | string? | Comma-separated list of email recipients for order-level notifications.                                                                               |
| `inventoryScan`           | bool    | Trigger an inventory scan before the run starts.                                                                                                      |
| `clearStorage`            | bool    | Clear storage at the end of the run.                                                                                                                  |
| `shouldBeValidated`       | bool    | When `true`, the order is created in an _invalidated_ state and will not be usable in Cellario until validated. Set `false` for a ready-to-use order. |
| `createDefaultParameters` | bool    | When `true`, the default run-order parameters are created automatically. Recommended.                                                                 |
| `order`                   | object? | Manual order payload. Do not combine with `template`.                                                                                                 |
| `template`                | object? | Template order payload. Do not combine with `order`.                                                                                                  |

## Method 1: Manual order (explicit `Order`)

A manual order specifies a protocol and an array of **plate sets** — one entry per plate protocol (thread) you want to populate. Each plate set says which thread it targets, what labware to use, how many plates (via barcodes or `explicitCount`), and how those plates should be placed onto resources.

### `OrderSetRequest`

| Field            | Type              | Description                                                                                 |
| ---------------- | ----------------- | ------------------------------------------------------------------------------------------- |
| `protocolId`     | int               | The protocol to run.                                                                        |
| `plateSets`      | PlateSetRequest[] | One entry per thread to populate.                                                           |
| `scheduleDetail` | object?           | Optional scheduling. Omit for immediate scheduling. See [Scheduling](#scheduling-an-order). |

### `PlateSetRequest`

| Field                            | Type     | Description                                                                                                                                                      |
| -------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `plateProtocolId`                | int      | The plate protocol (thread) this set targets.                                                                                                                    |
| `labwareType`                    | string?  | Labware/plate type name (e.g. `"Generic 384 Microtiter Plate"`).                                                                                                 |
| `placementProcess`               | enum     | `Available` (default), `Fixed`, or `FixedAndIncrement`. See below.                                                                                               |
| `barcodes`                       | string[] | Barcodes for the plates. The number of barcodes determines the number of plates.                                                                                 |
| `explicitCount`                  | int?     | Number of plates when no barcodes are supplied. Either `barcodes` or `explicitCount` is required.                                                                |
| `resourcePositionId`             | int?     | The starting resource position for these plates. Required for `Fixed`/`FixedAndIncrement`; for `Available` it overrides the thread's assigned starting resource. |
| `outputResourcePositionId`       | int?     | Sets the starting range of the thread's "end" step (output).                                                                                                     |
| `outputEndingResourcePositionId` | int?     | Sets the ending range of the output. Requires `outputResourcePositionId`.                                                                                        |
| `batchId`                        | int?     | Optional batch identifier for the plates.                                                                                                                        |

> **Plate count source.** If `barcodes` is non-empty, its length is the plate count. Otherwise `explicitCount` is used. Supplying neither is a validation error.

### Placement processes

The `placementProcess` controls how each plate in the set is assigned to a physical resource position:

| Value               | Behavior                                                                                                                                                                                                                                                        |
| ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Available`         | The scheduler finds the next _available_ position for each plate, starting from the thread's assigned starting resource (or `resourcePositionId` if provided). Use this for general-purpose ordering where you don't care which exact nest each plate lands in. |
| `Fixed`             | Every plate in the set is assigned to the **same** position (`resourcePositionId`).                                                                                                                                                                             |
| `FixedAndIncrement` | The first plate starts at `resourcePositionId`; each subsequent plate is placed at the next incremental position. Use this to lay plates out across consecutive nests deterministically.                                                                        |

### Generic "Available" placement

The most common case: let the scheduler pick positions. If the thread's input step already has resources assigned in the protocol, you can omit `resourcePositionId` entirely.

```http
POST /orders
Content-Type: application/json

{
  "description": "Cell viability assay - Batch A",
  "user": "lab_technician",
  "emailRecipient": "lab@company.com",
  "clearStorage": false,
  "createDefaultParameters": true,
  "order": {
    "protocolId": 27,
    "plateSets": [
      {
        "plateProtocolId": 118,
        "labwareType": "Generic 384 Microtiter Plate",
        "placementProcess": "Available",
        "barcodes": ["PREP001", "PREP002", "PREP003", "PREP004"]
      },
      {
        "plateProtocolId": 119,
        "labwareType": "Generic 384 Microtiter Plate",
        "placementProcess": "Available",
        "barcodes": ["REAG001", "REAG002", "REAG003", "REAG004"]
      },
      {
        "plateProtocolId": 120,
        "labwareType": "30uL Single - 384",
        "placementProcess": "Available",
        "explicitCount": 2
      }
    ]
  }
}
```

> **Note.** If a thread's input step has no assigned resources in the protocol and you don't provide `resourcePositionId`, order creation fails with _"Input Thread {id} must have starting resources assigned or specified during order creation."_

### Specific resource assignment (`Fixed` / `FixedAndIncrement`)

When you need plates to land in specific, known positions — for example loading from a particular stacker or hotel — set `resourcePositionId` and choose a fixed placement process.

**`FixedAndIncrement`** — lay four plates across consecutive positions starting at position `5012`:

```http
POST /orders
Content-Type: application/json

{
  "description": "Fixed-position load from Stacker 1",
  "user": "lab_technician",
  "inventoryScan": false,
  "clearStorage": false,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "order": {
    "protocolId": 42,
    "plateSets": [
      {
        "plateProtocolId": 101,
        "labwareType": "Generic 384 Microtiter Plate",
        "placementProcess": "FixedAndIncrement",
        "resourcePositionId": 5012,
        "barcodes": ["PREP001", "PREP002", "PREP003", "PREP004"]
      }
    ]
  }
}
```

**`Fixed`** — every plate assigned to the same position (e.g. a single-nest serial device that consumes plates one at a time):

```http
POST /orders
Content-Type: application/json

{
  "description": "Serial loader",
  "user": "lab_technician",
  "inventoryScan": false,
  "clearStorage": false,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "order": {
    "protocolId": 42,
    "plateSets": [
      {
        "plateProtocolId": 101,
        "labwareType": "Generic 384 Microtiter Plate",
        "placementProcess": "Fixed",
        "resourcePositionId": 5050,
        "barcodes": ["PLATE001", "PLATE002", "PLATE003"]
      }
    ]
  }
}
```

> Resource position IDs are system-specific. Retrieve them from the system resources endpoints before building a fixed-placement order, and verify they exist in the target system. An unknown ID returns _"ResourcePosition '{id}' not found."_

### Output resource selection

To control where a thread's output ("end" step) plates are placed, set `outputResourcePositionId` (start of the output range) and optionally `outputEndingResourcePositionId` (end of the range — requires the start to be set):

```json
{
  "plateProtocolId": 101,
  "labwareType": "Generic 384 Microtiter Plate",
  "placementProcess": "Available",
  "barcodes": ["PREP001", "PREP002"],
  "outputResourcePositionId": 6001,
  "outputEndingResourcePositionId": 6024
}
```

## Method 2: Template order (`Template`)

A template order is built from a saved order template. Instead of specifying every plate set, you provide a `templateId` and scaling parameters; Cellario expands the template into a full order. Per-thread `plateSets` are optional and act as **overrides** on top of the template.

### `TemplateSetRequest`

| Field                    | Type              | Description                                                                                      |
| ------------------------ | ----------------- | ------------------------------------------------------------------------------------------------ |
| `templateId`             | int               | The order template to use.                                                                       |
| `batchCount`             | int?              | Number of batches to create from the template.                                                   |
| `primaryPlateProtocolId` | int?              | The driving thread used to scale the rest of the order.                                          |
| `primaryLabwareQuantity` | int?              | Number of plates for the primary (driving) thread; other threads scale by the template's ratios. |
| `excludeOptionalThreads` | bool              | When `true`, optional threads in the template are not included.                                  |
| `plateSets`              | PlateSetRequest[] | Optional per-thread overrides (barcodes, labware, placement, or `excludeThread`).                |
| `scheduleDetail`         | object?           | Optional scheduling. See [Scheduling](#scheduling-an-order).                                     |

The `plateSets` entries here use the same `PlateSetRequest` shape as manual orders, plus one template-only field:

| Field           | Type | Description                                                                                |
| --------------- | ---- | ------------------------------------------------------------------------------------------ |
| `excludeThread` | bool | When `true`, no plates are created for this thread, excluding it from the templated order. |

### Example: scale a template and supply barcodes

Create 8 primary plates from template `7`, supplying barcodes for the primary thread and excluding an optional thread:

```http
POST /orders
Content-Type: application/json

{
  "description": "Nightly run from template",
  "user": "lab_technician",
  "emailRecipient": "lab@company.com",
  "inventoryScan": true,
  "clearStorage": true,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "template": {
    "templateId": 7,
    "primaryPlateProtocolId": 101,
    "primaryLabwareQuantity": 8,
    "excludeOptionalThreads": false,
    "plateSets": [
      {
        "plateProtocolId": 101,
        "barcodes": ["PREP001", "PREP002", "PREP003", "PREP004",
                     "PREP005", "PREP006", "PREP007", "PREP008"]
      },
      {
        "plateProtocolId": 104,
        "excludeThread": true
      }
    ]
  }
}
```

### Example: batch a template

Create 3 batches of the template as-is:

```http
POST /orders
Content-Type: application/json

{
  "description": "3 batches from template 7",
  "user": "lab_technician",
  "inventoryScan": false,
  "clearStorage": false,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "template": {
    "templateId": 7,
    "batchCount": 3
  }
}
```

## Method 3: Transfer-list order

A transfer-list order builds an order from an explicit list of source → destination (→ tip) transfers, letting Cellario derive the plates and protocol layout. Use the dedicated endpoint:

```http
POST /orders/transferlist
Content-Type: application/json
```

### `TransferListSetRequest`

| Field               | Type                           | Description                                                                   |
| ------------------- | ------------------------------ | ----------------------------------------------------------------------------- |
| `protocolId`        | int?                           | Protocol to use. Provide this **or** `templateId`.                            |
| `templateId`        | int?                           | Template (and protocol) to use. Provide this **or** `protocolId`.             |
| `transferListItems` | TransferListItemRequest[]      | The transfers that define the order.                                          |
| `labwareTypes`      | LabwareTypeForBarcodeRequest[] | Optional per-barcode labware overrides; otherwise template selection is used. |
| `transferPriority`  | enum?                          | `Source` or `Destination` — which side to prioritize when building plates.    |
| `algorithm`         | enum                           | Order-creation algorithm. Defaults to `Protocol`.                             |
| `scheduleDetail`    | object?                        | Optional scheduling.                                                          |

### `TransferListItemRequest` (key fields)

| Field                                                                                        | Type    | Description                               |
| -------------------------------------------------------------------------------------------- | ------- | ----------------------------------------- |
| `sourceBarcode` / `sourcePlateProtocolId` / `sourceRow` / `sourceColumn`                     |         | Source well.                              |
| `destinationBarcode` / `destinationPlateProtocolId` / `destinationRow` / `destinationColumn` |         | Destination well (required).              |
| `tipBarcode` / `tipPlateProtocolId` / `tipRow` / `tipColumn`                                 |         | Optional tip well.                        |
| `volume`                                                                                     | decimal | Transfer volume.                          |
| `unit`                                                                                       | string? | Volume unit.                              |
| `solventType`                                                                                | string? | Solvent type.                             |
| `isVirtualDestinationBarcode` / `isVirtualTipBarcode`                                        | bool    | Mark destination/tip barcodes as virtual. |

### Example

```http
POST /orders/transferlist
Content-Type: application/json

{
  "description": "Cherry-pick from transfer list",
  "user": "lab_technician",
  "inventoryScan": false,
  "clearStorage": false,
  "shouldBeValidated": false,
  "createDefaultParameters": true,
  "transferListSet": {
    "protocolId": 42,
    "transferPriority": "Source",
    "algorithm": "Protocol",
    "transferListItems": [
      {
        "sourceBarcode": "SRC001",
        "sourcePlateProtocolId": 201,
        "sourceRow": 1,
        "sourceColumn": 1,
        "destinationBarcode": "DST001",
        "destinationPlateProtocolId": 202,
        "destinationRow": 1,
        "destinationColumn": 1,
        "volume": 25.0,
        "unit": "uL"
      }
    ]
  }
}
```

## Scheduling an order

Any of the three methods can include a `scheduleDetail` object (on `order`, `template`, or `transferListSet`) to control when the order is scheduled. Omit it to schedule immediately.

### `OrderScheduleDto`

| Field            | Type      | Description                                                            |
| ---------------- | --------- | ---------------------------------------------------------------------- |
| `scheduleAt`     | datetime? | Absolute date/time at which to schedule the order.                     |
| `scheduleAtUtc`  | datetime? | UTC equivalent of `scheduleAt` (derived from `scheduleAt` if omitted). |
| `scheduledAfter` | int?      | Schedule this order after another order/plate completes.               |
| `protocolStepId` | int?      | Protocol step used as the scheduling anchor.                           |
| `plateNumber`    | int?      | Plate number used as the scheduling anchor.                            |

> For manual (`order`) orders, you cannot create a scheduled order with no samples: if `scheduleAt` or a positive `scheduledAfter` is set but `plateSets` is empty, creation fails with _"Cannot create an Order that has a 'Schedule' but has no samples assigned."_ This specific check is only performed for the manual `order` method, not for `template` or `transferListSet`.

## Validation errors

Order creation returns `400 Bad Request` with a validation problem-details body. Common cases:

| Message                                                                                         | Cause                                                                  |
| ----------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| _"Invalid Order request. Please provide Template or Order."_                                    | Neither `order` nor `template` supplied.                               |
| _"PlateProtocol ID not specified in PlateSet"_                                                  | `plateProtocolId` not found in the protocol.                           |
| _"Input Thread {id} must have starting resources assigned or specified during order creation."_ | No assigned resources and no `resourcePositionId` for an input thread. |
| _"Input Thread {id} - Either ExplicitCount or Barcodes list must be provided"_                  | A plate set has neither barcodes nor `explicitCount`.                  |
| _"Input Thread {id} cannot find any available resource positions..."_                           | `Available` placement found no free positions.                         |
| _"ResourcePosition '{id}' not found."_                                                          | An invalid `resourcePositionId` / output position ID.                  |
| _"Cannot create an Order that has a 'Schedule' but has no samples assigned."_                   | A schedule was set on an order with no samples.                        |

## Related documentation

- [API Examples Overview](examples-overview.md)
- [OpenAPI Endpoints](openapi-endpoints.md)
- [Authentication](authentication.md)
