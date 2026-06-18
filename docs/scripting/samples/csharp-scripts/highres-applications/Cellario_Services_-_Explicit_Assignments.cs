/**
 * Cellario Services - Explicit Assignments
 * 
 * Creates new run orders programmatically using the Cellario Services API
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using HRB.Cellario.Scripting.API;
using HRB.Cellario.Services.Client;
using HRB.Cellario.Services.DTO;

namespace Customer.Scripting
{
    public class ProtocolLauncher : AbstractScript
    {
        /// <summary>
        /// Method called during Cellario protocol execution when a sample arrives at the scripting step.
        /// </summary>
        /// <remarks>Executes synchronously with the run scheduler. Device operation results not available
        /// until <see cref="ReleaseResources"/> method is called.</remarks>
        /// <param name="api">Access to properties and methods for interacting with the Cellario run-time scheduler, 
        /// samples, resources and devices operations.</param>
        public override void Execute(IScriptingApi api)
        {
            var ret = OrderCreator.CreateOrder("Screening - Hit Screen - Dispenser", 25);
            ret = OrderCreator.ScheduleOrder();
        }
    }

    public static class OrderCreator
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CellarioClient client = null;
        private static Order order = null;
        private static Dictionary<string, List<int>> resourcePositions = new Dictionary<string, List<int>>();

        /// <summary>
        /// Schedules the current order after another order or directly.
        /// </summary>
        /// <param name="startAfterOrderId">Order id to start after or 0 to start directly.</param>
        /// <returns>'true' if successful.</returns>
        public static bool ScheduleOrder(int startAfterOrderId = 0)
        {
            try
            {
                if (startAfterOrderId != 0)
                {
                    var schedule = new OrderSchedule();
                    schedule.ScheduledAfter = startAfterOrderId;
                    order.ScheduleDetail = schedule;
                    client.IssueOrderAction(order.Id, OrderAction.Schedule);
                }
                else
                {
                    client.IssueOrderAction(order.Id, OrderAction.Start);
                };

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an order.
        /// </summary>
        /// <param name="protocolName">Protocol name.</param>
        /// <param name="plateCount">Number of plates per thread to create.</param>
        /// <returns>'true' if successful.</returns>
        public static bool CreateOrder(string protocolName, int plateCount = 1)
        {
            try
            {
                // connect to Cellario
                Connect();

                // lookup protocol
                var protocol = client.GetProtocols().FirstOrDefault(p => p.Name.ToLower() == protocolName.ToLower());
                var protocolId = protocol.Id;
                protocol = client.GetProtocol(protocolId, true);
                if (protocol == null)
                {
                    return false;
                }

                // set description
                var description = "Order created at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ", PlateCount: " + plateCount;

                // create order
                order = client.CreateOrder(protocolId, user: "ScriptOrder", description: description);

                // get labware types
                var labwareTypes = client.GetLabwareTypes().Select(p => p.Name).ToList();
                var defaultType = labwareTypes.FirstOrDefault(p => p.Contains("Generic 96"));

                // create plates (OrderSample)
                resourcePositions = new Dictionary<string, List<int>>();
                foreach (var thread in protocol.PlateProtocols)
                {
                    var inputName = thread.InputResources.FirstOrDefault();
                    var outputName = thread.OutputResources.FirstOrDefault();
                    for (var index = 0; index < plateCount; index++)
                    {
                        var inputResId = GetResourcePosition(inputName);
                        var outputResId = GetResourcePosition(outputName);
                        var newPlate = new SinglePlate()
                        {
                            Barcode = null,
                            FindAvailavlePosition = false,
                            LabwareType = defaultType,
                            ResourcePositionId = inputResId,
                            OutputResourcePositionId = outputResId,
                            PlateProtocolId = thread.Id,
                        };
                        var orderSample = client.CreateOrderSample(order.Id, newPlate);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the next available resource position and occupies it.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>Resource id or null if not succeeded.</returns>
        private static int? GetResourcePosition(string resourceName)
        {
            int? pos = null;
            if (!string.IsNullOrEmpty(resourceName))
            {
                // create resource dictionary if not available yet
                List<int> positions;
                if (!resourcePositions.TryGetValue(resourceName, out positions))
                {
                    var resource = client.GetResource(resourceName, true);
                    positions = resource.ResourcePositions.Select(p => p.Id).ToList();
                    resourcePositions[resourceName] = positions;
                }

                // if a position is available, get and occupy it
                if (positions != null && positions.Count > 0)
                {
                    pos = positions.First();
                    positions.RemoveAt(0);
                }
            }

            return pos;
        }

        /// <summary>
        /// Connect to Cellario.
        /// </summary>
        /// <returns>'true' if successful.</returns>
        public static bool Connect()
        {
            try
            {
                if (client == null)
                {
                    // get local host ip address
                    var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

                    // connect
                    client = new CellarioClient(ipAddress, 8444, true);
                    var info = client.GetSystemInfo();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}