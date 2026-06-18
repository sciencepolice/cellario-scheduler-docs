/**
 * Add Operation
 * 
 * Dynamically adds new protocol operations to plates during execution
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      AddOperationsScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;


namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example Script to Add (Insert) Operations to a plate 
    /// </summary>
    public class AddOperationScript : AbstractScript
    {

        /// <summary>
        /// Execute Example Script to Add (Insert) Operations to a plate 
        /// </summary>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            //effect only even plate numbers
            if (api.CurrentPlate.PlateNumber % 2 != 0) return;

            var cp = api.CurrentPlate;

            //CurrentThread will contain all the operations for the thread and can be used to add another similar step to plate
            var moveStep = api.CurrentPlate.CurrentThread.Steps.FirstOrDefault(a => a.StepName == "Move");
            var spinStep = api.CurrentPlate.CurrentThread.Steps.FirstOrDefault(a => a.StepName == "Spin");

            // New step to be added must be a clone of existing step
            var newMoveStep1 = moveStep.CloneStep();
            var newMoveStep2 = moveStep.CloneStep();
            var newSpinStep = spinStep.CloneStep();

            //To create a new operation, clone a Remaining Step passing in a valid operation name
            var newOpStep = cp.RemainingSteps.LastOrDefault().CloneStep("Dispense");

            //cloned steps with new operation name do not have resources assigned. Must be added if required
            var resList = api.Resources.Values.ToList();
            var resource = resList.Where(a => a.Name.Contains("Dispenser")).FirstOrDefault();
            (newOpStep as IScriptingProtocolRemainingStep).AssignedResources.Add(resource);

            //cloned steps with new operation name does not have parameters assinged. Must be added if required
            (newOpStep as IScriptingProtocolRemainingStep).OperationParameters.Add("Prime Volume", 10);

            //add steps to tree without optional position it will be added before End
            //add insert index to InsertStep(step,index) to place at specific place in list.			
            cp.RemainingSteps.InsertStep(newMoveStep1);
            cp.RemainingSteps.InsertStep(newOpStep);
            cp.RemainingSteps.InsertStep(newMoveStep1);
            cp.RemainingSteps.InsertStep(newSpinStep);

            //modify values for steps in remaining steps
            TimeSpan addSimTime = TimeSpan.FromSeconds(60);
            cp.RemainingSteps.FirstOrDefault(a => a.StepName == "Dispense").SimulationTime = addSimTime;
        }
    }
}
