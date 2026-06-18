/**
 * Change Digital Output State
 * 
 * Controls digital I/O signals for external device integration
 */

using System;
using System.Linq;
using System.Threading;
using HRB.Cellario.Scripting.API;
//using HRB.Libraries;

//reference .\ScriptDependancies\HRB.Libraries.AdamControl.dll

namespace Scripps.Scripting
{
	public class ChangeDigitalOutputState : AbstractScript
	{
		public override void Execute(IScriptingApi api)
		{
			// Skip this script if running the protocol in the Cellario Analyze tab
			if (!api.IsExecutingInAnalysis)
			{
				// Get script parameters
				string adamIPAddress = api.CurrentScript.ScriptingScriptParameters.FirstOrDefault(p => p.ParameterName == "Adam IP Address").DefaultValue;
				string adamDevicePort = api.CurrentScript.ScriptingScriptParameters.FirstOrDefault(p => p.ParameterName == "Adam Device Port").DefaultValue;
				string adamDOPort = api.CurrentScript.ScriptingScriptParameters.FirstOrDefault(p => p.ParameterName == "Adam Digital Output Port").DefaultValue;
				bool adamDOState = bool.Parse(api.CurrentScript.ScriptingScriptParameters.FirstOrDefault(p => p.ParameterName == "Adam Digital Output State").DefaultValue);
				
				// Create AdamIoModule Object
				var Adam = new AdamIoModule();
				string adamIpPort = adamIPAddress + ":" + adamDevicePort;
				Adam.IpEndPoint = AdamIoModule.CreateIpEndPoint(adamIpPort);
				Adam.LogMethod = message => Adam.Connect();

				// Update DO state
				Adam.SetState(adamDOPort, adamDOState);
			}
		}
	}
}