/**
 * Modify Loop Count
 * 
 * Changes the number of loop iterations based on plate or run conditions
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      ModifyLoopCount.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class ModifyLoopCount : AbstractScript
    {
        /// <summary>
        /// Method called during Cellario protocol execution when a sample arrives at the scripting step.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            /* This example script should be run as a Script Operation before a looping section.
				The number of loops will be changed to the plateNumber, so a test run with multiple
				plates will produce different results for each plate in a thread.
			*/
            var steps = api.CurrentPlate.RemainingSteps.ToList();
            foreach (var step in steps)
            {
                if (step.StepName == "Loop End")
                {
                    //only step type where Number of Loop value has any meaning
                    var oldLoopCount = step.NumberOfLoops;
                    var newLoopCount = api.CurrentPlate.PlateNumber;
                    step.NumberOfLoops = newLoopCount;

                    api.Messaging.Notify("Modify Looping Test", "Changed Looping Count from " +
                        oldLoopCount.ToString() + " to " + newLoopCount.ToString() + " for plate " + api.CurrentPlate.Name);
                }
            }
        }
    }
}
