# Cellario Scripting API Reference for AI Agents

## Overview

The Cellario Scripting API provides comprehensive control over laboratory automation workflows through C# scripts. These scripts can interact with devices, manage plates and samples, control system state, handle data persistence, and communicate with users through messaging and email.

## Core API Structure

### Main Interface: IScriptingApi

The primary interface that provides access to all scripting capabilities:

```csharp
public interface IScriptingApi : IScriptingApiDevices, IScriptingApiPlates, IScriptingApiLabware, IScriptingApiRunOrders
{
    // Execution context
    bool IsExecutingInAnalysis { get; }

    // Main subsystems
    IScriptingApiSystem System { get; }           // System control and status
    IScriptingApiData Data { get; }               // Data persistence
    IScriptingApiFrameworks Messaging { get; }    // User messaging and emails

    // Resources and devices
    IReadOnlyDictionary<string, IScriptingDeviceResource> Resources { get; }

    // Web API client
    CellarioClient WebClient { get; }

    // Debugging
    void SetBreakPoint();
}
```

### Base Script Classes

Scripts must inherit from one of these base classes:

```csharp
// Synchronous script execution
public abstract class AbstractScript : ICsharpScriptApi
{
    // Optional: Allocate resources before execution
    public virtual void AllocateResources(IScriptingApiAllocation api) { }

    // Required: Main script logic
    public abstract void Execute(IScriptingApi api);

    // Optional: Post-execution cleanup and result processing
    public virtual void ReleaseResources(IScriptingApiPostExecute api) { }
}

// Asynchronous script execution
public abstract class AbstractAsyncScript : ICsharpAsyncScriptApi
{
    public abstract void ExecuteAsync(IScriptingAsyncApi api);
}
```

## System Control (IScriptingApiSystem)

Control and monitor the Cellario system state:

```csharp
public interface IScriptingApiSystem
{
    ScriptingSystemState SystemState { get; }    // Current system state
    void Pause();                                 // Pause the system
    void Stop();                                  // Stop the system
    void SuspendRuntimeLoop();                    // Suspend runtime processing
    void ResumeRuntimeLoop();                     // Resume runtime processing
}

public enum ScriptingSystemState
{
    Unknown, Errored, Stopped, Starting, Running, Pausing
}
```

## Plate and Sample Management (IScriptingApiPlates)

Access and control plate processing:

```csharp
public interface IScriptingApiPlates
{
    IScriptingPlate CurrentPlate { get; }              // Current plate being processed
    IScriptingPlate[] GetPlates();                     // All currently active plates for this run
    IScriptingPlate[] GetPlatesForCurrentThread();     // All currently active plates for the current thread
}

public interface IScriptingPlate
{
    // Identification
    ScriptingContainerType ContainerType { get; }
    string Name { get; set; }
    string Barcode { get; }
    int PlateNumber { get; }
    long? OrderSampleId { get; }

    // Physical properties
    double Mass { get; set; }
    IScriptingLabware Labware { get; }

    // Status and location
    ScriptingPlateStatus Status { get; }
    IScriptingResourceLocation CurrentLocation { get; }
    IScriptingStorageLocation CurrentStorageLocation { get; }
    IScriptingStorageLocation StartingLocation { get; }
    IScriptingStorageLocation EndingLocation { get; }
    IScriptingResourceLocation AssociatedLidLocation { get; }

    // Protocol execution context
    IScriptingProtocolRemainingStep CurrentStep { get; }
    IScriptingProtocolRemainingSteps RemainingSteps { get; }
    IScriptingProtocolThread CurrentThread { get; }
    IScriptingProtocol CurrentProtocol { get; }
    IScriptingRunOrder CurrentRun { get; }

    // Operations
    bool? IsMatch(IScriptingPlate other);
    bool Cancel();                                    // Cancel remaining operations
    bool SendToStorage(string resourceName);         // Send to storage by name
    bool SendToStorage(IScriptingResource resource);  // Send to storage by resource
    bool SendToStorage(IScriptingStorageLocation location); // Send to specific location
}

public enum ScriptingPlateStatus { NotStarted, Active, Finished }
public enum ScriptingContainerType { Unknown, Plate, Rack }
```

## Resource and Device Management

Access and control laboratory devices and resources:

```csharp
public interface IScriptingDeviceResource : IScriptingResource, IAllocatableResource
{
    IReadOnlyDictionary<string, ScriptedDeviceOperation> Operations { get; }
    bool InitializeDevice();        // Initialize the device
    void UninitializeDevice();      // Uninitialize/shutdown the device
    string GetData { get; }         // Get driver result data
}

public interface IScriptingResource : INamedResource
{
    bool IsErrored { get; }
    string CurrentErrorMessage { get; }
    ScriptingDeviceState DeviceState { get; }
    IScriptingResource ParentResource { get; }

    // Storage management
    IScriptingStorageLocation GetAvailableLocation();
    IScriptingStorageLocation[] GetStorageLocations();
    bool UpdateStorageLocation(IScriptingStorageLocation storageLocation);
}

public interface INamedResource
{
    string Name { get; }
    bool IsEnabled { get; }
    bool IsSimulated { get; }
    string ResourceType { get; }
}

// Device operations wrapper
public class ScriptedDeviceOperation
{
    public string Name { get; }
    public IDictionary<string, object> OperationParameters { get; }
    public object Execute();                              // Execute the operation
    public object GetOperationParameter(string name);    // Get operation parameter
}

public enum ScriptingDeviceState
{
    None, Errored, Unconnected, Connecting, Connected,
    Initializing, Ready, Busy
}
```

## Data Persistence (IScriptingApiData)

Store and retrieve data across script executions:

```csharp
public interface IScriptingApiData
{
    IDataDictionary PerStepData { get; }    // Data per script step in current run
    IDataDictionary PerPlateData { get; }   // Data per plate in current run
    IDataDictionary PerRunData { get; }     // Data across entire run
    bool IsValidDataType<T>(T data);       // Check if data type can be persisted
}

public interface IDataDictionary : IDictionary<string, object>
{
    string GetKey<T>(Expression<Func<T>> selector);  // Create typed property keys
}
```

## Messaging and Communication (IScriptingApiFrameworks)

User notifications, logging, and email communication:

```csharp
public interface IScriptingApiFrameworks
{
    bool IsExecutingInAnalysis { get; }
    TaskScheduler UiContext { get; }

    // Logging
    void WriteDiagnostic(ScriptLogLevel level, string message);
    void WriteDiagnostic(ScriptLogLevel level, string message, string details);
    void WriteError(ScriptErrorSeverity severity, string message);
    void WriteError(ScriptErrorSeverity severity, string message, string details);
    void WriteError(ScriptErrorSeverity severity, Exception exception, string message);
    void WriteError(ScriptErrorSeverity severity, Exception exception, string message, string details);

    // User interaction (blocking)
    ScriptMessageResponse Notify(string heading, string message);
    ScriptMessageResponse Notify(string heading, string message, string details);
    ScriptMessageResponse Notify(string heading, string message, string details,
                               ScriptMessageType type, ScriptMessageSeverity severity);
    ScriptMessageResponse Notify(string heading, string message, Exception exception);
    ScriptMessageResponse Notify(string heading, string message, Exception exception,
                               ScriptMessageSeverity severity);

    // Email
    void Email(string to, string subject, string message, ScriptEmailSeverity severity);
    void Email(string to, string subject, string message, Exception exception, ScriptEmailSeverity severity);
    void Email(string to, string subject, string message);
    void Email(string to, string subject, string message, Exception exception);
    void Email(string[] to, string subject, string message, ScriptEmailSeverity severity);
    void Email(string[] to, string subject, string message, Exception exception, ScriptEmailSeverity severity);
    void Email(string[] to, string subject, string message);
    void Email(string[] to, string subject, string message, Exception exception);
}

// Enums for messaging
public enum ScriptLogLevel { None, Minimal, Normal, Verbose }
public enum ScriptErrorSeverity { Informational, Warning, Error, Fatal }
public enum ScriptEmailSeverity { Informational, Warning, Error }
public enum ScriptMessageSeverity { Informational, Warning, Serious, Fatal }
public enum ScriptMessageType { Information, Question, CancelQuestion }
public enum ScriptMessageResponse { OK, Yes, No, Cancel }
```

## Protocol and Run Order Management

Access and modify protocol execution:

```csharp
public interface IScriptingProtocolRemainingSteps : IReadOnlyCollection<IScriptingProtocolRemainingStep>
{
    bool CanRemove(IScriptingProtocolRemainingStep step);
    bool Remove(IScriptingProtocolRemainingStep step);
    bool InsertStep(IScriptingProtocolStep step, int index = -1);
}

public interface IScriptingProtocolRemainingStep : IScriptingProtocolStep
{
    IScriptingStepResources AssignedResources { get; }
    IScriptingResource[] GetAvailableResources();
    IScriptingOperationParameters OperationParameters { get; }
    int Priority { get; set; }          // Priority for this step operation (1-based)
    TimeSpan TimeOut { get; set; }      // Timeout imposed on this step operation
    TimeSpan PauseBefore { get; set; }  // Incubation time before this step operation
    TimeSpan PauseAfter { get; set; }   // Delay after completing this step operation
}

public interface IScriptingRunOrder
{
    long? RunNumber { get; }                                       // Uniquely identifies the run
    string Description { get; }                                    // Run description
    string CreatedBy { get; }                                     // User who created the run
    MailAddress[] EmailRecipients { get; }                       // Email targets for run information
    IScriptingCriticalSegment[] CriticalSegments { get; set; }    // Critical timings tied to the run order
    IScriptingWellTransfer[] WellTransfers { get; }
    IScriptingDeckLoadUnloadSequence[] DeckLoadUnloadSequences { get; set; }
    IScriptingRunOrderParameters RunOrderParameters { get; set; }
    ScriptingRunState RunState { get; }                           // Current run state

    bool Pause();        // Pause the run; returns true if it can be paused
    bool BatchPause();   // Pause the run when the current batch is complete
    bool? IsMatch(IScriptingRunOrder other);
}

public enum ScriptingRunState
{
    Unknown, Canceled, Created, Submitted, Running,
    Finished, Archived, Pausing, Paused
}
```

## Basic C# Script Template

Here's a comprehensive template that demonstrates the key patterns for Cellario scripting:

```csharp
using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace Customer.Scripting
{
    /// <summary>
    /// Basic script template demonstrating core Cellario scripting patterns
    /// </summary>
    public class BasicScriptTemplate : AbstractScript
    {
        /// <summary>
        /// Optional: Allocate resources before execution
        /// Called once per run to reserve devices/resources
        /// </summary>
        public override void AllocateResources(IScriptingApiAllocation api)
        {
            // Example: Reserve a specific device for exclusive use
            // api.Resources["Dispenser1"]?.Allocate();
        }

        /// <summary>
        /// Main script execution - called for each plate/sample
        /// </summary>
        public override void Execute(IScriptingApi api)
        {
            try
            {
                // Access current plate being processed
                var currentPlate = api.CurrentPlate;

                // Log information
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                    $"Processing plate {currentPlate.Name} (#{currentPlate.PlateNumber})");

                // Check execution context
                if (api.IsExecutingInAnalysis)
                {
                    api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal,
                        "Running in analysis mode - skipping device operations");
                    return;
                }

                // Example: Store data for later retrieval
                api.Data.PerPlateData["ProcessingStartTime"] = DateTime.Now;
                api.Data.PerRunData["PlatesProcessed"] =
                    ((int?)api.Data.PerRunData["PlatesProcessed"] ?? 0) + 1;

                // Example: Device operation
                ExecuteDeviceOperation(api);

                // Example: Conditional logic based on plate properties
                if (currentPlate.PlateNumber % 2 == 0)
                {
                    // Even plates get special handling
                    HandleEvenPlate(api, currentPlate);
                }

                // Example: System control
                if (ShouldPauseSystem(api))
                {
                    api.System.Pause();
                    api.Messaging.Notify("System Paused",
                        "Script has paused the system for review");
                }

                // Example: User interaction
                if (RequiresUserConfirmation(currentPlate))
                {
                    var response = api.Messaging.Notify(
                        "Confirmation Required",
                        $"Continue processing plate {currentPlate.Name}?",
                        string.Empty,
                        ScriptMessageType.Question,
                        ScriptMessageSeverity.Informational);

                    if (response != ScriptMessageResponse.Yes)
                    {
                        currentPlate.Cancel();
                        return;
                    }
                }

                // Example: Protocol modification
                ModifyProtocolSteps(api, currentPlate);

                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                    $"Successfully processed plate {currentPlate.Name}");

            }
            catch (Exception ex)
            {
                // Error handling
                api.Messaging.WriteError(ScriptErrorSeverity.Error, ex,
                    "Error in script execution");

                // Notify user of error
                api.Messaging.Notify("Script Error",
                    "An error occurred during script execution", ex);

                // Send email notification
                api.Messaging.Email(string.Empty,
                    "Cellario Script Error",
                    $"Error processing plate {api.CurrentPlate?.Name}: {ex.Message}",
                    ScriptEmailSeverity.Error);

                throw; // Re-throw to stop execution
            }
        }

        /// <summary>
        /// Optional: Post-execution operations with access to device results
        /// </summary>
        public override void ReleaseResources(IScriptingApiPostExecute api)
        {
            try
            {
                // Example: Process device operation results
                var results = ProcessDeviceResults(api);

                // Store results
                if (results != null)
                {
                    api.Data.PerPlateData["DeviceResults"] = results;
                }

                // Example: Email results
                if (ShouldEmailResults())
                {
                    api.Messaging.Email(string.Empty,
                        "Processing Complete",
                        $"Plate {api.CurrentPlate.Name} processed successfully",
                        ScriptEmailSeverity.Informational);
                }

                // Clean up any allocated resources
                // Resources allocated in AllocateResources are automatically released

            }
            catch (Exception ex)
            {
                api.Messaging.WriteError(ScriptErrorSeverity.Warning, ex,
                    "Error in post-execution cleanup");
            }
        }

        #region Helper Methods

        private void ExecuteDeviceOperation(IScriptingApi api)
        {
            // Example: Execute operation on a device
            var device = api.Resources.Values.FirstOrDefault(r => r.Name.Contains("Dispenser"));
            if (device != null && device.DeviceState == ScriptingDeviceState.Ready)
            {
                // Set operation parameters
                var operation = device.Operations["Dispense"];
                operation.OperationParameters["Volume"] = 100.0;
                operation.OperationParameters["Speed"] = "Normal";

                // Execute the operation
                var result = operation.Execute();

                api.Messaging.WriteDiagnostic(ScriptLogLevel.Verbose,
                    $"Device operation completed: {result}");
            }
        }

        private void HandleEvenPlate(IScriptingApi api, IScriptingPlate plate)
        {
            // Example: Special handling for even-numbered plates
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                $"Applying special processing for even plate #{plate.PlateNumber}");

            // Modify plate mass
            plate.Mass += 5.0;
        }

        private bool ShouldPauseSystem(IScriptingApi api)
        {
            // Example: Pause system based on conditions
            var platesProcessed = (int?)api.Data.PerRunData["PlatesProcessed"] ?? 0;
            return platesProcessed > 0 && platesProcessed % 10 == 0; // Every 10 plates
        }

        private bool RequiresUserConfirmation(IScriptingPlate plate)
        {
            // Example: Require confirmation for certain plates
            return plate.Barcode?.StartsWith("SPECIAL") == true;
        }

        private void ModifyProtocolSteps(IScriptingApi api, IScriptingPlate plate)
        {
            // Example: Add or modify protocol steps
            var remainingSteps = plate.RemainingSteps;

            // Find a step to clone
            var moveStep = plate.CurrentThread.Steps.FirstOrDefault(s => s.StepName == "Move");
            if (moveStep != null)
            {
                // Clone and add additional step
                var newStep = moveStep.CloneStep();
                remainingSteps.InsertStep(newStep, 0); // Insert at beginning

                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                    "Added additional move step to protocol");
            }
        }

        private object ProcessDeviceResults(IScriptingApiPostExecute api)
        {
            // Example: Process results from device operations
            var device = api.Resources.Values.FirstOrDefault(r => r.Name.Contains("Dispenser"));
            if (device != null)
            {
                var data = device.GetData;
                if (!string.IsNullOrEmpty(data))
                {
                    // Process and return structured data
                    return new { RawData = data, ProcessedTime = DateTime.Now };
                }
            }
            return null;
        }

        private bool ShouldEmailResults()
        {
            // Example: Determine if results should be emailed
            return DateTime.Now.Hour >= 17; // After 5 PM
        }

        #endregion
    }
}
```

## Common Scripting Patterns

### Device Interaction Pattern

```csharp
// Get device by name
var device = api.Resources["DeviceName"] ??
             api.Resources.Values.FirstOrDefault(r => r.Name.Contains("DeviceName"));

// Check device state
if (device?.DeviceState == ScriptingDeviceState.Ready)
{
    // Execute operation
    var operation = device.Operations["OperationName"];
    operation.OperationParameters["Parameter1"] = value1;
    var result = operation.Execute();
}
```

### Data Persistence Pattern

```csharp
// Store data with type safety
api.Data.PerRunData["Key"] = value;

// Retrieve with null checking
var value = api.Data.PerRunData.TryGetValue("Key", out var obj) ? obj : defaultValue;

// Strongly typed access
var count = (int?)api.Data.PerRunData["Count"] ?? 0;
api.Data.PerRunData["Count"] = count + 1;
```

### Error Handling Pattern

```csharp
try
{
    // Risky operation
}
catch (Exception ex)
{
    // Log error
    api.Messaging.WriteError(ScriptErrorSeverity.Error, ex, "Operation failed");

    // Notify user
    var response = api.Messaging.Notify("Error", "Operation failed. Continue?",
        ex.Message, ScriptMessageType.Question, ScriptMessageSeverity.Warning);

    if (response == ScriptMessageResponse.No)
    {
        api.CurrentPlate.Cancel();
        return;
    }
}
```

### Protocol Modification Pattern

```csharp
// Clone existing step
var existingStep = api.CurrentPlate.CurrentThread.Steps
    .FirstOrDefault(s => s.StepName == "TargetStepName");

if (existingStep != null)
{
    var newStep = existingStep.CloneStep();

    // Modify parameters if needed
    // newStep.OperationParameters["Parameter"] = newValue;

    // Insert into remaining steps
    api.CurrentPlate.RemainingSteps.InsertStep(newStep, insertIndex);
}
```

## Important Notes for AI Agents

1. **Script Lifecycle**: Scripts follow a three-phase lifecycle: AllocateResources → Execute → ReleaseResources
2. **Error Handling**: Always wrap operations in try-catch blocks and use appropriate logging
3. **Context Awareness**: Check `IsExecutingInAnalysis` to avoid executing the script during analysis
4. **Resource Management**: Resources allocated in `AllocateResources` are automatically released
5. **Data Types**: Only certain data types can be persisted - use `IsValidDataType()` to check
6. **User Interaction**: Notification methods block script execution until user responds
7. **System Control**: Use system pause/stop judiciously as they affect the entire system
8. **Thread Safety**: Scripts execute in scheduler context - use `UiContext` for UI operations
