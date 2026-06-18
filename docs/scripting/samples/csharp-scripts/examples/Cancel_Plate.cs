/**
 * Cancel Plate
 * 
 * Cancels remaining protocol steps for specific plates based on conditions
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      CancelPlateScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System.Linq;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for cancelling all further operations for a sample
    /// </summary>
    public class CancelPlateScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            var current = api.CurrentPlate;
            if (current.Name.EndsWith("2"))
            {
                // this example cancels all remaing steps for the current sample if the sample name ends in 2
                var cancelled = current.Cancel();
            }
        }
    }
}