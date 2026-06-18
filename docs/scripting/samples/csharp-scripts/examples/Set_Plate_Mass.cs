/**
 * Set Plate Mass
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2024 **
// 
//    File:      SetPlateMass.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class SetPlateMass : AbstractScript
    {
        /// <summary>
        /// Show Set plate.Mass example
        /// </summary>
        /// <param name="api">IScriptingApi</param>
        public override void Execute(IScriptingApi api)
        {
            // This will test the plate.Mass property by reading and updating the property for the 1st Plate
            // The first value of Mass will be that set up on creation of CurrentPlate (from OrderSample that
            // will get the plate Mass volume from Labware Mass value. If labware Mass volume is not defined
            // will get the plate Mass volume from Labware FillVolume value. Lacking that => 0.
            // Set this script in multiple places in a protocol (or in a loop) to see value increment.

            api.SetBreakPoint();
            if (api.CurrentPlate.PlateNumber != 1) return;

            var plate = api.CurrentPlate;
            var curPlateMass = plate.Mass;

            api.CurrentPlate.Mass = curPlateMass + 10;

            var msg = " plate " + plate.Name + " Mass changed from " + curPlateMass + " to " + api.CurrentPlate.Mass;

            api.Messaging.Notify("Mass-FillVolume", msg);

        }
    }
}
