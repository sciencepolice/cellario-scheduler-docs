/**
 * Reader Data Decision
 * 
 * Demonstration script showcasing specific Cellario features and integration capabilities
 */

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// DEMO ONLY. THIS USES A RANDOM NUMBER GENERATOR TO MAKE DECISIONS FOR DEMONSTRATING DYNAMIC BEHAVIOR
namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        
        public override void Execute(IScriptingApi api)
        {
            // 
            // TODO - implement reader output analysis to determine whether plate passed or failed threshold check 
            // for now, just assign a random pass or fail to this plate
            IScriptingRunOrderParameter threshold = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Signal Threshold").FirstOrDefault();
            //IScriptingRunOrderParameter ready = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "ReadyToFix").FirstOrDefault();
            Random rnd = new Random();
            int modulus = rnd.Next(3,5);
            // you can generate a random number in any range
            if (api.CurrentPlate.PlateNumber % modulus == 0)
            {
                threshold.ParameterValue = "90";
                //ready.ParameterValue = true;
            }
            else
            {
                threshold.ParameterValue = "70";
                //ready.ParameterValue = false;
            }
        }
    }
}