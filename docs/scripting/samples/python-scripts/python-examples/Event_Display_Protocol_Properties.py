"""
Event Display Protocol Properties

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    #about this script 
    #info about this script from api.CurrentRun() and api.CurrentProtocol()
    runorderId = api.CurrentPlate.CurrentRun.RunNumber
    runOrderDesc = api.CurrentPlate.CurrentRun.Description
    protocolName = api.CurrentPlate.CurrentProtocol.ProtocolName
    
    message = "Before Order State Change for run = " + str(runorderId) + " \n"
    message += "Order Description '" + runOrderDesc + "' \n"
    message += "Order State - " + str(api.CurrentPlate.CurrentRun.RunState) + " \n"
    message += "Protocol Name - " + protocolName + " \n"
    api.Messaging.Notify("Before Order State Change", message)
    
    
    properties = api.CurrentPlate.CurrentProtocol.Properties
    
    index = 0
    for property in properties:
        index += 1
        pname = property.PropertyName
        pid = property.PropertyId
        pvalue = property.PropertyValue
        pdesc = property.PropertyDescription
    
        message = "Property " + str(index) + " Name is '" + pname + "' \n"
        message += "Property Id is '" + str(pid) + "' \n"
        message += "Property Value is '" + pvalue + "' \n"
        message += "Property Desc is '" + pdesc + "' \n"
    
        api.Messaging.Notify("Protocol Property " + str(pname), str(message))