/**
 * Re-Prioritize Post-Step Move
 * 
 * Advanced production script for specialized laboratory automation workflows
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      ChangeStepPropertiesScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using System.Collections.Generic;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{

    /// <summary>
    /// Set to "After Move".  Re-prioritizes post-Transfer Move Step for selected plates to prevent crashing.
    
    /// Script pre-prioritizes the post-transfer robot move for selected plates to change the order of loading out (prevents crashes).
    /// </summary>
    public class ChangeStepPropertiesScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            // Get the current plate - during a protocol run this will always have a value
            var current = api.CurrentPlate;
            if (current != null)
            {				
				// If plate is the 5th in the current batch, set the post-transfer move priority to very low (1)
				if ((current.PlateNumber + 4) % 5 == 0)
				{
					current.RemainingSteps.Where(s => s.StepName == "Move").FirstOrDefault().Priority = 1;
				}
				// If plate is the 5th in the current batch, set the post-transfer move priority to very high (10)
				if ((current.PlateNumber + 4) % 5 == 4)
				{
					current.RemainingSteps.Where(s => s.StepName == "Move").FirstOrDefault().Priority = 10;
				}
            }
        }
    }
}