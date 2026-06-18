"""
Event Write Well Transfer Log

Generates CSV reports of liquid transfer operations
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *
from pathlib import Path
import csv


# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : PythonScriptingApi):
    ro_id = str(api.CurrentPlate.CurrentRun.RunNumber)
    api.Messaging.Notify("Run Order Id = ", ro_id)
    
    # note - replace hard coded paths!
    path = Path("C:/Users/AKupriianov/Desktop/Work/Cellario/VM/TCs Data/4.0/Python scripting/Event Write Well Transfer Log")
    if (not path.is_dir()):
        api.Messaging.Notify("Current directory doesn't exist", "something went wrong / your path doesn't exist")
    else:
        filename = "Transfers_Run_" + ro_id + ".csv"
        output_file = path.joinpath(filename)
        api.Messaging.Notify("Output file to write Well Transfer Log", str(output_file))
        
        x_fers = api.CurrentPlate.CurrentRun.WellTransfers
        x_fers = list(x_fers)
        
        if (len(x_fers) > 0):
            #api.Messaging.Notify("Well Transfers Count", str(len(x_fers)))
            csv_data_list = []
            for e in x_fers:
                row = []
                s_id = str(e.SourceOrderSampleId)
                row.append(s_id)
                s_row = str(e.SourceWellRow)
                row.append(s_row)
                s_col = str(e.SourceWellCol)
                row.append(s_col)
                d_id = str(e.DestOrderSampleId)
                row.append(d_id)
                d_row = str(e.DestWellRow)
                row.append(d_row)
                d_col = str(e.DestWellCol)
                row.append(d_col)
                vol = str(e.Volume)
                row.append(vol)
                #api.Messaging.Notify("Well Transfers Data", str(row))
                csv_data_list.append(row)
            
            with open(str(output_file), 'w') as f:
                write = csv.writer(f)
                write.writerows(csv_data_list)
                
           
      