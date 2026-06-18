"""
Event Update Storage Location

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


''' 
Example Event UpdateStorageLocation Script. For a given Resource, GetStorageLocations() and UpdateStorageLocation(location)
can be used to add a plate not associated with a run order to a storage location, essentially blocking the location from 
being allocated by the cell controller (but not blocked from new run orders which assume the plates are where they are assigned.
For this example we use a small storage device and at the start of the run we fill all the storage locations by adding barocode 
or SampleType fields. The protocol should have a plate that starts outside of that storage but ends in storage. 
The run should deadlock if all storage locations are filled, work if script leaves available slots. 
'''


def Execute(api : PythonScriptingApi):
    resource_name = "Plate_Hotel_8pos_01"
    # note: will fail if not valid
    sample_type_name = "Generic 96 Microtiter Plate"
    resource_list = list(api.Resources)
    for e in resource_list:
        if (e.Value.Name == resource_name):
            resource = e.Value
    rsp_list = resource.GetStorageLocations()
    #api.Messaging.Notify("Rsp_List", str(rsp_list[1]))
    
    for location in rsp_list:
        if location.Position == 1:
            location.Barcode = "Script00" + str(location.Position)
            location.SampleType = sample_type_name
        elif location.Position == 2:
            location.SampleType = sample_type_name
            #enable this to fail and leave position open:
            #location.SampleType = "InvalidName"
        else:
            location.Barcode = "Script00" + str(location.Position)
        result = resource.UpdateStorageLocation(location)
        if (result):
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal,
                "Update Resource Success for " + resource_name + " location " + str(location.Position))
        else:
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Minimal,
                "Update Resource Failed for " + resource_name + " location " + str(location.Position))
         
        
            
         


  