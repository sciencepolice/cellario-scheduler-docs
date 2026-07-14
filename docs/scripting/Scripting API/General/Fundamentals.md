---
title: Fundamentals
description: Discover the properties and methods of the Cellario scripting API, including access to labware types, plate handling, data storage, sample retrieval, analysis context determination, messaging capabilities, resource listing, and system status control.
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

# AllLabware Property

`api.AllLabware`

## Description

The AllLabware list gives access to all the defined Cellario labware types.

## Syntax Example

Displays a list of all the labware defined in Cellario

```csharp
public override void Execute(IScriptingApi api)
{ 
    foreach (var labware in api.AllLabware)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Labware Name = {0}", labware.Name));
    }  
}
```

Example Cellario output:

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/alllabwareproperty.png" size="72" width="445" height="119" position="center" showCaption="false"}

## Property Type

IScriptingLabwares

***

# CurrentPlate Property

`api.CurrentPlate`

## Description

The CurrentPlate property provides a handle to the plate that has trigged the script execution.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Name = {0}", api.CurrentPlate.Name));
}
```

During the Cellario run, the script displays the name of the current plate in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/currentplateproperty.png" size="96" width="577" height="60" position="center" showCaption="false"}

## Property Type

IScriptingPlate

***

# Data Property

`api.Data`

## Description

The Data property is a persistent data storage mechanism for per-plate, per-run and per-step data.

## Syntax Example

See example scripts Human-readable Label and [Run Data Next](../../samples/csharp-scripts/examples/Run_Data_Next.cs).

## Property Type

IScriptingApiData

***

# GetPlates Method

`api.GetPlates`

## Description

The GetPlates method shows all currently active samples for the run.

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
In this example order, there are three plates. Following is the output from the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/getplatesmethod.png" size="96" width="586" height="102" position="center" showCaption="false"}

## Method Parameters

This method does not require any arguments.

## Returns

An array of IScriptingPlate

***

# GetPlatesForCurrentThread Method

`api.GetPlatesForCurrentThread`

## Description

The GetPlatesForCurrentThread method shows all currently active plates for the current thread.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    var plates = api.GetPlatesForCurrentThread(); api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("There are {0} plates in thread {1}", plates.Length, api.CurrentPlate.CurrentThread.ThreadName));
    foreach(var plate in plates)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Plate Number={0} Name={1}", plate.PlateNumber, plate.Name));
    }
}
```

During the Cellario run, this script displays the total number of plates in the current thread
called “Assay Plate 0” and shows the plate number and name for each plate in the current thread.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/getplatesforcurrentthreadmethod.png" size="96" width="577" height="121" position="center" showCaption="false"}

## Method Parameters

This method does not require any arguments.

## Returns

An array of IScriptingPlate

***

# IsExecutingInAnalysis Property

`api.IsExecutingInAnalysis`

## Description

When a Cellario order is simulated in Run Analysis, Cellario will execute the script.
In some situations, this may cause undesired results. For example, if the script was to update an LIMS system or prime a device.

The property IsExecutingInAnalysis denotes if the script is running in the context of Run Analysis rather than a live run.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    if (api.IsExecutingInAnalysis)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Simulated Run");
    }
    else
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Live Run");
    }
}
```

During a live Cellario run the message “Live Run” is displayed in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/isexecutinginanalysisproperty.png" size="96" width="579" height="64" position="center" showCaption="false"}

## Property Type

Boolean; *true* if this script is executing in the context of run analysis rather than a live run

***

# Messaging Property

## Description

The Messaging property shows messages and dialogs, send emails.

## Syntax Example

See the example script [Log Message](../../samples/csharp-scripts/examples/Log_Message.cs).

## Property Type

IScriptingApiFrameworks

***

# Resources Property

## Description

The Resources property produces a list of all the Cellario resources.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach (var resource in api.Resources)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Resource Name = {0}", resource.Key));
    }
}
```

Displays a list of all the Cellario resources

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/resourcesproperty.png" size="96" width="564" height="183" position="center" showCaption="false"}

## Property Type

A dictionary of IScriptingDeviceResource

&#x20;The dictionary key is the resource name.

***

# System Property

## Description

The System property shows the system status and control.

## Syntax Example

Displays the current Cellario system state in the Cellario message window

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current System State = {0}", api.System.SystemState));
}
```

## Property Type

IScriptingApiSystem
