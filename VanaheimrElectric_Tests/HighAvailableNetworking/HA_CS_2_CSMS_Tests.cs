/*
 * Copyright (c) 2015-2025 GraphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of WWCP Vanaheimr Electric <https://github.com/OpenChargingCloud/VanaheimrElectric>
 *
 * Licensed under the Affero GPL license, Version 3.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.gnu.org/licenses/agpl.html
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
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;

using cloud.charging.open.protocols.WWCP.NetworkingNode;
using cloud.charging.open.protocols.OCPP.WebSockets;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// High Availability Overlay Network Tests
    /// 
    ///        🡕 [lc1] ⭨
    /// [cs] ──→ [lc2] ━━━► [csms]
    ///        ⭨ [lc3] 🡕
    /// </summary>
    [TestFixture]
    public class HA_CS_2_CSMS_Tests : AHighAvailableNetworking
    {

        #region SendBootNotification_viaLC1()

        /// <summary>
        /// Send BootNotification test via Local Controller #1.
        /// </summary>
        [Test]
        public async Task SendBootNotification_viaLC1()
        {

            #region Initial checks

            if (csms                 is null ||
                ocppLocalController1 is null ||
                ocppLocalController2 is null ||
                ocppLocalController3 is null ||
                chargingStation      is null)
            {

                Assert.Multiple(() => {

                    if (csms               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (ocppLocalController1 is null)
                        Assert.Fail("The local controller #1 must not be null!");

                    if (ocppLocalController2 is null)
                        Assert.Fail("The local controller #2 must not be null!");

                    if (ocppLocalController3 is null)
                        Assert.Fail("The local controller #3 must not be null!");

                    if (chargingStation    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation_jsonRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2a. The OCPP Local Controller #1 receives and forwards the BootNotification request

            var ocppLocalController1_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController1_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController1_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController1_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController1_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController1_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2b. The OCPP Local Controller #2 receives nothing

            var ocppLocalController2_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController2_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController2_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController2_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController2_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController2_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2c. The OCPP Local Controller #3 receives nothing

            var ocppLocalController3_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController3_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController3_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController3_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController3_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController3_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 4. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5a. The OCPP Local Controller #1 receives and forwards the BootNotification response

            var ocppLocalController1_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController1_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController1_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5b. The OCPP Local Controller #2 receives nothing

            var ocppLocalController2_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController2_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController2_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5c. The OCPP Local Controller #3 receives nothing

            var ocppLocalController3_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController3_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController3_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The Charging Station receives the BootNotification response

            var chargingStation_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            chargingStation.Routing.RemoveStaticRouting(NetworkingNode_Id.CSMS);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController1.Id, Priority: 10);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController2.Id, Priority: 20, Weight: 50);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController3.Id, Priority: 20, Weight: 50);

            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController1.Id, Priority: 10);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController2.Id, Priority: 20, Weight: 50);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController3.Id, Priority: 20, Weight: 50);

            ocppLocalController1.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController2.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController3.OCPP.FORWARD.ForwardUnknownResponses = false;


            var bootNotificationResponse = await chargingStation.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      TimeSpan.FromHours(3),
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval > TimeSpan.Zero,                                     Is.True);
                Assert.That(bootNotificationResponse.Signatures.Count,                                             Is.EqualTo(1));
                Assert.That(bootNotificationResponse.DestinationId,                                                Is.EqualTo(chargingStation.Id));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(bootNotificationResponse.NetworkPath,                                                  Is.EqualTo(new[] { ocppLocalController1.Id }));


                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation.Id ]).ToString()));
                Assert.That(chargingStation_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController1_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppLocalController2_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController3_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController1.Id ]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

                Assert.That(ocppLocalController1_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesReceived.                       Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesSent.                           Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController1_jsonResponseMessagesSent.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_jsonResponseMessagesSent.     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppLocalController2_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController3_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));

                Assert.That(chargingStation_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));

                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppLocalController1.Id ]).ToString()));
                //Assert.That(bootNotificationResponseX.NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendBootNotification_viaLC2()

        /// <summary>
        /// Send BootNotification test via Local Controller #2.
        /// </summary>
        [Test]
        public async Task SendBootNotification_viaLC2()
        {

            #region Initial checks

            if (csms                 is null ||
                ocppLocalController1 is null ||
                ocppLocalController2 is null ||
                ocppLocalController3 is null ||
                chargingStation      is null)
            {

                Assert.Multiple(() => {

                    if (csms               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (ocppLocalController1 is null)
                        Assert.Fail("The local controller #1 must not be null!");

                    if (ocppLocalController2 is null)
                        Assert.Fail("The local controller #2 must not be null!");

                    if (ocppLocalController3 is null)
                        Assert.Fail("The local controller #3 must not be null!");

                    if (chargingStation    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation_jsonRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController1_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController1_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController1_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController1_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController1_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2b. The OCPP Local Controller #2 receives and forwards the BootNotification request

            var ocppLocalController2_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController2_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController2_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController2_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController2_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController2_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2c. The OCPP Local Controller #3 receives nothing

            var ocppLocalController3_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController3_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController3_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController3_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController3_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController3_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 4. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController1_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController1_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5b. The OCPP Local Controller #2 receives and forwards the BootNotification response

            var ocppLocalController2_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController2_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController2_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5c. The OCPP Local Controller #3 receives nothing

            var ocppLocalController3_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController3_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController3_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The Charging Station receives the BootNotification response

            var chargingStation_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            chargingStation.Routing.RemoveStaticRouting(NetworkingNode_Id.CSMS);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController1.Id, Priority: 40);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController2.Id, Priority: 10, Weight: 50);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController3.Id, Priority: 20, Weight: 50);

            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController1.Id, Priority: 40);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController2.Id, Priority: 10, Weight: 50);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController3.Id, Priority: 20, Weight: 50);

            ocppLocalController1.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController2.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController3.OCPP.FORWARD.ForwardUnknownResponses = false;


            var bootNotificationResponse = await chargingStation.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      TimeSpan.FromHours(3),
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval > TimeSpan.Zero,                                     Is.True);
                Assert.That(bootNotificationResponse.Signatures.Count,                                             Is.EqualTo(1));
                Assert.That(bootNotificationResponse.DestinationId,                                                Is.EqualTo(chargingStation.Id));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(bootNotificationResponse.NetworkPath,                                                  Is.EqualTo(new[] { ocppLocalController2.Id }));


                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation.Id ]).ToString()));
                Assert.That(chargingStation_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController2_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));

                Assert.That(ocppLocalController1_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController3_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

                Assert.That(ocppLocalController2_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesReceived.                       Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesSent.                           Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_jsonResponseMessagesSent.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_jsonResponseMessagesSent.     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppLocalController1_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController3_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));

                Assert.That(chargingStation_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));

                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppLocalController2.Id ]).ToString()));
                //Assert.That(bootNotificationResponseX.NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendBootNotification_viaLC2_backViaLC3()

        /// <summary>
        /// Send BootNotification test via Local Controller #2 and back via Local Controller #3.
        /// </summary>
        [Test]
        public async Task SendBootNotification_viaLC2_backViaLC3()
        {

            #region Initial checks

            if (csms                 is null ||
                ocppLocalController1 is null ||
                ocppLocalController2 is null ||
                ocppLocalController3 is null ||
                chargingStation      is null)
            {

                Assert.Multiple(() => {

                    if (csms               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (ocppLocalController1 is null)
                        Assert.Fail("The local controller #1 must not be null!");

                    if (ocppLocalController2 is null)
                        Assert.Fail("The local controller #2 must not be null!");

                    if (ocppLocalController3 is null)
                        Assert.Fail("The local controller #3 must not be null!");

                    if (chargingStation    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation_jsonRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController1_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController1_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController1_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController1_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController1_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2b. The OCPP Local Controller #2 receives and forwards the BootNotification request

            var ocppLocalController2_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController2_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController2_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController2_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController2_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController2_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2c. The OCPP Local Controller #3 receives and forwards the BootNotification request

            var ocppLocalController3_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController3_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController3_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController3_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController3_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController3_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 4. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController1_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController1_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5b. The OCPP Local Controller #2 receives and forwards the BootNotification response

            var ocppLocalController2_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController2_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController2_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5c. The OCPP Local Controller #3 receives and forwards the BootNotification response

            var ocppLocalController3_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController3_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController3_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The Charging Station receives the BootNotification response

            var chargingStation_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            chargingStation.Routing.RemoveStaticRouting(NetworkingNode_Id.CSMS);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController1.Id, Priority: 30);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController2.Id, Priority: 10);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController3.Id, Priority: 20);

            csms.           Routing.RemoveStaticRouting(chargingStation.Id);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController1.Id, Priority: 30);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController2.Id, Priority: 20);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController3.Id, Priority: 10);

            ocppLocalController1.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController2.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController3.OCPP.FORWARD.ForwardUnknownResponses = true;


            var bootNotificationResponse = await chargingStation.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      TimeSpan.FromHours(3),
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval > TimeSpan.Zero,                                     Is.True);
                Assert.That(bootNotificationResponse.Signatures.Count,                                             Is.EqualTo(1));
                Assert.That(bootNotificationResponse.DestinationId,                                                Is.EqualTo(chargingStation.Id));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(bootNotificationResponse.NetworkPath,                                                  Is.EqualTo(new[] { ocppLocalController3.Id }));


                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation.Id ]).ToString()));
                Assert.That(chargingStation_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController2_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController2_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));

                Assert.That(ocppLocalController1_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController3_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

                Assert.That(ocppLocalController3_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesReceived.                       Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_BootNotificationResponsesSent.                           Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController3_jsonResponseMessagesSent.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController1_jsonResponseMessagesSent.     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppLocalController1_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));
                Assert.That(ocppLocalController2_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));

                Assert.That(chargingStation_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));

                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppLocalController3.Id ]).ToString()));
                //Assert.That(bootNotificationResponseX.NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendBootNotification_viaLC2or3_ForwardingUnknownResponses()

        /// <summary>
        /// Send BootNotification test via Local Controller #2 or #3.
        /// 'ForwardingUnknownResponses' is enabled in both local controllers.
        /// </summary>
        [Test]
        public async Task SendBootNotification_viaLC2or3_ForwardingUnknownResponses()
        {

            #region Initial checks

            if (csms                 is null ||
                ocppLocalController1 is null ||
                ocppLocalController2 is null ||
                ocppLocalController3 is null ||
                chargingStation      is null)
            {

                Assert.Multiple(() => {

                    if (csms               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (ocppLocalController1 is null)
                        Assert.Fail("The local controller #1 must not be null!");

                    if (ocppLocalController2 is null)
                        Assert.Fail("The local controller #2 must not be null!");

                    if (ocppLocalController3 is null)
                        Assert.Fail("The local controller #3 must not be null!");

                    if (chargingStation    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Charging Station

            var chargingStation_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var chargingStation_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            chargingStation.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation_jsonRequestMessageSent.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController1_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController1_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController1_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController1_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController1_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController1_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2b. The OCPP Local Controller #2 receives and forwards the BootNotification request

            var ocppLocalController2_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController2_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController2_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController2_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController2_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController2_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController2_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2c. The OCPP Local Controller #3 receives and forwards the BootNotification request

            var ocppLocalController3_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController3_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController3_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController3_jsonRequestMessageSent                       = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController3_jsonRequestMessageReceived.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest, ct) => {
                ocppLocalController3_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppLocalController3_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 4. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5a. The OCPP Local Controller #1 receives nothing

            var ocppLocalController1_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController1_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController1_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController1_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController1_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController1_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5b. The OCPP Local Controller #2 receives and forwards the BootNotification response

            var ocppLocalController2_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController2_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController2_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController2.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController2_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController2_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController2.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController2_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 5c. The OCPP Local Controller #3 receives and forwards the BootNotification response

            var ocppLocalController3_jsonResponseMessagesReceived                 = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController3_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController3_jsonResponseMessagesSent                     = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController3.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController3_jsonResponseMessagesReceived.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppLocalController3_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController3.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController3_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The Charging Station receives the BootNotification response

            var chargingStation_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                chargingStation_jsonMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            chargingStation.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                chargingStation_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            chargingStation.Routing.RemoveStaticRouting(NetworkingNode_Id.CSMS);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController1.Id, Priority: 40);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController2.Id, Priority: 10, Weight: 50);
            chargingStation.Routing.AddOrUpdateStaticRouting(csms.Id,             ocppLocalController3.Id, Priority: 20, Weight: 50);

            csms.Routing.RemoveStaticRouting(chargingStation.Id);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController1.Id, Priority: 40);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController2.Id, Priority: 20, Weight: 50);
            csms.           Routing.AddOrUpdateStaticRouting(chargingStation.Id,  ocppLocalController3.Id, Priority: 10, Weight: 50);

            ocppLocalController1.OCPP.FORWARD.ForwardUnknownResponses = false;
            ocppLocalController2.OCPP.FORWARD.ForwardUnknownResponses = true;
            ocppLocalController3.OCPP.FORWARD.ForwardUnknownResponses = true;


            var bootNotificationResponse = await chargingStation.SendBootNotification(

                                                     BootReason:          BootReason.PowerUp,
                                                     CustomData:          null,

                                                     SignKeys:            null,
                                                     SignInfos:           null,
                                                     Signatures:          null,

                                                     RequestId:           null,
                                                     RequestTimestamp:    null,
                                                     RequestTimeout:      TimeSpan.FromHours(3),
                                                     EventTrackingId:     null

                                                 );

            Assert.Multiple(() => {

                Assert.That(bootNotificationResponse.Status,                                                       Is.EqualTo(RegistrationStatus.Accepted));
                Assert.That(Math.Abs((Timestamp.Now - bootNotificationResponse.CurrentTime).TotalMinutes) < 1,     Is.True);
                Assert.That(bootNotificationResponse.Interval > TimeSpan.Zero,                                     Is.True);
                Assert.That(bootNotificationResponse.Signatures.Count,                                             Is.EqualTo(1));
                Assert.That(bootNotificationResponse.DestinationId,                                                Is.EqualTo(chargingStation.Id));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(
                    bootNotificationResponse.NetworkPath.Equals(new NetworkPath(ocppLocalController2.Id)) ||
                    bootNotificationResponse.NetworkPath.Equals(new NetworkPath(ocppLocalController3.Id)),
                    Is.True,
                    "Either LC2 or LC3 should be the previous hop!"
                );


                //StatusInfo


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(chargingStation_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(chargingStation_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(chargingStation_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation.Id ]).ToString()));
                Assert.That(chargingStation_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));


                Assert.That(ocppLocalController1_jsonRequestMessageReceived.                               Count,   Is.EqualTo(0));
                Assert.That(
                    (ocppLocalController2_jsonRequestMessageReceived.Count == 1 && ocppLocalController3_jsonRequestMessageReceived.Count == 0) ||
                    (ocppLocalController2_jsonRequestMessageReceived.Count == 0 && ocppLocalController3_jsonRequestMessageReceived.Count == 1),
                    Is.True,
                    "Either LC2 or LC3 should have received exactly one JSON request!"
                );

                Assert.That(ocppLocalController1_BootNotificationRequestsReceived.Count, Is.EqualTo(0));
                Assert.That(
                    (ocppLocalController2_BootNotificationRequestsReceived.Count == 1 && ocppLocalController3_BootNotificationRequestsReceived.Count == 0) ||
                    (ocppLocalController2_BootNotificationRequestsReceived.Count == 0 && ocppLocalController3_BootNotificationRequestsReceived.Count == 1),
                    Is.True,
                    "Either LC2 or LC3 should have received exactly one BootNotificationRequest request!"
                );

                Assert.That(ocppLocalController1_BootNotificationRequestsForwardingDecisions.Count, Is.EqualTo(0));
                Assert.That(
                    (ocppLocalController2_BootNotificationRequestsForwardingDecisions.Count == 1 && ocppLocalController3_BootNotificationRequestsForwardingDecisions.Count == 0) ||
                    (ocppLocalController2_BootNotificationRequestsForwardingDecisions.Count == 0 && ocppLocalController3_BootNotificationRequestsForwardingDecisions.Count == 1),
                    Is.True,
                    "Either LC2 or LC3 should have forwarded exactly one BootNotificationRequest request!"
                );

                Assert.That(ocppLocalController1_BootNotificationRequestsSent.Count, Is.EqualTo(0));
                Assert.That(
                    (ocppLocalController2_BootNotificationRequestsSent.Count == 1 && ocppLocalController3_BootNotificationRequestsSent.Count == 0) ||
                    (ocppLocalController2_BootNotificationRequestsSent.Count == 0 && ocppLocalController3_BootNotificationRequestsSent.Count == 1),
                    Is.True,
                    "Either LC2 or LC3 should have sent exactly one BootNotificationRequest request!"
                );

                Assert.That(ocppLocalController1_jsonRequestMessageSent.Count, Is.EqualTo(0));
                Assert.That(
                    (ocppLocalController2_jsonRequestMessageSent.Count == 1 && ocppLocalController3_jsonRequestMessageSent.Count == 0) ||
                    (ocppLocalController2_jsonRequestMessageSent.Count == 0 && ocppLocalController3_jsonRequestMessageSent.Count == 1),
                    Is.True,
                    "Either LC2 or LC3 should have sent exactly one JSON request!"
                );

                //Assert.That(ocppLocalController1_jsonRequestMessageReceived.Count, Is.EqualTo(0));
                //Assert.That(
                //    (ocppLocalController2_jsonRequestMessageReceived.Count == 1 && ocppLocalController3_jsonRequestMessageReceived.Count == 0) ||
                //    (ocppLocalController2_jsonRequestMessageReceived.Count == 0 && ocppLocalController3_jsonRequestMessageReceived.Count == 1),
                //    Is.True,
                //    "Either LC2 or LC3 should have forwarded exactly one request!"
                //);

                //Assert.That(ocppLocalController2_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));


                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation.Id, ocppLocalController2.Id ]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));



                //Assert.That(ocppLocalController2_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(1));
                ////Assert.That(ocppLocalController1_BootNotificationResponsesReceived.                       Count,   Is.EqualTo(1));
                ////Assert.That(ocppLocalController1_BootNotificationResponsesSent.                           Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController2_jsonResponseMessagesSent.                                Count,   Is.EqualTo(1));
                ////Assert.That(ocppLocalController1_jsonResponseMessagesSent.     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController1.Id ]).ToString()));

                //Assert.That(ocppLocalController1_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));
                //Assert.That(ocppLocalController3_jsonResponseMessagesReceived.                            Count,   Is.EqualTo(0));





                Assert.That(chargingStation_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));

                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation_jsonMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation.Id));
                Assert.That(
                    chargingStation_jsonMessageResponseReceived.First().NetworkPath.Equals(new NetworkPath(ocppLocalController2.Id)) ||
                    chargingStation_jsonMessageResponseReceived.First().NetworkPath.Equals(new NetworkPath(ocppLocalController3.Id)),
                    Is.True,
                    "Either LC2 or LC3 should be the previous hop!"
                );


            });

        }

        #endregion


    }

}
