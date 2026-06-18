/**
 * Run Data
 * 
 * Demonstrates saving and retrieving data between protocol steps and scripts
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      RunDataScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for saving and retrieving local script data, and saving global run data for retrieval by another script.
    /// </summary>
    /// <remarks>Script data is used to save and restore values between samples in a given script, and
	/// among scripts in a given run.  </remarks>
    public class RunDataScript : AbstractScript 
    {
        public override void AllocateResources(IScriptingApiAllocation api)
        {
            // save a sample index
            if (api.CurrentPlate.PlateNumber == 1)
                api.Data.PerStepData["Index"] = 0;
            else
            {
                // update index for each sample
                int saved = (int)api.Data.PerStepData["Index"];
                api.Data.PerStepData["Index"] = saved + 1;
            }
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                string.Format("Index is {1} at resource allocation call for {0}.", api.CurrentPlate.Name, api.Data.PerStepData["Index"]));
        }

        public override void Execute(IScriptingApi api)
        {
            // log saved sample index 
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("Index is {1} at script execute for {0}.",
                api.CurrentPlate.Name, api.Data.PerStepData["Index"]));
            // save message for another script to retrieve
            api.Data.PerRunData["Index"] = String.Format("Current index is {0}", api.Data.PerStepData["Index"]);
        }

        public override void ReleaseResources(IScriptingApiPostExecute api)
        {
            // log saved sample index
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal,
                string.Format("Index is {1} at post-execute call for {0}.", api.CurrentPlate.Name, api.Data.PerStepData["Index"]));
        }

    }
}