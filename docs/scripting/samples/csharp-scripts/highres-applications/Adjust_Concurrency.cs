/**
 * Adjust Concurrency
 * 
 * Dynamically changes the number of concurrent plates based on conditions
 */


using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using System.Collections.Generic;

// This script automatically adjusts the allowed concurrncy of plates based on the duration of a specified incubation (in this case, "dialysis".).
// If one plate per 30min fits, we assess the current value and calculate allowable concurrency.
// We set the value of the existing protocol parameter "Concurrency", and use that to set the "Loop Count" values at ends of threads MT0 and DW1.
// we set the overall flow gate to the same value.

namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {

        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber != 1) return;
            
            double secondsPerSR0 = 1800; // approximate time that a single batch takes to complete thread SR0
            
            // identify working objects
            IScriptingRunOrderParameter myOverride = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Override Dialysis Time").FirstOrDefault();
            IScriptingRunOrderParameter concurrency = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Concurrency").FirstOrDefault();
            IScriptingCriticalSegment concurrencySegment = api.CurrentPlate.CurrentRun.CriticalSegments.Where(s => s.EndProtocolStepName == "End" && s.ThreadName == api.CurrentPlate.CurrentThread.ThreadName).FirstOrDefault();
            
            // if any of the parameters or critical segments does not exist, notify user and exit the script.
            if (myOverride == null || concurrency == null || concurrencySegment == null)
            {
                api.Messaging.Notify("Missing Parameters or Segments", "Script 'Adjust Concurrency' is missing a required protocol parameter or criticla segment. Script will be skipped.");
                return;
            }
            
            string permIncString = myOverride.ParameterValue.ToString();
            
            // assess duration of the permeation incubate
            TimeSpan permInc = TimeSpan.Parse(permIncString);
            double permIncSeconds = Convert.ToDouble(permInc.TotalSeconds);
            int capacity = Convert.ToInt32(Math.Floor(permIncSeconds/secondsPerSR0));
            
            // set the protocol parameter which controls Merge loop number values
            concurrency.ParameterValue = capacity.ToString();
            
            // set the flow gate
            concurrencySegment.MaximumPlates = Convert.ToInt32(capacity);
            
            // delete all instances of this script for plates that are NOT plate 1
            IScriptingProtocolRemainingStep uselessScript;
            List<IScriptingPlate> plates = api.GetPlatesForCurrentThread().Where(p => p.PlateNumber != 1).ToList();
            foreach (IScriptingPlate plate in plates)
            {
                uselessScript = plate.RemainingSteps.Where(s => s.StepName == "Script").FirstOrDefault();
                plate.RemainingSteps.Remove(uselessScript);
            }
        }
    }
}