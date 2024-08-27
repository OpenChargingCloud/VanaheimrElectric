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
using cloud.charging.open.protocols.WWCP.NetworkingNode;
using cloud.charging.open.protocols.WWCP.WebSockets;

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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation2    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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

            chargingStation2.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation2_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation2.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFiltered   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation2_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation2_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation2.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation2_jsonMessageResponseReceived.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation2.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation2.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,              Is.EqualTo(chargingStation2.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation2_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation2_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation2_jsonMessageResponseReceived.      First().Destination.Next,              Is.EqualTo(chargingStation2.Id));
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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation3    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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

            chargingStation3.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation3_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation3.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
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

            //ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult) => {
            //    ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
            //    return Task.CompletedTask;
            //};

            ocppGateway1.OCPP.FORWARD.OnAnyJSONRequestFiltered           += (timestamp, sender, connection, jsonRequest, forwardingDecision, ct) => {
                ocppGateway_JSONRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms2_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms2_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms2.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms2_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms2.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms2_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms2_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms2_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms2.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms2_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms2.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms2_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation3_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation3_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation3.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation3_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation3.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_JSONRequestsForwardingDecisions.                                  Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms2_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation3.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms2_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms2_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms2_jsonResponseMessagesSent.                     First().Destination.Next,              Is.EqualTo(chargingStation3.Id));
                Assert.That(csms2_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms2.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation3_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation3_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation3_jsonMessageResponseReceived.      First().Destination.Next,              Is.EqualTo(chargingStation3.Id));
                //Assert.That(chargingStation3_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation3.Id));
                Assert.That(chargingStation3_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation3_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion


        #region SendBinaryBootNotification1()

        /// <summary>
        /// Send binary BootNotification.
        /// </summary>
        [Test]
        public async Task SendBinaryBootNotification1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await chargingStation1.SendBootNotification(

                                                     BootReason:           BootReason.PowerUp,
                                                     CustomData:           null,

                                                     SignKeys:             null,
                                                     SignInfos:            null,
                                                     Signatures:           null,

                                                     RequestId:            null,
                                                     RequestTimestamp:     null,
                                                     RequestTimeout:       null,
                                                     EventTrackingId:      null,
                                                     SerializationFormat:  SerializationFormats.JSON_UTF8_Binary

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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendMixedBootNotification1()

        /// <summary>
        /// Send a binary BootNotification that will be translated to a
        /// JSON BootNotification within the Local Controller and vice versa.
        /// </summary>
        [Test]
        public async Task SendMixedBootNotification1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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


            //ToDo: Implement me!

            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation1_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation1_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
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

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppGateway_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms1_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms1_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await chargingStation1.SendBootNotification(

                                                     BootReason:           BootReason.PowerUp,
                                                     CustomData:           null,

                                                     SignKeys:             null,
                                                     SignInfos:            null,
                                                     Signatures:           null,

                                                     RequestId:            null,
                                                     RequestTimestamp:     null,
                                                     RequestTimeout:       null,
                                                     EventTrackingId:      null,
                                                     SerializationFormat:  SerializationFormats.JSON_UTF8_Binary

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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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
            var chargingStation1_jsonRequestMessageSent          = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                chargingStation1_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent.  TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.           TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, jsonDataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, jsonDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, jsonDataTransferRequest, sentMessageResult, ct) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.           TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, jsonDataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, jsonDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, jsonDataTransferRequest, sentMessageResult, ct) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms1_jsonRequestMessageReceived          = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_DataTransferRequestsReceived        = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived  += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
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
            var csms1_jsonResponseMessagesSent       = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the DataTransfer response

            var ocppGateway_jsonResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_jsonResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_DataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppLocalController_jsonResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_jsonResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the DataTransfer response

            var chargingStation1_jsonMessageResponseReceived       = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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
            var chargingStation1_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                chargingStation1_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent.        TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.           TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, jsonDataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, jsonDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, connection, sender, jsonDataTransferRequest, sentMessageResult, ct) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, connection, sender, requestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.               TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.           TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, jsonDataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, jsonDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, jsonDataTransferRequest, sentMessageResult, ct) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(jsonDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.               TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms1_jsonRequestMessageReceived        = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_DataTransferRequestsReceived        = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived  += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
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
            var csms1_jsonResponseMessagesSent       = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the DataTransfer response

            var ocppGateway_jsonResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_jsonResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_DataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppLocalController_jsonResponseMessagesReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_jsonResponseMessagesSent               = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the DataTransfer response

            var chargingStation1_jsonMessageResponseReceived       = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await chargingStation1.TransferData(

                                                   Destination:         SourceRouting.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                "TestData",

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse2  = await chargingStation1.TransferData(

                                                   Destination:         SourceRouting.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                JSONObject.Create(new JProperty("test", "data")),

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse3  = await chargingStation1.TransferData(

                                                   Destination:         SourceRouting.CSMS,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                new JArray("test", "data"),

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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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


            #region 1. The BinaryDataTransfer request leaves the Charging Station

            var chargingStation1_BinaryDataTransferRequestsSent        = new ConcurrentList<BinaryDataTransferRequest>();
            var chargingStation1_BinaryRequestMessageSent              = new ConcurrentList<OCPP_BinaryRequestMessage>();

            chargingStation1.OCPP.OUT.OnBinaryDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                chargingStation1_BinaryDataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnBinaryRequestMessageSent      += (timestamp, sender, connection, binaryRequestMessage, sentMessageResult, ct) => {
                chargingStation1_BinaryRequestMessageSent.      TryAdd(binaryRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the BinaryDataTransfer request

            var ocppLocalController_BinaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppLocalController_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppLocalController_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                ocppLocalController_BinaryRequestMessageReceived.                 TryAdd(binaryRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppLocalController_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sentMessageResult, ct) => {
                ocppLocalController_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, binaryRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_BinaryRequestMessageSent.                     TryAdd(binaryRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BinaryDataTransfer request

            var ocppGateway_binaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppGateway_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppGateway_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_binaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                ocppGateway_binaryRequestMessageReceived.                 TryAdd(binaryRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest, ct) => {
                ocppGateway_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, connection, binaryDataTransferRequest, sentMessageResult, ct) => {
                ocppGateway_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, binaryRequestMessage, sentMessageResult, ct) => {
                ocppGateway_binaryRequestMessageSent.                     TryAdd(binaryRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BinaryDataTransfer request

            var csms1_BinaryRequestMessageReceived              = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var csms1_BinaryDataTransferRequestsReceived        = new ConcurrentList<BinaryDataTransferRequest>();

            csms1.OCPP.IN. OnBinaryRequestMessageReceived      += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                csms1_BinaryRequestMessageReceived.      TryAdd(binaryRequestMessage);
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

            csms1.OCPP.OUT.OnBinaryDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BinaryDataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnBinaryResponseMessageSent      += (timestamp, sender, connection, binaryResponseMessage, sentMessageResult, ct) => {
                csms1_BinaryResponseMessagesSent.     TryAdd(binaryResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BinaryDataTransfer response

            var ocppGateway_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppGateway_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(binaryResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBinaryDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_BinaryDataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, binaryResponseMessage, sentMessageResult, ct) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(binaryResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BinaryDataTransfer response

            var ocppLocalController_BinaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppLocalController_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                ocppLocalController_BinaryResponseMessagesReceived.     TryAdd(binaryResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBinaryDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, binaryResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(binaryResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BinaryDataTransfer response

            var chargingStation1_BinaryMessageResponseReceived             = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var chargingStation1_BinaryDataTransferResponsesReceived       = new ConcurrentList<BinaryDataTransferResponse>();

            chargingStation1.OCPP.IN.OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                chargingStation1_BinaryMessageResponseReceived.      TryAdd(binaryResponseMessage);
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

                Assert.That(binaryDataTransferResponse.Status,                                          Is.EqualTo(BinaryDataTransferStatus.Accepted));
                Assert.That(binaryDataTransferResponse.Data?.ToUTF8String(),                            Is.EqualTo("ataDtseT"));
                //StatusInfo
                //Assert.That(chargingStation1_BinaryDataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

                Assert.That(chargingStation1_BinaryDataTransferRequestsSent.Count,                      Is.EqualTo(1));
                Assert.That(chargingStation1_BinaryRequestMessageSent.Count,                            Is.EqualTo(1));

                Assert.That(ocppLocalController_BinaryRequestMessageReceived.Count,                     Is.EqualTo(1));
                Assert.That(ocppLocalController_BinaryDataTransferRequestsReceived.Count,               Is.EqualTo(1));
                Assert.That(ocppLocalController_BinaryDataTransferRequestsForwardingDecisions.Count,    Is.EqualTo(1));
                Assert.That(ocppLocalController_BinaryDataTransferRequestsSent.Count,                   Is.EqualTo(1));
                Assert.That(ocppLocalController_BinaryRequestMessageSent.Count,                         Is.EqualTo(1));

                Assert.That(ocppGateway_binaryRequestMessageReceived.Count,                             Is.EqualTo(1));
                Assert.That(ocppGateway_BinaryDataTransferRequestsReceived.Count,                       Is.EqualTo(1));
                Assert.That(ocppGateway_BinaryDataTransferRequestsForwardingDecisions.Count,            Is.EqualTo(1));
                Assert.That(ocppGateway_BinaryDataTransferRequestsSent.Count,                           Is.EqualTo(1));
                Assert.That(ocppGateway_binaryRequestMessageSent.Count,                                 Is.EqualTo(1));

                Assert.That(csms1_BinaryRequestMessageReceived.Count,                                   Is.EqualTo(1));
                Assert.That(csms1_BinaryDataTransferRequestsReceived.Count,                             Is.EqualTo(1));


                Assert.That(csms1_BinaryDataTransferResponsesSent.Count,                                Is.EqualTo(1));
                Assert.That(csms1_BinaryResponseMessagesSent.Count,                                     Is.EqualTo(1));

                Assert.That(ocppGateway_binaryResponseMessagesReceived.Count,                           Is.EqualTo(1));
                //Assert.That(ocppGateway_BinaryDataTransferResponsesReceived.Count,                      Is.EqualTo(1));
                //Assert.That(ocppGateway_BinaryDataTransferResponsesSent.Count,                          Is.EqualTo(1));
                Assert.That(ocppGateway_binaryResponseMessagesSent.Count,                               Is.EqualTo(1));

                Assert.That(ocppLocalController_BinaryResponseMessagesReceived.Count,                   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BinaryDataTransferResponsesReceived.Count,              Is.EqualTo(1));
                //Assert.That(ocppLocalController_BinaryDataTransferResponsesSent.Count,                  Is.EqualTo(1));
                Assert.That(ocppLocalController_BinaryResponseMessagesSent.Count,                       Is.EqualTo(1));

                Assert.That(chargingStation1_BinaryMessageResponseReceived.Count,                       Is.EqualTo(1));
                Assert.That(chargingStation1_BinaryDataTransferResponsesReceived.Count,                 Is.EqualTo(1));


            });

        }

        #endregion


        #region SendMessageTransfer1()

        /// <summary>
        /// Send a SendMessageTransfer *implicitly* to the CSMS.
        /// </summary>
        [Test]
        public async Task SendMessageTransfer1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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


            #region 1. The MessageTransfer request leaves the Energy Meter

            var chargingStation1_MessageTransferMessagesSent        = new ConcurrentList<MessageTransferMessage>();
            var chargingStation1_jsonSendMessagesSent               = new ConcurrentList<OCPP_JSONSendMessage>();

            chargingStation1.OCPP.OUT.OnMessageTransferMessageSent += (timestamp, sender, connection, messageTransferMessage, sentMessageResult, ct) => {
                chargingStation1_MessageTransferMessagesSent.TryAdd(messageTransferMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONSendMessageSent        += (timestamp, sender, connection, sendMessage, sentMessageResult, ct) => {
                chargingStation1_jsonSendMessagesSent.       TryAdd(sendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the MessageTransfer message

            var ocppLocalController_jsonSendMessagesReceived                    = new ConcurrentList<OCPP_JSONSendMessage>();
            var ocppLocalController_MessageTransferMessagesReceived             = new ConcurrentList<MessageTransferMessage>();
            var ocppLocalController_MessageTransferMessagesForwardingDecisions  = new ConcurrentList<ForwardingDecision<MessageTransferMessage>>();
            var ocppLocalController_MessageTransferMessagesSent                 = new ConcurrentList<MessageTransferMessage>();
            var ocppLocalController_jsonSendMessagesSent                        = new ConcurrentList<OCPP_JSONSendMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONSendMessageReceived         += (timestamp, sender, connection, jsonSendMessage, ct) => {
                ocppLocalController_jsonSendMessagesReceived.                  TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageReceived  += (timestamp, sender, connection, jsonMessageTransferMessage, ct) => {
                ocppLocalController_MessageTransferMessagesReceived.           TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageFiltered  += (timestamp, sender, connection, jsonMessageTransferMessage, forwardingDecision, ct) => {
                ocppLocalController_MessageTransferMessagesForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageSent      += (timestamp, sender, connection, jsonMessageTransferMessage, sentMessageResult, ct) => {
                ocppLocalController_MessageTransferMessagesSent.               TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONSendMessageSent             += (timestamp, sender, connection, jsonSendMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonSendMessagesSent.                      TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the MessageTransfer request

            var ocppGateway_jsonSendMessagesReceived                    = new ConcurrentList<OCPP_JSONSendMessage>();
            var ocppGateway_MessageTransferMessagesReceived             = new ConcurrentList<MessageTransferMessage>();
            var ocppGateway_MessageTransferMessagesForwardingDecisions  = new ConcurrentList<ForwardingDecision<MessageTransferMessage>>();
            var ocppGateway_MessageTransferMessagesSent                 = new ConcurrentList<MessageTransferMessage>();
            var ocppGateway_jsonSendMessagesSent                        = new ConcurrentList<OCPP_JSONSendMessage>();

            ocppGateway1.OCPP.IN.     OnJSONSendMessageReceived         += (timestamp, sender, connection, jsonSendMessage, ct) => {
                ocppGateway_jsonSendMessagesReceived.                  TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageReceived  += (timestamp, sender, connection, jsonMessageTransferMessage, ct) => {
                ocppGateway_MessageTransferMessagesReceived.           TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageFiltered  += (timestamp, sender, connection, jsonMessageTransferMessage, forwardingDecision, ct) => {
                ocppGateway_MessageTransferMessagesForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageSent      += (timestamp, sender, connection, jsonMessageTransferMessage, sentMessageResult, ct) => {
                ocppGateway_MessageTransferMessagesSent.               TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONSendMessageSent             += (timestamp, sender, connection, jsonSendMessage, sentMessageResult, ct) => {
                ocppGateway_jsonSendMessagesSent.                      TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the MessageTransfer request

            var csms1_jsonSendMessagesReceived               = new ConcurrentList<OCPP_JSONSendMessage>();
            var csms1_MessageTransferMessagesReceived        = new ConcurrentList<MessageTransferMessage>();

            csms1.OCPP.IN. OnJSONSendMessageReceived        += (timestamp, sender, connection, jsonSendMessage, ct) => {
                csms1_jsonSendMessagesReceived.       TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnMessageTransferMessageReceived += (timestamp, sender, connection, request, ct) => {
                csms1_MessageTransferMessagesReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion


            var messageTransferResponse1  = await chargingStation1.SendMessage(

                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                "TestData",

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );

            var messageTransferResponse2  = await chargingStation1.SendMessage(

                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                JSONObject.Create(new JProperty("test", "data")),

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );

            var messageTransferResponse3  = await chargingStation1.SendMessage(

                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                new JArray("test", "data"),

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );


            #region Wait a little bit for the messages to arrive...

            var rounds = 0;
            do
            {
                await Task.Delay(10);
                rounds++;
            } while (csms1_MessageTransferMessagesReceived.Count < 3 && rounds < 1000);

            #endregion


            Assert.Multiple(() => {

                Assert.That(messageTransferResponse1.Result,                                                                        Is.EqualTo(SentMessageResults.Success));
                Assert.That(messageTransferResponse2.Result,                                                                        Is.EqualTo(SentMessageResults.Success));
                Assert.That(messageTransferResponse3.Result,                                                                        Is.EqualTo(SentMessageResults.Success));

                Assert.That(chargingStation1_MessageTransferMessagesSent.Count,                                                     Is.EqualTo(3));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(0).Signatures.Count,                             Is.EqualTo(1));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(1).Signatures.Count,                             Is.EqualTo(1));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(2).Signatures.Count,                             Is.EqualTo(1));
                Assert.That(chargingStation1_jsonSendMessagesSent.Count,                                                            Is.EqualTo(3));

                Assert.That(ocppLocalController_jsonSendMessagesReceived.Count,                                                     Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesReceived.Count,                                              Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesForwardingDecisions.Count,                                   Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesSent.Count,                                                  Is.EqualTo(3));
                Assert.That(ocppLocalController_jsonSendMessagesSent.Count,                                                         Is.EqualTo(3));

                Assert.That(ocppGateway_jsonSendMessagesReceived.Count,                                                             Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesReceived.Count,                                                      Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesForwardingDecisions.Count,                                           Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesSent.Count,                                                          Is.EqualTo(3));
                Assert.That(ocppGateway_jsonSendMessagesSent.Count,                                                                 Is.EqualTo(3));

                Assert.That(csms1_jsonSendMessagesReceived.Count,                                                                   Is.EqualTo(3));
                Assert.That(csms1_MessageTransferMessagesReceived.Count,                                                            Is.EqualTo(3));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Data?.Type,                                          Is.EqualTo(JTokenType.String));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Data?.ToString(),                                    Is.EqualTo("TestData"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Signatures.Count,                                    Is.EqualTo(1));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"data\"}"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Signatures.Count,                                    Is.EqualTo(1));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"test\",\"data\"]"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Signatures.Count,                                    Is.EqualTo(1));

            });

        }

        #endregion

        #region SendMessageTransfer2()

        /// <summary>
        /// Send a SendMessageTransfer *explicitly* to the CSMS.
        /// </summary>
        [Test]
        public async Task SendMessageTransfer2()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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


            #region 1. The MessageTransfer request leaves the Energy Meter

            var chargingStation1_MessageTransferMessagesSent        = new ConcurrentList<MessageTransferMessage>();
            var chargingStation1_jsonSendMessagesSent               = new ConcurrentList<OCPP_JSONSendMessage>();

            chargingStation1.OCPP.OUT.OnMessageTransferMessageSent += (timestamp, sender, connection, messageTransferMessage, sentMessageResult, ct) => {
                chargingStation1_MessageTransferMessagesSent.TryAdd(messageTransferMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONSendMessageSent        += (timestamp, sender, connection, sendMessage, sentMessageResult, ct) => {
                chargingStation1_jsonSendMessagesSent.       TryAdd(sendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the MessageTransfer message

            var ocppLocalController_jsonSendMessagesReceived                    = new ConcurrentList<OCPP_JSONSendMessage>();
            var ocppLocalController_MessageTransferMessagesReceived             = new ConcurrentList<MessageTransferMessage>();
            var ocppLocalController_MessageTransferMessagesForwardingDecisions  = new ConcurrentList<ForwardingDecision<MessageTransferMessage>>();
            var ocppLocalController_MessageTransferMessagesSent                 = new ConcurrentList<MessageTransferMessage>();
            var ocppLocalController_jsonSendMessagesSent                        = new ConcurrentList<OCPP_JSONSendMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONSendMessageReceived         += (timestamp, sender, connection, jsonSendMessage, ct) => {
                ocppLocalController_jsonSendMessagesReceived.                  TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageReceived  += (timestamp, sender, connection, jsonMessageTransferMessage, ct) => {
                ocppLocalController_MessageTransferMessagesReceived.           TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageFiltered  += (timestamp, sender, connection, jsonMessageTransferMessage, forwardingDecision, ct) => {
                ocppLocalController_MessageTransferMessagesForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMessageTransferMessageSent      += (timestamp, sender, connection, jsonMessageTransferMessage, sentMessageResult, ct) => {
                ocppLocalController_MessageTransferMessagesSent.               TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONSendMessageSent             += (timestamp, sender, connection, jsonSendMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonSendMessagesSent.                      TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the MessageTransfer request

            var ocppGateway_jsonSendMessagesReceived                    = new ConcurrentList<OCPP_JSONSendMessage>();
            var ocppGateway_MessageTransferMessagesReceived             = new ConcurrentList<MessageTransferMessage>();
            var ocppGateway_MessageTransferMessagesForwardingDecisions  = new ConcurrentList<ForwardingDecision<MessageTransferMessage>>();
            var ocppGateway_MessageTransferMessagesSent                 = new ConcurrentList<MessageTransferMessage>();
            var ocppGateway_jsonSendMessagesSent                        = new ConcurrentList<OCPP_JSONSendMessage>();

            ocppGateway1.OCPP.IN.     OnJSONSendMessageReceived         += (timestamp, sender, connection, jsonSendMessage, ct) => {
                ocppGateway_jsonSendMessagesReceived.                  TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageReceived  += (timestamp, sender, connection, jsonMessageTransferMessage, ct) => {
                ocppGateway_MessageTransferMessagesReceived.           TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageFiltered  += (timestamp, sender, connection, jsonMessageTransferMessage, forwardingDecision, ct) => {
                ocppGateway_MessageTransferMessagesForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMessageTransferMessageSent      += (timestamp, sender, connection, jsonMessageTransferMessage, sentMessageResult, ct) => {
                ocppGateway_MessageTransferMessagesSent.               TryAdd(jsonMessageTransferMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONSendMessageSent             += (timestamp, sender, connection, jsonSendMessage, sentMessageResult, ct) => {
                ocppGateway_jsonSendMessagesSent.                      TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the MessageTransfer request

            var csms1_jsonSendMessagesReceived               = new ConcurrentList<OCPP_JSONSendMessage>();
            var csms1_MessageTransferMessagesReceived        = new ConcurrentList<MessageTransferMessage>();

            csms1.OCPP.IN. OnJSONSendMessageReceived        += (timestamp, sender, connection, jsonSendMessage, ct) => {
                csms1_jsonSendMessagesReceived.       TryAdd(jsonSendMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnMessageTransferMessageReceived += (timestamp, sender, connection, request, ct) => {
                csms1_MessageTransferMessagesReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion


            var messageTransferResponse1  = await chargingStation1.SendMessage(

                                                      Destination:         SourceRouting.CSMS,
                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                "TestData",

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );

            var messageTransferResponse2  = await chargingStation1.SendMessage(

                                                      Destination:         SourceRouting.CSMS,
                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                JSONObject.Create(new JProperty("test", "data")),

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );

            var messageTransferResponse3  = await chargingStation1.SendMessage(

                                                      Destination:         SourceRouting.CSMS,
                                                      VendorId:            Vendor_Id. GraphDefined,
                                                      MessageId:           Message_Id.GraphDefined_TestMessage,
                                                      Data:                new JArray("test", "data"),

                                                      SignKeys:            null,
                                                      SignInfos:           null,
                                                      Signatures:          null,

                                                      RequestId:           null,
                                                      RequestTimestamp:    null,
                                                      EventTrackingId:     null

                                                  );


            #region Wait a little bit for the messages to arrive...

            var rounds = 0;
            do
            {
                await Task.Delay(10);
                rounds++;
            } while (csms1_MessageTransferMessagesReceived.Count < 3 && rounds < 1000);

            #endregion


            Assert.Multiple(() => {

                Assert.That(messageTransferResponse1.Result,                                                                        Is.EqualTo(SentMessageResults.Success));
                Assert.That(messageTransferResponse2.Result,                                                                        Is.EqualTo(SentMessageResults.Success));
                Assert.That(messageTransferResponse3.Result,                                                                        Is.EqualTo(SentMessageResults.Success));

                Assert.That(chargingStation1_MessageTransferMessagesSent.Count,                                                     Is.EqualTo(3));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(0).Signatures.Count,                             Is.EqualTo(1));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(1).Signatures.Count,                             Is.EqualTo(1));
                //Assert.That(chargingStation1_MessageTransferMessagesSent.ElementAt(2).Signatures.Count,                             Is.EqualTo(1));
                Assert.That(chargingStation1_jsonSendMessagesSent.Count,                                                            Is.EqualTo(3));

                Assert.That(ocppLocalController_jsonSendMessagesReceived.Count,                                                     Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesReceived.Count,                                              Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesForwardingDecisions.Count,                                   Is.EqualTo(3));
                Assert.That(ocppLocalController_MessageTransferMessagesSent.Count,                                                  Is.EqualTo(3));
                Assert.That(ocppLocalController_jsonSendMessagesSent.Count,                                                         Is.EqualTo(3));

                Assert.That(ocppGateway_jsonSendMessagesReceived.Count,                                                             Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesReceived.Count,                                                      Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesForwardingDecisions.Count,                                           Is.EqualTo(3));
                Assert.That(ocppGateway_MessageTransferMessagesSent.Count,                                                          Is.EqualTo(3));
                Assert.That(ocppGateway_jsonSendMessagesSent.Count,                                                                 Is.EqualTo(3));

                Assert.That(csms1_jsonSendMessagesReceived.Count,                                                                   Is.EqualTo(3));
                Assert.That(csms1_MessageTransferMessagesReceived.Count,                                                            Is.EqualTo(3));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Data?.Type,                                          Is.EqualTo(JTokenType.String));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Data?.ToString(),                                    Is.EqualTo("TestData"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(0).Signatures.Count,                                    Is.EqualTo(1));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"data\"}"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(1).Signatures.Count,                                    Is.EqualTo(1));

                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"test\",\"data\"]"));
                //Assert.That(csms1_MessageTransferMessagesReceived.ElementAt(2).Signatures.Count,                                    Is.EqualTo(1));

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

            if (csms1                is null ||
                csms1_roamingNetwork is null ||
                csms1_cso            is null ||
                csms1_emp            is null ||
                csms1_remoteEMP      is null ||
                csms2                is null ||
                csms2                is null ||
                ocppGateway1          is null ||
                ocppLocalController1  is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
            {

                Assert.Multiple(() => {

                    if (csms1                is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms1_roamingNetwork is null)
                        Assert.Fail("The csms roaming network must not be null!");

                    if (csms1_cso            is null)
                        Assert.Fail("The csms CSO must not be null!");

                    if (csms1_emp            is null)
                        Assert.Fail("The csms EMP must not be null!");

                    if (csms1_remoteEMP      is null)
                        Assert.Fail("The csms remote EMP must not be null!");

                    if (csms2                is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1          is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1  is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1     is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2     is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3     is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            // OCPP

            #region .1. The Authorize request leaves the Charging Station

            var chargingStation1_AuthorizeRequestsSent          = new ConcurrentList<AuthorizeRequest>();
            var chargingStation1_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnAuthorizeRequestSent   += (timestamp, sender, connection, authorizeRequest, sentMessageResult, ct) => {
                chargingStation1_AuthorizeRequestsSent. TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .2. The OCPP Local Controller receives and forwards the Authorize request

            var ocppLocalController_jsonRequestMessagesReceived            = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_AuthorizeRequestsReceived              = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_AuthorizeRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppLocalController_AuthorizeRequestsSent                  = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_jsonRequestMessagesSent                = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessagesReceived.         TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeRequestReceived   += (timestamp, sender, connection, authorizeRequest, ct) => {
                ocppLocalController_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeRequestFiltered   += (timestamp, sender, connection, authorizeRequest, forwardingDecision, ct) => {
                ocppLocalController_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeRequestSent       += (timestamp, sender, connection, authorizeRequest, sentMessageResult, ct) => {
                ocppLocalController_AuthorizeRequestsSent.               TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessagesSent.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .3. The OCPP Gateway receives and forwards the Authorize request

            var ocppGateway_jsonRequestMessagesReceived            = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_AuthorizeRequestsReceived              = new ConcurrentList<AuthorizeRequest>();
            var ocppGateway_AuthorizeRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppGateway_AuthorizeRequestsSent                  = new ConcurrentList<AuthorizeRequest>();
            var ocppGateway_jsonRequestMessagesSent                = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessagesReceived.         TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnAuthorizeRequestReceived   += (timestamp, sender, connection, authorizeRequest, ct) => {
                ocppGateway_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnAuthorizeRequestFiltered   += (timestamp, sender, connection, authorizeRequest, forwardingDecision, ct) => {
                ocppGateway_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnAuthorizeRequestSent       += (timestamp, sender, connection, authorizeRequest, sentMessageResult, ct) => {
                ocppGateway_AuthorizeRequestsSent.               TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessagesSent.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .4. The CSMS receives and maps the Authorize request

            var csms1_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_AuthorizeRequestsReceived          = new ConcurrentList<AuthorizeRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnAuthorizeRequestReceived   += (timestamp, sender, connection, request, ct) => {
                csms1_AuthorizeRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion


            // EV Roaming

            #region .5. RoamingNetwork Request

            var csms1_roamingNetwork_OnAuthorizeStartRequests = new ConcurrentList<LocalAuthentication>();

            csms1_roamingNetwork.OnAuthorizeStartRequest += (logTimestamp,
                                                             requestTimestamp,
                                                             sender,
                                                             senderId,
                                                             eventTrackingId,
                                                             roamingNetworkId,
                                                             empRoamingProviderId,
                                                             csoRoamingProviderId,
                                                             operatorId,
                                                             localAuthentication,
                                                             chargingLocation,
                                                             chargingProduct,
                                                             sessionId,
                                                             cpoPartnerSessionId,
                                                             iSendAuthorizeStartStops,
                                                             requestTimeout) => {
                csms1_roamingNetwork_OnAuthorizeStartRequests.TryAdd(localAuthentication);
                return Task.CompletedTask;
            };

            #endregion

            #region .6. EV-Mobility Provider Request

            var csms1_remoteEMP_OnAuthorizeStartRequests = new ConcurrentList<LocalAuthentication>();

            csms1_remoteEMP.OnAuthorizeStartRequest += (logTimestamp,
                                                        requestTimestamp,
                                                        sender,
                                                        senderId,
                                                        eventTrackingId,
                                                        roamingNetworkId,
                                                        empRoamingProviderId,
                                                        csoRoamingProviderId,
                                                        operatorId,
                                                        localAuthentication,
                                                        chargingLocation,
                                                        chargingProduct,
                                                        sessionId,
                                                        cpoPartnerSessionId,
                                                        iSendAuthorizeStartStops,
                                                        requestTimeout) => {
                csms1_remoteEMP_OnAuthorizeStartRequests.TryAdd(localAuthentication);
                return Task.CompletedTask;
            };

            #endregion

            // processing

            #region .7. EV-Mobility Provider Response

            var csms1_remoteEMP_OnAuthorizeStartResponses = new ConcurrentList<LocalAuthentication>();

            csms1_remoteEMP.OnAuthorizeStartResponse += (logTimestamp,
                                                         requestTimestamp,
                                                         sender,
                                                         senderId,
                                                         eventTrackingId,
                                                         roamingNetworkId,
                                                         empRoamingProviderId,
                                                         csoRoamingProviderId,
                                                         operatorId,
                                                         localAuthentication,
                                                         chargingLocation,
                                                         chargingProduct,
                                                         sessionId,
                                                         cpoPartnerSessionId,
                                                         iSendAuthorizeStartStops,
                                                         requestTimeout,
                                                         result,
                                                         runtime) => {
                csms1_remoteEMP_OnAuthorizeStartResponses.TryAdd(localAuthentication);
                return Task.CompletedTask;
            };

            #endregion

            #region .8. RoamingNetwork Response

            var csms1_roamingNetwork_OnAuthorizeStartResponses = new ConcurrentList<LocalAuthentication>();

            csms1_roamingNetwork.OnAuthorizeStartResponse += (logTimestamp,
                                                              requestTimestamp,
                                                              sender,
                                                              senderId,
                                                              eventTrackingId,
                                                              roamingNetworkId,
                                                              empRoamingProviderId,
                                                              csoRoamingProviderId,
                                                              operatorId,
                                                              localAuthentication,
                                                              chargingLocation,
                                                              chargingProduct,
                                                              sessionId,
                                                              cpoPartnerSessionId,
                                                              iSendAuthorizeStartStops,
                                                              requestTimeout,
                                                              result,
                                                              runtime) => {
                csms1_roamingNetwork_OnAuthorizeStartResponses.TryAdd(localAuthentication);
                return Task.CompletedTask;
            };

            #endregion


            // OCPP

            #region .9. The CSMS receives and maps the Authorize response

            var csms1_AuthorizeResponsesSent          = new ConcurrentList<AuthorizeResponse>();
            var csms1_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnAuthorizeResponseSent   += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_AuthorizeResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 10. The OCPP Gateway receives and forwards the Authorize response

            var ocppGateway_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_AuthorizeResponsesReceived              = new ConcurrentList<AuthorizeResponse>();
            var ocppGateway_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppGateway_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_AuthorizeResponsesSent.      TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 11. The OCPP Local Controller receives and forwards the Authorize response

            var ocppLocalController_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_AuthorizeResponsesReceived              = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_AuthorizeResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 12. The Charging Station receives the Authorize response

            var chargingStation1_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_AuthorizeResponsesReceived         = new ConcurrentList<AuthorizeResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnAuthorizeResponseReceived   += (timestamp, sender, connection, request, response, runtime, ct) => {
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

                Assert.That(authorizeResponse.IdTokenInfo.Status,                                                   Is.EqualTo(AuthorizationStatus.Accepted));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation1_AuthorizeRequestsSent.                                        Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeRequestsSent.First().Signatures.                     Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_jsonRequestMessageSent.                                       Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                Assert.That(chargingStation1_jsonRequestMessageSent.First().Payload["signatures"]?.        Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessagesReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessagesSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessagesSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessagesReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsReceived.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsForwardingDecisions.                              Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_AuthorizeRequestsSent.                                             Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessagesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessagesSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_AuthorizeRequestsReceived.                                               Count,   Is.EqualTo(1));

                Assert.That(csms1_roamingNetwork_OnAuthorizeStartRequests.                                 Count,   Is.EqualTo(1));
                Assert.That(csms1_remoteEMP_OnAuthorizeStartRequests.                                      Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_remoteEMP_OnAuthorizeStartResponses.                                     Count,   Is.EqualTo(1));
                Assert.That(csms1_roamingNetwork_OnAuthorizeStartResponses.                                Count,   Is.EqualTo(1));

                Assert.That(csms1_AuthorizeResponsesSent.                                                  Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_AuthorizeResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_AuthorizeResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.               First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_AuthorizeResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,              Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_AuthorizeResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_AuthorizeResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region Transaction1()

        /// <summary>
        /// Transaction test.
        /// </summary>
        [Test]
        public async Task Transaction1()
        {

            #region Initial checks

            if (csms1                is null ||
                csms1_roamingNetwork is null ||
                csms1_cso            is null ||
                csms1_emp            is null ||
                csms1_remoteEMP      is null ||
                csms2                is null ||
                csms2                is null ||
                ocppGateway1          is null ||
                ocppLocalController1  is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
            {

                Assert.Multiple(() => {

                    if (csms1                is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms1_roamingNetwork is null)
                        Assert.Fail("The csms roaming network must not be null!");

                    if (csms1_cso            is null)
                        Assert.Fail("The csms CSO must not be null!");

                    if (csms1_emp            is null)
                        Assert.Fail("The csms EMP must not be null!");

                    if (csms1_remoteEMP      is null)
                        Assert.Fail("The csms remote EMP must not be null!");

                    if (csms2                is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1          is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1  is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1     is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2     is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3     is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            // OCPP

            #region .1. The Authorize request leaves the Charging Station

            var chargingStation1_TransactionEventRequestsSent   = new ConcurrentList<TransactionEventRequest>();
            var chargingStation1_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation1.OCPP.OUT.OnTransactionEventRequestSent += (timestamp, sender, connection, transactionEventRequest, sentMessageResult, ct) => {
                chargingStation1_TransactionEventRequestsSent. TryAdd(transactionEventRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .2. The OCPP Local Controller receives and forwards the TransactionEvent request

            var ocppLocalController_jsonRequestMessagesReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_TransactionEventRequestsReceived              = new ConcurrentList<TransactionEventRequest>();
            var ocppLocalController_TransactionEventRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<TransactionEventRequest, TransactionEventResponse>>();
            var ocppLocalController_TransactionEventRequestsSent                  = new ConcurrentList<TransactionEventRequest>();
            var ocppLocalController_jsonRequestMessagesSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived        += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessagesReceived.                TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnTransactionEventRequestReceived   += (timestamp, sender, connection, transactionEventRequest, ct) => {
                ocppLocalController_TransactionEventRequestsReceived.           TryAdd(transactionEventRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnTransactionEventRequestFiltered   += (timestamp, sender, connection, transactionEventRequest, forwardingDecision, ct) => {
                ocppLocalController_TransactionEventRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnTransactionEventRequestSent       += (timestamp, sender, connection, transactionEventRequest, sentMessageResult, ct) => {
                ocppLocalController_TransactionEventRequestsSent.               TryAdd(transactionEventRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent            += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessagesSent.                    TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .3. The OCPP Gateway receives and forwards the TransactionEvent request

            var ocppGateway_jsonRequestMessagesReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_TransactionEventRequestsReceived              = new ConcurrentList<TransactionEventRequest>();
            var ocppGateway_TransactionEventRequestsForwardingDecisions   = new ConcurrentList<ForwardingDecision<TransactionEventRequest, TransactionEventResponse>>();
            var ocppGateway_TransactionEventRequestsSent                  = new ConcurrentList<TransactionEventRequest>();
            var ocppGateway_jsonRequestMessagesSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessagesReceived.                TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnTransactionEventRequestReceived   += (timestamp, sender, connection, transactionEventRequest, ct) => {
                ocppGateway_TransactionEventRequestsReceived.           TryAdd(transactionEventRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnTransactionEventRequestFiltered   += (timestamp, sender, connection, transactionEventRequest, forwardingDecision, ct) => {
                ocppGateway_TransactionEventRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnTransactionEventRequestSent       += (timestamp, sender, connection, transactionEventRequest, sentMessageResult, ct) => {
                ocppGateway_TransactionEventRequestsSent.               TryAdd(transactionEventRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent            += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessagesSent.                    TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .4. The CSMS receives and maps the TransactionEvent request

            var csms1_jsonRequestMessageReceived                = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_TransactionEventRequestsReceived          = new ConcurrentList<TransactionEventRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived        += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnTransactionEventRequestReceived   += (timestamp, sender, connection, request, ct) => {
                csms1_TransactionEventRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion


            // EV Roaming

            #region .5. RoamingNetwork Request

            //var csms1_roamingNetwork_OnTransactionEventStartRequests = new ConcurrentList<LocalAuthentication>();

            //csms1_roamingNetwork.OnTransactionEventStartRequest += (logTimestamp,
            //                                                        requestTimestamp,
            //                                                        sender,
            //                                                        senderId,
            //                                                        eventTrackingId,
            //                                                        roamingNetworkId,
            //                                                        empRoamingProviderId,
            //                                                        csoRoamingProviderId,
            //                                                        operatorId,
            //                                                        localAuthentication,
            //                                                        chargingLocation,
            //                                                        chargingProduct,
            //                                                        sessionId,
            //                                                        cpoPartnerSessionId,
            //                                                        iSendTransactionEventStartStops,
            //                                                        requestTimeout) => {
            //    csms1_roamingNetwork_OnTransactionEventStartRequests.TryAdd(localAuthentication);
            //    return Task.CompletedTask;
            //};

            #endregion

            #region .6. EV-Mobility Provider Request

            //var csms1_remoteEMP_OnTransactionEventStartRequests = new ConcurrentList<LocalAuthentication>();

            //csms1_remoteEMP.OnTransactionEventStartRequest += (logTimestamp,
            //                                                   requestTimestamp,
            //                                                   sender,
            //                                                   senderId,
            //                                                   eventTrackingId,
            //                                                   roamingNetworkId,
            //                                                   empRoamingProviderId,
            //                                                   csoRoamingProviderId,
            //                                                   operatorId,
            //                                                   localAuthentication,
            //                                                   chargingLocation,
            //                                                   chargingProduct,
            //                                                   sessionId,
            //                                                   cpoPartnerSessionId,
            //                                                   iSendTransactionEventStartStops,
            //                                                   requestTimeout) => {
            //    csms1_remoteEMP_OnTransactionEventStartRequests.TryAdd(localAuthentication);
            //    return Task.CompletedTask;
            //};

            #endregion

            // processing

            #region .7. EV-Mobility Provider Response

            //var csms1_remoteEMP_OnTransactionEventStartResponses = new ConcurrentList<LocalAuthentication>();

            //csms1_remoteEMP.OnTransactionEventStartResponse += (logTimestamp,
            //                                                    requestTimestamp,
            //                                                    sender,
            //                                                    senderId,
            //                                                    eventTrackingId,
            //                                                    roamingNetworkId,
            //                                                    empRoamingProviderId,
            //                                                    csoRoamingProviderId,
            //                                                    operatorId,
            //                                                    localAuthentication,
            //                                                    chargingLocation,
            //                                                    chargingProduct,
            //                                                    sessionId,
            //                                                    cpoPartnerSessionId,
            //                                                    iSendTransactionEventStartStops,
            //                                                    requestTimeout,
            //                                                    result,
            //                                                    runtime) => {
            //    csms1_remoteEMP_OnTransactionEventStartResponses.TryAdd(localAuthentication);
            //    return Task.CompletedTask;
            //};

            #endregion

            #region .8. RoamingNetwork Response

            //var csms1_roamingNetwork_OnTransactionEventStartResponses = new ConcurrentList<LocalAuthentication>();

            //csms1_roamingNetwork.OnTransactionEventStartResponse += (logTimestamp,
            //                                                         requestTimestamp,
            //                                                         sender,
            //                                                         senderId,
            //                                                         eventTrackingId,
            //                                                         roamingNetworkId,
            //                                                         empRoamingProviderId,
            //                                                         csoRoamingProviderId,
            //                                                         operatorId,
            //                                                         localAuthentication,
            //                                                         chargingLocation,
            //                                                         chargingProduct,
            //                                                         sessionId,
            //                                                         cpoPartnerSessionId,
            //                                                         iSendTransactionEventStartStops,
            //                                                         requestTimeout,
            //                                                         result,
            //                                                         runtime) => {
            //    csms1_roamingNetwork_OnTransactionEventStartResponses.TryAdd(localAuthentication);
            //    return Task.CompletedTask;
            //};

            #endregion


            // OCPP

            #region .9. The CSMS receives and maps the TransactionEvent response

            var csms1_TransactionEventResponsesSent        = new ConcurrentList<TransactionEventResponse>();
            var csms1_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnTransactionEventResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_TransactionEventResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 10. The OCPP Gateway receives and forwards the TransactionEvent response

            var ocppGateway_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_TransactionEventResponsesReceived            = new ConcurrentList<TransactionEventResponse>();
            var ocppGateway_TransactionEventResponsesSent                = new ConcurrentList<TransactionEventResponse>();
            var ocppGateway_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnTransactionEventResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_TransactionEventResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnTransactionEventResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_TransactionEventResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 11. The OCPP Local Controller receives and forwards the TransactionEvent response

            var ocppLocalController_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_TransactionEventResponsesReceived            = new ConcurrentList<TransactionEventResponse>();
            var ocppLocalController_TransactionEventResponsesSent                = new ConcurrentList<TransactionEventResponse>();
            var ocppLocalController_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnTransactionEventResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_TransactionEventResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnTransactionEventResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_TransactionEventResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 12. The Charging Station receives the TransactionEvent response

            var chargingStation1_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_TransactionEventResponsesReceived       = new ConcurrentList<TransactionEventResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnTransactionEventResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation1_TransactionEventResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var transactionEventResponse = await chargingStation1.SendTransactionEvent(

                                                     TransactionEvents.Started,
                                                     Timestamp.Now,
                                                     TriggerReason.Authorized,
                                                     1,
                                                     new Transaction(

                                                         Transaction_Id.NewRandom,
                                                         ChargingState:       ChargingStates.Charging,
                                                         TimeSpentCharging:   TimeSpan.Zero,
                                                         StoppedReason:       null,
                                                         RemoteStartId:       null,
                                                         OperationMode:       OperationMode.ChargingOnly,
                                                         TransactionLimits:   new TransactionLimits(
                                                                                  MaxCost:      null,
                                                                                  MaxEnergy:    null,
                                                                                  MaxTime:      null,
                                                                                  CustomData:   null
                                                                              ),
                                                         ChargingTariffId:    null,

                                                         CustomData:          null

                                                     ),

                                                     Offline:                 null,
                                                     NumberOfPhasesUsed:      null,
                                                     CableMaxCurrent:         null,
                                                     ReservationId:           null,
                                                     IdToken:                 null,
                                                     EVSE:                    null,
                                                     MeterValues:             null,
                                                     PreconditioningStatus:   null,

                                                     CustomData:              null,

                                                     SignKeys:                null,
                                                     SignInfos:               null,
                                                     Signatures:              null,

                                                     RequestId:               null,
                                                     RequestTimestamp:        null,
                                                     RequestTimeout:          null,
                                                     EventTrackingId:         null

                                                 );

            Assert.Multiple(() => {

                // TotalCost
                // ChargingPriority
                // IdTokenInfo
                // UpdatedPersonalMessage

                Assert.That(transactionEventResponse.Result.ResultCode,                                             Is.EqualTo(ResultCode.OK));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation1_TransactionEventRequestsSent.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_TransactionEventRequestsSent.First().Signatures.              Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_jsonRequestMessageSent.                                       Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_jsonRequestMessageSent.            First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                Assert.That(chargingStation1_jsonRequestMessageSent.First().Payload["signatures"]?.        Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessagesReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_TransactionEventRequestsReceived.                          Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_TransactionEventRequestsForwardingDecisions.               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_TransactionEventRequestsSent.                              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessagesSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessagesSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessagesReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_TransactionEventRequestsReceived.                                  Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_TransactionEventRequestsForwardingDecisions.                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_TransactionEventRequestsSent.                                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessagesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessagesSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_TransactionEventRequestsReceived.                                        Count,   Is.EqualTo(1));

                //Assert.That(csms1_roamingNetwork_OnTransactionEventStartRequests.                          Count,   Is.EqualTo(1));
                //Assert.That(csms1_remoteEMP_OnTransactionEventStartRequests.                               Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                //Assert.That(csms1_remoteEMP_OnTransactionEventStartResponses.                              Count,   Is.EqualTo(1));
                //Assert.That(csms1_roamingNetwork_OnTransactionEventStartResponses.                         Count,   Is.EqualTo(1));

                Assert.That(csms1_TransactionEventResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_TransactionEventResponsesReceived.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_TransactionEventResponsesSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                          Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.               First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                              Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_TransactionEventResponsesReceived.                         Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_TransactionEventResponsesSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                  Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.       First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                  Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_TransactionEventResponsesReceived.                            Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_TransactionEventResponsesReceived.First().Signatures.         Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,            Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_TransactionEventResponsesReceived.First().DestinationId,               Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().NetworkPath.ToString(),      Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(chargingStation1_TransactionEventResponsesReceived.First().NetworkPath.ToString(),      Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

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
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1 is null)
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

            chargingStation1.OCPP.OUT.OnMeterValuesRequestSent += (timestamp, sender, connection, meterValuesRequest, sentMessageResult, ct) => {
                chargingStation1_MeterValuesRequestsSent.TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent. TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the MeterValues request

            var ocppLocalController_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_MeterValuesRequestsReceived            = new ConcurrentList<MeterValuesRequest>();
            var ocppLocalController_MeterValuesRequestsForwardingDecisions = new ConcurrentList<ForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
            var ocppLocalController_MeterValuesRequestsSent                = new ConcurrentList<MeterValuesRequest>();
            var ocppLocalController_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.            TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMeterValuesRequestReceived += (timestamp, sender, connection, meterValuesRequest, ct) => {
                ocppLocalController_MeterValuesRequestsReceived.           TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMeterValuesRequestFiltered += (timestamp, sender, connection, meterValuesRequest, forwardingDecision, ct) => {
                ocppLocalController_MeterValuesRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMeterValuesRequestSent     += (timestamp, sender, connection, meterValuesRequest, sentMessageResult, ct) => {
                ocppLocalController_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.                TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the MeterValues request

            var ocppGateway_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_MeterValuesRequestsReceived            = new ConcurrentList<MeterValuesRequest>();
            var ocppGateway_MeterValuesRequestsForwardingDecisions = new ConcurrentList<ForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
            var ocppGateway_MeterValuesRequestsSent                = new ConcurrentList<MeterValuesRequest>();
            var ocppGateway_jsonRequestMessageSent                 = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.            TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMeterValuesRequestReceived += (timestamp, sender, connection, meterValuesRequest, ct) => {
                ocppGateway_MeterValuesRequestsReceived.           TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMeterValuesRequestFiltered += (timestamp, sender, connection, meterValuesRequest, forwardingDecision, ct) => {
                ocppGateway_MeterValuesRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMeterValuesRequestSent     += (timestamp, sender, connection, meterValuesRequest, sentMessageResult, ct) => {
                ocppGateway_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the MeterValues request

            var csms1_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms1_MeterValuesRequestsReceived        = new ConcurrentList<MeterValuesRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms1_jsonRequestMessageReceived.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnMeterValuesRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms1_MeterValuesRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the MeterValues request

            var csms1_MeterValuesResponsesSent        = new ConcurrentList<MeterValuesResponse>();
            var csms1_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnMeterValuesResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_MeterValuesResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms1_jsonResponseMessagesSent.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the MeterValues response

            var ocppGateway_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_MeterValuesResponsesReceived            = new ConcurrentList<MeterValuesResponse>();
            var ocppGateway_MeterValuesResponsesSent                = new ConcurrentList<MeterValuesResponse>();
            var ocppGateway_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMeterValuesResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnMeterValuesResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_MeterValuesResponsesSent.      TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the MeterValues response

            var ocppLocalController_jsonResponseMessagesReceived            = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_MeterValuesResponsesReceived            = new ConcurrentList<MeterValuesResponse>();
            var ocppLocalController_MeterValuesResponsesSent                = new ConcurrentList<MeterValuesResponse>();
            var ocppLocalController_jsonResponseMessagesSent                = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMeterValuesResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnMeterValuesResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_MeterValuesResponsesReceived.  TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the MeterValues response

            var chargingStation1_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_MeterValuesResponsesReceived       = new ConcurrentList<MeterValuesResponse>();

            chargingStation1.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation1_jsonMessageResponseReceived. TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnMeterValuesResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_MeterValuesRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_MeterValuesResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_MeterValuesResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_MeterValuesResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
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

            if (csms1                is null ||
                csms1_roamingNetwork is null ||
                csms1_cso            is null ||
                csms1_emp            is null ||
                csms1_remoteEMP      is null ||
                csms2                is null ||
                ocppGateway1          is null ||
                ocppLocalController1  is null ||
                chargingStation1     is null ||
                e1                   is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
            {

                Assert.Multiple(() => {

                    if (csms1                is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms1_roamingNetwork is null)
                        Assert.Fail("The csms roaming network must not be null!");

                    if (csms1_cso            is null)
                        Assert.Fail("The csms CSO must not be null!");

                    if (csms1_emp            is null)
                        Assert.Fail("The csms EMP must not be null!");

                    if (csms1_remoteEMP      is null)
                        Assert.Fail("The csms remote EMP must not be null!");

                    if (csms2                is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway1          is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController1  is null)
                        Assert.Fail("The local controller must not be null!");

                    if (chargingStation1     is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (e1                   is null)
                        Assert.Fail("The EVSE 1 of charging station 1 must not be null!");

                    if (chargingStation2     is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3     is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            // EV Roaming

            #region .1. EV-Mobility Provider Request

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

            #endregion

            #region .2. RoamingNetwork Request

            var roamingNetwork_OnRemoteStartRequest = 0;

            csms1_roamingNetwork.OnRemoteStartRequest += (logTimestamp,
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

            #endregion

            #region .3. Charging Station Operator Request

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

            #endregion

            #region .4. Charging Pool Request

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

            #endregion

            #region .5. Charging Station Request

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

            #endregion

            #region .6. EVSE Request

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

            #endregion


            // OCPP

            #region  .7. The OCPP RequestStartTransaction request leaves the CSMS

            var chargingStation1_RequestStartTransactionRequestSent  = new ConcurrentList<RequestStartTransactionRequest>();
            var chargingStation1_jsonRequestMessageSent              = new ConcurrentList<OCPP_JSONRequestMessage>();

            csms1.OCPP.OUT.OnRequestStartTransactionRequestSent     += (timestamp, sender, connection, requestStartTransactionRequest, sentMessageResult, ct) => {
                chargingStation1_RequestStartTransactionRequestSent.TryAdd(requestStartTransactionRequest);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONRequestMessageSent                 += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                chargingStation1_jsonRequestMessageSent. TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .8. The OCPP Gateway receives and forwards the OCPP RequestStartTransaction request

            var ocppGateway_jsonRequestMessageReceived                          = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_RequestStartTransactionRequestsReceived             = new ConcurrentList<RequestStartTransactionRequest>();
            var ocppGateway_RequestStartTransactionRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<RequestStartTransactionRequest, RequestStartTransactionResponse>>();
            var ocppGateway_RequestStartTransactionRequestsSent                 = new ConcurrentList<RequestStartTransactionRequest>();
            var ocppGateway_jsonRequestMessageSent                              = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived              += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.                        TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnRequestStartTransactionRequestReceived  += (timestamp, sender, connection, RequestStartTransactionRequest, ct) => {
                ocppGateway_RequestStartTransactionRequestsReceived.           TryAdd(RequestStartTransactionRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnRequestStartTransactionRequestFiltered  += (timestamp, sender, connection, RequestStartTransactionRequest, forwardingDecision, ct) => {
                ocppGateway_RequestStartTransactionRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnRequestStartTransactionRequestSent      += (timestamp, sender, connection, RequestStartTransactionRequest, sentMessageResult, ct) => {
                ocppGateway_RequestStartTransactionRequestsSent.               TryAdd(RequestStartTransactionRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent                  += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                            TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region .9. The OCPP Local Controller receives and forwards the OCPP RequestStartTransaction request

            var ocppLocalController_jsonRequestMessageReceived                          = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_RequestStartTransactionRequestsReceived             = new ConcurrentList<RequestStartTransactionRequest>();
            var ocppLocalController_RequestStartTransactionRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<RequestStartTransactionRequest, RequestStartTransactionResponse>>();
            var ocppLocalController_RequestStartTransactionRequestsSent                 = new ConcurrentList<RequestStartTransactionRequest>();
            var ocppLocalController_jsonRequestMessageSent                              = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived              += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.                        TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnRequestStartTransactionRequestReceived  += (timestamp, sender, connection, RequestStartTransactionRequest, ct) => {
                ocppLocalController_RequestStartTransactionRequestsReceived.           TryAdd(RequestStartTransactionRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnRequestStartTransactionRequestFiltered  += (timestamp, sender, connection, RequestStartTransactionRequest, forwardingDecision, ct) => {
                ocppLocalController_RequestStartTransactionRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnRequestStartTransactionRequestSent      += (timestamp, sender, connection, RequestStartTransactionRequest, sentMessageResult, ct) => {
                ocppLocalController_RequestStartTransactionRequestsSent.               TryAdd(RequestStartTransactionRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent                  += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.                            TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 10. The Charging Station receives the OCPP RequestStartTransaction request

            var chargingStation1_jsonRequestMessageReceived                    = new ConcurrentList<OCPP_JSONRequestMessage>();
            var chargingStation1_RequestStartTransactionRequestsReceived       = new ConcurrentList<RequestStartTransactionRequest>();

            chargingStation1.OCPP.IN.OnJSONRequestMessageReceived             += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                chargingStation1_jsonRequestMessageReceived.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnRequestStartTransactionRequestReceived += (timestamp, sender, connection, RequestStartTransactionRequest, ct) => {
                chargingStation1_RequestStartTransactionRequestsReceived.TryAdd(RequestStartTransactionRequest);
                return Task.CompletedTask;
            };

            #endregion


            // processing...


            // OCPP

            #region 11. The Charging Station responds the OCPP RequestStartTransaction request

            var chargingStation1_RequestStartTransactionResponsesSent        = new ConcurrentList<RequestStartTransactionResponse>();
            var chargingStation1_jsonResponseMessagesSent                    = new ConcurrentList<OCPP_JSONResponseMessage>();

            chargingStation1.OCPP.OUT.OnRequestStartTransactionResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                chargingStation1_RequestStartTransactionResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnJSONResponseMessageSent             += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                chargingStation1_jsonResponseMessagesSent.            TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 12. The OCPP Gateway receives and forwards the BootNotification response

            var ocppLocalController_jsonResponseMessagesReceived                        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_RequestStartTransactionResponsesReceived            = new ConcurrentList<RequestStartTransactionResponse>();
            var ocppLocalController_RequestStartTransactionResponsesSent                = new ConcurrentList<RequestStartTransactionResponse>();
            var ocppLocalController_jsonResponseMessagesSent                            = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived             += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_jsonResponseMessagesReceived.            TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnRequestStartTransactionResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController_RequestStartTransactionResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnRequestStartTransactionResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController_RequestStartTransactionResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent                 += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.                TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 13. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppGateway_jsonResponseMessagesReceived                        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_RequestStartTransactionResponsesReceived            = new ConcurrentList<RequestStartTransactionResponse>();
            var ocppGateway_RequestStartTransactionResponsesSent                = new ConcurrentList<RequestStartTransactionResponse>();
            var ocppGateway_jsonResponseMessagesSent                            = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway1.OCPP.IN.     OnJSONResponseMessageReceived             += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_jsonResponseMessagesReceived.            TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnRequestStartTransactionResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppGateway_RequestStartTransactionResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnRequestStartTransactionResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppGateway_RequestStartTransactionResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent                 += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.                TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 14. The Charging Station receives the BootNotification response

            var csms1_jsonMessageResponseReceived                    = new ConcurrentList<OCPP_JSONResponseMessage>();
            var csms1_RequestStartTransactionResponsesReceived       = new ConcurrentList<RequestStartTransactionResponse>();

            csms1.OCPP.IN.OnJSONResponseMessageReceived             += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                csms1_jsonMessageResponseReceived.             TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN.OnRequestStartTransactionResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                csms1_RequestStartTransactionResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion



            // EV Roaming

            #region EVSE Response

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

            #endregion

            #region Charging Station Response

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

            #endregion

            #region Charging Pool Response

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

            #endregion

            #region Charging Station Operator Response

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

            #endregion

            #region Roaming Network Response

            var roamingNetwork_OnRemoteStartResponse = 0;

            csms1_roamingNetwork.OnRemoteStartResponse += (logTimestamp,
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

            #endregion

            #region E-Mobility Provider Response

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

            #endregion


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

                Assert.That(remoteStartResult.Result,               Is.EqualTo(RemoteStartResultTypes.Success));

                Assert.That(csms1_remoteEMP_OnRemoteStartRequest,   Is.EqualTo(1));
                Assert.That(roamingNetwork_OnRemoteStartRequest,    Is.EqualTo(1));
                Assert.That(csms1_cso_OnRemoteStartRequest,         Is.EqualTo(1));
                Assert.That(p1_OnRemoteStartRequest,                Is.EqualTo(1));
                Assert.That(s1_OnRemoteStartRequest,                Is.EqualTo(1));
                Assert.That(e1_OnRemoteStartRequest,                Is.EqualTo(1));

                Assert.That(e1_OnRemoteStartResponse,               Is.EqualTo(1));
                Assert.That(s1_OnRemoteStartResponse,               Is.EqualTo(1));
                Assert.That(p1_OnRemoteStartResponse,               Is.EqualTo(1));
                Assert.That(csms1_cso_OnRemoteStartResponse,        Is.EqualTo(1));
                Assert.That(roamingNetwork_OnRemoteStartResponse,   Is.EqualTo(1));
                Assert.That(csms1_remoteEMP_OnRemoteStartResponse,  Is.EqualTo(1));

            });

        }

        #endregion


    }

}
