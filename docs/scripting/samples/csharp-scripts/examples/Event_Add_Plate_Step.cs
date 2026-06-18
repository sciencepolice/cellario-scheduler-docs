/**
 * Event Add Plate Step
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_AddPlateStep.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_AddPlateStep : AbstractScript
    {
        /// <summary>
        /// Method called at start of Cellario run.
        /// </summary>
        /// <remarks>Expects a simple protocol with one thread with a Dispense Operation already existing</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            //about this script 
            var runorderId = api.CurrentRun.RunNumber;
            var runOrderDesc = api.CurrentRun.Description;
            var protocolName = api.CurrentRun.Protocol.ProtocolName;

            var message = "Before Order State Change for run = " + runorderId.ToString() + " \n";
            message += "Order Description '" + runOrderDesc + "' \n";
            message += "Order State - " + api.CurrentRun.RunState + " \n";
            message += "Protocol Name - " + protocolName + " \n";
            api.Messaging.Notify("Before Order State Change", message);

            TimeSpan addSimTime = TimeSpan.FromSeconds(180);

            var protocol = api.CurrentRun.Protocol;
            var threads = protocol.Threads;
            var steps = threads.First().Steps;
            steps.First().SimulationTime = addSimTime;

            var plate = api.GetPlates().FirstOrDefault();
            var stepCount = plate.RemainingSteps.Count();
            var cloneStep = plate.RemainingSteps.First(a => a.StepName == "Dispense").CloneStep();
            var cloneMoveStep = plate.RemainingSteps.First(a => a.StepName == "Move").CloneStep();
            plate.RemainingSteps.InsertStep(cloneMoveStep);
            plate.RemainingSteps.InsertStep(cloneStep);

            api.Messaging.Notify("Add Step", "added step to first plate");
        }
    }
}
