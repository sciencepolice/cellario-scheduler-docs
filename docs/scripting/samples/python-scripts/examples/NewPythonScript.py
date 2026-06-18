"""
NewPythonScript

Example script showing basic Cellario protocol automation and customization features
"""


  


#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


# Method called ahead of execution to optionally allocate resources.
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
 
def AllocateResources(api : ScriptedApi):
    
     # e.g. api.Resources['Multimode_Reader_01'].Allocate()
     pass




# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : ScriptedApi):
   #  e.g. current = api.CurrentPlate.Barcode
   # api.Resources access  Cellario resources
   #  e.g. api.Resources['Multimode_Reader_01'].DeviceState
    pass




# Method called during Cellario protocol execution when any device operations for the sample are complete,
# allowing access to their results.
# <remarks>If a resource is allocated but not released, it will remain unavailable to the Cellario run-time scheduler.</remarks>
# <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and device operations, including releasing allocated resources.</param>
def ReleaseResources(api : ScriptedApi):
	# api.Resources['Multimode_Reader_01'].Release()	
         pass


  