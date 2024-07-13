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

using cloud.charging.open.protocols.OCPP;
using cloud.charging.open.protocols.OCPP.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.NN;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;
using cloud.charging.open.protocols.OCPPv1_6.WebSockets;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// Overlay Network Tests.
    /// </summary>
    [TestFixture]
    public class OverlayNetworkTests : AOverlayNetwork
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


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation1_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation1_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, bootNotificationRequest) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, requestMessage, sendMessageResult) => {
                chargingStation1_jsonRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the BootNotification request

            var ocppLocalController_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, bootNotificationRequest) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BootNotification request

            var ocppGateway_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppGateway_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestFiltered   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, bootNotificationRequest) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                csms_jsonRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, responseMessage, sendMessageResult) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                chargingStation1_jsonMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                chargingStation1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion




            var bootNotificationResponse = await chargingStation1.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     DestinationNodeId:   null, // default: "CSMS"
                                                     NetworkPath:         null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      null,
                                                     EventTrackingId:     null

                                                 );

            Assert.That(bootNotificationResponse.Status,                                                     Is.EqualTo(RegistrationStatus.Accepted));
            Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,   Is.True);
            Assert.That(bootNotificationResponse.Interval                    > TimeSpan.Zero,                Is.True);
            //StatusInfo


            // -<request>--------------------------------------------------------------------------------------------------
            Assert.That(chargingStation1_BootNotificationRequestsSent.                                Count, Is.EqualTo(1));
            Assert.That(chargingStation1_jsonRequestMessageSent.                                      Count, Is.EqualTo(1));
            Assert.That(chargingStation1_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));

            Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id ]).ToString()));

            Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count, Is.EqualTo(1));
            Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count, Is.EqualTo(1));
            Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count, Is.EqualTo(1));
            Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count, Is.EqualTo(1));
            Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count, Is.EqualTo(1));
            Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

            Assert.That(csms_jsonRequestMessageReceived.                                              Count, Is.EqualTo(1));
            Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
            Assert.That(csms_BootNotificationRequestsReceived.                                        Count, Is.EqualTo(1));

            // -<response>-------------------------------------------------------------------------------------------------
            Assert.That(csms_BootNotificationResponsesSent.                                           Count, Is.EqualTo(1));
            Assert.That(csms_jsonResponseMessagesSent.                                                Count, Is.EqualTo(1));
            Assert.That(csms_jsonResponseMessagesSent.                     First().DestinationId,            Is.EqualTo(chargingStation1.Id));
            Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

            Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count, Is.EqualTo(1));
            //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count, Is.EqualTo(1));
            //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count, Is.EqualTo(1));
            Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count, Is.EqualTo(1));
            Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id ]).ToString()));

            Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count, Is.EqualTo(1));
            //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count, Is.EqualTo(1));
            //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count, Is.EqualTo(1));
            Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count, Is.EqualTo(1));
            //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

            Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count, Is.EqualTo(1));
            Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count, Is.EqualTo(1));
            // Note: The charging stations use "normal" networking and thus have no valid networking information!
            Assert.That(chargingStation1_jsonMessageResponseReceived.      First().DestinationId,            Is.EqualTo(chargingStation1.Id));
            //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().DestinationId,            Is.EqualTo(chargingStation1.Id));
            Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
            //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

        }

        #endregion

        #region SendReset1()

        /// <summary>
        /// Send Reset test.
        /// </summary>
        [Test]
        public async Task SendReset1()
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


            var response1 = await csms.Reset(

                                      DestinationNodeId:   chargingStation1.Id,
                                      ResetType:           ResetType.OnIdle,
                                      CustomData:          null,

                                      NetworkPath:         null,

                                      SignKeys:            null,
                                      SignInfos:           null,
                                      Signatures:          null,

                                      RequestId:           null,
                                      RequestTimestamp:    null,
                                      RequestTimeout:      null,
                                      EventTrackingId:     null

                                  );


            Assert.That(response1.Status, Is.EqualTo(ResetStatus.Accepted));


        }

        #endregion


    }

}
