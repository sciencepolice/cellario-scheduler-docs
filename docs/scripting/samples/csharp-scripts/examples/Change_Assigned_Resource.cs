/**
 * Change Assigned Resource
 * 
 * Dynamically reassigns protocol steps to different devices or resources
 */

//css_reference Common\Newtonsoft.Json.dll
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HRB.Cellario.Scripting.API;
using System.Windows.Documents;


namespace HRB.Cellario.Scripting.ScriptLibrary
{
	public class UpdateODTCParameters : AbstractScript
	{
		private static readonly HttpClient client = new HttpClient();
		private static readonly Random random = new Random();


		public override void Execute(IScriptingApi api)
		{
			try
			{
				string deviceToCheck = "ODTC";


				string configPath = @"C:\ODTC_config.json";
				if (!File.Exists(configPath))
				{
					api.Messaging.WriteError(ScriptErrorSeverity.Error, "Config file not found at: " + configPath);
					return;
				}


				// Load config and get available ODTCs
				var config = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(configPath));
				var available = config.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList();


				if (available.Count == 0)
				{
					api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "No available ODTCs (all set to 0). Using protocol defined ODTC assignment");
					return;
				}


				// Select one randomly
				string selected = available[random.Next(available.Count)];
				if (api.Data.PerRunData.ContainsKey("SelectedODTC"))
				{
					selected = api.Data.PerRunData["SelectedODTC"].ToString();
				}
				api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Selected ODTC: " + selected);

				//grab all steps for the current plate assigned to devices that have part of the name of the device we're looking for in the name, ie: 'ODTC' or 'Prime'
				var odtcSteps = api.CurrentPlate.RemainingSteps.Where(x => (x.AssignedResources.All(y => y.Name.Contains(deviceToCheck))) && x.AssignedResources.Count() > 0);

				// grabs all steps from all plates in the run that use the kind of device we're loooking for, ie: 'ODTC'. Selecting all at once will not log as well the barcodes for all the plates whose
				// steps got edited, because many of the plates won't have started yet and will not have a sample name or barcode associated with themselves yet
				//var odtcSteps = api.GetPlates().SelectMany(x => x.RemainingSteps).Where(x=> (x.AssignedResources.All(y=> y.Name.Contains(deviceToCheck))) &&  x.AssignedResources.Count() > 0);

				if (!odtcSteps.First().AssignedResources.Any(x => x.Name.Contains(selected)))
				{
					api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Selected resource name not found in list of assigned resources, check that resource names in config match the names of resources in cellario");
				}


				foreach (var odtcStep in odtcSteps)
				{
					var resourcesToRemove = odtcStep.AssignedResources.Where(x => !(x.Name.Contains(selected))).ToArray();
					foreach (var resourceToRemove in resourcesToRemove)
					{
						odtcStep.AssignedResources.Remove(resourceToRemove);
					}
				}

				// Save to PerRunData
				api.Data.PerRunData["SelectedODTC"] = selected;
				api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Saved " + selected + " to PerRunData.");


				// Update config: selected = 0 (used), others unchanged
				config[selected] = 0;
				File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
				api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Updated config: " + selected + " set to 0.");


			}


			catch (Exception ex)
			{
				api.Messaging.WriteError(ScriptErrorSeverity.Error, "Script failed: " + ex.Message);
			}


		}
	}
}