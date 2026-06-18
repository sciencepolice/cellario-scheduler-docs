/**
 * Pre-Run Count Available Prestos
 * 
 * Counts enabled KingFisher Presto devices and adjusts protocol flow accordingly
 */

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using System.Collections.Generic;

/// Script counts enabled KingFisher Prestos on system, and adjusts the protocol's primary thread flow gate to match
/// Prestos are named "Presto_1.1" and "Presto_1.2" etc.
/// Flow gate is notes contains string "Presto"


// TODO - customize namespace and class name
namespace Customer.Scripting
{    
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            // find available Prestos
            List<IScriptingResource> OnPrestos = new List<IScriptingResource>();
            int myOnPrestos = 0;
            foreach (IScriptingResource device in api.Resources.Values)
            {
                if (device.ResourceType.ToString() == "Presto" && device.IsEnabled == true)
                {
                    myOnPrestos++;
                    OnPrestos.Add(device);
                    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("{0} is enabled", device.Name.ToString()));
                }    
            }
            
            IScriptingCriticalSegment numPrestos = api.CurrentRun.CriticalSegments.Where(s => s.StartProtocolStepName.StartsWith("Plate")).FirstOrDefault();
            
            if (numPrestos != null)
            {
            	numPrestos.MaximumPlates = myOnPrestos;
            }	
            else
			{
				api.Messaging.Notify("Error", "Script 'Count Prestos' on Event Start could not identify target Flow Gate.  Please ensure that a flow gate covers primary thread (starting with \"Start\" step) to control number of concurrent plates.");
				return;
			}
        }
    }
}