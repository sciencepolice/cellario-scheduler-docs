---
title: Resource
description: Learn about resource management in Cellario script programming, including properties like DeviceState, IsEnabled, IsErrored, IsSimulated, Name, and Operations. Discover how to use the GetAvailableLocation method to find storage space, and explore examples
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

*Resource* is a set of properties and methods common to:

- `api.CurrentPlate.AssociatedLidLocation`
- `api.CurrentPlate.CurrentLocation`
- `api.Resources`
- Storage Location (`.location`)

# DeviceState Property

## Description

The DeviceState property indicates the current state of the resource.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api) 
{ 
  api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Device state = {0}", api.CurrentPlate.CurrentLocation.Resource.DeviceState)); 
}
```

Cellario output shows the device state for the device claimed by the current plate.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/devicestateproperty.png" size="68" width="441" height="63" position="center" showCaption="false"}

## Property Type

ScriptingDeviceState

Possible values are:

- **None** — Not a device
- **Errored** — Device is errored
- **Unconnected** — Device is ready for Connect command
- **Connecting** — Device is connecting
- **Connected** — Device is connected, read for Initialize command
- **Initializing** — Device is initializing
- **Ready** — Device is initialized and ready to perform operations
- **Busy** — Device is busy performing an operation

***

# GetAvailableLocation Method

## Description

The GetAvailableLocation meethod finds the next available location within the storage resource.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    string resourceName = "SteriStore 1";
    var resource = api.Resources.Values.Single(x => x.Name == resourceName);  
    if (resource != null)
    {
        var location = resource.GetAvailableLocation();  
        if (location != null)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Storage Stack={0} Position={1} in resource {2}", location.Stack, location.Position, location.ResourceName));
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

## Method Parameters

No arguments are passed to this method.

## Returns

IScriptingStorageLocation

***

# IsEnabled Property

## Description

The IsEnabledProperty checks if the resource is enabled or disabled.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api) 
{ 
  foreach(IScriptingDeviceResource device in api.Resources.Values) 
    { 
      api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Scripting Resource Name = {0} IsEnabled={1}", device.Name, device.IsEnabled)); 
    }
}
```

This example script iterates all the resources displaying the resource name and IsEnabled value.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/isenabledproperty.png" size="88" width="592" height="81" position="center" showCaption="false"}

## Property Type

Boolean; *true* when the device is enabled

***

# IsErrored Property

## Description

The IsErrored property is a status flag, denoting if an error has occurred on the resource.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    var resource = api.CurrentPlate.CurrentLocation.Resource;  
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Resource {0} IsErrored={1}", resource.Name, resource.IsErrored));
}
```

Example Cellario output showing the error state of the resource claimed by the current plate:

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/iserroredproperty.png" size="62" width="409" height="64" position="center" showCaption="false"}

## Property Type

Boolean; *true* when an error has occurred

***

# IsSimulated Property

## Description

The IsSimulated property checks if the device is simulated or unsimulated.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(IScriptingDeviceResource device in api.Resources.Values)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Scripting Resource Name = {0} IsSimulated={1}", device.Name, device.IsSimulated));
    }
}
```

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/issimulatedproperty.png" size="88" width="622" height="142" position="center" showCaption="false"}

## Property Type

Boolean; *true* when simulated

***

# Name Property

## Description

The Name property is the name of the resource.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(IScriptingDeviceResource device in api.Resources.Values)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Scripting Resource Name = {0}", device.Name));
    }
}
```

This example script iterates all the scripting resources and displayed the resource name
in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/resource-nameproperty.png" size="82" width="568" height="101" position="center" showCaption="false"}

## Property Type

String

***

# Operations Property

## Description

The Operations property is a dictionary of Cellario operations that the specified resource can perform.

## Syntax Example

```csharp
foreach(var opp in api.Resources["Bravo 1"].Operations.Keys)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation Name = {0}", opp));
}
```

Example Cellario output showing all the operation for a resource called *Bravo 1*:

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/operationsproperty.png" size="62" width="425" height="145" position="center" showCaption="false"}

:::hint{type="info"}
`resource.Operations` is a `DeviceOperationDictionary`. Its indexer automatically creates a named `ScriptedDeviceOperation` on first access if one does not already exist for that key. As a result, `resource.Operations["Spin"]` never returns null and never throws a KeyNotFound exception — the operation is created on demand. This is why example scripts can set values such as `resource.Operations["Spin"].OperationParameters[...]` directly without first checking whether the operation exists.
:::

## Property Type

A dictionary of ScriptedDeviceOperation where the dictionary key is the operation name.

See the section [Device Operation](<../Protocols and Orders/Device Operation.md>) for the properties and method.

***

# ResourceType Property

## Description

Resources within Cellario are classified by resource type. The classification enables Cellario to group like resources into resource pools.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var resource in api.Resources.Values)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Resource Name={0} Resource Type={1}", resource.Name, resource.ResourceType));
    }
}
```

Example Cellario output, showing the resource name and resource type:

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/resourcetypeproperty.png" size="84" width="576" height="240" position="center" showCaption="false"}

## Property Type

String

***

# Modifying Cellario Inventory

**(GetStorageLocations and UpdateStorageLocation Methods)**

A script can read the inventory in storage devices and reserve storage by adding barcodes and/or labware types to resource positions.

:::hint{type="info"}
This function cannot modify resource positions where plates are already identified as part of a run order (any resource position with an order sample ID).
:::

## Syntax Example

```csharp
var sampleTypeName = "Corning96_Cellstar_Black"; 
  //note - will fail if not valid
var barcode = "SRC00010001";
var resource = api.Resources.FirstOrDefault(a => a.Value.Name == resourceName).Value;
var rspList = resource.GetStorageLocations();
var resourcePosition = rspList.First();
resourcePosition.Barcode = barcode;
resourcePosition.SampleType = sampleTypeName;
var updated = resource.UpdateStorageLocation(resourcePosition);
if (updated)
{
//success!
}
else
{
//fail
}
```

***

# Resource Families

## Description

In addition to device resources, two marker resource families identify specialized resource kinds:

- **IScriptingStorageUnit** — a storage unit resource
- **IScriptingLidHotel** — a lid hotel resource

Both are `IScriptingResource` and expose the same base resource members documented on this page (such as `Name`, `IsEnabled`, `ResourceType`, and `Operations`). Their distinct interface types let scripts recognize a resource's role when iterating `api.Resources`.
