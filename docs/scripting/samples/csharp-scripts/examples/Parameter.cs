/**
 * Parameter
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */


using System;
using System.Linq;
using HRB.Cellario.Scripting.API;


// TODO - customize namespace and class name
namespace Customer.Scripting
{
    public class ScriptParameterScript : AbstractScript
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
        /// This is a test of Script Properties. Note - add properties before testing! 
        /// Note1 - add properties in Script Library view
        /// Note2 - use with Protocol Parameters for more flexibility
        /// </summary>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            else
                System.Diagnostics.Debugger.Launch();

            try
            {
 
                if (api.CurrentScript.ScriptingScriptParameters == null)
                {
                    api.Messaging.Notify("Script Test", "ScriptParameters null");
                    return;
                }

                if (api.CurrentScript.ScriptingScriptParameters.Count == 0)
                {
                    api.Messaging.Notify("Script Test", "No ScriptParameters defined!");
                    return;
                }

                var parameterList = api.CurrentScript.ScriptingScriptParameters.ToList();
                foreach (var item in parameterList)
                {
                    var sParamName = item.ParameterName;
                    var sParamValue = item.DefaultValue.ToString();
                    api.Messaging.Notify("Script Test", "Parameter - " + sParamName + " Value - " + sParamValue);
                }
            }
            catch (Exception ex)
            {
                api.Messaging.Notify("Script Test", "Exception error - " + ex.Message);

            }
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
