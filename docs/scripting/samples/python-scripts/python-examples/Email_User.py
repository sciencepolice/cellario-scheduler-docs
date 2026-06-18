"""
Email User

Sends email notifications during protocol execution
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
    if api.CurrentPlate.PlateNumber == 1:
       api.Messaging.Email(["user@example.com", "other@example.com"], "Example email subject", "Started protocol {0}.".format(api.CurrentPlate.CurrentProtocol.ProtocolName))
       api.Messaging.Notify("Email", "Email has been sent")