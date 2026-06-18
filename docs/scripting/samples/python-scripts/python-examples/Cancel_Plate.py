"""
Cancel Plate

Cancels remaining protocol steps for specific plates based on conditions
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
    current = api.CurrentPlate
    if (current.Name.endswith("2")):
        #this example cancels all remaing steps for the current sample if the sample name ends in 2
        current.Cancel()
        api.Messaging.Notify("Cancel Plate Modal", "This sample: " + api.CurrentPlate.Name + " cancels all remaing steps for the current sample")