/**
 * Rotate Quadrant
 * 
 * Implements quadrant-based liquid handling for 384-well plates
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
            // do not run this in simulation
            if (api.IsExecutingInAnalysis) return;
            
            // Get the current plate - during a protocol run this will always have a value
            var current = api.CurrentPlate;
            if (current != null)
            {

                // determine the target destination quadrant based on the current plate number
                int myPlateNum = api.CurrentPlate.PlateNumber;
                int quad = myPlateNum % 4 + 1;
                
                // find the next Liquid Transfer step
                var myTransfer = current.RemainingSteps.FirstOrDefault(s => s.StepName.StartsWith("LiquidTransfer"));
                if (myTransfer != null)
                {
                    // change the method name to the method that aspirates the correct quadrant
                    myTransfer.OperationParameters["Method File Name"] = "Stamp_From_Quadrant_" + quad.ToString();
                }  

            }
        }
    }
}