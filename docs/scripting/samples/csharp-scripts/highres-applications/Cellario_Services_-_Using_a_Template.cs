/**
 * Cellario Services - Using a Template
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
       			Template = new TemplateSet() // There are more options but this is the most straightforward template use case.
       			{
       				TemplateId = 100, // ID of the template
       				BatchCount = 5 // Five Batches of plates. So in a 4-1 Compression, 5 batches will produce to 20 source, 5 destination.
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