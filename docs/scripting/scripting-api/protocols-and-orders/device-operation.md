---
title: Device Operation
description: This document provides a comprehensive overview of the Execute method in Cellario, including the Name property and OperationParameters property. It includes syntax examples and screenshots to help illustrate the concepts involved in running operations on 
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

# Execute Method

## Description

The Execute method runs an operation on a resource.

:::hint{type="info"}
Script execution does not wait for the operation to complete. Any code that must be executed after the operation completes should be placed in the ReleaseResources method.
:::

## Syntax Example

See the example script [Clean Washer With Plate](../../samples/csharp-scripts/examples/Clean_Washer_With_Plate.cs).

## Method Arguments

No arguments are passed to this method.

## Returns

This method does not return a value.

***

# Name Property

## Description

The Name property describes the name of the operation.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    string resourceName = "Bravo 1";
    var resource = api.Resources.Values.FirstOrDefault(x => x.Name == resourceName);
    if (resource == null)
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Fatal, "Failed to find resource");
    }
    foreach(var operation in resource.Operations.Values)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Resource {0} can perform operation {1}", resourceName, operation.Name));
    }
}
```

This script displays all the operation that a Bravo dispenser can perform.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/deviceoperationnameproperty.png" size="78" width="538" height="142" position="center" showCaption="false"}

## Property Type

String

***

# OperationParameters Property

## Description

The OperationParameters property describes the parameters associated with a Cellario operation.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    string ResourceName = "Bravo 1";
    var operation = api.Resources[ResourceName].Operations["LiquidTransfer"];
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Operation Name = {0}", operation.Name));
    foreach(var key in operation.OperationParameters.Keys)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Parameter Name='{0}' Value='{1}'", key, operation.OperationParameters[key].ToString()));
    }
    api.Resources[ResourceName].Operations["LiquidTransfer"].OperationParameters["Bravo Protocol"] = "Cellario.test";
}
```

The example script displays all of the operation parameters for the operation LiquidTransfer on a Bravo liquid transfer device.

The script then updates the value for the operation parameter *Bravo Protocol*.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/deviceoperationoperationparametersproperty.png" size="76" width="522" height="81" position="center" showCaption="false"}

## Property Type

Dictionary, where the key is type *string* and the value is type *object*
