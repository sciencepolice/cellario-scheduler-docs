---
title: Messaging
description: Learn how to use the Email and Notify methods in the `api.messaging` namespace with this informative document. Discover how the Email method enables you to send emails, and how the Notify method facilitates the display of notification dialogues, waiting f
docTags: 
createdAt: Wed Aug 02 2023 15:38:59 GMT+0000 (Coordinated Universal Time)
---

`api.messaging`

# Audit Method

## Description

The Audit method writes an event to Cellario's audit trail from a script. Use it to record significant script actions so they appear in the audit log alongside other system events.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.Audit("PlateProcessed", "Workflow", $"Plate {api.CurrentPlate.Barcode} completed");
}
```

During the Cellario run, the script writes an audit entry recording that the current plate has completed processing.

## Method Parameters

```csharp
void Audit(string eventType, string category = null, string note = null);
```

| **Name**      | **Type** | **Description**                                        |
| ------------- | -------- | ------------------------------------------------------ |
| **eventType** | String   | The type of event being audited (required)             |
| **category**  | String   | The category of the event (optional)                   |
| **note**      | String   | An optional note with additional detail for the event  |

## Returns

This method does not return a value.

***

# Email Method

## Description

The Email method sends an email from a script.

:::hint{type="info"}
Configure your email server settings in the nlog configuration file.
:::

## Syntax Example

```csharp
public override void Execute(IScriptingApi api) 
{ 
  if (api.CurrentPlate.PlateNumber == 1) 
  { 
    api.Messaging.Email(new[] {"user@example.com", "other@example.com"},
    "Example email subject", 
      string.Format( "Started protocol {0}.",
      api.CurrentPlate.CurrentProtocol.ProtocolName)); 
   } 
}
```

During the Cellario run, on the first plate, this script sends an email to the addresses"user\@example.com" and "other\@example.com" with a message containing the name of the protocol that has just started.

## Method Parameters

There are eight overloads for the Email method. Each overload takes a different set of parameters.

```csharp
void Email(string to, string subject, string message, ScriptEmailSeverity severity);
```

| **Name**     | **Type**            | **Description**                                                                 |
| ------------ | ------------------- | ------------------------------------------------------------------------------- |
| **to**       | String              | The target to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**  | String              | The subject line of the email                                                   |
| **message**  | String              | The message to be emailed                                                       |
| **severity** | ScriptEmailSeverity | May be used to filter targets based configuration                               |

***

```csharp
void Email(string to, string subject, string message, Exception exception, ScriptEmailSeverity severity);
```

| **Name**      | **Type**            | **Description**                                                                 |
| ------------- | ------------------- | ------------------------------------------------------------------------------- |
| **to**        | String              | The target to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**   | String              | The subject line of the email                                                   |
| **message**   | String              | The message to be emailed                                                       |
| **exception** | Exception           | The error to be reported to the target                                          |
| **severity**  | ScriptEmailSeverity | May be used to filter targets based configuration                               |

***

```csharp
void Email(string to, string subject, string message);
```

| **Name**    | **Type** | **Description**                                                                 |
| ----------- | -------- | ------------------------------------------------------------------------------- |
| **to**      | String   | The target to be emailed<br />If empty, the email is sent to default target(s). |
| **subject** | String   | The subject line of the email                                                   |
| **message** | String   | The message to be emailed                                                       |

***

```csharp
void Email(string to, string subject, string message, Exception exception);
```

| **Name**      | **Type**  | **Description**                                                                 |
| ------------- | --------- | ------------------------------------------------------------------------------- |
| **to**        | String    | The target to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**   | String    | The subject line of the email                                                   |
| **message**   | String    | The message to be emailed                                                       |
| **exception** | Exception | The error to be reported to the target                                          |

***

```csharp
void Email(string[] to, string subject, string message, ScriptEmailSeverity severity);
```

| **Name**     | **Type**            | **Description**                                                                      |
| ------------ | ------------------- | ------------------------------------------------------------------------------------ |
| **to**       | String\[ ]          | List of targets to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**  | String              | The subject line of the email                                                        |
| **message**  | String              | The message to be emailed                                                            |
| **severity** | ScriptEmailSeverity | May be used to filter targets based configuration                                    |

***

```csharp
void Email(string[] to, string subject, string message, Exception exception, ScriptEmailSeverity severity);
```

| **Name**      | **Type**            | **Description**                                                                      |
| ------------- | ------------------- | ------------------------------------------------------------------------------------ |
| **to**        | String\[ ]          | List of targets to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**   | String              | The subject line of the email                                                        |
| **message**   | String              | The message to be emailed                                                            |
| **exception** | Exception           | The error to be reported to the target                                               |
| **severity**  | ScriptEmailSeverity | May be used to filter targets based configuration                                    |

***

```csharp
void Email(string[] to, string subject, string message);
```

| **Name**    | **Type**   | **Description**                                                                      |
| ----------- | ---------- | ------------------------------------------------------------------------------------ |
| **to**      | String\[ ] | List of targets to be emailed<br />If empty, the email is sent to default target(s). |
| **subject** | String     | The subject line of the email                                                        |
| **message** | String     | The message to be emailed                                                            |

***

```csharp
void Email(string[] to, string subject, string message, Exception exception);
```

| **Name**      | **Type**   | **Description**                                                                      |
| ------------- | ---------- | ------------------------------------------------------------------------------------ |
| **to**        | String\[ ] | List of targets to be emailed<br />If empty, the email is sent to default target(s). |
| **subject**   | String     | The subject line of the email                                                        |
| **message**   | String     | The message to be emailed                                                            |
| **exception** | Exception  | The error to be reported to the target                                               |

## Returns

This method does not return a value.

***

# Notify Method

## Description

The Notify method displays a notification dialog and waits for a response.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api) 
{ 
  // show a question returning yes/no response
  var response = api.Messaging.Notify("Notify User Example Script", "Can you see this warning message ?", null, ScriptMessageType.Question, ScriptMessageSeverity.Warning); 
 // show a simple message returning OK response 
 api.Messaging.Notify("Notify User Example Script", string.Format("User response to warning was {0}.", response), "This is an example of displaying a simple, informational, message box.", ScriptMessageType.Information, ScriptMessageSeverity.Informational); 
 try 
  { 
   throw new Exception("Exception deliberately thrown for example of notifiying user of a handled exception."); 
  } 
  catch (Exception ex) 
  { 
    // show an exception to the user - implementation may show and log stack trace of the exception. 
    // If message severity is fatal, Cellario will shut down. If message severity is serious, the user may elect to shut down. 
    api.Messaging.Notify("Notify User Example Script", null, ex, ScriptMessageSeverity.Serious); 
  } 
}
```

During the Cellario run, the script displays a dialog window with **Yes** and **No** buttons.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/notifymethod.png" size="46" width="294" height="165" position="center" alt="Notify dialog window" showCaption="false"}

After clicking a button, the dialog window closes and a message window is displayed.
The message text reports which button was clicked.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/notifymethod2.png" size="72" width="475" height="178" position="center" alt="Message window with report" showCaption="false"}

After clicking **OK**, the window closes. Next, the script intentionally creates an
exception to demonstrate the notification of errors.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/notifymethod3.png" size="72" width="476" height="313" position="center" showCaption="false"}

- If message severity is fatal, Cellario shuts down.
  If message severity is serious, the user may elect to shut down.

## Method Parameters

```csharp
ScriptMessageResponse Notify(string heading, string message);
```

| **Name**    | **Type** | **Description**                                             |
| ----------- | -------- | ----------------------------------------------------------- |
| **heading** | String   | The heading of the user notification (may be null or empty) |
| **message** | String   | The message to be shown to the user                         |

***

```csharp
ScriptMessageResponse Notify(string heading, string message, string details);
```

| **Name**    | **Type** | **Description**                                             |
| ----------- | -------- | ----------------------------------------------------------- |
| **heading** | String   | The heading of the user notification (may be null or empty) |
| **message** | String   | The message to be shown to the user                         |
| **details** | String   | Optional details to be shown to the user                    |

***

```csharp
ScriptMessageResponse Notify(string heading, string message, string details, ScriptMessageType type, ScriptMessageSeverity severity);
```

| **Name**     | **Type**              | **Description**                                             |
| ------------ | --------------------- | ----------------------------------------------------------- |
| **heading**  | String                | The heading of the user notification (may be null or empty) |
| **message**  | String                | The message to be shown to the user                         |
| **details**  | String                | Optional details to be shown to the user                    |
| **type**     | ScriptMessageType     | The message type (determines possible responses)            |
| **severity** | ScriptMessageSeverity | May be used to determine UI elements, such as icons         |

***

```csharp
ScriptMessageResponse Notify(string heading, string message, Exception exception);
```

| **Name**      | **Type**  | **Description**                                             |
| ------------- | --------- | ----------------------------------------------------------- |
| **heading**   | String    | The heading of the user notification (may be null or empty) |
| **message**   | String    | The message to be shown to the user                         |
| **exception** | Exception | The error to be shown to the user                           |

***

```csharp
ScriptMessageResponse Notify(string heading, string message, Exception exception, ScriptMessageSeverity severity); 
```

| **Name**      | **Type**              | **Description**                                             |
| ------------- | --------------------- | ----------------------------------------------------------- |
| **heading**   | String                | The heading of the user notification (may be null or empty) |
| **message**   | String                | The message to be shown to the user                         |
| **exception** | Exception             | The error to be shown to the user                           |
| **severity**  | ScriptMessageSeverity | May be used to determine UI elements, such as icons         |

## Returns

Method returns a *ScriptMessageResponse* object. ScriptMessageResponse is an enum with the possible values: OK, Yes, No, and Cancel.

***

# SetBreakPoint Method

## Description

The SetBreakPoint method is used only for debugging.

:::hint{type="warning"}
Using this method in a script stops the Cellario run order.
:::

When you use SetBreakPoint in a script and have a debugging application installed, the call stops script execution and opens the debugger to the SetBreakPoint line in the script. This lets you move through the code line-by-line to evaluate variable values.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
  api.SetBreakPoint();
}
```

## Method Parameters

None

## Returns

None, but the call opens the script in the debugging application (if one is installed)

***

# ShowChecklist Method

## Description

The ShowChecklist method can be used to display a custom list of messages, which the operator must satisfy.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api)
{
    if (api.CurrentPlate.PlateNumber == 1)
    {
        // optionally, get a result list of all checklist items and their checked state
        IEnumerable<Tuple<string, bool>> results;
        if (!api.Messaging.ShowChecklist("Run Validation", new[] {"DMSO bottle is full", "Destination plates are loaded"}, out results))
        {
            if (results.Any(r => !r.Item2))
            {
                // system can be either paused or stopped
                api.Messaging.WriteError(ScriptErrorSeverity.Error, "Stopping
                system due to required items not checked.");
                api.System.Stop();
            }
            else
            {
                // or run can be paused or batch paused
                api.Messaging.WriteError(ScriptErrorSeverity.Error, "Pausing
                run due to requirement checklist not acknowleged.");
                api.CurrentPlate.CurrentRun.Pause();
            }
        }
    }
}
```

During the Cellario run, the first plate triggers the checklist to be displayed.

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/showchecklistmethod.png" size="56" width="355" height="241" position="center" alt="Run Validation diaog winw" showCaption="false"}

Depending on the user input, either the system stops or the order pauses.

## Method Parameters

```csharp
bool ShowChecklist(string title, IEnumerable<string> items);
```

| **Name**  | **Type**             | **Description**                        |
| --------- | -------------------- | -------------------------------------- |
| **title** | String               | The dialog window title                |
| **items** | IEnumerable\<string> | The items to be marked off by the user |

***

```csharp
bool ShowChecklist(string title, IEnumerable<string> items, out IEnumerable<Tuple<string, bool>> results);
```

| **Name**    | **Type**                              | **Description**                        |
| ----------- | ------------------------------------- | -------------------------------------- |
| **title**   | String                                | The dialog window title                |
| **items**   | IEnumerable\<string>                  | The items to be marked off by the user |
| **results** | out IEnumerable\<Tuple\<string,bool>> | The items that were marked off         |

***

```csharp
Dictionary<string, bool> ShowChecklist(string title, string[] values);
```

:::hint{type="info"}
This overload is Python compatible.
:::

| **Name**   | **Type**   | **Description**                        |
| ---------- | ---------- | -------------------------------------- |
| **title**  | String     | The dialog window title                |
| **values** | String\[ ] | The items to be marked off by the user |

This overload returns a dictionary mapping each item to a Boolean indicating whether the user checked it.

## Returns

Boolean; *true* if the user has individually and collectively acknowledged the items

The `Dictionary<string, bool>` overload instead returns a dictionary whose keys are the checklist items and whose values are *true* when the corresponding item was checked.

***

# UiContext Property

## Description

The UiContext property returns the current UI task scheduler context of the script.

Runs started manually from the Cellario UI have a UiContext. Runs started automatically, through a scheduler for example, do not have a valid UiContext. This is important for scripts that open a window for the user, such as those that produce a dialog window with check boxes—the call *ShowChecklist* will fail if there is no UiContext.

## Syntax Example

The following example uses the UiContext property used with the Show Checklist script. (See the [Show Checklist](../../samples/csharp-scripts/examples/Show_Checklist.cs) example script.)

```csharp
var curUIContext = api.Messaging.UiContext;
if (curUIContext == null)
  {
     api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal, "Cannot show checklist - Order not started via the UI");
      return;
  }
IEnumerable<Tuple<string, bool>> results = null;
if (!api.Messaging.ShowChecklist("Run Validation", new[] {"DMSO bottle is full", "Destination plates are loaded"}, out results))
  {
    if (results.Any(r => !r.Item2))
    {
        //system can be either paused or stopped
        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Stopping system due to required items not marked.");
        api.System.Stop();
    }
    else
    {
        // or run can be paused or batch paused
        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Pausin un due to requirement checklist not acknowledged.");
        api.CurrentPlate.CurrentRun.Pause();
    }
  }
```

The curUIContext returns a value (ID).

![curUIContext returned value](https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/uicontext-resultid.png)

## Property Type

Object of type System.Threading.Tasks.TaskScheduler

***

# WriteDiagnostic Method

## Description

The WriteDiagnostic method writes a diagnostic log message to the Cellario message window.&#x20;

In standard C#, you can use the methods Console.WriteLine and Debug.WriteLine to print log information to the output console. The WriteDiagnostic method is the Cellario equivalent of these methods.

## Syntax Example

In this example, the barcode for the current plate will be displayed in the Cellario message window.

```csharp
public override void Execute(IScriptingApi api)
{
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Current Plate Barcode = {0}", api.CurrentPlate.Barcode));
}
```

Example Cellario output

::Image[]{src="https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/writediagnosticmethod.png" size="96" width="533" height="60" position="center" showCaption="false"}

*ScriptLogLevel* is an enum with the possible values: Minimal, Normal, None, Verbose.

### Method Parameters

```csharp
void WriteDiagnostic(ScriptLogLevel level, string message, string details);
```

| **Name**    | **Type**       | **Description**                                                  |
| ----------- | -------------- | ---------------------------------------------------------------- |
| **level**   | ScriptLogLevel | The value that logging can use to filter or redirect the message |
| **message** | String         | The logged message                                               |
| **details** | String         | Optional details                                                 |

***

```csharp
void WriteDiagnostic(ScriptLogLevel level, string message);
```

| **Name**    | **Type**       | **Description**                                                  |
| ----------- | -------------- | ---------------------------------------------------------------- |
| **level**   | ScriptLogLevel | The value that logging can use to filter or redirect the message |
| **message** | String         | The logged message                                               |

### Returns

This method does not return a value.

***

# WriteError Method

## Description

The WriteError method logs an error message.

## Syntax Example

```csharp
public override void Execute(IScriptingApi api) 
{
  // This example causes a log item to be written to the configured diagnostic log. 
  // Log level is used by log configuration to filter logged items. 
  // Details may be omitted entirely, as in error log example below.
  api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Example diagnostic log item written from script.", "Optional details in log item."); 
  // This example causes a log item to be written to the configured error log. 
  // Error severity is reported in the log and may be used by log configuration to filter logged items.
  api.Messaging.WriteError(ScriptErrorSeverity.Informational, "Example error log item written from script"); try 
  { 
    throw new Exception("Forced exception."); 
  } 
  catch (Exception ex) 
  { 
    // This example causes a log item that includes exception information to be written to the configured error log. 
    // Depending on the log configuration, stack trace details may be logged.      
    // As above, severity is reported in the log and may be used by log configuration to filter logged items. 
    api.Messaging.WriteError(ScriptErrorSeverity.Warning, ex, "Example error log for an exception", "Optional additional details."); 
  } 
}
```

## Method Parameters

```csharp
void WriteError(ScriptErrorSeverity severity, string message);
```

| **Name**     | **Type**            | **Description**                                                  |
| ------------ | ------------------- | ---------------------------------------------------------------- |
| **severity** | ScriptErrorSeverity | The value that logging can use to filter or redirect the message |
| **message**  | string              | The logged message                                               |

***

```csharp
void WriteError(ScriptErrorSeverity severity, string message, string details);
```

| **Name**     | **Type**            | **Description**                                                  |
| ------------ | ------------------- | ---------------------------------------------------------------- |
| **severity** | ScriptErrorSeverity | The value that logging can use to filter or redirect the message |
| **message**  | String              | The logged message                                               |
| **details**  | String              | Optional details                                                 |

***

```csharp
void WriteError(ScriptErrorSeverity severity, Exception exception, string message, string details);
```

| **Name**      | **Type**            | **Description**                                                  |
| ------------- | ------------------- | ---------------------------------------------------------------- |
| **severity**  | ScriptErrorSeverity | The value that logging can use to filter or redirect the message |
| **exception** | Exception           | The exception to be logged                                       |
| **message**   | String              | The logged message                                               |
| **details**   | String              | Optional details                                                 |

***

```csharp
void WriteError(ScriptErrorSeverity severity, Exception exception, string message);
```

| **Name**      | **Type**            | **Description**                                                  |
| ------------- | ------------------- | ---------------------------------------------------------------- |
| **severity**  | ScriptErrorSeverity | The value that logging can use to filter or redirect the message |
| **exception** | Exception           | The exception to be logged                                       |
| **message**   | String              | The logged message                                               |

## Returns

This method does not return a value.
