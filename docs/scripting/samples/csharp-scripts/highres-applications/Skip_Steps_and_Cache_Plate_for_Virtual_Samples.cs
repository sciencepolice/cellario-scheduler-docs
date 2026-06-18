/**
 * Skip Steps and Cache Plate for Virtual Samples
 * 
 * Advanced production script for specialized laboratory automation workflows
 */


using System;
using System.Linq;
using HRB.Cellario.Scripting.API;
using System.Collections.Generic;

namespace Customer.Scripting
{
    public class RemoveSteps : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
        	//only execute this script once -- when the first plate in the thread is introduced
        	if (api.CurrentPlate.PlateNumber != 1) return;
        	
            var plates = api.GetPlatesForCurrentThread();
            foreach (var plate in plates)
            {
                if (plates.Any(p => p.StartingLocation.ResourceName == plate.StartingLocation.ResourceName &&
                                    p.StartingLocation.Stack == plate.StartingLocation.Stack &&
                                    p.StartingLocation.Position == plate.StartingLocation.Position &&
                                    p.PlateNumber < plate.PlateNumber))
                {
                    RemoveStep(plate, "SPIN");
                    RemoveStep(plate, "PIERCE");
                }
                if (plates.Any(p => p.StartingLocation.ResourceName == plate.StartingLocation.ResourceName &&
                                    p.StartingLocation.Stack == plate.StartingLocation.Stack &&
                                    p.StartingLocation.Position == plate.StartingLocation.Position &&
                                    p.PlateNumber > plate.PlateNumber))
                    RemoveStep(plate, "SEAL");
                    ChangeEndResource(api, plate, "Plate_Hotel_8pos_01");
            }
        }
        
        public void RemoveStep(IScriptingPlate plate, string stepName)
        {
            var ind = plate.RemainingSteps.ToList().FindIndex(x => x.StepName.ToUpper().Contains(stepName));
            var moveStep = plate.RemainingSteps.ElementAt(ind + 1);
            plate.RemainingSteps.Remove(moveStep);
            var step = plate.RemainingSteps.ElementAt(ind);
            plate.RemainingSteps.Remove(step);
        }
        
        public void ChangeEndResource(IScriptingApi api, IScriptingPlate plate, string ResourceName)
        	{
            	plate.RemainingSteps.LastOrDefault(s => s.StepName.Contains("End")).AssignedResources.Clear();
            	plate.RemainingSteps.LastOrDefault(s => s.StepName.Contains("End")).AssignedResources.Add(api.Resources[ResourceName]);
            }
    }
}