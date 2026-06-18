/**
 * Custom Dialog
 * 
 * Creates custom Windows Forms dialogs for complex user interactions
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
            if (api.CurrentPlate.PlateNumber == 1)
            {
	            Form dialog = new Form() { Text = "Startup Dialog" }; // Create a new winforms dialog
	            FlowLayoutPanel mainPanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.TopDown, AutoSize = true }; // Create a flow layout panel, all of the controls will be a child of this control
	
	
	            // Get user input for End Point Reagent Volume
	            FlowLayoutPanel EPRVolumePanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
	            EPRVolumePanel.Controls.Add(new Label() { Text = string.Format("End Point Reagent Volume{0}(uL per reaction)", Environment.NewLine), AutoSize = true });
	            TextBox txtEPRVolume = new TextBox() { Name = "txtEPRVolume" };
	            EPRVolumePanel.Controls.Add(txtEPRVolume);
	            mainPanel.Controls.Add(EPRVolumePanel);
	
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
					
					// Add to Run Data
					api.Data.PerRunData.Add("EPR Volume", txtEPRVolume.Text);
					
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
}