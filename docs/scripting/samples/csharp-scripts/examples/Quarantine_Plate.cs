/**
 * Quarantine Plate
 * 
 * Moves plates to quarantine storage locations when issues are detected
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      QuarantinePlateScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for skipping remaining operations for a sample in a thread and sending it to
    /// quarantine storage.
    /// </summary>
    public class QuarantinePlateScript : AbstractScript
    {
        // Name of resource plates will be quarantined to
        private static readonly string QuarantineResource = "NanoServe_02";

        public override void Execute(IScriptingApi api)
        {
            
            // This example sends the second plate in the thread to the first free position in quarantine resource
            if (api.CurrentPlate.PlateNumber == 1)
            {
                var resource = api.Resources.Values.FirstOrDefault(r => r.Name.StartsWith(QuarantineResource));
                if (resource == null)
                {
                    api.Messaging.WriteError(ScriptErrorSeverity.Error, 
                        string.Format( "No quarantine resource found ({0}).", QuarantineResource ));
                }
                else
                {
                    api.CurrentPlate.SendToStorage(resource.Name);
                }
            }
            // This example sends the second plate in the thread to a specific location in quarantine resource, in this case
            // the next free position
            if (api.CurrentPlate.PlateNumber == 2)
            {
                IScriptingStorageLocation location = null;
                var resource = api.Resources.Values.FirstOrDefault(r => r.Name.StartsWith(QuarantineResource));
                if (resource != null)
                    location = resource.GetAvailableLocation();
                if (location != null)
                {
                    api.CurrentPlate.SendToStorage(location);
                }
                else
                    api.Messaging.WriteError(ScriptErrorSeverity.Error, 
                        string.Format("No quarantine resource location found ({0}).", QuarantineResource));

            }
        }
    }
}