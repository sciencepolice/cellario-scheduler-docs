/**
 * Incubate Time Course
 * 
 * Implements variable incubation times for time-course experiments
 */

// Allows varying incubation durations for a time-course study, with fixed min and max values, and interstitial values based on batch size
// Can choose exponential course, power course or linear course.  Default is no adjustment.
// Can set min and max incubation intervals by modifying lines 17 and 18 (in minutes).

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;


namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class IncubateTimeCourse : AbstractScript
    {
    	///<TIME COURSE BOUNDARY SPECIFICATIONS>
        // Calculate the fit for a time course 10min to 24hr for this order size
        private static double firstinterval = 10;	// duration in minutes of first incubate duration
        private static double lastinterval = 1440;	// duration in minutes of last incubate duration
        private static double[] intervals = {0, 60*12, 60*16, 60*18, 60*20, 60*21, 60*22, 60*22.5, 60*23, 60*23.5}; // specific durations in minutes

		///<TIME COURSE TYPE>
		// select fit by commenting in/out from the following
        //private static string fit = "no adjustment";
        private static string fit = "exponential";
		//private static string fit = "power";
		//private static string fit = "linear";
		//private static string fit = "specified";
		
        public override void Execute(IScriptingApi api)
        {									
        	double batchsize = api.GetPlatesForCurrentThread().Count();
        	
        	// Exponential curve - isexponential = true to get an exponential interval course
        	double omega1 = Math.Log(firstinterval);
        	double omega2 = Math.Log(lastinterval);
        	double slopexomega = (omega2-omega1)/(batchsize-1);
        	double interceptxomega = omega1-slopexomega*1;
        	double A = Math.Exp(interceptxomega);
        	double k = slopexomega;
        	
        	// Power curve - ispower = true to get a power interval course
        	double lnx = Math.Log(batchsize);
        	double lny = Math.Log(lastinterval);
        	double powerm = (Math.Log(lastinterval)-Math.Log(firstinterval))/(Math.Log(batchsize)-Math.Log(1));
        	double logk = lny - powerm*lnx;
        	double powerk = Math.Exp(logk);
        	
        	// Linear curve - isexponential = false and ispower = false to get a linear interval course
        	double m = (lastinterval - firstinterval)/(batchsize - 1);
        	double b = firstinterval - m*1;
        	
            // Get the current plate - during a protocol run this will always have a value
            var current = api.CurrentPlate;
            if (current != null)
            {
                // calculate the incubation time based on the plate number
                int platenumber = api.CurrentPlate.PlateNumber;
                
                // calculate incubation time in minutes according to the function characterized above
                double timepoint = 0;
                
                switch (fit)
	            {
	            	case "exponential":
						timepoint = A*Math.Exp(k*platenumber);	// exponential time course
	            		break;
	            	case "power":
						timepoint = powerk*Math.Pow(platenumber,powerm);
						break;
					case "linear":
						timepoint = m*platenumber + b;	// linear time course
						break;
					case "specified":
						timepoint = intervals[platenumber-1];
						break;		
					default:  // no adjustment in default case
						timepoint = current.RemainingSteps.First(s => s.StepName.StartsWith("Incubate")).PauseBefore.TotalMinutes;	// default is no adjustment
						break;						
	            }
                
                // find an upcoming incubate step
                var incubate = current.RemainingSteps.First(s => s.StepName.StartsWith("Incubate"));
                if (incubate != null)
                {
                    // change pause before for next incubate operaton for the current plate to 10 seconds
                    incubate.PauseBefore = TimeSpan.FromMinutes(timepoint);
                    // change timeout for next incubate operaton for the current plate to 30 seconds
                    incubate.TimeOut  = TimeSpan.FromMinutes(timepoint+15);
                }

             }
        }
    }
}