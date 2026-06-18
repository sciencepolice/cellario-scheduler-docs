"""
Modify Simulation Time

Adjusts protocol step timing estimates for better scheduling
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *
import clr
clr.AddReference("System")
from System import *


# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
    newSimTime = TimeSpan(0, 30, 0)
    if (api.CurrentPlate.PlateNumber % 2 == 0):
        for step in api.CurrentPlate.RemainingSteps:
            step.SimulationTime = newSimTime
