/**
 * Modify Run Order Parameter
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      ModifyRunOrderParameter.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary.CSharp
{
	public class ModifyRunOrderParameter : AbstractScript
	{
		public override void Execute(IScriptingApi api)
		{
			if (api.CurrentPlate.PlateNumber != 1) return;

			var newValue = string.Empty;

			//what we can access
			var pprops = api.CurrentPlate.CurrentProtocol.Properties.ToList();
			if (pprops.Count > 0)
			{
				var firstProtProp = pprops.First() as IScriptingProtocolProperty;
				var fppPropDescription = firstProtProp.PropertyDescription;
				var fppPropId = firstProtProp.PropertyId;
				var fppPropName = firstProtProp.PropertyName;
				var fppPropValue = firstProtProp.PropertyValue;

				var protPropMsg = "There are " + pprops.Count().ToString() + " Protocol Properties in this run \n";
				protPropMsg += " The first one: \n";
				protPropMsg += " Name - " + fppPropName + "\n";
				protPropMsg += " Id - " + fppPropId.ToString() + ";\n";
				protPropMsg += " Value - " + fppPropValue + "; \n";
				protPropMsg += " Description - " + fppPropDescription;
				api.Messaging.Notify("Protocol Property", protPropMsg);
			}
			else
			{
				api.Messaging.Notify("Protocol Property", "No Protocol Properties Found");
			}

			var pParameters = api.CurrentPlate.CurrentProtocol.Parameters.ToList();
			if (pParameters.Count > 0)
			{
				var firstProtParam = pParameters.First() as IScriptingProtocolParameter;
				var fppChoicesList = firstProtParam.ChoicesList;
				var fppDefaultValue = firstProtParam.DefaultValue;
				var fppDescription = firstProtParam.Description;
				var fppParameterLevel = firstProtParam.ParameterLevel;
				var fppParameterName = firstProtParam.ParameterName;
				var fppParameterType = firstProtParam.ParameterType;
				var fppProtocolParamaterId = firstProtParam.ProtocolParameterId;

				var protParamMsg = "There are " + pprops.Count().ToString() + " Protocol Parameters in this run \n";
				protParamMsg += " The first one: \n";
				protParamMsg += " Name - " + fppParameterName + "\n";
				protParamMsg += " Id - " + fppProtocolParamaterId.ToString() + ";\n";
				protParamMsg += " Type - " + fppParameterType + ";\n";
				protParamMsg += " Level - " + fppParameterLevel + ";\n";
				protParamMsg += " Value - " + fppDefaultValue + "; \n";
				foreach (var val in fppChoicesList)
				{
					protParamMsg += " Value Option - " + val + "; \n";
				}
				protParamMsg += " Description - " + fppDescription;

				api.Messaging.Notify("Protocol Parameter", protParamMsg);
				newValue = fppChoicesList.Last();
			}
			else
			{
				api.Messaging.Notify("Protocol Parameter", "No Protocol Parameters Found");
			}

			var roParams = api.CurrentPlate.CurrentRun.RunOrderParameters.ToList();
			if (roParams.Count > 0)
			{
				var firstRoParams = roParams.First() as IScriptingRunOrderParameter;
				var rParamPName = firstRoParams.ParameterName;
				var rParamPValue = firstRoParams.ParameterValue;
				var rParamId = firstRoParams.RunOrderParameterId;
				var rParampParamId = firstRoParams.ProtocolParameterId;

				var roParamMsg = "There are " + pprops.Count().ToString() + " Run Order Parameters in this run \n";
				roParamMsg += " The first one: \n";
				roParamMsg += " Name - " + rParamPName + "\n";
				roParamMsg += " Id - " + rParamId.ToString() + ";\n";
				roParamMsg += " ProtParamId - " + rParampParamId.ToString() + ";\n";
				roParamMsg += " Value - " + rParamPValue + "; \n";
				api.Messaging.Notify("Run Order Parameter", roParamMsg);

			}
			else
			{
				api.Messaging.Notify("Run Order Parameter", "No Run Order Parameters Found");
			}

			if (roParams.Count > 0 && pParameters.Count > 0)
			{
				var firstRoParams = roParams.First() as IScriptingRunOrderParameter;
				var oldValue = firstRoParams.ParameterValue;
				firstRoParams.ParameterValue = newValue;
				var modifyMsg = "Changed the Run Order Parameter " + firstRoParams.ParameterName + "\n";
				modifyMsg += " From: " + oldValue + "\n";
				modifyMsg += " To: " + newValue + "\n";
				api.Messaging.Notify("Run Order Parameter Changed", modifyMsg);
			}
		}
	}
}
