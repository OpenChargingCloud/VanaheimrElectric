/*
 * Copyright (c) 2015-2024 GraphDefined GmbH
 * This file is part of WWCP Vanaheimr Electric <https://github.com/OpenChargingCloud/VanaheimrElectric>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using NUnit.Framework;

using org.GraphDefined.Vanaheimr.Illias;

using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.NN;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPP.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;
using cloud.charging.open.protocols.OCPP;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// OCPP v2.1 tests.
    /// </summary>
    [TestFixture]
    public class ChargingTests : AOCPPInfrastructure
    {

        #region SendBootNotification1()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendBootNotification1()
        {

            #region Initial checks

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {
                Assert.Fail("Failed precondition(s)!");
                return;
            }

            #endregion


            #region The OCPP Local Controller receives and forwards the BootNotification request. Same for the response!

            var ocppLocalController_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_jsonRequestMessageSent                       = new ConcurrentList<SendOCPPMessageResult>();

            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();

            ocppLocalController.OCPP.IN.     OnJSONMessageRequestReceived       += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };
            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };
            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestLogging   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, bootNotificationRequest) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnJSONRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.                     TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };



            ocppLocalController.OCPP.OUT.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };
            ocppLocalController.OCPP.IN. OnBootNotificationResponseReceived += (timestamp, sender,             request, response, runtime) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion

            #region The OCPP Gateway receives and forwards the BootNotification request. Same for the response!

            var ocppGateway_BootNotificationRequestsReceived                 = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_BootNotificationRequestsSent                     = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_BootNotificationResponsesReceived                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                    = new ConcurrentList<BootNotificationResponse>();

            ocppGateway.OCPP.IN. OnBootNotificationRequestReceived          += (timestamp, sender, connection, request)                    => { ocppGateway_BootNotificationRequestsReceived. TryAdd(request);  return Task.CompletedTask; };
            ocppGateway.OCPP.OUT.OnBootNotificationRequestSent              += (timestamp, sender,             request)                    => { ocppGateway_BootNotificationRequestsSent.     TryAdd(request);  return Task.CompletedTask; };
            ocppGateway.OCPP.OUT.OnBootNotificationResponseSent             += (timestamp, sender, connection, request, response, runtime) => { ocppGateway_BootNotificationResponsesReceived.TryAdd(response); return Task.CompletedTask; };
            ocppGateway.OCPP.IN. OnBootNotificationResponseReceived         += (timestamp, sender,             request, response, runtime) => { ocppGateway_BootNotificationResponsesReceived.TryAdd(response); return Task.CompletedTask; };

            #endregion

            #region The CSMS receives and responds the BootNotification request.

            var csms_BootNotificationRequestsReceived                        = new ConcurrentList<BootNotificationRequest>();
            var csms_BootNotificationResponsesSent                           = new ConcurrentList<BootNotificationResponse>();

            csms.               OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request)                    => { csms_BootNotificationRequestsReceived. TryAdd(request);  return Task.CompletedTask; };
            csms.               OCPP.OUT.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime) => { csms_BootNotificationResponsesSent.TryAdd(response); return Task.CompletedTask; };

            #endregion


            var response1 = await chargingStation1.SendBootNotification(

                                      BootReason:          BootReason.PowerUp,
                                      CustomData:          null,

                                      DestinationNodeId:   null,
                                      NetworkPath:         null,

                                      SignKeys:            null,
                                      SignInfos:           null,
                                      Signatures:          null,

                                      RequestId:           null,
                                      RequestTimestamp:    null,
                                      RequestTimeout:      null,
                                      EventTrackingId:     null

                                  );

        }

        private Task OcppLocalController_OnJSONMessageRequestReceived(DateTime Timestamp, protocols.OCPPv2_1.NetworkingNode.INetworkingNodeChannel Server, org.GraphDefined.Vanaheimr.Hermod.WebSocket.IWebSocketConnection Connection, protocols.OCPP.NetworkingNode_Id DestinationNodeId, protocols.OCPP.NetworkPath NetworkPath, EventTracking_Id EventTrackingId, DateTime RequestTimestamp, Newtonsoft.Json.Linq.JArray RequestMessage, CancellationToken CancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

}
