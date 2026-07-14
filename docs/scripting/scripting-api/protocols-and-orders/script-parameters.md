---
title: Script Parameters
description: Learn how to define and read per-script custom parameters in Cellario. Scripts stored in the Script Library can declare named parameters with a type, default value, and choices, and read them at run time through api.CurrentScript and the IScriptParameter members.
docTags: 
createdAt: Mon Jul 14 2026 00:00:00 GMT+0000 (Coordinated Universal Time)
---

`api.CurrentScript`

Scripts stored in the Script Library can define custom parameters. Each parameter has a name, type, default value, and (optionally) a set of choices, configured per Script Library entry. At run time a script reads its configured parameters through `api.CurrentScript`, allowing the same script to behave differently depending on how the library entry is configured.

# CurrentScript Property

## Description

The CurrentScript property returns the script definition for the currently executing script. It is of type `IScriptingApiScript`, which exposes the collection of parameters configured for the script in the Script Library.

## Property Type

`IScriptingApiScript`, which exposes:

- ScriptingScriptParameters (`List<IScriptParameter>`, read/write) — the custom parameters configured for this script

***

# Script Parameter

## Description

Each entry in `api.CurrentScript.ScriptingScriptParameters` is an `IScriptParameter`, describing a single custom parameter configured for the script.

## Properties

- ScriptParameterId (integer)
- ScriptId (integer)
- ParameterName (string)
- ParameterType (string)
- DefaultValue (string)
- Description (string)
- Choices (string)
- ChoicesList (`List<string>`)

## Syntax Example

This example reads a parameter named *Speed* from the current script's parameters and uses its default value.

```csharp
public override void Execute(IScriptingApi api)
{
    var speedParam = api.CurrentScript.ScriptingScriptParameters
        .FirstOrDefault(p => p.ParameterName == "Speed");
    if (speedParam != null)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Script parameter '{0}' (type {1}) has value {2}", speedParam.ParameterName, speedParam.ParameterType, speedParam.DefaultValue));
    }
    else
    {
        api.Messaging.WriteError(ScriptErrorSeverity.Warning, "Script parameter 'Speed' is not configured.");
    }
}
```

During the Cellario run, the script looks up the *Speed* parameter configured on its Script Library entry and displays its type and value in the Cellario message window.

The equivalent script in Python:

```python
def Execute(api : PythonScriptingApi):
    speedParam = next((p for p in api.CurrentScript.ScriptingScriptParameters if p.ParameterName == "Speed"), None)
    if speedParam is not None:
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Script parameter '{0}' (type {1}) has value {2}".format(speedParam.ParameterName, speedParam.ParameterType, speedParam.DefaultValue))
    else:
        api.Messaging.WriteError(ScriptErrorSeverity.Warning, "Script parameter 'Speed' is not configured.")
```
