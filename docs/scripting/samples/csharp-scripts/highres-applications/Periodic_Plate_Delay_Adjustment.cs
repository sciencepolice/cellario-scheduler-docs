/**
 * Periodic Plate Delay Adjustment
 * 
 * Adds scheduled delays between plates for specific workflow requirements
 */

// changes the pacing time for the next plate, based on criteria which are selected below
using System;
using System.Linq;
using System.Collections.Generic;
using HRB.Cellario.Scripting.API;

namespace Customer.Scripting
{
    public class PlateDelayAdjustment : AbstractScript
    {
    
    // set selectiontype to choose periodic or specific plates to adjust.  Comment out the ones you don't want.
    
	    //private static string selectiontype = "periodic";
	    private static string selectiontype = "specific";   
	    
	// set the parameters for plate selection
		private static List<int> platesToChange = new List<int>(new int[]{2});	// to add plates to the list, add to comma separated values inside curly brackets e.g. {2,10,25}
    	private static double period = 3;		// period of adjustment (e.g. - every 3rd plate)
    	private static double platesafter = 0;	// how many plates after the periodic point (e.g. - 1 plate after the absolute period)
    	private static double delaymin = 30;	// desired pacing time or additional pacing time, in minutes
    	    	
        public override void Execute(IScriptingApi api)
        {
            // first, get the next plate
            var nextplates = api.GetPlatesForCurrentThread().Where(s => s.PlateNumber == api.CurrentPlate.PlateNumber+1).ToList();
            if (!nextplates.Any()) return;
          	
          	bool changedelay = false;
            
            // decide if the next plate should be adjusted or not, based on criteria selected in main
            foreach (var plate in nextplates)
            {
            	switch (selectiontype)
	            {
	            	case "periodic":
	            		if (plate.PlateNumber % period == platesafter)
	            		{
	            			changedelay = true;
	            		}
	            		break;
	            	case "specific":
						if (platesToChange.Where(p => p == plate.PlateNumber).Any())
						{
							changedelay = true;
						}
						break;					
	            }

				if (changedelay)
				{
					var platestep = plate.RemainingSteps.FirstOrDefault();
					if (platestep == null) return;						
					platestep.PauseAfter = TimeSpan.FromMinutes(delaymin);
				}
			}     
        }
    }
}