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
using cloud.charging.open.protocols.OCPP.WebSockets;
using cloud.charging.open.protocols.OCPP;

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
    public class ChargingStation_2_CSMS_YetToDo_Tests : AOverlayNetwork
    {

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
            var chargingStation1_binaryRequestMessageSent      = new ConcurrentList<OCPP_BinaryRequestMessage>();

            chargingStation1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                chargingStation1_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.OUT.OnBinaryRequestMessageSent      += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                chargingStation1_binaryRequestMessageSent.    TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the BootNotification request

            var ocppLocalController_binaryRequestMessageReceived                 = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppLocalController_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_binaryRequestMessageSent                     = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryRequestMessageReceived     += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                ocppLocalController_binaryRequestMessageReceived.                 TryAdd(binaryRequestMessage);
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

            ocppLocalController1.OCPP.OUT.    OnBinaryRequestMessageSent         += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppLocalController_binaryRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BootNotification request

            var ocppGateway_binaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppGateway_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppGateway_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_binaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                ocppGateway_binaryRequestMessageReceived.                 TryAdd(binaryRequestMessage);
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

            ocppGateway1.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppGateway_binaryRequestMessageSent.                     TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms1_binaryRequestMessageReceived               = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var csms1_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnBinaryRequestMessageReceived       += (timestamp, sender, connection, binaryRequestMessage, ct) => {
                csms1_binaryRequestMessageReceived.      TryAdd(binaryRequestMessage);
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
            var csms1_binaryResponseMessagesSent             = new ConcurrentList<OCPP_BinaryResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms1_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnBinaryResponseMessageSent      += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                csms1_binaryResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Gateway receives and forwards the BootNotification response

            var ocppGateway_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(binaryResponseMessage);
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

            ocppGateway1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                ocppLocalController_binaryResponseMessagesReceived.     TryAdd(binaryResponseMessage);
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

            ocppLocalController1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, responseMessage, sentMessageResult, ct) => {
                ocppLocalController_binaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_binaryMessageResponseReceived             = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnBinaryResponseMessageReceived      += (timestamp, sender, connection, binaryResponseMessage, ct) => {
                chargingStation1_binaryMessageResponseReceived.      TryAdd(binaryResponseMessage);
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
                Assert.That(chargingStation1_binaryRequestMessageSent.                                    Count,   Is.EqualTo(1));
                //Assert.That(chargingStation1_binaryRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ chargingStation1.Id ]).ToString()));
                //Assert.That(chargingStation1_binaryRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_binaryRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_binaryRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_binaryRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_binaryRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_binaryRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_binaryRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms1_binaryRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms1_binaryRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ chargingStation1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms1_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms1_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms1_binaryResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms1_binaryResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                Assert.That(csms1_binaryResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_binaryResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_binaryResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_binaryResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_binaryResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_binaryResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_binaryResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(chargingStation1_binaryMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(chargingStation1_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(chargingStation1_binaryMessageResponseReceived.      First().Destination.Next,           Is.EqualTo(chargingStation1.Id));
                //Assert.That(chargingStation1_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(chargingStation1.Id));
                Assert.That(chargingStation1_binaryMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
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
            var ocppLocalController_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
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
            var ocppGateway_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
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


    }

}
