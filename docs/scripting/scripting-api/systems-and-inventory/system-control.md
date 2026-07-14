---
title: System Control
description: Learn about the `api.System` class in Cellario and its three key aspects: the `Pause` method for temporarily halting the system, the `Stop` method for completely stopping it, and the `SystemState` property that indicates the current state of the Cellario 
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.System`

# Pause Method

## Description

The Pause method pauses the Cellario system.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "System will pause");
    api.System.Pause();
}
```

## Method Arguments

No arguments are passed to this method.

## Returns

This method does not return a value.

***

# Stop Method

## Description

The Stop method stops the Cellario system.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "System will stop");
    api.System.Stop();
}
```

## Method Arguments

No arguments are passed to this method.

## Returns

This method does not return a value.

***

# SystemState Property

## Description

The SystemState property displays the current Cellario system state in the Cellario message window.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current System State = {0}", api.System.SystemState));
}
```

## Property Type

ScriptingSystemState

Possible values are:

- Unknown
- Errored
- Stopped
- Starting
- Running
- Pausing
