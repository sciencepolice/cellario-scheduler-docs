/**
 * SetSteriTemp
 * 
 * Demonstration script showcasing specific Cellario features and integration capabilities
 */


using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace Customer.Scripting
{
    public class SetSteriTemp : AbstractScript
    {
        /// <summary>
        /// Method called ahead of execution to optionally allocate resources.
        /// </summary>
        /// <param name="api">Use to claim device resources that will be used in the script.</param>
        public override void AllocateResources(IScriptingApiAllocation api)
        {
            // TODO - implement or remove if not used
            // e.g. api.Resources["resource name"].Allocate();
        }

        /// <summary>
        /// Method called during Cellario protocol execution when a sample arrives at the scripting step.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
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