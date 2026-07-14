---
title: Plate
description: This document provides a comprehensive guide to the properties and methods of the `api.CurrentPlate` object in Cellario scripting API. Learn about the functionality and syntax of important properties and methods such as `AssociatedLidLocation`, `Barcode`,
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.CurrentPlate`

# AssociatedLidLocation Property

## Description

If the lid has been removed from the plate, this property describes the resource used to store the lid.

## Syntax Example

```csharp
using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            var lidResource = api.CurrentPlate.AssociatedLidLocation;
            if (lidResource != null)
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} lid is at Resource = {1}", api.CurrentPlate.Name, lidResource.ResourceName));
            }
            else
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Lid Resource is null");
            }
        }
    }
}
```

## Property Type

IScriptingResourceLocation. See the section [Resource](./Resource.md).

***

# Barcode Property

## Description

The Barcode Property is th barcode associated with the plate. If the barcode has not been defined in the order or has not been scanned by the system, the barcode property is empty. In this case, you could use the *Name* property to access the plate name.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", api.CurrentPlate.Barcode));
}
```

In this example, the barcode for the current plate is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/barcodeproperty.png" size="88" width="599" height="84" position="center" showCaption="false"}

## Property Type

String

***

# Cancel Method

## Description

The Cancel method cancels all remaining operations for a plate except the final move to storage.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    // Display the current barcode
    string barcode = api.CurrentPlate.Barcode;
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", barcode));
    // Check the barcode has been scanned.
    if (string.IsNullOrEmpty(barcode))
    {
        // Plate does not have a barcode. Remaining steps will be cancelled.
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Plate does not have a barcode. Remaining steps will be cancelled.");
        api.CurrentPlate.Cancel();
    }
}
```

In order 3550, the plate does not have a barcode and the remain steps after the script operation have been cancelled.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/cancelmethod.jpg" size="86" width="619" height="104" position="center" showCaption="false"}

In order 3551 the plate has the barcode 12345.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/cancelmethod2.png" size="76" width="537" height="65" position="center" showCaption="false"}

## Method Parameters

No arguments are required.

## Returns

*True* if the plate was canceled successfully

***

# ContainerType Property

## Description

The ContainerType property describes the type of container.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Container Type = {0}", api.CurrentPlate.ContainerType));
}
```

During the Cellario order, this script displays the current plates ContainerType in the Cellario message log.

## Property Type

ScriptingContainerType, which is an enum with the possible values: Unknown, Plate, Rack

***

# CurrentLocation Property

## Description

The CurrentLocation property describes the current location of the plate.

## Syntax Example

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentlocationproperty.png" size="32" width="186" height="217" position="flex-start" alt="Example thread" showCaption="false"}

In this example, the Spin operation is followed by the script operation. The operation parameter *Execution Event* for the script is set to **Before Move**.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentlocationproperty2.png" size="48" width="317" height="132" position="flex-start" alt="Execution Event parameter set to Before Move" showCaption="false"}

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Location Resource = {0}", api.CurrentPlate.CurrentLocation.ResourceName));
}
```

During the Cellario order, the Spin operation is completed and then the script is executed before the plate is removed from the MicroSpin resource. The script writes the current plate’s location to the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentlocationproperty3.png" size="84" width="574" height="62" position="center" showCaption="false"}

## Property Type

IScriptingResourceLocation. See the section [Resource](./Resource.md).

***

# CurrentProtocol Property

## Description

The CurrentProtocol property provides a handle to the current protocol.

## Syntax Example

See the section [Protocol Thread](<../Protocols and Orders/Protocol Thread.md>) for examples of how to use CurrentProtocol.

## Property Type

IScriptingProtocol

## Protocol Properties and Parameters

### Description

Protocol properties and parameters are collections attached to the Run Order Protocol object. The properties and parameters are read-only in a run order.

:::hint{type="info"}
These properties and parameters are available to both CurrentRun.Protocol and CurrentPlate.CurrentProtocol.
:::

### Protocol Properties

- PropertyId (integer)
- PropertyName (string)
- PropertyDescription (string)
- PropertyValue (string)

**Syntax Example**

```csharp
var property = api.CurrentRun.Protocol.Properties.First();
var pname = property.PropertyName;
var pid = property.PropertyId;
var pvalue = property.PropertyValue;
var pdesc = property.PropertyDescription;
```

### Protocol Parameters

- ProtocolParameterId (integer)
- ParameterName (string)
- Description (string)
- ParameterLevel (string)
- ParameterType (string)
- DefaultValue (string)
- ChoicesList (list of strings)

**Syntax Example**

```csharp
var parameter = api.CurrentRun.Protocol.Parameters.First();
var pname = parameter.ParameterName;
var ppid = parameter.ProtocolParameterId;
var pvalue = parameter.DefaultValue;
var pdesc = parameter.Description;
var plevel = parameter.ParameterLevel;
var ptype = parameter.ParameterType;
var pclist = parameter.ChoicesList;
```

***

# CurrentRun Property

:::hint{type="info"}
CurrentRun is now at the main level (api.CurrentRun), but is also preserved as a property of CurrentPlate to remain backward-compatible.
:::

Functionality as of Cellario v3.5 has each run order look for scripts to be run at the beginning and end of the run. You can execute multiple scripts for each run order state (users can select the run order).

Because Cellario executes the event scripts while there are no active plates, the existing API object api.CurrentPlate is null. To modify plate operations, the function GetPlates is still available, and to modify run order variables, the data object api.CurrentRun is available as of v3.5.

The new api.CurrentRun object is similar to the api.CurrentPlate.CurrentRun but also includes a Protocol object to retrieve protocol data for the run.

## Syntax Examples

```csharp
//pre-3.5 data objects   
    var plateRun = api.CurrentPlate.CurrentRun;
    var plateRunProtocol = api.CurrentPlate.CurrentProtocol;
    var plate = api.CurrentPlate;
 //new 3.5 data objects available for event scripting
    var eventRun = api.CurrentRun;
    var eventProtocol = api.CurrentRun.Protocol;
    var runPlateList = api.GetPlates();
```

See the section [RunOrder](<../Protocols and Orders/RunOrder.md>) for complete information on CurrentRun properties and methods.

***

# CurrentStep Property

## Description

The CurrentStep property provides a handle to the current Cellario operation.&#x20;

When using this property it is important to consider the value of the operation parameter *Execution Event*. The Execution Event parameter determines if the script is executed before or after the robot move.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Step Name={0}", api.CurrentPlate.CurrentStep.StepName));
}
```

This example script displays the name of the current operation in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentstepproperty.png" size="60" width="410" height="62" position="center" showCaption="false"}

## Property Type

IScriptingProtocolRemainingStep (See the section [Remaining Protocol Steps](<../Protocols and Orders/Remaining Protocol Steps.md>))

***

# CurrentStorage Location

## Description

CurrentStorageLocation holds the location information for the plate when stored in a current storage resource.

## Syntax Example

```csharp
foreach(var plate in api.GetPlates())
{
    var location = plate.CurrentStorageLocation;
    if (location != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} is in storage resource {1} Column {2} Row {3}",plate.Name,location.ResourceName, location.Stack, location.Position));
    }
}
```

When the script executes, the *foreach* loop iterates all the plates in the order. If the plate is currently within a storage resource, the string format method generates
a message displaying the name of the storage resource and the location of the plate.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentstoragelocation.png" size="90" width="614" height="122" position="center" showCaption="false"}

## Property Type

IScriptingStorageLocation (See the section [StorageLocation](./StorageLocation.md))

***

# CurrentThread Property

## Description

The CurrentThread property provides a handle to the current Cellario thread.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Thread = {0}", api.CurrentPlate.CurrentThread.ThreadName));
}
```

During the Cellario run, this script displays the thread name for the current plate in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentthreadproperty.png" size="86" width="571" height="63" position="center" showCaption="false"}

## Property Type

IScriptingProtocolThread

***

# EndingLocation Property

## Description

The EndingLocation property is the explicit ending location of the sample, if any. The value is null if there is a single end location for the thread.

:::hint{type="info"}
An explicit ending location cannot be defined via the Cellario GUI.
:::

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    var endLocation = api.CurrentPlate.EndingLocation;
    if (endLocation != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("The ending location for plate {0} will in resource {1} position {2}", api.CurrentPlate.Name,  endLocation.ResourceName, endLocation.Position.Value));
    }
    else
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} does not have a defined ending location", api.CurrentPlate.Name));
    }
}
```

During the Cellario order, the above script displays the ending location for the current plate
if an ending location has been defined.

## Property Type

IScriptingStorageLocation (See the section [StorageLocation](./StorageLocation.md))

***

# Name Property

## Description

The Name property is the name of the plate

The default name for the plate is the thread name plus the plate index.
If the plate has a barcode defined in the order or if the barcode has been scanned by the system, the plate name is updated to the plate barcode.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Name = {0}", api.CurrentPlate.Name));
}
```

During the Cellario run, the script displays the name of the current plate in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/platenameproperty.png" size="86" width="581" height="64" position="center" showCaption="false"}

## Property Type

String

***

# RemainingSteps Property

## Description

The RemainingSteps property is a list of the remaining operation parameters.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Remaining protocol steps");
    foreach(var steps in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Step = {0}", steps.StepName));
    }
}
```

In this example protocol after the script operation, there are Dispense, Read, and Clean operations. During the Cellario run, the script displays all the remaining operations names in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/remainingstepsproperty.png" size="82" width="579" height="181" position="center" showCaption="false"}

## Property Type

IScriptingProtocolRemainingSteps

See the section [Remaining Protocol Steps](<../Protocols and Orders/Remaining Protocol Steps.md>) for the complete list of RemainingSteps properties and methods.

## Add Steps to Plate

You can add an operation to a plate via scripting, but some important knowledge of the system is needed to implement it correctly.

- To create a new step, you must clone it from an existing step using the command `CloneStep(stepname?)`, and then place it into the RemainingSteps collection using the command `InsertStep(step, index)`.
- You must maintain the correct sequence of steps and include new Move steps where needed for the plate to move.
  The system normally inserts Move steps in the protocol automatically during protocol creation, which tell the robot to move a plate from one location to another. If you do not add a Move step in the script, the plate cannot request a robot move and causes a deadlock.
  - If no step name is included, the new step that is created is a copy of the original.
  - If a step name is included, the new step is of that type; however, the resources and parameters for the step are empty. You must add them to the step for it to work correctly.
- The operation `CloneStep(stepname?)` creates another step.
- `RemainingSteps.InsertStep(step, index)` adds the new step to the existing collection of operations for the plate.
  - *index* is optional and reflects where in the current collection to place the step.
  - If *step* is not included, the step is added to the end of the operational steps, just before the End step.

### Syntax Example

```csharp
//CurrentThread will contain all the operations for the thread and can be used to add another similar step to plate
    var moveStep = api.CurrentPlate.CurrentThread.Steps.FirstOrDefault(a => a.StepName == "Move");
    var spinStep = api.CurrentPlate.CurrentThread.Steps.FirstOrDefault(a => a.StepName == "Spin");
// New step to be added must be a clone of existing step
    var newMoveStep1 = moveStep.CloneStep();
    var newMoveStep2 = moveStep.CloneStep();
    var newSpinStep = spinStep.CloneStep();
//To create a new operation, clone a Remaining Step passing in a valid operation name
    var newOpStep = api.CurrentPlate.RemainingSteps.LastOrDefault().CloneStep("Dispense");
//Cloned steps with new operation name do not have resources assigned, must be added if required
    var resList = api.Resources.Values.ToList();
    var resource = resList.Where(a => a.Name.Contains("Dispenser")).FirstOrDefault();
    (newOpStep as IScriptingProtocolRemainingStep).AssignedResources.Add(resource);
//Cloned steps with new operation name does not have parameters assigned, must be added if required
    (newOpStep as IScriptingProtocolRemainingStep).OperationParameters.Add("Prime Volume", 10);
//Add steps to tree without optional position, will be added before End
    api.CurrentPlate.RemainingSteps.InsertStep(newMoveStep1);
    api.CurrentPlate.RemainingSteps.InsertStep(newOpStep);
//Add insert index to InsertStep(step,index) to place at specific place in list
    api.CurrentPlate.RemainingSteps.InsertStep(newMoveStep1,1);
    api.CurrentPlate.RemainingSteps.InsertStep(newSpinStep,1);
```

***

# PlateNumber Property

## Description

The PlateNumber property is the plate index number.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} is plate index {1}", api.CurrentPlate.Name, api.CurrentPlate.PlateNumber));
}
```

In this example, have plates have been ordered. During the Cellario run, the script displays the plate number in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/platenumberproperty.png" size="64" width="456" height="135" position="center" showCaption="false"}

## Property Type

Integer

***

# SendToStorage Method

## Description

The SendToStorage method cancels all remaining operations for the plate and sends it to storage.

## Syntax Example

In the first example below, if the plate does not have a barcode, it is sent to a storage resource using the name of the storage resource.

```csharp
public override void Execute(IScriptingApi api)
{
    // Display the current barcode
    string barcode = api.CurrentPlate.Barcode;
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", barcode));
    if (string.IsNullOrEmpty(barcode))
    {
        // Plate does not have a barcode. Plate will go to storage
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Plate does not have a barcode. Plate will go to storage.");
        api.CurrentPlate.SendToStorage("PicoServe 1 Cart 2");
    }
}
```

In the second example below, if the plate does not have a barcode, it is sent to a storage resource using a *reference* to a resource object.

```csharp
public override void Execute(IScriptingApi api)
{
    // Display the current barcode
    string barcode = api.CurrentPlate.Barcode;
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", barcode));
    if (string.IsNullOrEmpty(barcode))
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Plate does not have a barcode. Plate will go to storage.");
        string storageName = "PicoServe 1 Cart 2";
        var storageResouce = api.Resources.Values.Single(x => x.Name == storageName);
        if (storageResouce != null)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Sending plate {0} to storage resource {1}", api.CurrentPlate.Name, storageName));
            api.CurrentPlate.SendToStorage(storageResouce);
        }
        else
        {
            api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Failed to find storage resource with the name {0}", storageName));
        }
    }
}
```

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/sendtostoragemethod.png" size="92" width="627" height="121" position="center" showCaption="false"}

In this example script, the next free storage location is selected. The plate without the barcode is then sent to the storage location.

```csharp
public override void Execute(IScriptingApi api)
{
    // Display the current barcode
    string barcode = api.CurrentPlate.Barcode;
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", barcode));
    if (string.IsNullOrEmpty(barcode))
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Plate does not have a barcode. Plate will go to storage.");
        string storageName = "PicoServe 1 Cart 2";
        var storageResouce = api.Resources.Values.Single(x => x.Name == storageName);
        if (storageResouce != null)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Sending plate {0} to storage resource {1}", api.CurrentPlate.Name, storageName));
            api.CurrentPlate.SendToStorage(storageResouce);
        }
        else
        {
            api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Failed to find storage resource with the name {0}", storageName));
        }
    }
}
```

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/sendtostoragemethod2.png" size="86" width="583" height="120" position="center" showCaption="false"}

## Method Parameters

There are three overloads for the SendToStorage method. Each overload takes a different set of parameters.

| **Name**     | **Type**                  | **Description**                  |
| ------------ | ------------------------- | -------------------------------- |
| resourceName | String                    | The name of the storage resource |
| resource     | IScriptingResource        | The resource object              |
| location     | IScriptingStorageLocation | A specified storage location     |

```csharp
bool SendToStorage(string resourceName);
```

```csharp
bool SendToStorage(IScriptingResource resource);
```

```csharp
bool SendToStorage(IScriptingStorageLocation location);
```

## Method Return

*True* if plate can be moved to storage.

***

# StartingLocation Property

## Description

The StartingLocation property describes the original starting location for the plate.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    var startingLocation = api.CurrentPlate.StartingLocation;
    if (startingLocation != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} started at position {1} in resource {2}", api.CurrentPlate.Name, startingLocation.Position, startingLocation.ResourceName));
    }
}
```

During the Cellario run, this script displays the starting location and resource for the current plate.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/startinglocationproperty.png" size="92" width="602" height="65" position="center" showCaption="false"}

## Property Type

IScriptingStorageLocation (See the section [StorageLocation](./StorageLocation.md))

***

# Status Property

## Description

The Status property describes the state of the plate within the order.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var plate in api.GetPlates())
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate {0} Status {1}", plate.Name, plate.Status));
    }
}
```

During the Cellario run, this script displays the current status of all the plates in the order.
In this example order, there are three plates. Below is the output from the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/statusproperty.png" size="86" width="586" height="438" position="center" showCaption="false"}

## Property Type

ScriptingPlateStatus, which is an enum with these possible values:

- NotStarted
- Active
- Finished
