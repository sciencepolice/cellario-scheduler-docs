/**
 * Generate Report
 * 
 * Creates detailed execution reports and data summaries
 */


using System;
using System.Linq;
using System.Windows.Documents;
using HRB.Cellario.Scripting.API;
using HRB.Cellario.Services.DTO;

namespace UFScripps.Scripting
{
	public class GenerateReport : AbstractScript
	{
		public override void Execute(IScriptingApi api)
		{
			int? myOrderID = (int)api.CurrentRun.RunNumber;

			var myTransferEvents = api.WebClient.Events.GetEvents(operation: "LiquidTransfer", orderId: myOrderID, state: "Finished");

			foreach (Cellario.Client.OperationEvent transferOpEvent in myTransferEvents)
			{
				Cellario.Client.Parameter transferParameter = transferOpEvent.Parameters.FirstOrDefault();
				string parameterName = transferParameter.Name;

				api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, string.Format("First Parameter Name: {0}",
					parameterName));
			}
		}
	}
}