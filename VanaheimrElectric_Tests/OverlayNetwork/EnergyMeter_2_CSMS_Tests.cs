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

            ocppEnergyMeter.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, bootNotificationRequest, sendMessageResult) => {
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

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, bootNotificationRequest, sendMessageResult) => {
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

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, bootNotificationRequest, sendMessageResult) => {
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

                Assert.That(ocppLocalController_jsonRequestMessageReceived.Count,                                          Is.EqualTo(1));
                Assert.That(ocppLocalController_BootNotificationRequestsReceived.Count,                                    Is.EqualTo(1));

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
                Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.Count,                                          Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkingMode,                         Is.EqualTo(NetworkingMode.OverlayNetwork));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().DestinationId,                          Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkPath.ToString(),                 Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.Count,                                    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().DestinationId,                    Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().Signatures.Count,                 Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.ElementAt(0).Signatures.First().Status,   Is.EqualTo(VerificationStatus.ValidSignature));

            });

        }

        #endregion

        #region SendDataTransfer1()

        /// <summary>
        /// Send DataTransfer test (implicitly to the CSMS).
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

                Assert.Multiple(() => {

                    if (csms                is null)
                        Assert.Fail("The csms must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (ocppEnergyMeter     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
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

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
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

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
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
                ocppEnergyMeter_jsonMessageResponseReceived.  TryAdd(responseMessage);
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

            var dataTransferResponse2  = await ocppEnergyMeter.TransferData(

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

                Assert.That(dataTransferResponse1.Status,                                                      Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse1.Data?.Type,                                                  Is.EqualTo(JTokenType.String));
                Assert.That(dataTransferResponse1.Data?.ToString(),                                            Is.EqualTo("ataDtseT"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(0).Signatures.Count,            Is.EqualTo(1));

                Assert.That(dataTransferResponse2.Status,                                                      Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse2.Data?.Type,                                                  Is.EqualTo(JTokenType.Object));
                Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),             Is.EqualTo("{\"test\":\"atad\"}"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(1).Signatures.Count,            Is.EqualTo(1));

                Assert.That(dataTransferResponse3.Status,                                                      Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse3.Data?.Type,                                                  Is.EqualTo(JTokenType.Array));
                Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),             Is.EqualTo("[\"tset\",\"atad\"]"));
                //StatusInfo
                Assert.That(ocppEnergyMeter_DataTransferRequestsSent.ElementAt(2).Signatures.Count,            Is.EqualTo(1));

            });

        }

        #endregion

        #region SendDataTransfer2()

        /// <summary>
        /// Send DataTransfer test (explicitly to the CSMS).
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

                Assert.Multiple(() => {

                    if (csms                is null)
                        Assert.Fail("The csms must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (ocppEnergyMeter     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
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

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
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

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
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
                ocppEnergyMeter_jsonMessageResponseReceived.  TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_DataTransferResponsesReceived.TryAdd(response);
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


        #region SendBinaryDataTransfer1()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendBinaryDataTransfer1()
        {

            #region Initial checks

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                ocppEnergyMeter     is null)
            {

                Assert.Multiple(() => {

                    if (csms                is null)
                        Assert.Fail("The csms must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (ocppEnergyMeter     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BinaryDataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_BinaryDataTransferRequestsSent        = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppEnergyMeter_BinaryRequestMessageSent              = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnBinaryDataTransferRequestSent += (timestamp, sender, dataTransferRequest, sendMessageResult) => {
                ocppEnergyMeter_BinaryDataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnBinaryRequestMessageSent      += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_BinaryRequestMessageSent.        TryAdd(requestMessage);
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
                ocppLocalController_BinaryRequestMessageReceived.                   TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest) => {
                ocppLocalController_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision) => {
                ocppLocalController_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, binaryDataTransferRequest, sendMessageResult) => {
                ocppLocalController_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppLocalController_BinaryRequestMessageSent.                       TryAdd(requestMessage);
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
                ocppGateway_binaryRequestMessageReceived.                   TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestReceived  += (timestamp, sender, connection, binaryDataTransferRequest) => {
                ocppGateway_BinaryDataTransferRequestsReceived.           TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestFiltered  += (timestamp, sender, connection, binaryDataTransferRequest, forwardingDecision) => {
                ocppGateway_BinaryDataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferRequestSent      += (timestamp, sender, binaryDataTransferRequest, sendMessageResult) => {
                ocppGateway_BinaryDataTransferRequestsSent.               TryAdd(binaryDataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppGateway_binaryRequestMessageSent.                       TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BinaryDataTransfer request

            var csms_BinaryRequestMessageReceived              = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var csms_BinaryDataTransferRequestsReceived        = new ConcurrentList<BinaryDataTransferRequest>();

            csms.OCPP.IN. OnBinaryRequestMessageReceived      += (timestamp, sender, requestMessage) => {
                csms_BinaryRequestMessageReceived.      TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnBinaryDataTransferRequestReceived += (timestamp, sender, connection, request) => {
                csms_BinaryDataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BinaryDataTransfer request

            var csms_BinaryDataTransferResponsesSent        = new ConcurrentList<BinaryDataTransferResponse>();
            var csms_BinaryResponseMessagesSent             = new ConcurrentList<OCPP_BinaryResponseMessage>();

            csms.OCPP.OUT.OnBinaryDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms_BinaryDataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnBinaryResponseMessageSent      += (timestamp, sender, responseMessage, sendMessageResult) => {
                csms_BinaryResponseMessagesSent.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the BinaryDataTransfer response

            var ocppGateway_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppGateway_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppGateway.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBinaryDataTransferResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_BinaryDataTransferResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.FORWARD.OnBinaryDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppLocalController_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the BinaryDataTransfer response

            var ocppEnergyMeter_BinaryMessageResponseReceived             = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppEnergyMeter_BinaryDataTransferResponsesReceived       = new ConcurrentList<BinaryDataTransferResponse>();

            ocppEnergyMeter.OCPP.IN.OnBinaryResponseMessageReceived      += (timestamp, sender, responseMessage) => {
                ocppEnergyMeter_BinaryMessageResponseReceived.      TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnBinaryDataTransferResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var binaryDataTransferResponse  = await ocppEnergyMeter.TransferBinaryData(

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
                //Assert.That(ocppEnergyMeter_BinaryDataTransferRequestsSent.ElementAt(0).Signatures.Count,   Is.EqualTo(1));

            });

        }

        #endregion


        #region Authorize_RejectedByLC_1()

        /// <summary>
        /// An Authorize test, that should fail, as the Energy Meter is not allowed to send any Authorize requests!
        /// Thus this request will be rejected by the OCPP Local Controller and a RequestErrorMessage
        /// will be sent back to the Energy Meter.
        /// </summary>
        [Test]
        public async Task Authorize_RejectedByLC_1()
        {

            #region Initial checks

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                ocppEnergyMeter     is null)
            {

                Assert.Multiple(() => {

                    if (csms                is null)
                        Assert.Fail("The csms must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (ocppEnergyMeter     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The Authorize request leaves the Energy Meter

            var ocppEnergyMeter_AuthorizeRequestsSent          = new ConcurrentList<AuthorizeRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnAuthorizeRequestSent   += (timestamp, sender, authorizeRequest, sendMessageResult) => {
                ocppEnergyMeter_AuthorizeRequestsSent. TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_jsonRequestMessageSent.TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and rejects the Authorize request

            var ocppLocalController_jsonRequestMessageReceived              = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_AuthorizeRequestsReceived               = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_AuthorizeRequestsForwardingDecisions    = new ConcurrentList<ForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppLocalController_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_jsonRequestErrorMessageSent             = new ConcurrentList<OCPP_JSONRequestErrorMessage>();

            ocppLocalController.OCPP.IN.     OnJSONRequestMessageReceived  += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.          TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeRequestReceived    += (timestamp, sender, connection, authorizeRequest) => {
                ocppLocalController_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeRequestFiltered    += (timestamp, sender, connection, authorizeRequest, forwardingDecision) => {
                ocppLocalController_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, authorizeRequest, authorizeResponse, runtime) => {
                ocppLocalController_AuthorizeResponsesSent.              TryAdd(authorizeResponse);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestErrorMessageSent += (timestamp, sender, requestErrorMessage, sendMessageResult) => {
                ocppLocalController_jsonRequestErrorMessageSent.         TryAdd(requestErrorMessage);
                return Task.CompletedTask;
            };

            #endregion

            // -----------------------------

            #region 8. The Charging Station receives the Authorize response

            var ocppEnergyMeter_jsonRequestErrorMessageResponseReceived  = new ConcurrentList<OCPP_JSONRequestErrorMessage>();
            var ocppEnergyMeter_AuthorizeResponsesReceived               = new ConcurrentList<AuthorizeResponse>();

            ocppEnergyMeter.OCPP.IN.OnJSONRequestErrorMessageReceived   += (timestamp, sender, requestErrorMessage) => {
                ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.TryAdd(requestErrorMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnAuthorizeResponseReceived         += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_AuthorizeResponsesReceived.             TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


                                          // There is no "Authorize" extension, as this request is not legal for energy meters!
            var authorizeResponse = await ocppEnergyMeter.OCPP.OUT.Authorize(
                                              new AuthorizeRequest(

                                                  DestinationId:                 NetworkingNode_Id.CSMS,

                                                  IdToken:                       IdToken.TryParseRFID("1234567890")!,
                                                  Certificate:                   null,
                                                  ISO15118CertificateHashData:   null,
                                                  CustomData:                    null,

                                                  SignKeys:                      null,
                                                  SignInfos:                     null,
                                                  Signatures:                    null,

                                                  RequestId:                     null,
                                                  RequestTimestamp:              null,
                                                  RequestTimeout:                null,
                                                  EventTrackingId:               null,

                                                  NetworkPath:                   NetworkPath.From(ocppEnergyMeter.Id)

                                              )
                                          );

            Assert.Multiple(() => {

                Assert.That(authorizeResponse.IdTokenInfo.Status,                                           Is.EqualTo(AuthorizationStatus.RequestError));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(ocppEnergyMeter_AuthorizeRequestsSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_AuthorizeRequestsSent.First().Signatures.                        Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.                                          Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().Payload["signatures"]?.           Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                                  Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsReceived.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsForwardingDecisions.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.First().DestinationId,                         Is.EqualTo(ocppEnergyMeter.Id));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.First().NetworkPath.ToString(),                Is.EqualTo(new NetworkPath([ ocppLocalController.Id ]).ToString()));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.First().DestinationId,                    Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ ocppLocalController.Id ]).ToString()));

                // ----------------------

                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.First().DestinationId,            Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ ocppLocalController.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().Signatures.                   Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().DestinationId,                         Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().NetworkPath.ToString(),                Is.EqualTo(new NetworkPath([ ocppLocalController.Id ]).ToString()));

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

            if (csms                is null ||
                ocppGateway         is null ||
                ocppLocalController is null ||
                ocppEnergyMeter     is null)
            {

                Assert.Multiple(() => {

                    if (csms                is null)
                        Assert.Fail("The csms must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
                        Assert.Fail("The local controller must not be null!");

                    if (ocppEnergyMeter     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The MeterValues request leaves the Charging Station

            var ocppEnergyMeter_MeterValuesRequestsSent        = new ConcurrentList<MeterValuesRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppEnergyMeter.OCPP.OUT.OnMeterValuesRequestSent += (timestamp, sender, meterValuesRequest, sendMessageResult) => {
                ocppEnergyMeter_MeterValuesRequestsSent.TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppEnergyMeter_jsonRequestMessageSent. TryAdd(requestMessage);
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

            ocppLocalController.OCPP.FORWARD.OnMeterValuesRequestSent     += (timestamp, sender, meterValuesRequest, sendMessageResult) => {
                ocppLocalController_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, requestMessage, sendMessageResult) => {
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

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestReceived   += (timestamp, sender, connection, meterValuesRequest) => {
                ocppGateway_MeterValuesRequestsReceived.           TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestFiltered   += (timestamp, sender, connection, meterValuesRequest, forwardingDecision) => {
                ocppGateway_MeterValuesRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnMeterValuesRequestSent       += (timestamp, sender, meterValuesRequest, sendMessageResult) => {
                ocppGateway_MeterValuesRequestsSent.               TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.    OnJSONRequestMessageSent     += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the MeterValues request

            var csms_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_MeterValuesRequestsReceived        = new ConcurrentList<MeterValuesRequest>();

            csms.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, requestMessage) => {
                csms_jsonRequestMessageReceived.TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            csms.OCPP.IN. OnMeterValuesRequestReceived += (timestamp, sender, connection, request) => {
                csms_MeterValuesRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the MeterValues request

            var csms_MeterValuesResponsesSent        = new ConcurrentList<MeterValuesResponse>();
            var csms_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms.OCPP.OUT.OnMeterValuesResponseSent += (timestamp, sender, connection, request, response, runtime) => {
                csms_MeterValuesResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, responseMessage, sendMessageResult) => {
                csms_jsonResponseMessagesSent.TryAdd(responseMessage);
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

            ocppGateway.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, responseMessage, sendMessageResult) => {
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

            ocppLocalController.OCPP.OUT.    OnJSONResponseMessageSent     += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessagesSent.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the MeterValues response

            var ocppEnergyMeter_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_MeterValuesResponsesReceived       = new ConcurrentList<MeterValuesResponse>();

            ocppEnergyMeter.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, responseMessage) => {
                ocppEnergyMeter_jsonMessageResponseReceived. TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            ocppEnergyMeter.OCPP.IN.OnMeterValuesResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppEnergyMeter_MeterValuesResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var meterValuesResponse = await ocppEnergyMeter.SendMeterValues(

                                              EVSEId:             EVSE_Id.Parse(0),
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
                Assert.That(ocppEnergyMeter_MeterValuesRequestsSent.                                Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_MeterValuesRequestsSent.First().Signatures.             Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppEnergyMeter_jsonRequestMessageSent.           First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().Payload["signatures"]?.       Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                               Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsForwardingDecisions.              Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_MeterValuesRequestsSent.                             Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ ocppEnergyMeter.Id, ocppLocalController.Id, ocppGateway.Id]).ToString()));
                Assert.That(csms_MeterValuesRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_MeterValuesResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().DestinationId,              Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_MeterValuesResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_MeterValuesResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));

                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.                                 Count,    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().DestinationId,                     Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkPath.ToString(),            Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.Count,                                    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().DestinationId,                    Is.EqualTo(ocppEnergyMeter.Id));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ csms.Id, ocppGateway.Id, ocppLocalController.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().Signatures.Count,                 Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.ElementAt(0).Signatures.First().Status,   Is.EqualTo(VerificationStatus.ValidSignature));


            });

        }

        #endregion


    }

}
