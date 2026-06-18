/**
 * Notify User
 * 
 * Displays interactive message boxes and dialogs to users
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2016 **
// 
//    File:      NotifyUserScript.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using HRB.Cellario.Scripting.API;

// NOTE: When copying, change namespace to avoid run-time name clash with this example
namespace HRB.Cellario.Scripting.ScriptLibrary
{
    /// <summary>
    /// Example script for showing a simple message-box style dialog to the user using Cellario Notification
    /// framework.
    /// </summary>
    /// <remarks>These example messages are blocking and will prevent the run from continuing until
    /// the user dismisses the message box.</remarks>
    public class NotifyUserScript : AbstractScript
    {
        /// <summary>
        /// Show several example blocking dialogs to the user
        /// </summary>
        /// <param name="api"></param>
        public override void Execute(IScriptingApi api)
        {
            // show a question returning yes/no response
            var response = api.Messaging.Notify("Notify User Example Script", "Can you see this warning message ?",
                null, ScriptMessageType.Question, ScriptMessageSeverity.Warning);
            // show a simple message returning OK response
            api.Messaging.Notify("Notify User Example Script", string.Format("User response to warning was {0}.", response),
                "This is an example of displaying a simple, informational, message box.",
                ScriptMessageType.Information, ScriptMessageSeverity.Informational);
            try
            {
                throw new Exception("Exception deliberately thrown for example of notifiying user of a handled exception.");
            }
            catch (Exception ex)
            {
                // show an exception to the user - implementation may show and log stack trace of the exception.
                // If message severity is fatal, Cellario will shut down. If message severity is serious, the user
                // may elect to shut down.
                api.Messaging.Notify("Notify User Example Script", null, ex, ScriptMessageSeverity.Serious);
            }
        }
    }
}