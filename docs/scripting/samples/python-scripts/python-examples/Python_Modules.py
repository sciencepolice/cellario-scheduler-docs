"""
Python Modules

Demonstrates importing and using Python libraries like NumPy and Pandas
"""

# Python libraries example. 
# You may need to run pip in order to install any missing library.
from base64 import *
import numpy  as np
import pandas as pd


# Method called during Cellario protocol execution when a sample arrives at the scripting step.
# <remarks>Executes synchronously with the run scheduler. Device operation results not available
# until <see cref="ReleaseResources"/> method is called.</remarks>
# <Object name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
# samples, resources and devices operations.</param>
def Execute(api : ScriptedApi):
   #Python built-in module example
   encoded = b64encode(b'data to be encoded')
 
   #Numpy example
   sqrt = np.sqrt(100)
  
   #Pandas Example
   s = pd.Series([1, 3, 5, np.nan, 6, 8])
 