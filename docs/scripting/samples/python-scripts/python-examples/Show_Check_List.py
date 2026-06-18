"""
Show Check List

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *

def Execute(api : PythonScriptingApi):
    if (api.CurrentPlate.PlateNumber != 1):
        return

    items = ["DMSO bottle is full", "Destination plates are loaded"]
    results = api.Messaging.ShowChecklist("Run Validation", items)

    if len(results) == 0:
        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Pausing run due to requirement checklist not acknowledged.")
        api.CurrentPlate.CurrentRun.Pause()
        return
        
    all_checked = all([val for val in results.Values])

    if not all_checked:
        api.Messaging.WriteError(ScriptErrorSeverity.Error, "Stopping system due to required items not checked.")
        api.System.Stop()
        return
             
            