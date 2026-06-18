"""
Event Run Order Parameters

Python scripting example demonstrating Cellario API integration and automation capabilities
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
    # run time parameter name
    reader_output_directory_name = "Output File Directory";
    
    # creation of a message about order run information
    run_order_id = api.CurrentPlate.CurrentRun.RunNumber
    run_order_desc = api.CurrentPlate.CurrentRun.Description
    protocol_name = api.CurrentPlate.CurrentProtocol.ProtocolName
    
    message = ("Before Order State Change for run = " + str(run_order_id) + " \n"
              + "Order Description '" + str(run_order_desc) + "' \n"
              + "Protocol Name - " + protocol_name + " \n")
    api.Messaging.Notify("Before Order State Change", message)
    
    # number of parameters in order run
    parameters = api.CurrentPlate.CurrentRun.RunOrderParameters
    # TBD api.Messaging.Notify("Run Order Parameter Count", "Found - ", str(len(parameters)))
    
    index = 0
    new_param_value = None
    for e in parameters:
        index += 1
        pname = e.ParameterName
        ppid = e.ProtocolParameterId
        pvalue = e.ParameterValue
        roparamid = e.RunOrderParameterId
        
        message = ("Parameter  " + str(index) + " Name is '" + str(pname) + "' \n"
              + "Protocol Parameter Id is '" + str(ppid) + "' \n"
              + "Parameter Value is '" + str(pvalue) + " \n"
              + "Run Order Parameter Id is '" + str(roparamid) + " \n")
        api.Messaging.Notify("Run Order Parameter ", message)
        
        if (pname == reader_output_directory_name):
            prot_param_list = api.CurrentPlate.CurrentProtocol.Parameters
            prot_param = next(c for c in prot_param_list if c.ProtocolParameterId == ppid)
            
            choices_list = prot_param.ChoicesList
            new_param_value = choices_list[len(choices_list) - 1]
            e.ParameterValue = new_param_value
            
            message = ("Parameter  " + str(index) + " Name '" + str(pname) + "' \n"
               + "Changed value from '" + str(pvalue) + "' to '" + str(new_param_value) + "' \n")
              
            api.Messaging.Notify("Run Order Parameter ", message)

  