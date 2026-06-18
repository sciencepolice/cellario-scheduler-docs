"""
New Python Script

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


# Method called ahead of execution to optionally allocate resources.
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def AllocateResources(api : PythonScriptingApi):
    # e.g. api.GetResource('MicroSpin 1.1').Allocate()
    pass
    
    
# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
    # e.g. current = api.CurrentPlate.Barcode
    # Helper methods for resources and operations:
    # api.GetResource('MicroSpin 1.1').GetOperation('Spin').GetOperationParameter('Speed')
    pass
    
    
# Method called during Cellario protocol execution when any device operations for the sample are complete,
# allowing access to their results.
# <remarks>If a resource is allocated but not released, it will remain unavailable to the Cellario run-time scheduler.</remarks>
# <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and device operations, including releasing allocated resources.</param>
def ReleaseResources(api : PythonScriptingApi):
    # api.GetResource('MicroSpin 1.1').Release()   
    pass
