/**
 * In-Line Count Available Prestos
 * 
 * Counts enabled KingFisher Presto devices and adjusts protocol flow accordingly
 */

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using System.Collections.Generic;

/// Script counts enabled KingFisher Prestos on system, and adjusts the protocol's primary thread flow gate to match
/// Prestos are named "Presto_1.1" and "Presto_1.2" etc.
/// Flow gate is identified by its position in the protocol - this can be changed easily
/// automatic adjustment is done with a Protocol Parameter called "Override Automatic Flow Gate Setting" with string value options including "No" and any desired integers.


// TODO - customize namespace and class name
namespace Customer.Scripting
{    
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber != 1) return;
            
            // delete all instances of this script for plates that are NOT plate 1
            IScriptingProtocolRemainingStep uselessScript;
            List<IScriptingPlate> plates = api.GetPlatesForCurrentThread().Where(p => p.PlateNumber != 1).ToList();
            foreach (IScriptingPlate plate in plates)
            {
                uselessScript = plate.RemainingSteps.Where(s => s.StepName == "Script").FirstOrDefault();
                plate.RemainingSteps.Remove(uselessScript);
            }
            
            // did user want flow gate manual override or automatic setting based on current number of enabled Prestos?
            IScriptingRunOrderParameter flowGateOverRideParameter = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Override Automatic Flow Gate Setting").FirstOrDefault();
            string flowgateoverride;
            if (flowGateOverRideParameter !=null)
            {
                flowgateoverride = flowGateOverRideParameter.ParameterValue.ToString();
            } else {
                flowgateoverride = "No";
            }            
            int flowgateSetting = 0;
            
            // did protocol author create the needed Protocol Parameter for override?
            bool overRideParameterExists;
            bool overRideParameterIsInteger;
            if (flowGateOverRideParameter != null)
            {
                overRideParameterIsInteger = int.TryParse(flowgateoverride, out flowgateSetting);
                overRideParameterExists = true;
            } else {
                overRideParameterExists = false;
                overRideParameterIsInteger = false;                
            }
            
            if (overRideParameterExists == false || overRideParameterIsInteger == false) // the protocol parameter value did not contain an integer
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
                    flowgateSetting = myOnPrestos;                    
                }
            } else { 
                // the protocol parameter contained an integer, which will be used to override the default flow gate value
                int.TryParse(flowgateoverride, out flowgateSetting);
            }
           
            // this assumes that the critical segment of interest is the only one in the thread containing this script.
            // you can add filter rules to narrow down if there are multiple choices (e.g. start or end step IDs or names, notes, etc.)
            // identify the thread bearing the controlling flow gate
            IScriptingProtocolThread critThread = api.CurrentPlate.CurrentThread;
            IScriptingCriticalSegment numPrestos = api.CurrentPlate.CurrentRun.CriticalSegments.Where(s => s.ThreadName == critThread.ThreadName).Where(s => s.EndProtocolStepName == "End").FirstOrDefault();
            //api.Messaging.Notify("foundit", string.Format("grabbed flow gate {0}", numPrestos.CriticalSegmentId));
            if (numPrestos != null) // indicates that a valid flow gate was found
            {
                numPrestos.MaximumPlates = flowgateSetting;
            }    
            else
            {
                api.Messaging.Notify("Error", "Script 'Count Prestos' could not identify target Flow Gate.  Please ensure that a flow gate matching script line 37 exists in the protocol design to control number of concurrent plates.");
                return;
            }
        }
    }
}