"""
Adjust Robot Move

Modifies robot movement parameters like pick heights during execution
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    # Modifies Pick Height for all subsequent move steps for this plate.
    # The Pick Height parameter must be configured in the database first.
    remaining_steps_list = list(api.CurrentPlate.RemainingSteps)
    for step in remaining_steps_list:
        if (step.StepName == "Move"):
            step.OperationParameters.Remove("Pick Height")
            step.OperationParameters.Add("Pick Height", "30")
            
            # Optional: trace per-step parameters
            per_step_op_parameters = list(step.OperationParameters)
            result_string = ""
            for step_op_parameter in per_step_op_parameters:
                parameter_values_string = "{0} = {1}".format(step_op_parameter.Key, step_op_parameter.Value)
                result_string = result_string + "
" + parameter_values_string
            diagnostic_lvl_message = "Step {0} operation parameters: {1}".format(step.StepName, result_string)
            api.Messaging.WriteDiagnostic(ScriptLogLevel.Verbose, diagnostic_lvl_message)
            
            
        