/**
 * Event Display Protocol Parameters
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_DisplayProtocolParameters.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_DisplayProtocolParameters : AbstractScript
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

            //about this script 
            var runorderId = api.CurrentRun.RunNumber;
            var runOrderDesc = api.CurrentRun.Description;
            var protocolName = api.CurrentRun.Protocol.ProtocolName;

            var message = "Before Order State Change for run = " + runorderId.ToString() + " \n";
            message += "Order Description '" + runOrderDesc + "' \n";
            message += "Order State - " + api.CurrentRun.RunState + " \n";
            message += "Protocol Name - " + protocolName + " \n";
            api.Messaging.Notify("Before Order State Change", message);

            var parameters = api.CurrentRun.Protocol.Parameters.ToList();
            api.Messaging.Notify("Protocol Parameter Count", "Found - " + parameters.Count().ToString());

            var index = 0;
            foreach (var parameter in parameters)
            {
                index++;
                var pname = parameter.ParameterName;
                var ppid = parameter.ProtocolParameterId;
                var pvalue = parameter.DefaultValue;
                var pdesc = parameter.Description;
                var plevel = parameter.ParameterLevel;
                var ptype = parameter.ParameterType;

                message = "Parameter " + index.ToString() + " Name is '" + pname + "' \n";
                message += "Parameter Id is '" + ppid + "' \n";
                message += "Parameter Value is '" + pvalue + "' \n";
                message += "Parameter Desc is '" + pdesc + "' \n";
                message += "Parameter Level is '" + plevel + "' \n";
                message += "Parameter Type is '" + ptype + "' \n";

                api.Messaging.Notify("Protocol Parameter " + index.ToString(), message);
            }
        }
    }
}
