/**
 * Plate Count Warning
 * 
 * Example script showing basic Cellario protocol automation and customization features
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
                // Are two LH pooled?
                IScriptingRunOrderParameter addSecondLHParam = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Add Liquid Handler 2").FirstOrDefault();
                bool addSecondLH;
                // identify the threads of interest so their count can be inspected later.  LINQ filter looks for any plates containing LT steps with Times Used > 1, and returns their threads to a Hash
                HashSet<IScriptingPlate> plates = api.GetPlates().Where(s => s.RemainingSteps.Where(p => p.TimesUsed > 1 && p.StepName == "LiquidTransfer").Any()).ToHashSet();
                
                var threadsAndTU = plates.Select(item=>new KeyValuePair<IScriptingProtocolThread,int?>(item.CurrentThread,item.RemainingSteps.Where(p => p.TimesUsed > 1 && p.StepName == "LiquidTransfer").FirstOrDefault().TimesUsed)).Distinct().ToList();
                api.Messaging.Notify("blargh", string.Format("found {0} threads containing high value Times Used", threadsAndTU.Count()));
                
                int orderSize;
                string message1 = "";
                string message2 = "";
                int numMessages = 0;
                List<string> messages = new List<string>();
                if (addSecondLHParam == null)
                {
                    return;
                } else {
                    addSecondLH = Convert.ToBoolean(addSecondLHParam.ParameterValue);
                    if (addSecondLH == true)
                    {
                        orderSize = api.GetPlatesForCurrentThread().Count();
                        
                        if (orderSize > 1 && orderSize <= 5)
                        {
                            message1 = "If using Lysis Buffer reservoirs, confirm that you've ordered 2, as you're using 2 Liquid Handlers.";
                            message2 = "If using Bead reservoirs, please confirm that you've ordered 2, as you're using 2 Liquid Handlers.";
                            messages.Add(message1);
                            messages.Add(message2);
                            numMessages = 2;
                        } else if (orderSize > 5 && orderSize <= 10) {
                            message1 = "If using Lysis Buffer reservoirs, confirm that you've ordered 2, as you're using 2 Liquid Handlers.";
                            messages.Add(message1);
                            numMessages = 1;
                        } else {
                            numMessages = 0;
                        }
                    }
                }
                // if there are any warnings to display, get a result list of all checklist items and their checked state
                IEnumerable<Tuple<string, bool>> results;
                if (numMessages == 0) return;
                if (!api.Messaging.ShowChecklist("Run Validation", messages.ToArray(), out results))
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