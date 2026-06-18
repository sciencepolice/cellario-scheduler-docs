/**
 * Event Display Protocol Properties
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_DisplayProtocolProperties.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_DisplayProtocolProperties :AbstractScript
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
            //info about this script from api.CurrentRun() and api.CurrentProtocol()
            var runorderId = api.CurrentRun.RunNumber;
            var runOrderDesc = api.CurrentRun.Description;
            var protocolName = api.CurrentRun.Protocol.ProtocolName;

            var message = "Before Order State Change for run = " + runorderId.ToString() + " \n";
            message += "Order Description '" + runOrderDesc + "' \n";
            message += "Order State - " + api.CurrentRun.RunState + " \n";
            message += "Protocol Name - " + protocolName + " \n";
            api.Messaging.Notify("Before Order State Change", message);

            var properties = api.CurrentRun.Protocol.Properties.ToList();

            var index = 0;
            foreach (var property in properties)
            {
                index++;
                var pname = property.PropertyName;
                var pid = property.PropertyId;
                var pvalue = property.PropertyValue;
                var pdesc = property.PropertyDescription;

                message = "Property " + index.ToString() + " Name is '" + pname + "' \n";
                message += "Property Id is '" + pid + "' \n";
                message += "Property Value is '" + pvalue + "' \n";
                message += "Property Desc is '" + pdesc + "' \n";

                api.Messaging.Notify("Protocol Propert " + index.ToString(), message);
            }
        }
    }
}
