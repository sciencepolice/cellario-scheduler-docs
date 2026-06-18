"""
Add Operation

Dynamically adds new protocol operations to plates during execution
"""

 # Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *

 


# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler,
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
    if (api.CurrentPlate.PlateNumber % 2 == 0):
        api.Messaging.Notify("Plate Number = ", str(api.CurrentPlate.PlateNumber))
        cp = api.CurrentPlate
        # CurrentThread will contain all the operations for the thread and can be used to add another similar step to plate
        stepList = api.CurrentPlate.CurrentThread.Steps
        spinStep = next(c for c in stepList if c.StepName == "Spin")

 

        # New step to be added must be a clone of existing step
        newSpinStep = spinStep.CloneStep();
        remaining_steps = cp.RemainingSteps
        step_to_clone = next(e for e in remaining_steps)
        newOpStep = step_to_clone.CloneStep("Dispense")

        current_value = None

 

        # Cloned steps with new operation name do not have resources assigned. Must be added if required
        for e in api.Resources.values():
            #api.Messaging.Notify("Test", str(e.Name))
            if ("Dispenser" in str(e.Name)):
                current_value = e
                break


        newOpStep = (IScriptingProtocolRemainingStep)(newOpStep)
        newOpStep.AssignedResources.Add(current_value)


        # Cloned steps with new operation name does not have parameters assinged. Must be added if required
        newOpStep.OperationParameters.Add("Prime Volume", "10")

 

        # Add steps to tree without optional position it will be added before End
        # Add insert index to InsertStep(step,index) to place at specific place in list.

        new_dispense_sim_time = TimeSpan(0, 0, 23)
        newOpStep.SimulationTime = new_dispense_sim_time
        cp.RemainingSteps.InsertStep(newSpinStep, 4)
        cp.RemainingSteps.InsertStep(newOpStep, 3)
        api.Messaging.Notify("Test", "Steps are inserted")

 

        # Modify values for steps in remaining steps

        new_spin_sim_time = TimeSpan(0, 0, 17)


        remaining_steps_list = list(cp.RemainingSteps)
        for step in remaining_steps_list:
            #api.Messaging.Notify("NAME=", step.StepName)
            if step.StepName == "Dispense":
                step.SimulationTime = new_dispense_sim_time
                api.Messaging.Notify("New Dispense Sim Time = ", str(step.SimulationTime))
            if step.StepName == "Spin":
                step.SimulationTime = new_spin_sim_time
                api.Messaging.Notify("New Spin Sim Time = ", str(step.SimulationTime))