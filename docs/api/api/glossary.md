# Glossary

## API Terms

### Authentication

The process of verifying client identity before granting access to API endpoints.

### Bearer Token

A JWT token used for authenticated requests, included in the Authorization header.

### Endpoint

A specific URL path that accepts HTTP requests and returns responses.

## Cellario Terms

### ACell

A HighRes Biosolutions robotic arm used to transfer labware across devices

### AuditLog

A subcategory of Events focused on tracked user actions in the UI.

### AutoRecovery

AutoRecovery is a way for a Script or Resource to get out of trouble.

### batch

A set of plates that are related to each other by how CellarioScheduler processes them

### Cell

A whole being controlled by a single Cell Controller of a Cellario. This is usually a cluster of Resources connected by various robotics, though Virtual Systems exist that do not have the automated connections.

### Cell Controller

The core scheduling engine of Cellario Scheduler that orchestrates all laboratory automation workflows within a single Cell/System. It manages resource allocation, device coordination, protocol execution sequencing, and real-time decision making to optimize throughput while handling constraints such as critical timings, resource conflicts, and operational dependencies. The Cell Controller continuously evaluates the current state and determines the next optimal actions to execute across all active protocols and orders.

### Critical Timing

Critical Timing is used to confirm that specified ProtocolSteps are completed within a specified time window. The existence of Critical Timings influences the Scheduler decision making.

### deadlock

An error and halt to a run that occurs when the scheduler cannot make progress because resources are waiting for each other in circular dependencies, or when no valid execution path exists due to conflicting resource requirements that cannot be resolved

### Device

A subcategory of Resource, Devices are what the Cell Controller has active control over while making the decisions.

### driver

A software component that lets CellarioScheduler and a device communicate with each other, and lets CellarioScheduler control the device (also called a "device driver")

### driving thread

In order templates, the designated thread whose labware quantity or barcode count determines the input ratios for all other threads in the template. The driving thread acts as the primary scaling factor that drives the proportional allocation of resources across the entire order.

### envelope

The collective locations that a robotic arm accesses in a work cell

### Event

A timestamped record of significant occurrences within the System, including device state changes, protocol step completions, errors, user actions, and system status updates. Events form the comprehensive audit trail used for monitoring, debugging, compliance tracking, and historical analysis of laboratory automation workflows.

### Inventory

The tracking and management of physical laboratory items such as plates, tubes, reagents, tips, and consumables within the System. Inventory management includes barcode scanning, location tracking, quantity monitoring, and status updates as items move through automated workflows, providing real-time visibility into available resources and their current locations.

### Labware

Physical containers used in laboratory workflows, including microplates, tubes, tips, reservoirs, and other standardized laboratory items that can be manipulated by robotic systems. Labware items adhere to SBS footprint standards for consistent robotic handling and have defined specifications (dimensions, well counts, capacities). They can be tracked through the System as they move between devices and locations during protocol execution.

### Merge

A relationship between two or more ProtocolSteps to confirm that all the ProtocolThread synchronize and do not proceed until all of them have reached the same point.

### nest

A designated physical location on a device or tray where labware can be delivered (placed) or retrieved (picked). Nests provide standardized positioning for robotic handling and can refer to specific numbered locations on trays (typically five locations per Prime tray) or individual plate positions on various laboratory devices.

### Operation

A discrete, executable task performed by a single device within the laboratory automation system. Operations represent specific actions such as liquid transfers, plate movements, incubation, sealing, or other device-specific functions. Each operation is atomic and serves as a building block for larger protocol workflows, with defined inputs, outputs, and execution parameters.

### Order

An executable instance of a protocol configured with specific labware, samples, and parameters. Orders represent concrete work requests that define what samples to process, which protocol steps to execute, and where outputs should be stored. Orders can be created from templates and are scheduled by the Cell Controller for execution across the laboratory automation system.

### OrderOutput

Designated storage locations where completed order results and processed labware will be placed upon completion. OrderOutputs are configured when creating orders through the API and specify the target destinations (such as storage devices, specific nests, or output trays) where the final products of the automated workflow should be delivered.

### OrderSample

Input labware items (plates, tubes, or other containers) that are specified when creating orders through the API. OrderSamples define the starting materials that will be processed during order execution, including their identities, locations, and any associated metadata such as barcodes or sample types.

### persistent storage

A storage nest state where the plate (output) is not removed from storage after an order run is completed

### protocol

A reusable workflow schema that defines a complete laboratory automation procedure. Protocols consist of multiple threads (parallel execution paths) containing sequences of operations, scripts, and device interactions. They serve as the blueprint for creating orders and specify the steps, parameters, timing constraints, and resource requirements needed to execute complex laboratory workflows. Protocols can be retrieved and referenced through the API when creating new orders.

### ProtocolStep

An individual step within a protocol workflow that defines a specific action to be performed during execution. ProtocolSteps encapsulate either script executions or device operations with their associated parameters, timing constraints, and protocol-specific configurations. Each step represents a discrete unit of work that contributes to the overall protocol execution sequence and can be monitored and tracked through the API as orders progress.

### random stacker

A stacker that is partitioned into shelves and lets labware be picked or placed at any time, and to specified locations (shelves)

See Also serial stacker.

### ratio

The relationship between the number of samples transferred from one thread to another in a Transfer operation

### resource pool

Two or more identical devices that are assigned together to perform an operation

### Resource

Any component of a system that the Cell Controller can use. These range from liquid handlers and storage devices to plate sealers and even waste containers.

### RunOrder

An instance of a protocol with the plates that should be run.

### SampleOperation

A subcategory of Events focused on the plate activities.

### Scheduler

A piece of software responsible for decisions on what is to be executed. This is a common component in operating systems.

### Script

A piece of code being executed in the context of a Cellario Protocol.

### serial stacker

A stacker that has no partitions, designed for stacked labware that is accessed in a first-in/last-out method

See Also random stacker.

### shelf

One of multiple locations within a random stacker that holds a a labware item

### stacker

A component that stores labware in a tower format

See Also serial stacker, random stacker.

### Subscriber

_To be defined based on current Subscriber concept_

### System

A whole being controlled by a single Cell Controller of a Cellario. This is usually a cluster of Resources connected by various robotics, though Virtual Systems exist that do not have the automated connections.

### template

A set of saved order information that can be applied to a protocol when creating an order

### thread

Interdependent operations in a CellarioScheduler protocol or Solution method that start with an input plate and end with an End step

Each thread defines storage locations, deck positions (in Solution methods), and the action sequence performed on that labware during protocol/method execution.

### virtual plate

A single plate that acts as multiple plates in an order

### Well Transfer

Transferring content between two wells on either a singular microplate or two different microplates.

### work cell

A set of devices (such as liquid handlers, storage, sealers) that are combined to perform dedicated laboratory operations

### ZCell

A HighRes Biosolutions robotic arm used to transfer labware across devices where higher reaches and payload capacities are needed

## Laboratory Automation Terms

### LIMS

Laboratory Information Management System

### Plate Protocol

See ProtocolThread

### Protocol Step

An execution of either a Script or an Operation. These are Protocol specific, whereas Scripts can be generic, and Operations are always generic, meaning that any Protocol specific information will be stored in a ProtocolStep.

### Robotic Liquid Handler

_To be defined based on typical usage in Cellario context_

## Related Documentation

- [Overview →](overview.md)
- [Events →](events.md)
- [Examples →](examples.md)
