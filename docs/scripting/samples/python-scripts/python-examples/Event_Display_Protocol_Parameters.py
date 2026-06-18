"""
Event Display Protocol Parameters

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    runorderId = api.CurrentPlate.CurrentRun.RunNumber
    runOrderDesc = api.CurrentPlate.CurrentRun.Description
    protocolName = api.CurrentPlate.CurrentProtocol.ProtocolName
    
    message = "Before Order State Change for run = " + str(runorderId) + " \n"
    message += "Order Description '" + runOrderDesc + "' \n"
    message += "Order State - " + str(api.CurrentPlate.CurrentRun.RunState) + " \n"
    message += "Protocol Name - " + protocolName + " \n"
    api.Messaging.Notify("Before Order State Change", message)
    
    parameters = api.CurrentPlate.CurrentProtocol.Parameters
    api.Messaging.Notify("Protocol Parameter Count", "Found - " + str(len(parameters)))
    
    index = 0
    for parameter in parameters:
        index += 1
        pname = parameter.ParameterName
        ppid = parameter.ProtocolParameterId
        pvalue = parameter.DefaultValue
        pdesc = parameter.Description
        plevel = parameter.ParameterLevel
        ptype = parameter.ParameterType

        message = "Parameter " + str(index) + " Name is '" + pname + "' \n"
        message += "Parameter Id is '" + str(ppid) + "' \n"
        message += "Parameter Value is '" + pvalue + "' \n"
        message += "Parameter Desc is '" + pdesc + "' \n"
        message += "Parameter Level is '" + plevel + "' \n"
        message += "Parameter Type is '" + ptype + "' \n"

        api.Messaging.Notify("Protocol Parameter " + str(index), message)
    