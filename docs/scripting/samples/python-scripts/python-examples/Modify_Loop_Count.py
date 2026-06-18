"""
Modify Loop Count

Changes the number of loop iterations based on plate or run conditions
"""

  #Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *



# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
   #  e.g. current = api.CurrentPlate.Barcode
   # api.Resources access  Cellario resources
   #  e.g. api.Resources['Multimode_Reader_01'].DeviceState
   steps = api.CurrentPlate.RemainingSteps
   for step in steps:
       if step.StepName == "Loop End":
           oldLoopCount = step.NumberOfLoops;
           newLoopCount = api.CurrentPlate.PlateNumber;
           step.NumberOfLoops = newLoopCount;
        
           api.Messaging.Notify("Modify Looping Test", "Changed Looping Count from " + str(oldLoopCount) + " to " + str(newLoopCount)+ " for plate " + api.CurrentPlate.Name);
  