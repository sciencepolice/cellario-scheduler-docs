/**
 * Cellario Services - Explicit Simple
 * 
 * Creates new run orders programmatically using the Cellario Services API
 */


using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using HRB.Cellario.Services.DTO;
using HRB.Cellario.Services.Client;
//css_reference ./Cellario Services/HRB.Cellario.Services.Client.dll
// TODO - customize namespace and class name
namespace Customer.Scripting
{
    public class CellarioServices : AbstractScript
    {        
        /// <summary>
        /// Method called during Cellario protocol execution when a sample arrives at the scripting step.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
       		var recipe = new OrderRecipe()
       		{
       			User = "Script",
       			Description = "Create Order Using Script",
       			Order = new OrderSet
       			{
       				ProtocolId = 100, // Protocol ID - 
       				PlateSets = new PlateSet[]
       				{
       					new PlateSet() // Can Specify multiple plate sets, 
       					{
       						PlateProtocolId = 1000, // Plate Protocol ID, Thread ID
							LabwareType = "Golden Plate", //Labware Type
							ExplicitCount = 3, // Define plate count either using 'Explict Count' or 'Barcodes' array. Barcodes array takes precedent
							//ResourcePositionId = 1000, //Optional - Specify starting position of all plates in a plate set 
							Barcodes = new[] 
							{
								"Barcode 1",
								"Barcode 2",
								"Barcode 3"
							}							
       					}
       				}
       			}
			};       		
        	var client = new CellarioClient("localhost",8444);
        	Order order = client.CreateOrder(recipe);
        	        	
            // TODO - implement 
            // e.g. api.Resources["resource name"].Operations["operation name"].OperationParameters["parameter name"] = 10;
        }

        /// <summary>
        /// Method called during Cellario protocol execution when any device operations for the sample are complete,
        /// allowing access to their results.
        /// </summary>
        /// <remarks>If a resource is allocated but not released, it will remain unavailable to the Cellario run-time scheduler.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and device operations, including releasing allocated resources.</param>
        public override void ReleaseResources(IScriptingApiPostExecute api)
        {
            // TODO  implement or remove if not used
            // e.g. var result = api.Resources["resource name"].Operations["operation name"].Execute();
            // e.g. api.Resources["resource name"].Release();
        }
    }
}