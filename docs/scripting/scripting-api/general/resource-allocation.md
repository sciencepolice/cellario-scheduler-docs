---
title: Resource Allocation
description: Learn how to control system resources using a script with methods like Allocate, Name, and Release. Allocate claims and guarantees availability of a resource, Name retrieves its name, and Release frees up the resource when no longer required. These method
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

# Allocate Method

## Description

To control resources on the system via a script, you must first claim the resource using the Allocate method. The allocate method ensures the resource is available and in a ready state. If required, you can allocate multiple resources.

When all allocated resources are available, Cellario calls the Execute method.

## Syntax Example

To claim a resource within the AllocateResources method, add a line to select the resource from the resources list, and then call *Allocate*.

```csharp
api.Resources["resource name"].Allocate();
```

In this example, the resource called *resource name* is claimed.

## Method Parameters

No arguments are passed to this method.

## Returns

This method does not return a value.

***

# Name Property

## Description

The Name property is the name of the resource.

## Syntax Example

In this example, the name of the robot resource is shown using the Name property.

```csharp
public override void AllocateResources(IScriptingApiAllocation api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.None, string.Format("Resource Name = {0}", api.Resources["Denso 1"].Name));
}
```

During the Cellario run, a message is displayed in the Cellario message window.

![Resource name displayed in the Cellario message window](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/nameproperty.jpg)

## Property Type

String

***

# Release Method

## Description

After the script finishes processing, the resources claimed by the script should be released.

Releasing the claims tells Cellario that the resources are free to be used by other operations and scripts.

## Syntax Example

In this example, the Release method removes the claim to the resource called *resource name*.

```csharp
api.Resources["resource name"].Release();
```

## Method Parameters

No arguments are passed to this method.

## Returns

This method does not return a value.
