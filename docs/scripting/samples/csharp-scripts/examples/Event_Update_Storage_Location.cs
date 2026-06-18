/**
 * Event Update Storage Location
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_UpdateStorageLocation.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_UpdateStorageLocation : AbstractScript
    {
        /// <summary>
        /// Event Method called at start of Cellario run.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            /* Example Event UpdateStorageLocation Script. For a given Resource, GetStorageLocations() and UpdateStorageLocation(location)
        		can be used to add a plate not associated with a run order to a storage location, essentially blocking the location from 
        		being allocated by the cell controller (but not blocked from new run orders which assume the plates are where they are assigned.
        		For this example we use a small storage device and at the start of the run we fill all the storage locations by adding barocode 
        		or SampleType fields. The protocol should have a plate that starts outside of that storage but ends in storage. 
        		The run should deadlock if all storage locations are filled, work if script leaves available slots. 
        	*/
            var resourceName = "Plate_Hotel_8pos_01"; //OK - only 6 positions

            //must use valid SampleType name i.e -> "Corning96_Cellstar_Black" 
            var sampleTypeName = "Corning96_Cellstar_Black"; //note will fail if not valid

            var resource = api.Resources.FirstOrDefault(a => a.Value.Name == resourceName).Value;
            var rspList = resource.GetStorageLocations();

            foreach (var location in rspList)
            {
                if (location.Position == 1)
                {
                    location.Barcode = "Script00" + location.Position.ToString();
                    location.SampleType = sampleTypeName;
                }
                else if (location.Position == 2)
                {
                    location.SampleType = sampleTypeName;
                    //location.SampleType = "InvalidName"; //enable this to fail and leave position open
                }
                else
                {
                    location.Barcode = "Script00" + location.Position.ToString();
                }
                var result = resource.UpdateStorageLocation(location);
                if (result)
                {
                    api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal,
                            "Update Resource Success for " + resourceName + " location " + location.Position.ToString());
                }
                else
                {
                    api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal,
                            "Update Resource Failed for " + resourceName + " location " + location.Position.ToString());
                }
            }
        }
    }
}
