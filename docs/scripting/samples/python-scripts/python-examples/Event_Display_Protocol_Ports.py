"""
Event Display Protocol Ports

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

   threads = api.CurrentPlate.CurrentProtocol.Threads

   for thread in threads:
       threadName = thread.ThreadName
       inputPortName = thread.InputPortName
       inputPortTag = thread.InputPortTag
       outputPortName = thread.OutputPortName
       outputPortTag = thread.OutputPortTag


       message = "Input Port Name is '" + inputPortName + "' \n"
       message += "Input Port Tag is '" + inputPortTag + "' \n"
       message += "Output Port Name is '" + outputPortName + "' \n"
       message += "Output Port Tag is '" + outputPortTag + "' \n"

       api.Messaging.Notify("Thread " + threadName, message)