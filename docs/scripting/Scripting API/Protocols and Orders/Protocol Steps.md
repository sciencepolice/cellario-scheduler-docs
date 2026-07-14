---
title: Protocol Steps
description: Learn about the important properties in a software program: StepName, StepNotes, and TimesUsed. Discover how these properties impact the operations and resource reuse rates, along with helpful syntax examples to implement them in your code.
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

# StepName Property

## Description

The StepName property describes operation name.

## Syntax Example

See the example in [StepNotes Property](./#stepnotes-property).

## Property Type

String

***

# StepNotes Property

## Description

The notes for the operation defined in the protocol design.

## Syntax Example

In this example protocol, both the Dispense operation and Read operation have step notes.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/stepnotesproperty.png" size="36" width="203" height="302" position="center" showCaption="false"}

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation {0} notes = {1}", step.StepName, step.StepNotes));
    }
}
```

## Property Type

String

***

# TimesUsed Property

## Description

The TimesUsed property controls reuse rates relative to other threads within a Transfer operation. For example, use one tip box for every 10 source plates.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,string.Format("Step Name='{0}' TimesUsed='{1}'", step.StepName, step.TimesUsed));
    }
}
```

In this example protocol, the TimesUsed for the secondary thread has been set to *10*.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/timesusedproperty.png" size="58" width="364" height="213" position="center" showCaption="false"}

During the Cellario run, the script displays the TimesUsed value for all the remaining operation steps.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/timesusedproperty2.png" size="82" width="578" height="123" position="center" showCaption="false"}

## Property Type

Integer
