"""
Modify Critical Sections

Adjusts protocol flow gates and timing constraints during execution
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    critical_segments = api.CurrentPlate.CurrentRun.CriticalSegments
    if critical_segments.Length > 0:
        cs = critical_segments.Get(0)
        # alternatively you can use:
        # cs = critical_segments[0]
        # but intellisense is lost this way.
    
        cs.MaximumTime = TimeSpan(0, 0, 30)
        cs.MaximumPlates = 3