---
title: Remaining Protocol Steps
description: Learn about the properties and methods of the "Remaining Protocol Steps" in a script operation. This document covers the "AssignedResources" property, the "GetAvailableResources" method, the "OperationParameters" property, and the "PauseAfter" property. D
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

*Remaining Protocol Steps* is a set of properties and methods common to:

- `CurrentPlate.CurrentProtocol.Threads.Steps`
- `CurrentPlate.CurrentStep`
- `CurrentPlate.CurrentThread.Steps`
- `CurrentPlate.RemainingSteps`

:::hint{type="info"}
`SimulationTime` and `NumberOfLoops` are available on all four of the locations above. `AssignedResources`, `GetAvailableResources`, `OperationParameters`, `Priority`, `TimeOut`, `PauseBefore`, `PauseAfter` and the `CanRemove`/`Remove` methods are only available on `CurrentStep` and on the steps contained in `RemainingSteps`, since only those return the extended remaining-step type. Steps returned by `CurrentPlate.CurrentProtocol.Threads.Steps` and `CurrentPlate.CurrentThread.Steps` are the base protocol step type documented in Protocol Steps, and only expose `StepName`, `StepNotes`, `TimesUsed`, `SimulationTime` and `NumberOfLoops`.
:::

# AssignedResources Property

## Description

The AssignedResourcesProperty holds a list of the assigned resources.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Remaining Steps"); foreach (var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Step name = {0}", step.StepName));
        foreach (var assignedResource in step.AssignedResources)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("\tAssigned Resource Name = {0}", assignedResource.Name));
        }
    }
}
```

## Property Type

Collection of IScriptingResource

***

# GetAvailableResources Method

## Description

The GetAvailableResources method produces a list of resources that can be assigned to the operation.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach (var step in api.CurrentPlate.RemainingSteps)
    {
        if (step.StepName != "Move")
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Step name = {0}", step.StepName));
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Assigned Resources");
            foreach (var assigned in step.AssignedResources)
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, assigned.Name);
            }
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Available Resources");
            foreach (var available in step.GetAvailableResources())
            {
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, available.Name);
            }
        }
    }
}
```

In this example protocol, after the script operation there is a Read operation with the assigned resource *InCell\_01*. On this system, there are three InCell devices. Devices InCell\_02 and InCell\_03 are capable but have not been assigned.

During the Cellario run, the script displays a list of assigned resources and then displays a list of resources that can be assigned to the operation.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/getavailableresourcesmethod.png" size="84" width="566" height="162" position="center" showCaption="false"}

## Method Parameters

This method does not require any arguments.

## Returns

Collection of IScriptingResource

***

# OperationParameters Property

## Description

The OperationParameters property describes the operation parameters associated with the operation step.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation name='{0}'", step.StepName));
        foreach(KeyValuePair<string,object> opparam in step.OperationParameters)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation parameter name='{0}' value={1}'", opparam.Key, opparam.Value.ToString()));
        }
    }
}
```

In this example protocol, the script is set to execute after Move. After the script operation,
there is a Read operation with the assigned resource *InCell\_01*.

During the Cellario order, the script displays the remaining operations steps and the operation parameters for each step.

![Remaining operation steps and their operation parameters](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/operationparametersproperty.png)

## Property Type

A dictionary list of operation parameters

The key is the parameter name, with type *string*. The value is the parameter value, with type *object*.

***

# PauseAfter Property

## Description

The PauseAfter property sets the time in hours\:minutes\:seconds (maximum 99:59:59) that the protocol holds the plate on the resource after the operation has completed.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation Name='{0}' PauseBefore='{1}' PauseAfter='{2}'", step.StepName, step.PauseBefore, step.PauseAfter));
    }
}
```

In this example protocol, the operations are performed in the following order: Plate > Move > Script > Read > Move > End. The script is set to execute after Move. The Read operation has a two second pause before execution, and a ten second pause after execution.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/pauseafterproperty.png" size="50" width="325" height="139" position="center" showCaption="false"}

During the Cellario run, the script displays the pause before and after for each of the remaining operation steps.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/pauseafterproperty2.png" size="96" width="632" height="103" position="center" showCaption="false"}

## Property Type

TimeSpan

***

# PauseBefore Property

## Description

The PauseBefore property sets the time in hours\:minutes\:seconds (maximum 99:59:59) that the protocol holds the plate on the resource before the operation starts.

## Syntax Example

See the example in [PauseAfter Property](./#pauseafter-property).

## Property Type

TimeSpan

***

# Priority Property

## Description

The Priority property sets the priority for how Cellario's internal scheduler handles the operation.

Higher priority operations are treated preferentially by the scheduler, which means that
when a high-priority operation is available, the robot moves the sample to the associated resource before handling lower-priority operations.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,string.Format("Step Name='{0}' Priority='{1}'", step.StepName, step.Priority));
    }
}
```

During the Cellario run, the script displays the step name and priority for all the remaining operations.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/priorityproperty.png" size="84" width="580" height="204" position="center" showCaption="false"}

## Property Type

An integer between 1 and 10, where 10 is the highest priority and 1 is the lowest priority.

***

# TimeOut Property

## Description

The timeout is the duration of time that Cellario waits before assuming this operation has failed. Once it elapses, Cellario issues a Timeout error and disconnects from the device.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach(var step in api.CurrentPlate.RemainingSteps)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,string.Format("Step Name='{0}' TimeOut='{1}'", step.StepName, step.TimeOut));
    }
}
```

During the Cellario run, the script displays the step name and timeout for each of the remaining operation steps.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/timeoutproperty.png" size="84" width="576" height="121" position="center" showCaption="false"}

## Property Type

TimeSpan

***

# CanRemove Method

## Description

The CanRemove method checks if the given step can be removed from the remaining steps.

## Syntax Example

See the example in [RemoveMethod](./#remove-method).

## Method Parameters

```csharp
bool CanRemove(IScriptingProtocolRemainingStep step);
```

*Step* (type IScriptingProtocolRemainingStep) is the step to be removed.&#x20;

## Returns

Boolean; *true* if the step can be removed

***

# Remove Method

## Description

The Remove method checks if the given step can be removed from the remaining steps.

## Syntax Example

The Cellario protocol for this example has a script operation followed by Dispense, Read, Dispense, Spin, and End.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/removemethod.png" size="36" width="209" height="386" position="center" showCaption="false"}

When removing Cellario operations, it’s important to remember that Cellario automatically
inserts Move operations in between the operations that are shown in the Protocol Designer. You must make sure that the Move operation is also removed; otherwise, deadlocks might occur.

The script in this example lists the remaining operations in the Cellario message window.

The script finds the index of the Read operation and then removes the Read operation.
The index for the remaining operations are then updated, which makes the index for Move the same as the Read operation that was removed. The script then removes the second Move operation.

The script then lists all of the remaining operations after the two operations have been removed.

```csharp
public class EgRemove : AbstractScript
{
    private void ShowRemainingSteps(IScriptingApi api)
    {
        foreach(var step in api.CurrentPlate.RemainingSteps)
        {
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,string.Format("Step Name='{0}'", step.StepName));
        }
    }
    private void FindAndRemoveStep(IScriptingApi api)
    {
        // Find the step to remove
        int indexOfStepToRemove = -1;
        for(int i =0; i < api.CurrentPlate.RemainingSteps.Count; i++)
        {
            if (api.CurrentPlate.RemainingSteps.ElementAt(i).StepName == "Read")
            {
                indexOfStepToRemove = i;
            }
        }
        if (indexOfStepToRemove!= -1)
        {
            // remove read
            RemoveOperation(api, indexOfStepToRemove);
            // remove move
            RemoveOperation(api, indexOfStepToRemove);
        }
    }
    private void RemoveOperation(IScriptingApi api, int operationIndex)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Index of operation to be removed={0}", operationIndex));
        if (operationIndex != -1)
        {
            var stepToRemove = api.CurrentPlate.RemainingSteps.ElementAt(operationIndex);
            // remove move step
            string stepName = stepToRemove.StepName; api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
            string.Format("Test if {0} operation can be removed", stepName));
            if (api.CurrentPlate.RemainingSteps.CanRemove(stepToRemove))
            {
                if (api.CurrentPlate.RemainingSteps.Remove(stepToRemove))
                {
                    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("{0} operation successful removed", stepName));
                }
                else
                {
                    api.Messaging.WriteError(ScriptErrorSeverity.Error, string.Format("Failed to remove {0} operation", stepName));
                }
            }
            else
            {
                api.Messaging.WriteError(ScriptErrorSeverity.Error, string.Format("{0} operation can not be removed", stepName));
            }
        }
    }
    public override void Execute(IScriptingApi api)
    {
        // Display the remaining steps api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Remaining steps
        before remove");
        ShowRemainingSteps(api);
        // Remove Read step and the Move step FindAndRemoveStep(api);
        // display the remaining steps after the remove api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Remaining steps
        after remove");
        ShowRemainingSteps(api);
    }
}
```

The Cellario Message Log below is highlighted to show where the Read and Move operations have been removed.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/removemethod2.png" size="84" width="587" height="615" position="center" showCaption="false"}

## Method Parameters

```csharp
bool Remove(IScriptingProtocolRemainingStep step);
```

*Step* (type IScriptingProtocolRemainingStep) is the step to be removed.

## Returns

Boolean; *true* if the step was removed

***

# SimulationTime Parameter

## Description

SimulationTime is a generic operation parameter available as read/write that can be updated via scripting. The parameter works in runtime and analysis modes.

## Syntax Example

```csharp
TimeSpan newSimTime = TimeSpan.FromSeconds(1800);
api.CurrentPlate.RemainingSteps.First().SimulationTime = newSimTime;
```

## Property Type

TimeSpan

***

# NumberOfLoops Parameter

## Description

The scripting API operation step property has a parameter, NumberOfLoops, with an integer value that can be read or written to. This parameter applies only to Loop End operation steps.

The value specifies the number of times to loop the operations that occur between the Loop Start and Loop End steps.

:::hint{type="warning"}
You should modify only these types of operations by scripting.
:::

## Syntax Example

```csharp
var steps = api.CurrentPlate.RemainingSteps.ToList();
var loopCount = steps.FirstOrDefault(a => a.StepName == "Loop End").NumberOfLoops;
if (loopCount < 2)
    {
       steps.FirstOrDefault(a => a.StepName == "Loop End").NumberOfLoops = 5;
    }
```

In this example, a script with a given plate receives the remaining steps, finds the first Loop End operation, and then gets the current value for NumberOfLoops. If the value is less than 2, then the script sets it to 5.
