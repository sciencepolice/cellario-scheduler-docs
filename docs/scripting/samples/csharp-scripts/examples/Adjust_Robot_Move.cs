/**
 * Adjust Robot Move
 * 
 * Modifies robot movement parameters like pick heights during execution
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      AdjustRobotMove.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for modifying a robot move parameter from a script.
    /// </summary>
    public class AdjustRobotMove : AbstractScript
    {
        /// <summary>
        /// Example modifies pick height for all subsequent move steps for this plate.
        /// </summary>
        /// <param name="api"></param>
        public override void Execute(IScriptingApi api)
        {

            foreach (var step in api.CurrentPlate.RemainingSteps.ToArray())
            {
                if (step.StepName == "Move")
                {
                    step.OperationParameters.Remove("Pick Height");
                    step.OperationParameters.Add("Pick Height", "30.0");
                }
                // Optional: trace per-step parameters
                var parameterValues = string.Join(Environment.NewLine,
                    step.OperationParameters.Select(op => string.Format("{0} = {1}", op.Key, op.Value)));
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Verbose, string.Format( @"Step {0} operation parameters:
{1}", step.StepName, parameterValues));
            }
        }
    }
}
