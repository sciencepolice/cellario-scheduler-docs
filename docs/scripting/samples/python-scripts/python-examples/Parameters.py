"""
Parameters

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
    try:
        script_params_list = list(api.CurrentScript.ScriptingScriptParameters)
        if script_params_list == None:
            api.Messaging.Notify("Script Test", "ScriptParameters null")
        elif len(script_params_list) == 0:
            api.Messaging.Notify("Script Test", "No ScriptParameters defined!")
        else:
            for item in script_params_list:
                s_param_name = item.ParameterName
                s_param_value = str(item.DefaultValue)
                api.Messaging.Notify("Script Test", "Parameter - " + s_param_name + " Value - " + s_param_value)
    except Exception as ex:
        api.Messaging.Notify("Script Test", "Exception error - " + str(ex))
                
    