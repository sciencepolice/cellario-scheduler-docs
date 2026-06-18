"""
07 -  NumPy

Shows advanced data analysis and visualization using NumPy and Matplotlib
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *
import matplotlib.pyplot as plt
import numpy as np

def Execute(api : ScriptedApi):
    mean = 0
    std_dev = 1
    size = 1000
    
    data = np.random.normal(mean, std_dev, size)
    
    # Plot the normal distribution
    plt.hist(data, bins=30, density=True, alpha=0.7, color='blue', edgecolor='black')
    
    # Add a title and labels to the plot
    plt.title('Normal Distribution')
    plt.xlabel('Values')
    plt.ylabel('Frequency')
    
    # Show the plot
    plt.show()
    pass


  