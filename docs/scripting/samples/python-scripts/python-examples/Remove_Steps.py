"""
Remove Steps

Removes specific protocol steps from plates dynamically
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    if (api.CurrentPlate.PlateNumber != 1):
        return

    plates = list(api.GetPlatesForCurrentThread())
    
    for plate in plates:
        remove_step(api, plate, "SPIN")


# Removes the first instance of the step from this plate.
def remove_step (api : PythonScriptingApi, plate, step_name):
    remaining_steps_list = list(plate.RemainingSteps)
    step = next(s for s in remaining_steps_list if s.StepName.upper() == step_name)
    ind = remaining_steps_list.index(step)
    plate.RemainingSteps.Remove(step)
    api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, "Removing step: " + step.StepName)
  