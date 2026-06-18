/**
 * Adjust Liquid Transfers for Missing Plates
 * 
 * Optimizes liquid handling operations when some plates are missing
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
        	//if (api.CurrentPlate.PlateNumber != 1) return;
        	
        	string myEmptyMethod = "Empty Method.esc";
        	          
            IScriptingProtocolThread mySourceThread = api.CurrentRun.Protocol.Threads.FirstOrDefault();
            IScriptingPlate[] mySourcePlates = api.GetPlates().Where(p => p.CurrentThread.ThreadName == mySourceThread.ThreadName).ToArray();
            IScriptingProtocolStep[] myTransfers = mySourceThread.Steps.Where(s => s.StepName == "LiquidTransfer").ToArray();
            int numTransfers = myTransfers.Count();
            
            // for each Transfer in MyTransfers, there exists a related thread with a secondary node.  Does that thread contain plates?
            bool containsPlates = false;
            string myThreadName = "";
            IScriptingProtocolRemainingStep[] myPlateTransfers = new IScriptingProtocolRemainingStep[numTransfers];	
			List<IScriptingProtocolRemainingStep> myStepsToRemove = new List<IScriptingProtocolRemainingStep>();			
            
            foreach (IScriptingPlate plate in mySourcePlates)
            {
	            myPlateTransfers = plate.RemainingSteps.Where(s => s.StepName == "LiquidTransfer").ToArray();

	            for (int j = numTransfers-1; j >=0; j--)
	            {
	            	myThreadName = api.CurrentRun.Protocol.Threads.ElementAt(j+1).ThreadName;
	            	containsPlates = api.GetPlates().Where(p => p.CurrentThread.ThreadName == myThreadName).Any();
	            	if (!containsPlates) // if the associated thread does NOT contain plates, run an empty method and reduce the simulation time (this is the cleanest way to accomplish the goal)
	            	{
	            		myPlateTransfers[j].SimulationTime = TimeSpan.FromSeconds(1);
	            		myPlateTransfers[j].OperationParameters["Pipetter Method"] = myEmptyMethod;
	            	}
	            }
	        }    
        }
    }
}