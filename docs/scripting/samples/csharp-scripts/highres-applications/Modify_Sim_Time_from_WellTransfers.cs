/**
 * Modify Sim Time from WellTransfers
 * 
 * Advanced production script for specialized laboratory automation workflows
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2021 **
// 
//    File:      ModifySimulationTime.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class ModifySimulationTime : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {

            int currentDest = Convert.ToInt32(api.Data.PerRunData["CurrentDest"]);
            
            // if accounting for multiple copies on deck, copy number is set in RunData and retrieved here.
            int copyfactor = 1;
            if (api.Data.PerRunData.ContainsKey("NumCopies")) copyfactor = Convert.ToInt32(api.Data.PerRunData["NumCopies"]);
            
            // how many transfers for current plate pair?
            double numXFers = Convert.ToDouble(api.CurrentRun.WellTransfers.Where(p => p.SourceOrderSampleId == api.CurrentPlate.OrderSampleId).Where(p => p.DestOrderSampleId == currentDest).ToList().Count);
            
            // tune the timing calculation to account for average pick rate.
			// default is 2 hits per well column.
			// to tune differently, set Protocol Parameter "Average Hits Per Column" to 1-8, where 8 is 100% hit rate per column.			
            double avgHitsPerColumn = 2;
            if (api.CurrentRun.RunOrderParameters.Any(p=>p.ParameterName == "Average Hits Per Column")) avgHitsPerColumn = Convert.ToDouble(api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Average Hits Per Column").FirstOrDefault().ParameterValue);
            
            // how many distict z-axis moves to make while accessing a column
            double dipsPerXFerSet = 8/avgHitsPerColumn;
            
            // seconds to complete a tip pickup, gantry translation and tip drop
            double tipshandling = 7+2+6;
            
            // seconds to complete a gantry translation and aspirate action (on average)
            double aspTime = 2+8;
            
            // seconds to complete a gantry translation and dispense action (on average)
            double dispTime = copyfactor*(2+5);
            
            // calculate number of times to access a column within this plate pair
            double numXFerSets = Math.Floor(numXFers / 8);
            double trailingXFers = numXFers % 8;
            
            // using variable values set above, calculate total time to make transfers between current plate pair
            double totaltime = numXFerSets*(tipshandling) + numXFerSets*((aspTime*dipsPerXFerSet) + dispTime) + trailingXFers*aspTime + dispTime;
            
            // set simulation time
            int secPerXFer = Convert.ToInt32(Math.Ceiling(totaltime));
            api.CurrentPlate.RemainingSteps.Where(s => s.StepName == "LiquidTransfer").FirstOrDefault().SimulationTime = TimeSpan.FromSeconds(secPerXFer);

        }
    }
}
