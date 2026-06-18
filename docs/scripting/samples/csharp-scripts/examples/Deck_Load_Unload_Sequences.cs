/**
 * Deck Load Unload Sequences
 * 
 * Example script showing basic Cellario protocol automation and customization features
 */

#region Header
// ** Copyright HighRes Biosolutions, Inc. 2025 **
// 
//    File:      DeckLoadUnloadSequence.cs 
//    Project:   ScriptLibrary
//    Solution:  CellarioWPF
#endregion

using System;
using System.Linq;
using HRB.Cellario.Scripting.API;

// TODO - customize namespace and class name
namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        /// <summary>
        /// Sample Script that will find the next Liquid Transfer in Remaining Steps, look to modify the DeckLoadUnload sequences if found.
        /// Note: tested with a protocol with multiple Liquid Transfers that have DeckLoadUnload sequences defined.
        /// </summary>
        /// <param name="api">Scripting api</param>
        public override void Execute(IScriptingApi api)
        {
            if (api.CurrentPlate.PlateNumber > 1) return;

            var liquidTransfer = api.CurrentRun.Protocol.Threads.First().Steps.FirstOrDefault(s => s.StepName == "LiquidTransfer");
            if (liquidTransfer == null)
                return;

            // get the sequences for selected Liquid Transfer
            var sequenceList = api.CurrentRun.DeckLoadUnloadSequences.Where(a => a.StepId == liquidTransfer.StepId).ToList();

            //Modify sequence value for Deck Load/Unload in selected plate step
            foreach (var sequence in sequenceList)
            {
                var oldLoadSequence = sequence.LoadSequence;
                var oldUnloadSequence = sequence.UnloadSequence;

                sequence.LoadSequence = sequence.LoadSequence + 2;
                sequence.UnloadSequence = sequence.UnloadSequence + 2;

                if (api.IsExecutingInAnalysis) continue;

                var msg = $"Resource - {sequence.ResourceName}, LoadSequence changed from {oldLoadSequence} to {sequence.LoadSequence}, UnloadSequence changed from {oldUnloadSequence} to {sequence.UnloadSequence}";
                api.Messaging.WriteDiagnostic(ScriptLogLevel.Normal, msg);
            }
        }

    }
}
