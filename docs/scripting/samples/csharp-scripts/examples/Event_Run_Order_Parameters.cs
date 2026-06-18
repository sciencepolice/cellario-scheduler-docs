/**
 * Event Run Order Parameters
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_RunOrderParameters.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_RunOrderParameters : AbstractScript
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

            var readerOutputDirectoryName = "Output File Directory";
            //about this script 
            var runorderId = api.CurrentRun.RunNumber;
            var runOrderDesc = api.CurrentRun.Description;
            var protocolName = api.CurrentRun.Protocol.ProtocolName;

            var message = "Before Order State Change for run = " + runorderId.ToString() + " \n";
            message += "Order Description '" + runOrderDesc + "' \n";
            message += "Order State - " + api.CurrentRun.RunState + " \n";
            message += "Protocol Name - " + protocolName + " \n";
            api.Messaging.Notify("Before Order State Change", message);

            var parameters = api.CurrentRun.RunOrderParameters.ToList();
            api.Messaging.Notify("Run Order Parameter Count", "Found - " + parameters.Count().ToString());

            var index = 0;
            var newParamValue = string.Empty;
            foreach (var parameter in parameters)
            {
                index++;
                var pname = parameter.ParameterName;
                var ppid = parameter.ProtocolParameterId;
                var pvalue = parameter.ParameterValue;
                var roparamid = parameter.RunOrderParameterId;

                message = "Parameter " + index.ToString() + " Name is '" + pname + "' \n";
                message += "Protocol Parameter Id is '" + ppid + "' \n";
                message += "Parameter Value is '" + pvalue + "' \n";
                message += "Run Order Parameter Id is '" + roparamid + "' \n";

                api.Messaging.Notify("Run Order Parameter " + index.ToString(), message);

                if (pname == readerOutputDirectoryName)
                {
                    var protParam = api.CurrentRun.Protocol.Parameters.SingleOrDefault(a => a.ProtocolParameterId == ppid);
                    var fppChoicesList = protParam.ChoicesList;
                    newParamValue = fppChoicesList.Last();
                    parameter.ParameterValue = newParamValue;
                    message = "Parameter " + index.ToString() + " Name '" + pname + "' \n";
                    message += "Changed value from '" + pvalue + "' to '" + newParamValue + "' \n";

                    api.Messaging.Notify("Run Order Parameter " + index.ToString(), message);
                }
            }
        }
    }
}
