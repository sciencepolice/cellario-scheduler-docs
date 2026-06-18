"""
Notify User

Displays interactive message boxes and dialogs to users
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
    # show a question returning yes/no response
    response = api.Messaging.Notify("Notify User Example Script", "Can you see this warning message ?",
            None, ScriptMessageType.Question, ScriptMessageSeverity.Warning)
        
    # show a simple message returning OK response
    response_mask = "User response to warning was {0}."
    response = response_mask.format(str(response))
    api.Messaging.Notify("Notify User Example Script", response, 
            "This is an example of displaying a simple, informational, message box.",
            ScriptMessageType.Information, ScriptMessageSeverity.Informational)
           
    ex = Exception("Exception deliberately thrown for example of notifiying user of a handled exception.")
    try:
        raise ex
    except Exception as ex:
        api.Messaging.Notify("Notify User Example Script", str(ex), None, ScriptMessageSeverity.Serious)