---
title: RunOrder
description: Learn about the `api.CurrentRun` object and its properties, methods, and syntax examples. Discover how to use properties such as `RunOrderParameters`, `Description`, `RunNumber`, `RunState`, `CriticalSegments`, and `WellTransfers`. Explore methods like `B
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.CurrentRun`

:::hint{type="info"}
CurrentRun is also preserved as a property of CurrentPlate (api.CurrentPlate.CurrentRun) to remain backward-compatible.
:::

See the [CurrentRun Property](<../Systems and Inventory/Plate.md>) section of Plate for more information.

:::hint{type="info"}
`api.CurrentRun` is of type `IScriptingRunOrder2`, which extends `IScriptingRunOrder` and additionally exposes `IScriptingProtocol Protocol { get; }` (accessed as `api.CurrentRun.Protocol`). The run order also provides `bool? IsMatch(IScriptingRunOrder other)`, mirroring the `IsMatch` pattern documented elsewhere for plates and steps.
:::

# Run Order Parameters

## Description

Run Order parameters are collections attached to the RunOrder object.

## Parameters

All parameters are read-only except *ParameterValue*, which is read/write.

- RunOrderParameterId (integer)
- ProtocolParameterId (integer)
- ParameterName (string)
- OrderSampleId (integer)
- ParameterValue (string)

## Syntax Example

```csharp
var parameter = api.CurrentRun.RunOrderParameters.First();
var pname = parameter.ParameterName;
var ppid = parameter.ProtocolParameterId;
var pvalue = parameter.ParameterValue;
var roparamid = parameter.RunOrderParameterId;
var osid = parameter.OrderSampleId;
var protParam = api.CurrentRun.Protocol.Parameters.SingleOrDefault(a => a.ProtocolParameterId == ppid);
var fppChoicesList = protParam.ChoicesList;
var newParamValue = fppChoicesList.Last();
    parameter.ParameterValue = newParamValue;
```

This example finds the first run order parameter, then finds the associated protocol parameter, and then sets the run order parameter to the last value in the protocol parameter choices list.

***

# BatchPause Method

## Description

The BatchPause method pauses the order when the current batch is complete.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", api.CurrentPlate.Barcode));
    if (string.IsNullOrEmpty(api.CurrentPlate.Barcode))
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Order will pause because the current plate barcode is missing");
        api.CurrentPlate.CurrentRun.BatchPause();
    }
}
```

During the Cellario run, if the current plate does not have a barcode, the order pauses.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/batchpausemethod.png" size="84" width="574" height="117" position="center" showCaption="false"}

The status of the current Cellario order changes to *Paused*.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/batchpausemethod2.png" size="90" width="621" height="60" position="center" showCaption="false"}

## Method Parameters

No arguments are required.

## Returns

*True* if the run can be batch-paused

***

# Description Property

## Description

The Description property describes the run order.

## Syntax Example

```csharp
public class EgOrderDescription : AbstractScript
{
    public override void Execute(IScriptingApi api)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Order description = {0}", api.CurrentPlate.CurrentRun.Description));
    }
}
```

During the Cellario run, the script displays the order description. The description is displayed on the **Run** tab and the **Order** tab.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/descriptionproperty.png" size="74" width="507" height="59" position="center" showCaption="false"}

The script displays the order description in the Cellario message window.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/descriptionproperty2.png" size="86" width="580" height="61" position="center" showCaption="false"}

## Property Type

String

***

# Pause Method

## Description

The Pause method pauses the run.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", api.CurrentPlate.Barcode));
    if (string.IsNullOrEmpty(api.CurrentPlate.Barcode))
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Order will pause because the current plates barcode is missing");
        api.CurrentPlate.CurrentRun.Pause();
    }
}
```

## Method Parameters

This method does not require any arguments.

## Returns

*True* if the order can be paused

***

# RunNumber Property

## Description

The RunNumberProperty provides the Cellario run order ID.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Run order id = {0}", api.CurrentPlate.CurrentRun.RunNumber));
}
```

During the Cellario run, the script displays the run order id in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/runnumberproperty.png" size="86" width="585" height="61" position="center" showCaption="false"}

## Property Type

Long number

***

# RunState Property

## Description

The RunState property identifies the current run state.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic( ScriptLogLevel.Normal, string.Format("Current System State = {0}",
    api.CurrentPlate.CurrentRun.RunState));
}
```

During the Cellario order, the script displays the current run state in the Cellario message log.

::Image[]{src="https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/scripting/runstateproperty.png" size="88" width="577" height="63" position="center" showCaption="false"}

## Property Type

ScriptingRunState, which is an enum with these possible values:

- **Unknown** — Status is unknown
- **Canceled** — Active run was canceled
- **Created** — Run has been created but is waiting for submittal to Cellario
- **Submitted** — Run is submitted to Cellario and is awaiting the start
- **Running** — Run is actively in progress on Cellario
- **Finished** — Run has finished on Cellario
- **Archived** — Run has been removed from Cellario UI, though it still exists in the database
- **Pausing** — Run is in the process of pausing
- **Paused** — Run has paused

***

# Critical Segments

## Description

Critical segments are a collection of timings related to the run order (not to plate operations or threads).

## Syntax

The following lists the critical timings for a run:

```csharp
var csList = api.CurrentPlate.CurrentRun.CriticalSegments;
```

## Properties

Each critical segment has these *read-only* properties:

- CriticalSegmentId (long)
- EndProtocolStepName (string)
- StartProtocolStepName (string)
- ThreadName (string)

Each critical segment has these *read/write* properties:

- MaximumPlates (integer)
- MaximumTime (timespan)
- Notes (string)

Changes are reflected in runtime and analysis modes.

## Examples

### Example 1

```csharp
TimeSpan newMaxTime = TimeSpan.FromSeconds(600);
var timeThread = csList.FirstOrDefault(a => a.ThreadName.Contains("Time"));
timeThread.MaximumTime = newMaxTime;
timeThread.Notes = "Script changed value of Maximum Time";
```

### Example 2

```csharp
int newMaxPlates = 2;
var plateThread = csList.FirstOrDefault(a => a.ThreadName.Contains("Plate"));
plateThread.MaximumPlates = newMaxPlates;
plateThread.Notes = "Script changed value of Maximum Plates";
```

***

# Well Transfers

## Description

Well transfer records can be read through the scripting API, which lets you build scripts to output a well transfer list or to use the data for other well transfer functions. The collection of records (for the current run only) is available via `CurrentRun.WellTransfers`.

## Properties

- WellTransferId (long)
- SourceOrderSampleId (integer)
- SourceWellRow (integer)
- SourceWellCol (integer)
- DestOrderSampleId (integer)
- DestWellRow (integer)
- DestWellCol (integer)
- TipOrderSampleId (integer)
- TipWellRow (integer)
- TipWellCol (integer)
- Volume (decimal)
- Notes (string)
- TransferDate (date)

## Syntax Examples

```csharp
var csvData = new List<string>();
var roId = api.CurrentRun.RunNumber.ToString();
var path = "c:\\SystemData\\CellarioWellTransfers\\";
var filename = "Transfers_Run_" + roId + ".csv";
var outputFile = Path.Combine(path, filename);
var xFers = api.CurrentRun.WellTransfers.ToList();
    foreach(var xFer in xFers)
    {
        var sId = xFer.SourceOrderSampleId.ToString();
        var sRow = xFer.SourceWellRow.ToString();
        var sCol = xFer.SourceWellCol.ToString();
        var dId = xFer.DestOrderSampleId.ToString();
        var dRow = xFer.DestWellRow.ToString();
        var dCol = xFer.DestWellCol.ToString();
        var vol = xFer.Volume.ToString();
        var notes = xFer.Notes.ToString();
        var dated = xFer.TransferDate.ToString();
        var tId = xFer.TipOrderSampleId.ToString();
        var tRow = xFer.TipWellRow.ToString();
        var tCol = xFer.TipWellCol.ToString();
        var xferId = xFer.WellTransferId.ToString();
        var item = sId + ", " + sRow + ", " + sCol + ", " + dId + ", " + dRow + ", " + dCol + ", " + vol; csvData.Add(item);
    }
File.WriteAllLines(outputFile, csvData);
api.Messaging.Notify("Run End Event Script", "Well transfer count = " + xFers.Count()); 
```

***

# Deck Load/Unload Sequences

## Description

The deck load/unload sequences describe the order in which resources are loaded onto and unloaded from a deck for the run order. The collection is available via `api.CurrentRun.DeckLoadUnloadSequences`, an array of `IScriptingDeckLoadUnloadSequence` elements.

## Properties

Each `IScriptingDeckLoadUnloadSequence` element has the following members:

- StepId (nullable long, read-only)
- ResourceName (string, read-only)
- LoadSequence (integer, read/write)
- UnloadSequence (integer, read/write)

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    foreach (var seq in api.CurrentRun.DeckLoadUnloadSequences)
    {
        api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Resource {0}: LoadSequence={1} UnloadSequence={2} StepId={3}", seq.ResourceName, seq.LoadSequence, seq.UnloadSequence, seq.StepId));
    }
}
```

During the Cellario run, the script iterates the deck load/unload sequences for the run order and displays the resource name, load sequence, unload sequence, and step id for each entry.

***

# Protocol Object

See the [Protocol Properties and Parameters](<../Systems and Inventory/Plate.md>) section of Plate for information about the collections attached to the Protocol object.
