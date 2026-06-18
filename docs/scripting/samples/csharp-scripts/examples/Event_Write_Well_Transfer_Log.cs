/**
 * Event Write Well Transfer Log
 * 
 * Generates CSV reports of liquid transfer operations
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2020 **
// 
//    File:      EventScript_WriteWellTransferLog.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HRB.Cellario.Scripting.API;

namespace HRB.Cellario.Scripting.ScriptLibrary
{
    public class EventScript_WriteWellTransferLog : AbstractScript
    {
        /// <summary>
        /// Requires a Cherry Pick protocol and run order with transfers.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            var roId = api.CurrentRun.RunNumber.ToString();

            //note - replace hard coded paths!
            var path = "c:\\SystemData\\CellarioWellTransfers\\";
            //verify path exists
            if (Path.GetDirectoryName(path) != path ) return;

            var filename = "Transfers_Run_" + roId + ".csv";
            var outputFile = Path.Combine(path, filename);


            var xFers = api.CurrentRun.WellTransfers.ToList();
            if (xFers.Count > 0)
            {
                var csvData = new List<string>();
                foreach (var xFer in xFers)
                {
                    var sId = xFer.SourceOrderSampleId.ToString();
                    var sRow = xFer.SourceWellRow.ToString();
                    var sCol = xFer.SourceWellCol.ToString();
                    var dId = xFer.DestOrderSampleId.ToString();
                    var dRow = xFer.DestWellRow.ToString();
                    var dCol = xFer.DestWellCol.ToString();
                    var vol = xFer.Volume.ToString();
                    var item = sId + ", " + sRow + ", " + sCol + ", " + dId + ", " + dRow + ", " + dCol + ", " + vol;
                    csvData.Add(item);
                }


                File.WriteAllLines(outputFile, csvData);

                api.Messaging.Notify("Run End Event Script", "Well transfer count = " + xFers.Count());

            }
        }
    }
}
