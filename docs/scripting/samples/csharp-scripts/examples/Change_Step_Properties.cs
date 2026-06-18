/**
 * Change Step Properties
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      ChangeStepPropertiesScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{

    /// <summary>
    /// Example script for modifying the pacing times (delay start, pause after) for next, all, or specific protocol steps.
    /// Also demonstrates changing the sample name and operation parameters.
    /// </summary>
    public class ChangeStepPropertiesScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            // Get the current plate - during a protocol run this will always have a value
            var current = api.CurrentPlate;
            if (current != null)
            {
                // modify the name of the current plate - this is just by way of example, not required when changing pacing times
                current.Name = string.Format("Scripted - {0}", current.Name);

                // find an upcoming incubate step
                var incubate = current.RemainingSteps.FirstOrDefault(s => s.StepName.StartsWith("Incubate"));
                if (incubate != null)
                {
                    // change pause before for next incubate operaton for the current plate to 10 seconds
                    incubate.PauseBefore = TimeSpan.FromSeconds(10);
                    // change timeout for next incubate operaton for the current plate to 30 seconds
                    incubate.TimeOut  = TimeSpan.FromSeconds(30);
                }

                // modify an operation parameter in the next transfer step, if any
                var transfer = api.CurrentPlate.RemainingSteps.FirstOrDefault(s => s.StepName.Contains("Transfer"));
                if (transfer != null)
                {
                    transfer.OperationParameters["Method File Name"] = string.Format("Transfers_{0}", current.PlateNumber + 1);
                }  

             }

            // get any other plate for the current thread except the current plate
            var plates = api.GetPlatesForCurrentThread().Where(s => s.IsMatch(current) != true ).ToList();

            // find all plates that haven't started yet and set pause after (delay start)
            foreach (var plate in plates.Where( p => p.Status == ScriptingPlateStatus.NotStarted))
            {
                // change names of plate if not already changed,  once again not required when changing pacing times
                if (!plate.Name.StartsWith( "Scripted"))
                    plate.Name = string.Format("Scripted - {0}", plate.Name);

                // here is an example of changing the delay start time for a specific step in a plate other than the current one
                var firstStep = plate.RemainingSteps.FirstOrDefault();
                if (firstStep != null)
                {
                    // delay start to 15 seconds
                    firstStep.PauseAfter = TimeSpan.FromSeconds(15);
                }
            }
        }
    }
}