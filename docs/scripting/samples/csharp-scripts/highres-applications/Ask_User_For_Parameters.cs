/**
 * Ask User For Parameters
 * 
 * Presents interactive dialogs to collect user input during protocol setup
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
            IScriptingRunOrderParameter DevTimeParameter = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Development Time").FirstOrDefault();
            
            string[] DevTimeMinutes = new[] {"5", "10", "15", "20", "25", "30"};
            
            IEnumerable<Tuple<string, bool>> results;
  			if (!api.Messaging.ShowChecklist("Please select the development incubation time in MINUTES that you want:", DevTimeMinutes, out results))
  			{
  				int checks = 0;
  				
  				foreach (var r in results)
  				{
  					if (r.Item2) checks = checks + 1;
  				}
  				
  				if (checks == 0)
  				{
  					api.Messaging.WriteError(ScriptErrorSeverity.Error, "Pausing run due to Development Time not selected.  Resume to continue with default value.");
                    api.CurrentPlate.CurrentRun.Pause();
                }
                else if (checks > 1)
                {
                	api.Messaging.WriteError(ScriptErrorSeverity.Error, "Stopping system due to too many values selected.  Please re-try and select only one.");
                    api.System.Stop();
                } 
                else
				{
					var DevTimeValue = results.Where(p => p.Item2).Select(p => p.Item1).FirstOrDefault();
					int DevTimeSeconds = Convert.ToInt32(DevTimeValue)*60;
					DevTimeParameter.ParameterValue = DevTimeSeconds.ToString();
					api.Messaging.Notify("Development Time Selected", string.Format("Setting Development Time to {0} minutes ({1} seconds) for this run.", DevTimeValue, DevTimeSeconds));
				}                   
  			}
        }
    }
}