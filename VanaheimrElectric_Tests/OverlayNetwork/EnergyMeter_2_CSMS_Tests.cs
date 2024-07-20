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
using cloud.charging.open.protocols.OCPPv2_1.EM;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Overlay Network Tests
    /// Energy Meter  --[LC]--[GW]-->  CSMS
    /// </summary>
    [TestFixture]
    public class EnergyMeter_2_CSMS_Tests : AOverlayNetwork
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
                ocppEnergyMeter     is null)
            {
                Assert.Fail("Failed precondition(s)!");
                return;
            }

            #endregion


            #region 1. The BootNotification request leaves the Energy Meter

            var ocppEnergyMeter_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, bootNotificationRequest) => {
                ocppEnergyMeter_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_jsonRequestMessageSent.      TryAdd(requestMessage);
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

            #region 8. The Energy Meter receives the BootNotification response

            var ocppEnergyMeter_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            ocppEnergyMeter.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppEnergyMeter_jsonMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await ocppEnergyMeter.SendBootNotification(

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
                Assert.That(ocppEnergyMeter_BootNotificationRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppEnergyMeter_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.                           Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().Signatures.        Count,   Is.EqualTo(1));
                // Note: The charging stations use "normal" networking and thus have no valid networking information!
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.      First().DestinationId,              Is.EqualTo(ocppEnergyMeter.Id));
                //Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().DestinationId,              Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));
                //Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ NetworkingNode_Id.CSMS ]).ToString()));

            });

        }

        #endregion

        #region SendDataTransfer1()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendDataTransfer1()
        {

            #region Initial checks

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                ocppEnergyMeter     is null)
            {
                Assert.Fail("Failed precondition(s)!");
                return;
            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, dataTransferRequest) => {
                ocppEnergyMeter_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_jsonRequestMessageSent.  TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.             TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, bootNotificationRequest) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestMessageSent.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                ocppGateway_jsonRequestMessageReceived.             TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, bootNotificationRequest) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms_jsonRequestMessageReceived           = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_DataTransferRequestsReceived         = new ConcurrentList<DataTransferRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived   += (timestamp, sender, requestMessage) => {
                csms_jsonRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnDataTransferRequestReceived  += (timestamp, sender, connection, request) => {
                csms_DataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the DataTransfer request

            var csms_DataTransferResponsesSent        = new ConcurrentList<DataTransferResponse>();
            var csms_jsonResponseMessagesSent         = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, responseMessage, sendMessageResult) => {
                csms_jsonResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppGateway_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppGateway.OCPP.IN.     OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppGateway_jsonResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_DataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent      += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppLocalController_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

            ocppLocalController.OCPP.IN.     OnJSONResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppLocalController_jsonResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppLocalController_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the DataTransfer response

            var ocppEnergyMeter_jsonMessageResponseReceived         = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            ocppEnergyMeter.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, responseMessage) => {
                ocppEnergyMeter_jsonMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion





            var dataTransferResponse1  = await ocppEnergyMeter.TransferData(

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

            //var dataTransferResponse2  = await ocppEnergyMeter.TransferData(

            //                                       VendorId:            Vendor_Id. GraphDefined,
            //                                       MessageId:           Message_Id.GraphDefined_TestMessage,
            //                                       Data:                JSONObject.Create(new JProperty("test", "data")),
            //                                       CustomData:          null,

            //                                       SignKeys:            null,
            //                                       SignInfos:           null,
            //                                       Signatures:          null,

            //                                       RequestId:           null,
            //                                       RequestTimestamp:    null,
            //                                       RequestTimeout:      null,
            //                                       EventTrackingId:     null

            //                                   );

            //var dataTransferResponse3  = await ocppEnergyMeter.TransferData(

            //                                       VendorId:            Vendor_Id. GraphDefined,
            //                                       MessageId:           Message_Id.GraphDefined_TestMessage,
            //                                       Data:                new JArray("test", "data"),
            //                                       CustomData:          null,

            //                                       SignKeys:            null,
            //                                       SignInfos:           null,
            //                                       Signatures:          null,

            //                                       RequestId:           null,
            //                                       RequestTimestamp:    null,
            //                                       RequestTimeout:      null,
            //                                       EventTrackingId:     null

            //                                   );


            Assert.Multiple(() => {

                Assert.That(dataTransferResponse1.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse1.Data?.Type,                                          Is.EqualTo(JTokenType.String));
                Assert.That(dataTransferResponse1.Data?.ToString(),                                    Is.EqualTo("ataDtseT"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

                //Assert.That(dataTransferResponse2.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                //Assert.That(dataTransferResponse2.Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                //Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"atad\"}"));
                ////StatusInfo
                //Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(1).Signatures.Count,   Is.EqualTo(1));

                //Assert.That(dataTransferResponse3.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                //Assert.That(dataTransferResponse3.Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                //Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"tset\",\"atad\"]"));
                ////StatusInfo
                //Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(2).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion

        #region SendDataTransfer2()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendDataTransfer2()
        {

            #region Initial checks

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                ocppEnergyMeter     is null)
            {
                Assert.Fail("Failed precondition(s)!");
                return;
            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, dataTransferRequest) => {
                ocppEnergyMeter_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_jsonRequestMessageSent.  TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await ocppEnergyMeter.TransferData(

                                                   DestinationId:       csms.Id,
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

            var dataTransferResponse2  = await ocppEnergyMeter.TransferData(

                                                   DestinationId:       csms.Id,
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

            var dataTransferResponse3  = await ocppEnergyMeter.TransferData(

                                                   DestinationId:       csms.Id,
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
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse2.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse2.Data?.Type,                                          Is.EqualTo(JTokenType.Object));
                Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("{\"test\":\"atad\"}"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(1).Signatures.Count,   Is.EqualTo(1));

                Assert.That(dataTransferResponse3.Status,                                              Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse3.Data?.Type,                                          Is.EqualTo(JTokenType.Array));
                Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),     Is.EqualTo("[\"tset\",\"atad\"]"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(2).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion


    }

}
