/**
 * Modify Crit Time for ELISA
 * 
 * Advanced production script for specialized laboratory automation workflows
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
        public override void Execute(IScriptingApi api)
        {

            TimeSpan secondsToAdd = TimeSpan.FromSeconds(60);
            IScriptingRunOrderParameter maxTime = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Development Time").FirstOrDefault();
            IScriptingCriticalSegment devTime = api.CurrentRun.CriticalSegments.Where(s => s.Notes.Contains("Development Time")).FirstOrDefault();
            string newMaxTime = maxTime.ParameterValue;
            //if (api.CurrentPlate.PlateNumber == 1)
            //{
                devTime.MaximumTime = TimeSpan.Parse(newMaxTime).Add(secondsToAdd);
            //}               
        }
    }
}