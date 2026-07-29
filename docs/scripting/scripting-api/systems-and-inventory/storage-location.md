---
title: StorageLocation
description: Learn about the properties of the StorageLocation object, such as IsEmpty, IsEnabled, Position, Resource, ResourceName, Stack, Unit, ThreadName, and Steps. Get syntax examples for using these properties in scripts, including information on their respectiv
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

*StorageLocation* is a set of properties common to:

- `CurrentPlate.CurrentStorageLocation`
- `CurrentPlate.EndingLocation`
- `CurrentPlate.StartingLocation`

# IsEmpty Property

## Description

The IsEmpty property checks if a storage location is empty.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    string resourceName = "SteriStore 1";
    var resource = api.Resources.Values.Single(x => x.Name == resourceName);
    if (resource != null)
    {
        var location = resource.GetAvailableLocation();
        if ((bool)location.IsEmpty)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "The location is empty");
        }
        else
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "The location is already populated");
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

During the Cellario run, the script finds a handle to the resource *SteriStore 1*, then find
an available location. Using the method IsEmpty, the location is confirmed to be empty.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/isemptyproperty.png" size="86" width="575" height="57" position="center" showCaption="false"}

## Property Type

Nullable Boolean

***

# IsEnabled Property

## Description

The IsEnabled property checks if a storage location is enabled.

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
            if ((bool)location.IsEnabled)
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "The location is enabled");
            }
            else
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "The location is disabled");
            }
        }
    }
}
```

During the Cellario run, the script finds a handle to the resource *SteriStore 1*, then finds
an available location. Using the method IsEnabled, the location is confirmed to be enabled.

## Property Type

Nullable Boolean

***

# Position Property

## Description

The Position property describes the row location of the storage space with the storage resources.

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
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Storage Stack={0} Position={1}", location.Stack, location.Position));
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

In this example, the SteriStore has 21 pitch stackers and 50 plates have been ordered.
Therefore the next available storage location with SteriStore is stacker 3, position 9.

During the Cellario run, the script finds a handle to the SteriStore resource.
Using the method GetAvailableLocation, the next available storage location is found.
The position and stack for the storage location are displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/positionproperty.png" size="84" width="573" height="63" position="center" showCaption="false"}

## Property Type

Nullable integer

***

# Resource Property

## Description

The Resource property describes the scripting resource, which contains the storage location.

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
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Storage Stack={0} Position={1} in resource {2}", location.Stack, location.Position, location.Resource.Name));
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

During the Cellario run, the script finds a handle to the SteriStore resource. Using the method GetAvailableLocation, the next available storage location is found.

The position, stack, and resource name for the storage location are displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/locationresourceproperty.png" size="82" width="575" height="65" position="center" showCaption="false"}

## Property Type

IScriptingResource (See the section [Resource](./resource.md))

***

# ResourceName Property

## Description

The ResourceName property describes the scripting resource name, which contains the storage location.

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

During the Cellario run, the script finds a handle to the SteriStore resource. Using the method GetAvailableLocation, the next available storage location is found.

The position, stack, and resource name for the storage location are displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/resourcenameproperty.png" size="84" width="574" height="62" position="center" showCaption="false"}

## Property Type

String

***

# ResourcePositionId Property

## Description

The ResourcePositionId property returns the database ResourcePosition identifier for the storage location. This is the underlying record id that Cellario uses to track the position in its inventory.

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
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Storage location ResourcePositionId={0}", location.ResourcePositionId));
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

During the Cellario run, the script finds a handle to the SteriStore resource, then finds the next available storage location and displays its ResourcePositionId in the Cellario message window.

## Property Type

Nullable integer (read-only)

***

# Stack Property

### Description

The Stack property describes the column location of the storage space with the storage resources.

## Syntax Example

See the [ResourceName Property](./#resourcename-property) section of StorageLocation.

## Property Type

Nullable integer

***

# Unit Property

## Description

The Unit property describes the scripting storage resource, which contains the storage location.

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
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Storage Unit={0}", location.Unit.Name));
        }
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, string.Format("Unable to find resource {0}", resourceName));
    }
}
```

During the Cellario run, the script finds a handle to the SteriStore resource.
Using the method GetAvailableLocation, the next available storage location is found.

The resource name for the storage unit is displayed in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/unitproperty.png" size="88" width="577" height="64" position="center" showCaption="false"}

## Property Type

IScriptingStorageUnit

***

# ThreadName Property

## Description

The ThreadName property describes the name of the protocol thread.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var thread in api.CurrentPlate.CurrentProtocol.Threads)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Thread name = {0}", thread.ThreadName));
    }
}
```

During the Cellario protocol, the script displays a list of all the thread names from the current protocol.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/threadnameproperty.png" size="88" width="600" height="107" position="center" showCaption="false"}

## Property Type

String

***

# Steps Property

### Description

The Steps property describes all of the step operations in the thread, in order of execution.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    var thread = api.CurrentPlate.CurrentThread;
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Thread '{0}' has {1} steps", thread.ThreadName, thread.Steps.Length));
    foreach(var steps in thread.Steps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Step name {0}", steps.StepName));
    }
}
```

In this example, the protocol has seven operation steps (Plate, Script, Dispense, Read, Incubate, Read, End).

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/stepsproperty.png" size="32" width="202" height="389" position="center" alt="Example thread" showCaption="false"}

The script displays the number of operations, and then the name for each operation.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/stepnotesproperty2.png" size="64" width="438" height="165" position="center" showCaption="false"}

## Property Type

IScriptingProtocolStep
