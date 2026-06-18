/**
 * Ask User for Development Time
 * 
 * Presents interactive dialogs to collect user input during protocol setup
 */

#region Header
// ** Demo only **
// This script is for demonstration purposes only.
// It is the responsibility of the customer to modify to work with their protocols and applications.
#endregion

using System;
using System.Linq;
using System.Windows.Forms;
using HRB.Cellario.Scripting.API;
using System.IO;
using System.Text;

namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {

	            Form dialog = new Form() { Text = "Startup Dialog", AutoSize = true }; // Create a new winforms dialog
	            FlowLayoutPanel mainPanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.TopDown, AutoSize = true }; // Create a flow layout panel, all of the controls will be a child of this control
				dialog.StartPosition = FormStartPosition.CenterScreen;
	
	            // Get user input for End Point Reagent Volume
	            FlowLayoutPanel DEVTIMEPanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
	            DEVTIMEPanel.Controls.Add(new Label() { Text = string.Format("Development Time {0}(minutes)", Environment.NewLine), AutoSize = true });
	            TextBox txtDEVTIME = new TextBox() { Name = "txtDEVTIME" };
	            DEVTIMEPanel.Controls.Add(txtDEVTIME);
	            mainPanel.Controls.Add(DEVTIMEPanel);
	
	            // Add buttons
	            FlowLayoutPanel buttonPanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
	
	            // Create the cancel button
	            Button btnCancel = new Button() { Text = "Cancel", Name = "CancelButton" };
	            btnCancel.Click +=(s,ea)=> 
	            {
	                // User clicked the cancel button
	                dialog.Close();
	            };
	            buttonPanel.Controls.Add(btnCancel);
	
	            // Create the ok button
	            Button btnOk = new Button() { Text = "Ok", Name = "OkButton" };
	            btnOk.Click += (s, ea) =>
	            {
	                // User clicked the OK button
					
					// change Run Order Parameter Value
					IScriptingRunOrderParameter DT = api.CurrentRun.RunOrderParameters.Where(p => p.ParameterName == "Development Time").FirstOrDefault();
					int NewDevTimeSec = Convert.ToInt32(DT.ParameterValue);
					string DevTimeChanged = "";
					int NewDevTimeMin = NewDevTimeSec/60;
					
					if (txtDEVTIME.Text == "")
					{
						DevTimeChanged = "User did not enter a new Development Time value, so order default value will be used. ";
					}
					else
					{
						NewDevTimeSec = Convert.ToInt32(txtDEVTIME.Text)*60;
						NewDevTimeMin = Convert.ToInt32(txtDEVTIME.Text);
					}
					
					api.Messaging.Notify("Development Time Setting", string.Format(DevTimeChanged + "The development time for this run has been set to {0} minutes ({1} seconds).", NewDevTimeMin, NewDevTimeSec));
					DT.ParameterValue = NewDevTimeSec.ToString();
						
					// Add to Run Data
					//api.Data.PerRunData.Add("Development Time", txtDEVTIME.Text);
					
					// Add to a Text File
	                //StringBuilder sb = new StringBuilder();
	                //sb.AppendLine("End Point Reagent Volume (per reaction):" + txtEPRVolume.Text);
	                //File.WriteAllText(@"c:\temp\dialog.txt", sb.ToString());
	
	                dialog.Close();
	            };
	            buttonPanel.Controls.Add(btnOk);
	            mainPanel.Controls.Add(buttonPanel);
	
	
	            dialog.Controls.Add(mainPanel); // Add the main panel to the dialog
	            dialog.ShowDialog(); // show the dialog
            
        }
    }
}