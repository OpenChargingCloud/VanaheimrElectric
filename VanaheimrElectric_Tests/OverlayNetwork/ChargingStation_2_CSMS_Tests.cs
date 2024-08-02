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

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using org.GraphDefined.Vanaheimr.Illias;

using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;
using cloud.charging.open.protocols.WWCP;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Overlay Network Tests
    /// 
    /// Charging Station 1  --[LC]--[GW]-->  CSMS 1
    /// Charging Station 2  --[LC]--[GW]-->  CSMS 1
    /// Charging Station 3  --[LC]--[GW]-->  CSMS 2
    /// </summary>
    [TestFixture]
    public class ChargingStation_2_CSMS_Tests : AOverlayNetwork
    {

        #region SendBootNotification1()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendBootNotification1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation1_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation1_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
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

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                csms1_jsonRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

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

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
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

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      null,
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval                    > TimeSpan.Zero,                  Is.True);
                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation1_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                Assert.That(chargingStation1_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendBootNotification2()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendBootNotification2()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation2    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation2_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation2_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation2.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                chargingStation2_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation2.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation2_jsonRequestMessageSent.      TryAdd(requestMessage);
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

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
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

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                csms1_jsonRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

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

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation2_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation2_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation2.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                chargingStation2_jsonMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation2.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                chargingStation2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await chargingStation2.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      null,
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval                    > TimeSpan.Zero,                  Is.True);
                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation2_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation2_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation2.Id ]).ToString()));
                Assert.That(chargingStation2_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(chargingStation2.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation2_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation2_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(chargingStation2.Id));
                //Assert.That(chargingStation2_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation2.Id));
                Assert.That(chargingStation2_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation2_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendBootNotification3()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendBootNotification3()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation3    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation3_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation3_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation3.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                chargingStation3_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation3.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation3_jsonRequestMessageSent.      TryAdd(requestMessage);
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

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BootNotification request

            var ocppGateway_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
         //   var ocppGateway_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
         //   var ocppGateway_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
         //   var ocppGateway_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_JSONRequestsForwardingDecisions              = new ConcurrentList<ForwardingDecision>();
            var ocppGateway_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            //ocppGateway.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
            //    ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
            //    return Task.CompletedTask;
            //};

            //ocppGateway.OCPP.FORWARD.OnBootNotificationRequestFiltered   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
            //    ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
            //    return Task.CompletedTask;
            //};

            //ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sendMessageResult) => {
            //    ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
            //    return Task.CompletedTask;
            //};

            ocppGateway.OCPP.FORWARD.OnAnyJSONRequestFiltered           += (timestamp, sender, connection, jsonRequest, forwardingDecision, ct) => {
                ocppGateway_JSONRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms2_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms2_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms2.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                csms2_jsonRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms2.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request) => {
                csms2_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms2_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms2_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms2.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms2_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms2.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms2_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

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

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation3_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation3_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation3.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                chargingStation3_jsonMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation3.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                chargingStation3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await chargingStation3.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      null,
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval                    > TimeSpan.Zero,                  Is.True);
                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation3_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation3_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation3.Id ]).ToString()));
                Assert.That(chargingStation3_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_JSONRequestsForwardingDecisions.                                  Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms2_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms2_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms2_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(chargingStation3.Id));
                Assert.That(csms2_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation3_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation3_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(chargingStation3.Id));
                //Assert.That(chargingStation3_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation3.Id));
                Assert.That(chargingStation3_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation3_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion


        #region SendDataTransfer1()

        /// <summary>
        /// Send a SendDataTransfer *implicitly* to the CSMS.
        /// </summary>
        [Test]
        public async Task SendDataTransfer1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var chargingStation1_DataTransferRequestsSent        = new ConcurrentList<DataTransferRequest>();
            var chargingStation1_BinaryRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sendMessageResult) => {
                chargingStation1_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation1_BinaryRequestMessageSent.        TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_BinaryRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_BinaryRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppLocalController_BinaryRequestMessageReceived.           TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sendMessageResult) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppLocalController_BinaryRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_binaryRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_binaryRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppGateway_binaryRequestMessageReceived.           TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sendMessageResult) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_binaryRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms1_BinaryRequestMessageReceived        = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_DataTransferRequestsReceived        = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived  += (timestamp, sender, requestMessage) => {
                csms1_BinaryRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnDataTransferRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms1_DataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the DataTransfer request

            var csms1_DataTransferResponsesSent        = new ConcurrentList<DataTransferResponse>();
            var csms1_BinaryResponseMessagesSent       = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_BinaryResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the DataTransfer response

            var ocppGateway_binaryResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_DataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppLocalController_BinaryResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_BinaryResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppLocalController_BinaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the DataTransfer response

            var chargingStation1_BinaryMessageResponseReceived       = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                chargingStation1_BinaryMessageResponseReceived.      TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await chargingStation1.TransferData(

                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                "TestData",
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse2  = await chargingStation1.TransferData(

                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                JSONObject.Create(new JProperty("test", "data")),
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse3  = await chargingStation1.TransferData(

                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                new JArray("test", "data"),
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );


            Assert.Multiple(() => {

                Assert.That(dataTransferResponse1.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse1.Data?.Type,                                          Is.EqualTo(JTokenType.String));
                Assert.That(dataTransferResponse1.Data?.ToString(),                                    Is.EqualTo("ataDtseT"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse2.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse2.Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"atad\"}"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(1).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse3.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse3.Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"tset\",\"atad\"]"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(2).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion

        #region SendDataTransfer2()

        /// <summary>
        /// Send a SendDataTransfer *explicitly* to the CSMS.
        /// </summary>
        [Test]
        public async Task SendDataTransfer2()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var chargingStation1_DataTransferRequestsSent        = new ConcurrentList<DataTransferRequest>();
            var chargingStation1_BinaryRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sendMessageResult) => {
                chargingStation1_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation1_BinaryRequestMessageSent.        TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_BinaryRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_BinaryRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppLocalController_BinaryRequestMessageReceived.           TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, connection, sender, binaryDataTransferRequest, sendMessageResult) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, connection, sender, requestMessage, sendMessageResult) => {
                ocppLocalController_BinaryRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_binaryRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_binaryRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppGateway_binaryRequestMessageReceived.           TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sendMessageResult) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_binaryRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms1_BinaryRequestMessageReceived        = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_DataTransferRequestsReceived        = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived  += (timestamp, sender, requestMessage) => {
                csms1_BinaryRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnDataTransferRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms1_DataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the DataTransfer request

            var csms1_DataTransferResponsesSent        = new ConcurrentList<DataTransferResponse>();
            var csms1_BinaryResponseMessagesSent       = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_BinaryResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the DataTransfer response

            var ocppGateway_binaryResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_DataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppLocalController_BinaryResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_BinaryResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppLocalController_BinaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the DataTransfer response

            var chargingStation1_BinaryMessageResponseReceived       = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                chargingStation1_BinaryMessageResponseReceived.      TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await chargingStation1.TransferData(

                                                   DestinationId:       NetworkingNode_Id.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                "TestData",
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse2  = await chargingStation1.TransferData(

                                                   DestinationId:       NetworkingNode_Id.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                JSONObject.Create(new JProperty("test", "data")),
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse3  = await chargingStation1.TransferData(

                                                   DestinationId:       NetworkingNode_Id.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                new JArray("test", "data"),
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );


            Assert.Multiple(() => {

                Assert.That(dataTransferResponse1.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse1.Data?.Type,                                          Is.EqualTo(JTokenType.String));
                Assert.That(dataTransferResponse1.Data?.ToString(),                                    Is.EqualTo("ataDtseT"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse2.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse2.Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"atad\"}"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(1).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse3.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse3.Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"tset\",\"atad\"]"));
                //StatusInfo
                Assert.That(chargingStation1_DataTransferRequestsSent.ElementAt(2).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion

        #region SendBinaryDataTransfer1()

        /// <summary>
        /// Send a SendBinaryDataTransfer *implicitly* to the CSMS.
        /// </summary>
        [Test]
        public async Task SendBinaryDataTransfer1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BinaryDataTransfer request leaves the Energy Meter

            var chargingStation1_BinaryDataTransferRequestsSent        = new ConcurrentList<BinaryDataTransferRequest>();
            var chargingStation1_BinaryRequestMessageSent              = new ConcurrentList<OCPP_BinaryRequestMessage>();

            chargingStation1.OCPP.OUT.OnBinaryDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sendMessageResult) => {
                chargingStation1_BinaryDataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnBinaryRequestMessageSent      += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation1_BinaryRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the BinaryDataTransfer request

            var ocppLocalController_BinaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppLocalController_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppLocalController_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppLocalController.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                ocppLocalController_BinaryRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppLocalController_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sendMessageResult) => {
                ocppLocalController_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppLocalController_BinaryRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BinaryDataTransfer request

            var ocppGateway_binaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppGateway_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppGateway_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_binaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppGateway.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, requestMessage) => {
                ocppGateway_binaryRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppGateway_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sendMessageResult) => {
                ocppGateway_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_binaryRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BinaryDataTransfer request

            var csms1_BinaryRequestMessageReceived              = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var csms1_BinaryDataTransferRequestsReceived        = new ConcurrentList<BinaryDataTransferRequest>();

            csms1.OCPP.IN. OnBinaryRequestMessageReceived      += (timestamp, sender, requestMessage) => {
                csms1_BinaryRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBinaryDataTransferRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms1_BinaryDataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BinaryDataTransfer request

            var csms1_BinaryDataTransferResponsesSent        = new ConcurrentList<BinaryDataTransferResponse>();
            var csms1_BinaryResponseMessagesSent             = new ConcurrentList<OCPP_BinaryResponseMessage>();

            csms1.OCPP.OUT.OnBinaryDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_BinaryDataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnBinaryResponseMessageSent      += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_BinaryResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BinaryDataTransfer response

            var ocppGateway_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppGateway_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppGateway.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_BinaryDataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BinaryDataTransfer response

            var ocppLocalController_BinaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppLocalController_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppLocalController.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppLocalController_BinaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the BinaryDataTransfer response

            var chargingStation1_BinaryMessageResponseReceived             = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var chargingStation1_BinaryDataTransferResponsesReceived       = new ConcurrentList<BinaryDataTransferResponse>();

            chargingStation1.OCPP.IN.OnBinaryResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                chargingStation1_BinaryMessageResponseReceived.      TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var binaryDataTransferResponse  = await chargingStation1.TransferBinaryData(

                                                        VendorId:           Vendor_Id. GraphDefined,
                                                        MessageId:          Message_Id.GraphDefined_TestMessage,
                                                        Data:               "TestData".ToUTF8Bytes(),

                                                        SignKeys:           null,
                                                        SignInfos:          null,
                                                        Signatures:         null,

                                                        RequestId:          null,
                                                        RequestTimestamp:   null,
                                                        RequestTimeout:     null,
                                                        EventTrackingId:    null

                                                    );


            Assert.Multiple(() => {

                Assert.That(binaryDataTransferResponse.Status,                                             Is.EqualTo(BinaryDataTransferStatus.Accepted));
                Assert.That(binaryDataTransferResponse.Data?.ToUTF8String(),                               Is.EqualTo("ataDtseT"));
                //StatusInfo
                //Assert.That(chargingStation1_BinaryDataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion


        #region Authorize1()

        /// <summary>
        /// Authorize test.
        /// </summary>
        [Test]
        public async Task Authorize1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion



            #region 1. The Authorize request leaves the Charging Station

            var chargingStation1_AuthorizeRequestsSent          = new ConcurrentList<AuthorizeRequest>();
            var chargingStation1_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnAuthorizeRequestSent   += (timestamp, sender, connection, authorizeRequest, sendMessageResult) => {
                chargingStation1_AuthorizeRequestsSent. TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation1_jsonRequestMessageSent.TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the Authorize request

            var ocppLocalController_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_AuthorizeRequestsReceived              = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_AuthorizeRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppLocalController_AuthorizeRequestsSent                  = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.          TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeRequestReceived   += (timestamp, sender, connection, authorizeRequest) => {
                ocppLocalController_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeRequestFiltered   += (timestamp, sender, connection, authorizeRequest, forwardingDecision) => {
                ocppLocalController_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeRequestSent       += (timestamp, sender, connection, authorizeRequest, sendMessageResult) => {
                ocppLocalController_AuthorizeRequestsSent.               TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.              TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the Authorize request

            var ocppGateway_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_AuthorizeRequestsReceived              = new ConcurrentList<AuthorizeRequest>();
            var ocppGateway_AuthorizeRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppGateway_AuthorizeRequestsSent                  = new ConcurrentList<AuthorizeRequest>();
            var ocppGateway_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                ocppGateway_jsonRequestMessageReceived.          TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnAuthorizeRequestReceived   += (timestamp, sender, connection, authorizeRequest) => {
                ocppGateway_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnAuthorizeRequestFiltered   += (timestamp, sender, connection, authorizeRequest, forwardingDecision) => {
                ocppGateway_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnAuthorizeRequestSent       += (timestamp, sender, connection, authorizeRequest, sendMessageResult) => {
                ocppGateway_AuthorizeRequestsSent.               TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.              TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the Authorize request

            var csms1_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_AuthorizeRequestsReceived          = new ConcurrentList<AuthorizeRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                csms1_jsonRequestMessageReceived.TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnAuthorizeRequestReceived   += (timestamp, sender, connection, request) => {
                csms1_AuthorizeRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the Authorize request

            var csms1_AuthorizeResponsesSent          = new ConcurrentList<AuthorizeResponse>();
            var csms1_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnAuthorizeResponseSent   += (timestamp, sender, connection, request, response, runtime) => {
                csms1_AuthorizeResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_jsonResponseMessagesSent.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the Authorize response

            var ocppGateway_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_AuthorizeResponsesReceived              = new ConcurrentList<AuthorizeResponse>();
            var ocppGateway_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppGateway_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                ocppGateway_jsonResponseMessagesReceived.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_AuthorizeResponsesSent.      TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessagesSent.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the Authorize response

            var ocppLocalController_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_AuthorizeResponsesReceived              = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                ocppLocalController_jsonResponseMessagesReceived.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the Authorize response

            var chargingStation1_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_AuthorizeResponsesReceived         = new ConcurrentList<AuthorizeResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                chargingStation1_jsonMessageResponseReceived.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime) => {
                chargingStation1_AuthorizeResponsesReceived. TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var authorizeResponse = await chargingStation1.Authorize(

                                              IdToken:                       IdToken.TryParseRFID(RFIDUID1)!,
                                              Certificate:                   null,
                                              ISO15118CertificateHashData:   null,
                                              CustomData:                    null,

                                              SignKeys:                      null,
                                              SignInfos:                     null,
                                              Signatures:                    null,

                                              RequestId:                     null,
                                              RequestTimestamp:              null,
                                              RequestTimeout:                null,
                                              EventTrackingId:               null

                                          );

            Assert.Multiple(() => {

                Assert.That(authorizeResponse.IdTokenInfo.Status,                                           Is.EqualTo(AuthorizationStatus.Accepted));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation1_AuthorizeRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                Assert.That(chargingStation1_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms1_AuthorizeRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_AuthorizeResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_AuthorizeResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_AuthorizeResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_AuthorizeResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_AuthorizeResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendMeterValues1()

        /// <summary>
        /// Send MeterValues test.
        /// </summary>
        [Test]
        public async Task SendMeterValues1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The MeterValues request leaves the Charging Station

            var chargingStation1_MeterValuesRequestsSent        = new ConcurrentList<MeterValuesRequest>();
            var chargingStation1_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnMeterValuesRequestSent += (timestamp, sender, connection, meterValuesRequest, sendMessageResult) => {
                chargingStation1_MeterValuesRequestsSent.TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                chargingStation1_jsonRequestMessageSent. TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the MeterValues request

            var ocppLocalController_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_MeterValuesRequestsReceived            = new ConcurrentList<MeterValuesRequest>();
            var ocppLocalController_MeterValuesRequestsForwardingDecisions = new ConcurrentList<ForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
            var ocppLocalController_MeterValuesRequestsSent                = new ConcurrentList<MeterValuesRequest>();
            var ocppLocalController_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.            TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnMeterValuesRequestReceived += (timestamp, sender, connection, meterValuesRequest) => {
                ocppLocalController_MeterValuesRequestsReceived.           TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnMeterValuesRequestFiltered += (timestamp, sender, connection, meterValuesRequest, forwardingDecision) => {
                ocppLocalController_MeterValuesRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnMeterValuesRequestSent     += (timestamp, sender, connection, meterValuesRequest, sendMessageResult) => {
                ocppLocalController_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.                TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the MeterValues request

            var ocppGateway_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_MeterValuesRequestsReceived            = new ConcurrentList<MeterValuesRequest>();
            var ocppGateway_MeterValuesRequestsForwardingDecisions = new ConcurrentList<ForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
            var ocppGateway_MeterValuesRequestsSent                = new ConcurrentList<MeterValuesRequest>();
            var ocppGateway_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                ocppGateway_jsonRequestMessageReceived.            TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestReceived += (timestamp, sender, connection, meterValuesRequest) => {
                ocppGateway_MeterValuesRequestsReceived.           TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestFiltered += (timestamp, sender, connection, meterValuesRequest, forwardingDecision) => {
                ocppGateway_MeterValuesRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestSent     += (timestamp, sender, connection, meterValuesRequest, sendMessageResult) => {
                ocppGateway_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the MeterValues request

            var csms1_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_MeterValuesRequestsReceived        = new ConcurrentList<MeterValuesRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                csms1_jsonRequestMessageReceived.TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnMeterValuesRequestReceived += (timestamp, sender, connection, request) => {
                csms1_MeterValuesRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the MeterValues request

            var csms1_MeterValuesResponsesSent        = new ConcurrentList<MeterValuesResponse>();
            var csms1_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnMeterValuesResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms1_MeterValuesResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                csms1_jsonResponseMessagesSent.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the MeterValues response

            var ocppGateway_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_MeterValuesResponsesReceived            = new ConcurrentList<MeterValuesResponse>();
            var ocppGateway_MeterValuesResponsesSent                = new ConcurrentList<MeterValuesResponse>();
            var ocppGateway_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                ocppGateway_jsonResponseMessagesReceived.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_MeterValuesResponsesSent.      TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessagesSent.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the MeterValues response

            var ocppLocalController_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_MeterValuesResponsesReceived            = new ConcurrentList<MeterValuesResponse>();
            var ocppLocalController_MeterValuesResponsesSent                = new ConcurrentList<MeterValuesResponse>();
            var ocppLocalController_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                ocppLocalController_jsonResponseMessagesReceived.TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnMeterValuesResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnMeterValuesResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppLocalController_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the MeterValues response

            var chargingStation1_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_MeterValuesResponsesReceived       = new ConcurrentList<MeterValuesResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                chargingStation1_jsonMessageResponseReceived. TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnMeterValuesResponseReceived += (timestamp, sender, request, response, runtime) => {
                chargingStation1_MeterValuesResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var meterValuesResponse = await chargingStation1.SendMeterValues(

                                              EVSEId:             protocols.OCPPv2_1.EVSE_Id.Parse(0),
                                              MeterValues:        [
                                                                      new MeterValue(
                                                                          Timestamp:       Timestamp.Now,
                                                                          SampledValues:   [
                                                                                               new SampledValue(
                                                                                                   Value:                 13.3M,
                                                                                                   Context:               ReadingContext.TransactionBegin,
                                                                                                   Measurand:             Measurand.Energy_Active_Export_Register,
                                                                                                   //Phase:                 3,
                                                                                                   MeasurementLocation:   MeasurementLocation.Outlet,
                                                                                                   SignedMeterValue:      null,
                                                                                                   UnitOfMeasure:         UnitsOfMeasure.kWh(),
                                                                                                   CustomData:            null
                                                                                               )
                                                                                           ],
                                                                          CustomData:      null
                                                                      )
                                                                  ],
                                              CustomData:         null,

                                              SignKeys:           null,
                                              SignInfos:          null,
                                              Signatures:         null,

                                              RequestId:          null,
                                              RequestTimestamp:   null,
                                              RequestTimeout:     null,
                                              EventTrackingId:    null

                                          );

            Assert.Multiple(() => {

                Assert.That(meterValuesResponse.Result.ResultCode,                                            Is.EqualTo(ResultCode.OK));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation1_MeterValuesRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_MeterValuesRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                Assert.That(chargingStation1_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms1_MeterValuesRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_MeterValuesResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_MeterValuesResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_MeterValuesResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_MeterValuesResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_MeterValuesResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion



        #region SendRemoteStart_viaEMP_Test1()

        /// <summary>
        /// Send SendRemoteStart via EMP test 1
        /// </summary>
        [Test]
        public async Task SendRemoteStart_viaEMP_Test1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms1_cso           is null ||
                csms1_emp           is null ||
                csms1_remoteEMP     is null ||
                csms2               is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                e1                  is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms1_cso           is null)
                        Assert.Fail("The csms CSO must not be null!");

                    if (csms1_emp           is null)
                        Assert.Fail("The csms EMP must not be null!");

                    if (csms1_remoteEMP     is null)
                        Assert.Fail("The csms remote EMP must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (e1                  is null)
                        Assert.Fail("The EVSE 1 of charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var csms1_remoteEMP_OnRemoteStartRequest = 0;

            csms1_remoteEMP.OnRemoteStartRequest += (logTimestamp,
                                                     requestTimestamp,
                                                     sender,
                                                     eventTrackingId,
                                                     roamingNetworkId,
                                                     chargingLocation,
                                                     remoteAuthentication,
                                                     sessionId,
                                                     reservationId,
                                                     chargingProduct,
                                                     empRoamingProviderId,
                                                     csoRoamingProviderId,
                                                     providerId,
                                                     requestTimeout) => {
                csms1_remoteEMP_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };

            var roamingNetwork_OnRemoteStartRequest = 0;

            csms1_remoteEMP.RoamingNetwork.OnRemoteStartRequest += (logTimestamp,
                                                                    requestTimestamp,
                                                                    sender,
                                                                    eventTrackingId,
                                                                    roamingNetworkId,
                                                                    chargingLocation,
                                                                    remoteAuthentication,
                                                                    sessionId,
                                                                    reservationId,
                                                                    chargingProduct,
                                                                    empRoamingProviderId,
                                                                    csoRoamingProviderId,
                                                                    providerId,
                                                                    requestTimeout) => {
                roamingNetwork_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };

            var csms1_cso_OnRemoteStartRequest = 0;

            csms1_cso.OnRemoteStartRequest += (logTimestamp,
                                               requestTimestamp,
                                               sender,
                                               eventTrackingId,
                                               roamingNetworkId,
                                               chargingLocation,
                                               remoteAuthentication,
                                               sessionId,
                                               reservationId,
                                               chargingProduct,
                                               empRoamingProviderId,
                                               csoRoamingProviderId,
                                               providerId,
                                               requestTimeout) => {
                csms1_cso_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };


            var p1_OnRemoteStartRequest = 0;

            p1.OnRemoteStartRequest += (logTimestamp,
                                        requestTimestamp,
                                        sender,
                                        eventTrackingId,
                                        roamingNetworkId,
                                        chargingLocation,
                                        remoteAuthentication,
                                        sessionId,
                                        reservationId,
                                        chargingProduct,
                                        empRoamingProviderId,
                                        csoRoamingProviderId,
                                        providerId,
                                        requestTimeout) => {
                p1_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };


            var s1_OnRemoteStartRequest = 0;

            s1.OnRemoteStartRequest += (logTimestamp,
                                        requestTimestamp,
                                        sender,
                                        eventTrackingId,
                                        roamingNetworkId,
                                        chargingLocation,
                                        remoteAuthentication,
                                        sessionId,
                                        reservationId,
                                        chargingProduct,
                                        empRoamingProviderId,
                                        csoRoamingProviderId,
                                        providerId,
                                        requestTimeout) => {
                s1_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };


            var e1_OnRemoteStartRequest = 0;

            e1.OnRemoteStartRequest += (logTimestamp,
                                        requestTimestamp,
                                        sender,
                                        eventTrackingId,
                                        roamingNetworkId,
                                        chargingLocation,
                                        remoteAuthentication,
                                        sessionId,
                                        reservationId,
                                        chargingProduct,
                                        empRoamingProviderId,
                                        csoRoamingProviderId,
                                        providerId,
                                        requestTimeout) => {
                e1_OnRemoteStartRequest++;
                return Task.CompletedTask;
            };





















            var e1_OnRemoteStartResponse = 0;

            e1.OnRemoteStartResponse += (logTimestamp,
                                         requestTimestamp,
                                         sender,
                                         eventTrackingId,
                                         roamingNetworkId,
                                         chargingLocation,
                                         remoteAuthentication,
                                         sessionId,
                                         reservationId,
                                         chargingProduct,
                                         empRoamingProviderId,
                                         csoRoamingProviderId,
                                         providerId,
                                         requestTimeout,
                                         result,
                                         runtime) => {
                e1_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var s1_OnRemoteStartResponse = 0;

            s1.OnRemoteStartResponse += (logTimestamp,
                                         requestTimestamp,
                                         sender,
                                         eventTrackingId,
                                         roamingNetworkId,
                                         chargingLocation,
                                         remoteAuthentication,
                                         sessionId,
                                         reservationId,
                                         chargingProduct,
                                         empRoamingProviderId,
                                         csoRoamingProviderId,
                                         providerId,
                                         requestTimeout,
                                         result,
                                         runtime) => {
                s1_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var p1_OnRemoteStartResponse = 0;

            p1.OnRemoteStartResponse += (logTimestamp,
                                         requestTimestamp,
                                         sender,
                                         eventTrackingId,
                                         roamingNetworkId,
                                         chargingLocation,
                                         remoteAuthentication,
                                         sessionId,
                                         reservationId,
                                         chargingProduct,
                                         empRoamingProviderId,
                                         csoRoamingProviderId,
                                         providerId,
                                         requestTimeout,
                                         result,
                                         runtime) => {
                p1_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var csms1_cso_OnRemoteStartResponse = 0;

            csms1_cso.OnRemoteStartResponse += (logTimestamp,
                                                requestTimestamp,
                                                sender,
                                                eventTrackingId,
                                                roamingNetworkId,
                                                chargingLocation,
                                                remoteAuthentication,
                                                sessionId,
                                                reservationId,
                                                chargingProduct,
                                                empRoamingProviderId,
                                                csoRoamingProviderId,
                                                providerId,
                                                requestTimeout,
                                                result,
                                                runtime) => {
                csms1_cso_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var roamingNetwork_OnRemoteStartResponse = 0;

            csms1_remoteEMP.RoamingNetwork.OnRemoteStartResponse += (logTimestamp,
                                                                     requestTimestamp,
                                                                     sender,
                                                                     eventTrackingId,
                                                                     roamingNetworkId,
                                                                     chargingLocation,
                                                                     remoteAuthentication,
                                                                     sessionId,
                                                                     reservationId,
                                                                     chargingProduct,
                                                                     empRoamingProviderId,
                                                                     csoRoamingProviderId,
                                                                     providerId,
                                                                     requestTimeout,
                                                                     result,
                                                                     runtime) => {
                roamingNetwork_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var csms1_remoteEMP_OnRemoteStartResponse = 0;

            csms1_remoteEMP.OnRemoteStartResponse += (logTimestamp,
                                                      requestTimestamp,
                                                      sender,
                                                      eventTrackingId,
                                                      roamingNetworkId,
                                                      chargingLocation,
                                                      remoteAuthentication,
                                                      sessionId,
                                                      reservationId,
                                                      chargingProduct,
                                                      empRoamingProviderId,
                                                      csoRoamingProviderId,
                                                      providerId,
                                                      requestTimeout,
                                                      result,
                                                      runtime) => {
                csms1_remoteEMP_OnRemoteStartResponse++;
                return Task.CompletedTask;
            };


            var remoteStartResult = await csms1_remoteEMP.RemoteStart(

                                              ChargingLocation:         ChargingLocation.FromEVSEId(e1.Id),
                                              ChargingProduct:          null,
                                              ReservationId:            null,
                                              SessionId:                null,
                                              RemoteAuthentication:     RemoteAuthentication.FromRemoteIdentification(
                                                                            EMobilityAccount_Id.Parse("DE-GDF-C12345678-X")
                                                                        ),
                                              AdditionalSessionInfos:   null,
                                              AuthenticationPath:       null,

                                              RequestTimestamp:         null,
                                              EventTrackingId:          null,
                                              RequestTimeout:           null

                                          );

            Assert.Multiple(() => {

                Assert.That(remoteStartResult.Result,   Is.EqualTo(RemoteStartResultTypes.Success));

            });

        }

        #endregion


    }

}
