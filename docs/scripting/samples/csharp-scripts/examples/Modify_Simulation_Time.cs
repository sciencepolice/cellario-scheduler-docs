/**
 * Modify Simulation Time
 * 
 * Adjusts protocol step timing estimates for better scheduling
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      ModifySimulationTime.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class ModifySimulationTime : AbstractScript
    {
        /// <summary>
        /// Test Method - will increase the Simulation Time for every other plate to illistrate ability.
        /// </summary>
        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber % 2 != 0) return;

            TimeSpan addSimTime = TimeSpan.FromSeconds(1800);

            foreach (var step in api.CurrentPlate.RemainingSteps)
            {
                step.SimulationTime = addSimTime;
            }
        }
    }
}