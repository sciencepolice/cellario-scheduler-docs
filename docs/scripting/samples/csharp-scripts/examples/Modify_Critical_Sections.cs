/**
 * Modify Critical Sections
 * 
 * Adjusts protocol flow gates and timing constraints during execution
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      ModifyCriticalSections.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class ModifyCriticalSections : AbstractScript
    {
        /// Method called during Cellario protocol execution when a sample arrives at the scripting step.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until ModifySimulationTime method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber != 1) return;

            TimeSpan newMaxTime = TimeSpan.FromSeconds(600);
            int newMaxPlates = 2;

            var csList = api.CurrentPlate.CurrentRun.CriticalSegments;
            if (api.CurrentPlate.CurrentThread.ThreadName.Contains("Time"))
            {
                var timeThread = csList.FirstOrDefault(a => a.ThreadName.Contains("Time"));
                if (timeThread != null)
                {
                    timeThread.MaximumTime = newMaxTime;
                    timeThread.Notes = "Script changed value of Maximum Time";
                }
            }

            if (api.CurrentPlate.CurrentThread.ThreadName.Contains("Plate"))
            {
                var plateThread = csList.FirstOrDefault(a => a.ThreadName.Contains("Plate"));
                if (plateThread != null)
                {
                    plateThread.MaximumPlates = newMaxPlates;
                    plateThread.Notes = "Script changed value of Maximum Plates";
                }
            }
        }
    }
}