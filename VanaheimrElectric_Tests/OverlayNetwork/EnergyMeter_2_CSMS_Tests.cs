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
using cloud.charging.open.protocols.WWCP.NetworkingNode;
using cloud.charging.open.protocols.WWCP;

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

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BootNotification request leaves the Energy Meter

            var ocppEnergyMeter_BootNotificationRequestsSent  = new ConcurrentList<BootNotificationRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent        = new ConcurrentList<OCPP_JSONRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnBootNotificationRequestSent += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_BootNotificationRequestsSent.TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnJSONRequestMessageSent      += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_jsonRequestMessageSent.      TryAdd(jsonRequestMessage);
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

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.                     TryAdd(jsonRequestMessage);
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

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFiltered   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision, ct) => {
                ocppGateway_BootNotificationRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestSent      += (timestamp, sender, connection, bootNotificationRequest, sentMessageResult, ct) => {
                ocppGateway_BootNotificationRequestsSent.               TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent           += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBootNotificationRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_BootNotificationRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BootNotification request

            var csms_BootNotificationResponsesSent        = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessagesSent             = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnBootNotificationResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent      += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the BootNotification response

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

            ocppGateway1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
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

            ocppLocalController1.OCPP.OUT.    OnJSONResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the BootNotification response

            var ocppEnergyMeter_jsonMessageResponseReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            gridEnergyMeter1.OCPP.IN.OnJSONResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppEnergyMeter_jsonMessageResponseReceived.    TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var bootNotificationResponse = await gridEnergyMeter1.SendBootNotification(

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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_BootNotificationRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms_BootNotificationRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_BootNotificationResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonResponseMessagesReceived.                                     Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesReceived.                                Count,   Is.EqualTo(1));
                //Assert.That(ocppGateway_BootNotificationResponsesSent.                                    Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.                                         Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonResponseMessagesSent.              First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id ]).ToString()));

                Assert.That(ocppLocalController_jsonResponseMessagesReceived.                             Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesReceived.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_BootNotificationResponsesSent.                            Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonResponseMessagesSent.      First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.Count,                                          Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkingMode,                         Is.EqualTo(NetworkingMode.OverlayNetwork));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().Destination.Next,                       Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkPath.ToString(),                 Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id, ocppLocalController1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.Count,                                    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().DestinationId,                    Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id, ocppLocalController1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.First().Signatures.Count,                 Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_BootNotificationResponsesReceived.ElementAt(0).Signatures.First().Status,   Is.EqualTo(VerificationStatus.ValidSignature));

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
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_jsonRequestMessageSent.  TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms_jsonRequestMessageReceived           = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_DataTransferRequestsReceived         = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnDataTransferRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_DataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the DataTransfer request

            var csms_DataTransferResponsesSent        = new ConcurrentList<DataTransferResponse>();
            var csms_jsonResponseMessagesSent         = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppGateway_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

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

            var ocppLocalController_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

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

            var ocppEnergyMeter_jsonMessageResponseReceived         = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            gridEnergyMeter1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppEnergyMeter_jsonMessageResponseReceived.  TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await gridEnergyMeter1.TransferData(

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

            var dataTransferResponse2  = await gridEnergyMeter1.TransferData(

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

            var dataTransferResponse3  = await gridEnergyMeter1.TransferData(

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
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The DataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_DataTransferRequestsSent  = new ConcurrentList<DataTransferRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent    = new ConcurrentList<OCPP_JSONRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_DataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnJSONRequestMessageSent  += (timestamp, sender, connection, requestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_jsonRequestMessageSent.  TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the DataTransfer request

            var ocppLocalController_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_DataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppLocalController_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppLocalController_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest, ct) => {
                ocppLocalController_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision, ct) => {
                ocppLocalController_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppLocalController_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestMessageSent.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the DataTransfer request

            var ocppGateway_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_DataTransferRequestsReceived             = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_DataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<DataTransferRequest, DataTransferResponse>>();
            var ocppGateway_DataTransferRequestsSent                 = new ConcurrentList<DataTransferRequest>();
            var ocppGateway_jsonRequestMessageSent                   = new ConcurrentList<OCPP_JSONRequestMessage>();

            ocppGateway1.OCPP.IN.     OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_jsonRequestMessageReceived.             TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestReceived  += (timestamp, sender, connection, dataTransferRequest, ct) => {
                ocppGateway_DataTransferRequestsReceived.           TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestFiltered  += (timestamp, sender, connection, dataTransferRequest, forwardingDecision, ct) => {
                ocppGateway_DataTransferRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.FORWARD.OnDataTransferRequestSent      += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppGateway_DataTransferRequestsSent.               TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            ocppGateway1.OCPP.OUT.    OnJSONRequestMessageSent       += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_jsonRequestMessageSent.                 TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the DataTransfer request

            var csms_jsonRequestMessageReceived           = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_DataTransferRequestsReceived         = new ConcurrentList<DataTransferRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived   += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnDataTransferRequestReceived  += (timestamp, sender, connection, request, ct) => {
                csms_DataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the DataTransfer request

            var csms_DataTransferResponsesSent        = new ConcurrentList<DataTransferResponse>();
            var csms_jsonResponseMessagesSent         = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_DataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent  += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the DataTransfer response

            var ocppGateway_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppGateway_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

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

            var ocppLocalController_jsonResponseMessagesReceived             = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_DataTransferResponsesReceived            = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_DataTransferResponsesSent                = new ConcurrentList<DataTransferResponse>();
            var ocppLocalController_jsonResponseMessagesSent                 = new ConcurrentList<OCPP_JSONResponseMessage>();

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

            var ocppEnergyMeter_jsonMessageResponseReceived         = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_DataTransferResponsesReceived       = new ConcurrentList<DataTransferResponse>();

            gridEnergyMeter1.OCPP.IN.OnJSONResponseMessageReceived  += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppEnergyMeter_jsonMessageResponseReceived.  TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_DataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var dataTransferResponse1  = await gridEnergyMeter1.TransferData(

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

            var dataTransferResponse2  = await gridEnergyMeter1.TransferData(

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

            var dataTransferResponse3  = await gridEnergyMeter1.TransferData(

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
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The BinaryDataTransfer request leaves the Energy Meter

            var ocppEnergyMeter_BinaryDataTransferRequestsSent        = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppEnergyMeter_BinaryRequestMessageSent              = new ConcurrentList<OCPP_BinaryRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnBinaryDataTransferRequestSent += (timestamp, sender, connection, dataTransferRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_BinaryDataTransferRequestsSent.TryAdd(dataTransferRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnBinaryRequestMessageSent      += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_BinaryRequestMessageSent.        TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the BinaryDataTransfer request

            var ocppLocalController_BinaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppLocalController_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppLocalController_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppLocalController_BinaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_BinaryRequestMessageReceived.                   TryAdd(jsonRequestMessage);
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

            ocppLocalController1.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppLocalController_BinaryRequestMessageSent.                       TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BinaryDataTransfer request

            var ocppGateway_binaryRequestMessageReceived                   = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var ocppGateway_BinaryDataTransferRequestsReceived             = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_BinaryDataTransferRequestsForwardingDecisions  = new ConcurrentList<RequestForwardingDecision<BinaryDataTransferRequest, BinaryDataTransferResponse>>();
            var ocppGateway_BinaryDataTransferRequestsSent                 = new ConcurrentList<BinaryDataTransferRequest>();
            var ocppGateway_binaryRequestMessageSent                       = new ConcurrentList<OCPP_BinaryRequestMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryRequestMessageReceived       += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppGateway_binaryRequestMessageReceived.                   TryAdd(jsonRequestMessage);
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

            ocppGateway1.OCPP.OUT.    OnBinaryRequestMessageSent           += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppGateway_binaryRequestMessageSent.                       TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BinaryDataTransfer request

            var csms_BinaryRequestMessageReceived              = new ConcurrentList<OCPP_BinaryRequestMessage>();
            var csms_BinaryDataTransferRequestsReceived        = new ConcurrentList<BinaryDataTransferRequest>();

            csms1.OCPP.IN. OnBinaryRequestMessageReceived      += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_BinaryRequestMessageReceived.      TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnBinaryDataTransferRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms_BinaryDataTransferRequestsReceived.TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the BinaryDataTransfer request

            var csms_BinaryDataTransferResponsesSent        = new ConcurrentList<BinaryDataTransferResponse>();
            var csms_BinaryResponseMessagesSent             = new ConcurrentList<OCPP_BinaryResponseMessage>();

            csms1.OCPP.OUT.OnBinaryDataTransferResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_BinaryDataTransferResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnBinaryResponseMessageSent      += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms_BinaryResponseMessagesSent.     TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the BinaryDataTransfer response

            var ocppGateway_binaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppGateway_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppGateway_binaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppGateway1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppGateway_binaryResponseMessagesReceived.     TryAdd(jsonResponseMessage);
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

            ocppGateway1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppGateway_binaryResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BinaryDataTransfer response

            var ocppLocalController_BinaryResponseMessagesReceived                 = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppLocalController_BinaryDataTransferResponsesReceived            = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryDataTransferResponsesSent                = new ConcurrentList<BinaryDataTransferResponse>();
            var ocppLocalController_BinaryResponseMessagesSent                     = new ConcurrentList<OCPP_BinaryResponseMessage>();

            ocppLocalController1.OCPP.IN.     OnBinaryResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppLocalController_BinaryResponseMessagesReceived.     TryAdd(jsonResponseMessage);
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

            ocppLocalController1.OCPP.OUT.    OnBinaryResponseMessageSent          += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                ocppLocalController_BinaryResponseMessagesSent.         TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Energy Meter receives the BinaryDataTransfer response

            var ocppEnergyMeter_BinaryMessageResponseReceived             = new ConcurrentList<OCPP_BinaryResponseMessage>();
            var ocppEnergyMeter_BinaryDataTransferResponsesReceived       = new ConcurrentList<BinaryDataTransferResponse>();

            gridEnergyMeter1.OCPP.IN.OnBinaryResponseMessageReceived      += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppEnergyMeter_BinaryMessageResponseReceived.      TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnBinaryDataTransferResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_BinaryDataTransferResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var binaryDataTransferResponse  = await gridEnergyMeter1.TransferBinaryData(

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

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The Authorize request leaves the Energy Meter

            var ocppEnergyMeter_AuthorizeRequestsSent          = new ConcurrentList<AuthorizeRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnAuthorizeRequestSent   += (timestamp, sender, connection, authorizeRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_AuthorizeRequestsSent. TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_jsonRequestMessageSent.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and rejects the Authorize request

            var ocppLocalController_jsonRequestMessageReceived              = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_AuthorizeRequestsReceived               = new ConcurrentList<AuthorizeRequest>();
            var ocppLocalController_AuthorizeRequestsForwardingDecisions    = new ConcurrentList<RequestForwardingDecision<AuthorizeRequest, AuthorizeResponse>>();
            var ocppLocalController_jsonRequestsForwardingDecisions         = new ConcurrentList<RequestForwardingDecision>();
            var ocppLocalController_AuthorizeResponsesSent                  = new ConcurrentList<AuthorizeResponse>();
            var ocppLocalController_jsonRequestErrorMessageSent             = new ConcurrentList<OCPP_JSONRequestErrorMessage>();

            ocppLocalController1.OCPP.IN.     OnJSONRequestMessageReceived  += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                ocppLocalController_jsonRequestMessageReceived.          TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeRequestReceived    += (timestamp, sender, connection, authorizeRequest, ct) => {
                ocppLocalController_AuthorizeRequestsReceived.           TryAdd(authorizeRequest);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeRequestFiltered    += (timestamp, sender, connection, authorizeRequest, forwardingDecision, ct) => {
                ocppLocalController_AuthorizeRequestsForwardingDecisions.TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAnyJSONRequestFiltered      += (timestamp, sender, connection, jsonRequest, forwardingDecision, ct) => {
                ocppLocalController_jsonRequestsForwardingDecisions.     TryAdd(forwardingDecision);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.FORWARD.OnAuthorizeResponseSent       += (timestamp, sender, connection, authorizeRequest, authorizeResponse, runtime, sentMessageResult, ct) => {
                ocppLocalController_AuthorizeResponsesSent.              TryAdd(authorizeResponse);
                return Task.CompletedTask;
            };

            ocppLocalController1.OCPP.OUT.    OnJSONRequestErrorMessageSent += (timestamp, sender, connection, requestErrorMessage, sentMessageResult, ct) => {
                ocppLocalController_jsonRequestErrorMessageSent.         TryAdd(requestErrorMessage);
                return Task.CompletedTask;
            };

            #endregion

            // -----------------------------

            #region 8. The Charging Station receives the Authorize response

            var ocppEnergyMeter_jsonRequestErrorMessageResponseReceived  = new ConcurrentList<OCPP_JSONRequestErrorMessage>();
            var ocppEnergyMeter_AuthorizeResponsesReceived               = new ConcurrentList<AuthorizeResponse>();

            gridEnergyMeter1.OCPP.IN.OnJSONRequestErrorMessageReceived   += (timestamp, sender, connection, requestErrorMessage, ct) => {
                ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.TryAdd(requestErrorMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnAuthorizeResponseReceived         += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_AuthorizeResponsesReceived.             TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


                                          // There is no "Authorize" extension, as this request is not legal for energy meters!
            var authorizeResponse = await gridEnergyMeter1.OCPP.OUT.Authorize(
                                              new AuthorizeRequest(

                                                  Destination:                   SourceRouting.CSMS,

                                                  IdToken:                       IdToken.TryParseRFID("01-23-45-67-89-AB-CD")!,
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

                                                  NetworkPath:                   NetworkPath.From(gridEnergyMeter1.Id)

                                              )
                                          );

            Assert.Multiple(() => {

                Assert.That(authorizeResponse.IdTokenInfo.Status,                                           Is.EqualTo(AuthorizationStatus.Error));


                // -<request>--------------------------------------------------------------------------------------------------
                Assert.That(ocppEnergyMeter_AuthorizeRequestsSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_AuthorizeRequestsSent.First().Signatures.                        Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.                                          Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_jsonRequestMessageSent.First().Payload["signatures"]?.           Count(), Is.EqualTo(1));

                Assert.That(ocppLocalController_jsonRequestMessageReceived.                                  Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsReceived.                                   Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_AuthorizeRequestsForwardingDecisions.                        Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.First().DestinationId,                         Is.EqualTo(ocppEnergyMeter.Id));
                //Assert.That(ocppLocalController_AuthorizeResponsesSent.First().NetworkPath.ToString(),                Is.EqualTo(new NetworkPath([ ocppLocalController.Id ]).ToString()));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.First().Destination.Next,                 Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppLocalController_jsonRequestErrorMessageSent.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ ocppLocalController1.Id ]).ToString()));

                // ----------------------

                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.                         Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.First().Destination.Next,         Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_jsonRequestErrorMessageResponseReceived.First().NetworkPath.ToString(),   Is.EqualTo(new NetworkPath([ ocppLocalController1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.                                      Count,   Is.EqualTo(1));
                //Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().Signatures.                   Count,   Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().DestinationId,                         Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_AuthorizeResponsesReceived.First().NetworkPath.ToString(),                Is.EqualTo(new NetworkPath([ ocppLocalController1.Id ]).ToString()));

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
                gridEnergyMeter1     is null)
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

                    if (gridEnergyMeter1     is null)
                        Assert.Fail("The energy meter must not be null!");

                });

                return;

            }

            #endregion


            #region 1. The MeterValues request leaves the Charging Station

            var ocppEnergyMeter_MeterValuesRequestsSent        = new ConcurrentList<MeterValuesRequest>();
            var ocppEnergyMeter_jsonRequestMessageSent         = new ConcurrentList<OCPP_JSONRequestMessage>();

            gridEnergyMeter1.OCPP.OUT.OnMeterValuesRequestSent += (timestamp, sender, connection, meterValuesRequest, sentMessageResult, ct) => {
                ocppEnergyMeter_MeterValuesRequestsSent.TryAdd(meterValuesRequest);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.OUT.OnJSONRequestMessageSent += (timestamp, sender, connection, jsonRequestMessage, sentMessageResult, ct) => {
                ocppEnergyMeter_jsonRequestMessageSent. TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            #endregion

            #region 2. The OCPP Local Controller receives and forwards the MeterValues request

            var ocppLocalController_jsonRequestMessageReceived             = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_MeterValuesRequestsReceived            = new ConcurrentList<MeterValuesRequest>();
            var ocppLocalController_MeterValuesRequestsForwardingDecisions = new ConcurrentList<RequestForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
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
            var ocppGateway_MeterValuesRequestsForwardingDecisions = new ConcurrentList<RequestForwardingDecision<MeterValuesRequest, MeterValuesResponse>>();
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

            var csms_jsonRequestMessageReceived         = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_MeterValuesRequestsReceived        = new ConcurrentList<MeterValuesRequest>();

            csms1.OCPP.IN. OnJSONRequestMessageReceived += (timestamp, sender, connection, jsonRequestMessage, ct) => {
                csms_jsonRequestMessageReceived.TryAdd(jsonRequestMessage);
                return Task.CompletedTask;
            };

            csms1.OCPP.IN. OnMeterValuesRequestReceived += (timestamp, sender, connection, request, ct) => {
                csms_MeterValuesRequestsReceived. TryAdd(request);
                return Task.CompletedTask;
            };

            #endregion

            // processing...

            #region 5. The CSMS responds the MeterValues request

            var csms_MeterValuesResponsesSent        = new ConcurrentList<MeterValuesResponse>();
            var csms_jsonResponseMessagesSent        = new ConcurrentList<OCPP_JSONResponseMessage>();

            csms1.OCPP.OUT.OnMeterValuesResponseSent += (timestamp, sender, connection, request, response, runtime, sentMessageResult, ct) => {
                csms_MeterValuesResponsesSent.  TryAdd(response);
                return Task.CompletedTask;
            };

            csms1.OCPP.OUT.OnJSONResponseMessageSent += (timestamp, sender, connection, jsonResponseMessage, sentMessageResult, ct) => {
                csms_jsonResponseMessagesSent.TryAdd(jsonResponseMessage);
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

            var ocppEnergyMeter_jsonMessageResponseReceived        = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppEnergyMeter_MeterValuesResponsesReceived       = new ConcurrentList<MeterValuesResponse>();

            gridEnergyMeter1.OCPP.IN.OnJSONResponseMessageReceived += (timestamp, sender, connection, jsonResponseMessage, ct) => {
                ocppEnergyMeter_jsonMessageResponseReceived. TryAdd(jsonResponseMessage);
                return Task.CompletedTask;
            };

            gridEnergyMeter1.OCPP.IN.OnMeterValuesResponseReceived += (timestamp, sender, connection, request, response, runtime, ct) => {
                ocppEnergyMeter_MeterValuesResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            #endregion


            var meterValuesResponse = await gridEnergyMeter1.SendMeterValues(

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
                Assert.That(ocppLocalController_jsonRequestMessageSent.        First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id ]).ToString()));

                Assert.That(ocppGateway_jsonRequestMessageReceived.                                       Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsReceived.                                 Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsForwardingDecisions.                      Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_MeterValuesRequestsSent.                                     Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                                           Count,   Is.EqualTo(1));
                Assert.That(ocppGateway_jsonRequestMessageSent.                First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));

                Assert.That(csms_jsonRequestMessageReceived.                                              Count,   Is.EqualTo(1));
                Assert.That(csms_jsonRequestMessageReceived.                   First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ gridEnergyMeter1.Id, ocppLocalController1.Id, ocppGateway1.Id]).ToString()));
                Assert.That(csms_MeterValuesRequestsReceived.                                        Count,   Is.EqualTo(1));

                // -<response>-------------------------------------------------------------------------------------------------
                Assert.That(csms_MeterValuesResponsesSent.                                           Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                                                Count,   Is.EqualTo(1));
                Assert.That(csms_jsonResponseMessagesSent.                     First().Destination.Next,           Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(csms_jsonResponseMessagesSent.                     First().NetworkPath.ToString(),     Is.EqualTo(new NetworkPath([ csms1.Id ]).ToString()));

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

                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.                                 Count,    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().Destination.Next,                  Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_jsonMessageResponseReceived.First().NetworkPath.ToString(),            Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id, ocppLocalController1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.Count,                                    Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().DestinationId,                    Is.EqualTo(gridEnergyMeter1.Id));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().NetworkPath.ToString(),           Is.EqualTo(new NetworkPath([ csms1.Id, ocppGateway1.Id, ocppLocalController1.Id ]).ToString()));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.First().Signatures.Count,                 Is.EqualTo(1));
                Assert.That(ocppEnergyMeter_MeterValuesResponsesReceived.ElementAt(0).Signatures.First().Status,   Is.EqualTo(VerificationStatus.ValidSignature));


            });

        }

        #endregion


    }

}
