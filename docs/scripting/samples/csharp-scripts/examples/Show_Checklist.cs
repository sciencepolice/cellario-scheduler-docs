/**
 * Show Checklist
 * 
 * Displays validation checklists that users must complete before proceeding
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      ShowChecklistScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for showing a simple checklist UI
    /// </summary>
    public class ShowChecklistScript : AbstractScript
    {
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
                        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Stopping system due to required items not checked.");
                        api.System.Stop();
                    }
                    else
                    {
                        // or run can be paused or batch paused
                        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Pausing run due to requirement checklist not acknowleged.");
                        api.CurrentPlate.CurrentRun.Pause();

                    }
                }
            }
        }
    }
}