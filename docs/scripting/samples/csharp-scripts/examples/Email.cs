/**
 * Email
 * 
 * Sends email notifications during protocol execution
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      EmailScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for sending an email
    /// </summary>
    /// <remarks>Email server must be configured (e.g. Nlog.config)</remarks>
    public class EmailScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber == 1)
            {
                api.Messaging.Email(new[] {"user@example.com", "other@example.com"}, "Example email subject",
                    string.Format( "Started protocol {0}.", api.CurrentPlate.CurrentProtocol.ProtocolName));
            }
        }
    }
}