/**
 * Event Display Protocol Ports
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_DisplayProtocolPorts.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_DisplayProtocolPorts : AbstractScript
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

            var threads = api.CurrentRun.Protocol.Threads.ToList();

            foreach (var thread in threads)
            {
                var threadName = thread.ThreadName;
                var inputPortName = thread.InputPortName;
                var inputPortTag = thread.InputPortTag;
                var outputPortName = thread.OutputPortName;
                var outputPortTag = thread.OutputPortTag;


                message = "Input Port Name is '" + inputPortName + "' \n";
                message += "Input Port Tag is '" + inputPortTag + "' \n";
                message += "Output Port Name is '" + outputPortName + "' \n";
                message += "Output Port Tag is '" + outputPortTag + "' \n";

                api.Messaging.Notify("Thread " + threadName, message);
            }
        }
    }
}
