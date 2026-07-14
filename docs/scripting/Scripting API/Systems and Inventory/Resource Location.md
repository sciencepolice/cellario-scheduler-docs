---
title: Resource Location
description: This document explains the 'Resource Property' and 'ResourceName Property' in an API, their significance in resource location, and provides a syntax example for using the 'Resource Property' in a script. Additionally, it highlights that the 'ResourceName 
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.CurrentPlate.CurrentLocation`

# Resource Property

## Description

The Resource property is the handle to the resource to which the resource location belongs.

## Syntax Example

In this example, the Spin operation is followed by the Script operation. The operation
parameter *Execution Event* for the script is set to “Before Move.”

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/resourceproperty.png" size="40" width="240" height="155" position="center" showCaption="false"}

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Location Resource = {0}", api.CurrentPlate.CurrentLocation.Resource.Name));
}
```

During the Cellario order, the Spin operation is completed and then the script is executed before the plate is removed from the MicroSpin resource. The script writes the current plate’s location to the Cellario message log.


::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/resourceproperty2.png" size="88" width="578" height="66" position="center" showCaption="false"}

## Property Type

IScriptingResource

See the section [Resource](./Resource.md) for Resource properties and methods.

***

# ResourceName Property

## Description

The ResourceName property is the name of the resource to which the resource location belongs.

## Syntax Example

See the example in the section Storage Location, [IsEnabled Property](./StorageLocation.md).

## Property Type

String
