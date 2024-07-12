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
using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.NN;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPP.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// OCPP v2.1 tests.
    /// </summary>
    [TestFixture]
    public class ChargingTestsReversed : AOCPPInfrastructureReversed
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


            #region 2. The OCPP Local Controller receives and forwards the BootNotification request

            var ocppLocalController_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppLocalController_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppLocalController_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppLocalController_jsonRequestMessageSent                       = new ConcurrentList<SendOCPPMessageResult>();

            ocppLocalController.OCPP.IN.     OnJSONMessageRequestReceived       += (timestamp, sender, requestMessage) => {
                ocppLocalController_jsonRequestMessageReceived.                 TryAdd(requestMessage);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestReceived  += (timestamp, sender, connection, bootNotificationRequest) => {
                ocppLocalController_BootNotificationRequestsReceived.           TryAdd(bootNotificationRequest);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.FORWARD.OnBootNotificationRequestFiltered   += (timestamp, sender, connection, bootNotificationRequest, forwardingDecision) => {
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

            #endregion

            #region 3. The OCPP Gateway receives and forwards the BootNotification request

            var ocppGateway_jsonRequestMessageReceived                   = new ConcurrentList<OCPP_JSONRequestMessage>();
            var ocppGateway_BootNotificationRequestsReceived             = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_BootNotificationRequestsForwardingDecisions  = new ConcurrentList<ForwardingDecision<BootNotificationRequest, BootNotificationResponse>>();
            var ocppGateway_BootNotificationRequestsSent                 = new ConcurrentList<BootNotificationRequest>();
            var ocppGateway_jsonRequestMessageSent                       = new ConcurrentList<SendOCPPMessageResult>();

            ocppGateway.OCPP.IN.     OnJSONMessageRequestReceived       += (timestamp, sender, requestMessage) => {
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

            ocppGateway.OCPP.FORWARD.OnJSONRequestMessageSent           += (timestamp, sender, requestMessage, sendMessageResult) => {
                ocppGateway_jsonRequestMessageSent.                     TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };

            #endregion

            #region 4. The CSMS receives the BootNotification request

            var csms_jsonRequestMessageReceived               = new ConcurrentList<OCPP_JSONRequestMessage>();
            var csms_BootNotificationRequestsReceived         = new ConcurrentList<BootNotificationRequest>();

            csms.OCPP.IN. OnJSONMessageRequestReceived       += (timestamp, sender, requestMessage) => {
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

            var csms_BootNotificationResponsesSent            = new ConcurrentList<BootNotificationResponse>();
            var csms_jsonResponseMessageSent                  = new ConcurrentList<SendOCPPMessageResult>();

            csms.OCPP.OUT.OnBootNotificationResponseSent  += (timestamp, sender, connection, request, response, runtime) => {
                csms_BootNotificationResponsesSent.TryAdd(response);
                return Task.CompletedTask;
            };

            //csms.OCPP.FORWARD.OnJSONResponseMessageSent      += (timestamp, sender, responseMessage, sendMessageResult) => {
            //    csms_jsonResponseMessageSent.         TryAdd(sendMessageResult);
            //    return Task.CompletedTask;
            //};

            csms.OCPP.OUT.OnJSONMessageResponseSent       += (timestamp, sender, responseMessage, sendMessageResult) => {
                csms_jsonResponseMessageSent.      TryAdd(sendMessageResult);

                if (responseMessage.DestinationId != chargingStation1.Id)
                {

                }

                return Task.CompletedTask;
            };

            #endregion

            #region 6. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppGateway_OnJSONMessageResponseReceived                = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppGateway_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppGateway_jsonResponseMessageSent                      = new ConcurrentList<SendOCPPMessageResult>();

            ocppGateway.OCPP.IN.OnJSONMessageResponseReceived           += (timestamp, sender, responseMessage) => {
                ocppGateway_OnJSONMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            //ocppGateway.OCPP.FORWARD.OnJSONRequestMessageSent .OnJSONMessageResponseReceived           += (timestamp, sender, responseMessage) => {
            //    ocppGateway_OnJSONMessageResponseReceived.    TryAdd(responseMessage);
            //    return Task.CompletedTask;
            //};

            ocppGateway.OCPP.FORWARD.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.FORWARD.OnBootNotificationResponseSent     += (timestamp, sender, connection, request, response, runtime) => {
                ocppGateway_BootNotificationResponsesSent.    TryAdd(response);
                return Task.CompletedTask;
            };



            ocppGateway.OCPP.FORWARD.OnJSONResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessageSent.          TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };

            ocppGateway.OCPP.OUT.OnJSONMessageResponseSent              += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppGateway_jsonResponseMessageSent.          TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };

            #endregion

            #region 7. The OCPP Local Controller receives and forwards the BootNotification response

            var ocppLocalController_OnJSONMessageResponseReceived                = new ConcurrentList<OCPP_JSONResponseMessage>();
            var ocppLocalController_BootNotificationResponsesReceived            = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_BootNotificationResponsesSent                = new ConcurrentList<BootNotificationResponse>();
            var ocppLocalController_jsonResponseMessageSent                      = new ConcurrentList<SendOCPPMessageResult>();

            ocppLocalController.OCPP.IN.OnJSONMessageResponseReceived           += (timestamp, sender, responseMessage) => {
                ocppLocalController_OnJSONMessageResponseReceived.    TryAdd(responseMessage);
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



            ocppLocalController.OCPP.FORWARD.OnJSONResponseMessageSent          += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessageSent.          TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };

            ocppLocalController.OCPP.OUT.OnJSONMessageResponseSent              += (timestamp, sender, responseMessage, sendMessageResult) => {
                ocppLocalController_jsonResponseMessageSent.          TryAdd(sendMessageResult);
                return Task.CompletedTask;
            };

            #endregion

            #region 8. The Charging Station receives the BootNotification response

            var chargingStation1_OnJSONMessageResponseReceived           = new ConcurrentList<OCPP_JSONResponseMessage>();
            var chargingStation1_BootNotificationResponsesReceived       = new ConcurrentList<BootNotificationResponse>();

            chargingStation1.OCPP.IN.OnJSONMessageResponseReceived      += (timestamp, sender, responseMessage) => {
                ocppGateway_OnJSONMessageResponseReceived.    TryAdd(responseMessage);
                return Task.CompletedTask;
            };

            chargingStation1.OCPP.IN.OnBootNotificationResponseReceived += (timestamp, sender, request, response, runtime) => {
                ocppGateway_BootNotificationResponsesReceived.TryAdd(response);
                return Task.CompletedTask;
            };

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


            Assert.That(ocppLocalController_jsonRequestMessageReceived.Count, Is.EqualTo(1));

            Assert.That(response1.Status, Is.EqualTo(RegistrationStatus.Accepted));


        }

        #endregion


    }

}
