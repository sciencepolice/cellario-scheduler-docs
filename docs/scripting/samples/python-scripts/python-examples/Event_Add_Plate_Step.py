"""
Event Add Plate Step

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *
import clr
clr.AddReference("System")
from System import *


# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Expects a simple protocol with one thread with a Dispense Operation already existing</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
    run_order_id = api.CurrentPlate.CurrentRun.RunNumber
    run_order_desc = api.CurrentPlate.CurrentRun.Description
    protocol_name = api.CurrentPlate.CurrentProtocol.ProtocolName
    
    message = ("Before Order State Change for run = " + str(run_order_id) + " \n"
              + "Order Description '" + str(run_order_desc) + "' \n"
              + "Protocol Name - " + protocol_name + " \n")
    api.Messaging.Notify("Before Order State Change", message)
    
    new_sim_time = TimeSpan(0, 0, 45)
    
    protocol = api.CurrentPlate.CurrentProtocol
    threads = protocol.Threads
    steps = threads[0].Steps
    #api.Messaging.Notify("Steps ", str(steps))
    if steps[0] == None:
        api.Messaging.Notify("Step[0] ", "doesn't exist")
    else:
        steps[0].SimulationTime = new_sim_time
        plates_list = api.GetPlates()
        plate = next(e for e in plates_list)
        #step_count = plate.RemainingSteps.len()
        clone_step = next(e for e in plate.RemainingSteps if e.StepName == 'Dispense').CloneStep()
        api.Messaging.Notify("Steps ", "Dispense step is cloned")
        clone_move_step = next(e for e in plate.RemainingSteps if e.StepName == 'Move').CloneStep()
        api.Messaging.Notify("Steps ", "Move step is cloned")
        plate.RemainingSteps.InsertStep(clone_move_step)
        plate.RemainingSteps.InsertStep(clone_step)
        
        api.Messaging.Notify("Add Step", "added step to first plate")