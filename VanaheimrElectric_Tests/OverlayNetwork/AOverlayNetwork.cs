﻿/*
 * Copyright (c) 2015-2024 GraphDefined GmbH
 * This file is part of WWCP OCPI <https://github.com/OpenChargingCloud/WWCP_OCPI>
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
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

using cloud.charging.open.protocols.OCPP;
using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.Gateway;
using cloud.charging.open.protocols.OCPPv2_1.EnergyMeter;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;
using cloud.charging.open.protocols.OCPPv2_1.LocalController;
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.EMP;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Charging infrastructure test defaults using an OCPP Overlay Network
    /// consisting of three Charging Stations, an OCPP Local Controller, an
    /// Energy Meter at the shared grid connection point, an OCPP Gateway
    /// and two Charging Station Management Systems.
    /// 
    /// The HTTP Web Socket connections are initiated in "normal order" from
    /// the Charging Stations to the Local Controller, to the Gateway and
    /// finally to the CSMS.
    /// 
    /// Between the Charging Stations and the Local Controller the "normal"
    /// OCPP transport JSON array is used. Between the Local Controller and
    /// the Gateway, between the Local Controller and the Energy Meter, and
    /// between the Gateway and the CSMS the OCPP Overlay Network transport
    /// is used.
    /// 
    /// Both CSMSs have an internal WWCP Roaming Network and an internal
    /// E-Mobility Service Provider (iEMP).
    /// 
    /// [cs1] ──⭨                   🡵 [csms1, RN1] ━━━► [iEMP1]
    /// [cs2] ───→ [lc] ━━━► [gw] ━━━► [csms2, RN2] ━━━► [iEMP2]
    /// [cs3] ──🡕    🡴━ [em]
    /// </summary>
    public abstract class AOverlayNetwork
    {

        #region Data

        public String                       RFIDUID1  = "11-22-33-44-55-66-77";
        public String                       RFIDUID2  = "AA-BB-CC-55-DD-EE-FF";

        public TestCSMSNode?                csms1;
        public IPPort                       csms1_tcpPort                               = IPPort.Parse(5001);
        public OCPPWebSocketServer?         csms1_OCPPWebSocketServer;
        public KeyPair?                     csms1_keyPair;
        public RoamingNetwork?              csms1_roamingNetwork;
        public IChargingStationOperator?    csms1_cso;
        public IEMobilityProvider?          csms1_emp;
        public EMobilityServiceProvider?    csms1_remoteEMP;

        public TestCSMSNode?                csms2;
        public IPPort                       csms2_tcpPort                               = IPPort.Parse(5002);
        public OCPPWebSocketServer?         csms2_OCPPWebSocketServer;
        public KeyPair?                     csms2_keyPair;
        public RoamingNetwork?              csms2_roamingNetwork;
        public IChargingStationOperator?    csms2_cso;
        public EMobilityServiceProvider?    csms2_emp;

        public TestGatewayNode?             ocppGateway;
        public IPPort                       ocppGateway_tcpPort                         = IPPort.Parse(5011);
        public OCPPWebSocketServer?         ocppGateway_OCPPWebSocketServer;
        public KeyPair?                     ocppGateway_keyPair;

        public TestLocalControllerNode?     ocppLocalController;
        public IPPort                       ocppLocalController_tcpPort                 = IPPort.Parse(5021);
        public OCPPWebSocketServer?         ocppLocalController_OCPPWebSocketServer;
        public KeyPair?                     ocppLocalController_keyPair;

        public TestEnergyMeterNode?         ocppEnergyMeter;
        public IPPort                       ocppEnergyMeter_tcpPort                     = IPPort.Parse(5031);
        public OCPPWebSocketServer?         ocppEnergyMeter_OCPPWebSocketServer;
        public KeyPair?                     ocppEnergyMeter_keyPair;

        public TestChargingStationNode?     chargingStation1;
        public KeyPair?                     chargingStation1_keyPair;
        public IChargingPool?               p1;
        public IChargingStation?            s1;
        public IEVSE?                       e1;

        public TestChargingStationNode?     chargingStation2;
        public KeyPair?                     chargingStation2_keyPair;

        public TestChargingStationNode?     chargingStation3;
        public KeyPair?                     chargingStation3_keyPair;

        public DNSClient                    DNSClient;

        #endregion

        #region Constructor(s)

        public AOverlayNetwork()
        {

            this.DNSClient           = new();

        }

        #endregion


        #region SetupOnce()

        [OneTimeSetUp]
        public async Task SetupOnce()
        {

            var notBefore = Timestamp.Now - TimeSpan.FromDays(1);
            var notAfter  = notBefore     + TimeSpan.FromDays(365);

            #region Setup Charging Station Management System 1

            csms1 = new TestCSMSNode(

                        Id:                          NetworkingNode_Id.Parse("csms1"),
                        VendorName:                  "GraphDefined",
                        Model:                       "vcsms1",
                        Description:                 I18NString.Create(Languages.en, "Charging Station Management System #1 for testing"),

                        SignaturePolicy:             null,
                        ForwardingSignaturePolicy:   null,

                        HTTPUploadPort:              null,
                        HTTPDownloadPort:            null,

                        DisableSendHeartbeats:       true,
                        SendHeartbeatsEvery:         null,
                        DefaultRequestTimeout:       null,

                        DisableMaintenanceTasks:     false,
                        MaintenanceEvery:            null,
                        DNSClient:                   DNSClient

                    );

            csms1_OCPPWebSocketServer = csms1.AttachWebSocketServer(

                                            HTTPServiceName:              null,
                                            IPAddress:                    null,
                                            TCPPort:                      csms1_tcpPort,
                                            Description:                  null,

                                            RequireAuthentication:        true,
                                            DisableWebSocketPings:        false,
                                            WebSocketPingEvery:           null,
                                            SlowNetworkSimulationDelay:   null,

                                            ServerCertificateSelector:    null,
                                            ClientCertificateValidator:   null,
                                            LocalCertificateSelector:     null,
                                            AllowedTLSProtocols:          null,
                                            ClientCertificateRequired:    null,
                                            CheckCertificateRevocation:   null,

                                            ServerThreadNameCreator:      null,
                                            ServerThreadPrioritySetter:   null,
                                            ServerThreadIsBackground:     null,
                                            ConnectionIdBuilder:          null,
                                            ConnectionTimeout:            null,
                                            MaxClientConnections:         null,

                                            AutoStart:                    true

                                        );

            #region Define signature policy

            csms1_keyPair = KeyPair.GenerateKeys()!;

            csms1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                           KeyPair:                csms1_keyPair!,
                                                           UserIdGenerator:        (signableMessage) => "cs001",
                                                           DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Charging Station Management System #1!"),
                                                           TimestampGenerator:     (signableMessage) => Timestamp.Now);

            csms1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                           VerificationRuleActions.VerifyAll);

            #endregion

            csms1_roamingNetwork          = new RoamingNetwork(
                                                RoamingNetwork_Id.Parse("PROD"),
                                                I18NString.Create("Default EV Roaming Network")
                                            );

            var csms1_addCSOResult        = await csms1_roamingNetwork.CreateChargingStationOperator(
                                                      Id:                                     ChargingStationOperator_Id.Parse("DE*GEF"),
                                                      Name:                                   I18NString.Create("GraphDefined CSO"),
                                                      Description:                            I18NString.Create("GraphDefined CSO Node 1")
                                                      //RemoteChargingStationOperatorCreator:   cso => new ChargingStationOperatorAdapter(csms1, cso)
                                                  );

            csms1_cso = csms1_addCSOResult.Entity!;





            csms1.OCPP.IN.RemoveAllEventHandlers(nameof(csms1.OCPP.IN.OnAuthorize));

            csms1.OCPP.IN.OnAuthorize += async (timestamp, sender, connection, authorizeRequest, ct) => {

                var cs               = authorizeRequest.NetworkPath.Source;

                var authStartResult  = await csms1_roamingNetwork.AuthorizeStart(
                                                 LocalAuthentication:   LocalAuthentication.FromAuthToken(
                                                                            AuthenticationToken.Parse(
                                                                                authorizeRequest.IdToken.Value
                                                                            )
                                                                        ),
                                                 ChargingLocation:      null,
                                                 ChargingProduct:       null,
                                                 SessionId:             null,
                                                 CPOPartnerSessionId:   null,
                                                 OperatorId:            csms1_cso.Id,

                                                 RequestTimestamp:      timestamp,
                                                 EventTrackingId:       authorizeRequest.EventTrackingId,
                                                 RequestTimeout:        null,
                                                 CancellationToken:     ct
                                             );

                return authStartResult.Result switch {

                    #region Authorized

                    AuthStartResultTypes.Authorized
                        => new AuthorizeResponse(
                               Request:                 authorizeRequest,
                               IdTokenInfo:             new IdTokenInfo(
                                                            Status:                AuthorizationStatus.Accepted,
                                                            ChargingPriority:      null,
                                                            CacheExpiryDateTime:   null,
                                                            ValidEVSEIds:          null,
                                                            HasChargingTariff:     null,
                                                            GroupIdToken:          null,
                                                            Language1:             null,
                                                            Language2:             null,
                                                            PersonalMessage:       null,
                                                            CustomData:            null
                                                        ),
                               CertificateStatus:       null,
                               AllowedEnergyTransfer:   null,
                               TransactionLimits:       null,
                               ResponseTimestamp:       authStartResult.ResponseTimestamp,

                               DestinationId:           authorizeRequest.NetworkPath.Source,
                               NetworkPath:             NetworkPath.Empty,

                               SignKeys:                null,
                               SignInfos:               null,
                               Signatures:              null,

                               CustomData:              null
                           ),

                    #endregion

                    #region default

                    _ => new AuthorizeResponse(
                             Request:                   authorizeRequest,
                             IdTokenInfo:               new IdTokenInfo(
                                                            AuthorizationStatus.Invalid
                                                        ),
                             CertificateStatus:         null,
                             AllowedEnergyTransfer:     null,
                             TransactionLimits:         null,
                             ResponseTimestamp:         authStartResult.ResponseTimestamp,

                             DestinationId:             authorizeRequest.NetworkPath.Source,
                             NetworkPath:               NetworkPath.Empty,

                             SignKeys:                  null,
                             SignInfos:                 null,
                             Signatures:                null,

                             CustomData:                null
                         )

                    #endregion

                };

            };


            var csms1_addEMPResult1   = await csms1_roamingNetwork.CreateEMobilityServiceProvider(
                                                  Id:            EMobilityProvider_Id.Parse("DE-GDF"),
                                                  Name:          I18NString.Create("GraphDefined EMP"),
                                                  Description:   I18NString.Create("GraphDefined EMP Node 1")
                                              );

            csms1_emp                 = csms1_addEMPResult1.Entity!;
            csms1_remoteEMP           = csms1_emp.RemoteEMobilityProvider as EMobilityServiceProvider;

            csms1_remoteEMP?.AddToken(
                LocalAuthentication.FromAuthToken(
                    AuthenticationToken.ParseHEX(RFIDUID1)
                ),
                TokenAuthorizationResultType.Authorized
            );

            #endregion

            #region Setup Charging Station Management System 2

            csms2 = new TestCSMSNode(

                        Id:                          NetworkingNode_Id.Parse("csms2"),
                        VendorName:                  "GraphDefined",
                        Model:                       "vcsms21",
                        Description:                 I18NString.Create(Languages.en, "Charging Station Management System #2 for testing"),

                        SignaturePolicy:             null,
                        ForwardingSignaturePolicy:   null,

                        HTTPUploadPort:              null,
                        HTTPDownloadPort:            null,

                        DisableSendHeartbeats:       true,
                        SendHeartbeatsEvery:         null,
                        DefaultRequestTimeout:       null,

                        DisableMaintenanceTasks:     false,
                        MaintenanceEvery:            null,
                        DNSClient:                   DNSClient

                    );

            csms2_OCPPWebSocketServer = csms2.AttachWebSocketServer(

                                            HTTPServiceName:              null,
                                            IPAddress:                    null,
                                            TCPPort:                      csms2_tcpPort,
                                            Description:                  null,

                                            RequireAuthentication:        true,
                                            DisableWebSocketPings:        false,
                                            WebSocketPingEvery:           null,
                                            SlowNetworkSimulationDelay:   null,

                                            ServerCertificateSelector:    null,
                                            ClientCertificateValidator:   null,
                                            LocalCertificateSelector:     null,
                                            AllowedTLSProtocols:          null,
                                            ClientCertificateRequired:    null,
                                            CheckCertificateRevocation:   null,

                                            ServerThreadNameCreator:      null,
                                            ServerThreadPrioritySetter:   null,
                                            ServerThreadIsBackground:     null,
                                            ConnectionIdBuilder:          null,
                                            ConnectionTimeout:            null,
                                            MaxClientConnections:         null,

                                            AutoStart:                    true

                                        );

            #region Define signature policy

            csms2_keyPair = KeyPair.GenerateKeys()!;

            csms2.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                           KeyPair:                csms2_keyPair!,
                                                           UserIdGenerator:        (signableMessage) => "cs002",
                                                           DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Charging Station Management System #2!"),
                                                           TimestampGenerator:     (signableMessage) => Timestamp.Now);

            csms2.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                           VerificationRuleActions.VerifyAll);

            #endregion

            csms2_roamingNetwork      = new RoamingNetwork(
                                            RoamingNetwork_Id.Parse("PROD"),
                                            I18NString.Create("Default EV Roaming Network")
                                        );

            var csms2_addCSOResult    = await csms2_roamingNetwork.CreateChargingStationOperator(
                                                  ChargingStationOperator_Id.Parse("DE*GEF"),
                                                  I18NString.Create("GraphDefined CSO"),
                                                  I18NString.Create("GraphDefined CSO Node 2")
                                              );

            csms2_cso = csms2_addCSOResult.Entity;

            #endregion


            #region Setup Gateway

            ocppGateway                      = new TestGatewayNode(

                                                   Id:                          NetworkingNode_Id.Parse("gw"),
                                                   VendorName:                  "GraphDefined",
                                                   Model:                       "vgw1",
                                                   Description:                 I18NString.Create(Languages.en, "An OCPP Gateway for testing"),

                                                   SignaturePolicy:             null,
                                                   ForwardingSignaturePolicy:   null,

                                                   DisableSendHeartbeats:       true,
                                                   SendHeartbeatsEvery:         null,
                                                   DefaultRequestTimeout:       null,

                                                   DisableMaintenanceTasks:     false,
                                                   MaintenanceEvery:            null,
                                                   DNSClient:                   DNSClient

                                               );

            #region Connect to CSMS1

            var ocppGatewayAuth1             = csms1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                             ocppGateway.Id,
                                                                             "gw2csms1_12345678"
                                                                         );

            var ocppGatewayConnectResult1    = await ocppGateway.ConnectWebSocketClient(

                                                   RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms1_tcpPort}"),
                                                   VirtualHostname:              null,
                                                   Description:                  null,
                                                   PreferIPv4:                   null,
                                                   RemoteCertificateValidator:   null,
                                                   LocalCertificateSelector:     null,
                                                   ClientCert:                   null,
                                                   TLSProtocol:                  null,
                                                   HTTPUserAgent:                null,
                                                   HTTPAuthentication:           ocppGatewayAuth1,
                                                   RequestTimeout:               null,
                                                   TransmissionRetryDelay:       null,
                                                   MaxNumberOfRetries:           3,
                                                   InternalBufferSize:           null,

                                                   SecWebSocketProtocols:        null,
                                                   NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                   NextHopNetworkingNodeId:      csms1.Id,

                                                   DisableWebSocketPings:        false,
                                                   WebSocketPingEvery:           null,
                                                   SlowNetworkSimulationDelay:   null,

                                                   DisableMaintenanceTasks:      false,
                                                   MaintenanceEvery:             null,

                                                   LoggingPath:                  null,
                                                   LoggingContext:               String.Empty,
                                                   LogfileCreator:               null,
                                                   HTTPLogger:                   null,
                                                   DNSClient:                    null

                                               );

            Assert.That(ocppGatewayConnectResult1.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Gateway could not connect to CSMS #1: {ocppGatewayConnectResult1.HTTPStatusCode}");

            #endregion

            #region Connect to CSMS2

            var ocppGatewayAuth2             = csms2_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                             ocppGateway.Id,
                                                                             "gw2csms2_12345678"
                                                                         );

            var ocppGatewayConnectResult2    = await ocppGateway.ConnectWebSocketClient(

                                                   RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms2_tcpPort}"),
                                                   VirtualHostname:              null,
                                                   Description:                  null,
                                                   PreferIPv4:                   null,
                                                   RemoteCertificateValidator:   null,
                                                   LocalCertificateSelector:     null,
                                                   ClientCert:                   null,
                                                   TLSProtocol:                  null,
                                                   HTTPUserAgent:                null,
                                                   HTTPAuthentication:           ocppGatewayAuth2,
                                                   RequestTimeout:               null,
                                                   TransmissionRetryDelay:       null,
                                                   MaxNumberOfRetries:           3,
                                                   InternalBufferSize:           null,

                                                   SecWebSocketProtocols:        null,
                                                   NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                   NextHopNetworkingNodeId:      csms2.Id,

                                                   DisableWebSocketPings:        false,
                                                   WebSocketPingEvery:           null,
                                                   SlowNetworkSimulationDelay:   null,

                                                   DisableMaintenanceTasks:      false,
                                                   MaintenanceEvery:             null,

                                                   LoggingPath:                  null,
                                                   LoggingContext:               String.Empty,
                                                   LogfileCreator:               null,
                                                   HTTPLogger:                   null,
                                                   DNSClient:                    null

                                               );

            Assert.That(ocppGatewayConnectResult2.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Gateway could not connect to CSMS #2: {ocppGatewayConnectResult2.HTTPStatusCode}");

            #endregion


            ocppGateway_OCPPWebSocketServer  = ocppGateway.AttachWebSocketServer(

                                                   HTTPServiceName:              null,
                                                   IPAddress:                    null,
                                                   TCPPort:                      ocppGateway_tcpPort,
                                                   Description:                  null,

                                                   RequireAuthentication:        true,
                                                   DisableWebSocketPings:        false,
                                                   WebSocketPingEvery:           null,
                                                   SlowNetworkSimulationDelay:   null,

                                                   ServerCertificateSelector:    null,
                                                   ClientCertificateValidator:   null,
                                                   LocalCertificateSelector:     null,
                                                   AllowedTLSProtocols:          null,
                                                   ClientCertificateRequired:    null,
                                                   CheckCertificateRevocation:   null,

                                                   ServerThreadNameCreator:      null,
                                                   ServerThreadPrioritySetter:   null,
                                                   ServerThreadIsBackground:     null,
                                                   ConnectionIdBuilder:          null,
                                                   ConnectionTimeout:            null,
                                                   MaxClientConnections:         null,

                                                   AutoStart:                    true

                                               );

            #region Define signature policy

            ocppGateway_keyPair = KeyPair.GenerateKeys()!;

            ocppGateway.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                 KeyPair:                ocppGateway_keyPair!,
                                                                 UserIdGenerator:        (signableMessage) => "gw001",
                                                                 DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Gateway!"),
                                                                 TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppGateway.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                 VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup Local Controller

            ocppLocalController                      = new TestLocalControllerNode(

                                                           Id:                          NetworkingNode_Id.Parse("lc"),
                                                           VendorName:                  "GraphDefined",
                                                           Model:                       "vlc1",
                                                           SerialNumber:                null,
                                                           SoftwareVersion:             null,
                                                           Modem:                       null,
                                                           Description:                 I18NString.Create(Languages.en, "An OCPP Local Controller for testing"),

                                                           SignaturePolicy:             null,
                                                           ForwardingSignaturePolicy:   null,

                                                           HTTPUploadPort:              null,
                                                           HTTPDownloadPort:            null,

                                                           DisableSendHeartbeats:       true,
                                                           SendHeartbeatsEvery:         null,
                                                           DefaultRequestTimeout:       null,

                                                           DisableMaintenanceTasks:     false,
                                                           MaintenanceEvery:            null,
                                                           DNSClient:                   DNSClient

                                                       );

            var ocppLocalControllerAuth              = ocppGateway_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                           ocppLocalController.Id,
                                                                                           "lc12345678"
                                                                                       );

            var ocppLocalControllerConnectResult     = await ocppLocalController.ConnectWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppGateway_tcpPort}"),
                                                                 VirtualHostname:              null,
                                                                 Description:                  null,
                                                                 PreferIPv4:                   null,
                                                                 RemoteCertificateValidator:   null,
                                                                 LocalCertificateSelector:     null,
                                                                 ClientCert:                   null,
                                                                 TLSProtocol:                  null,
                                                                 HTTPUserAgent:                null,
                                                                 HTTPAuthentication:           ocppLocalControllerAuth,
                                                                 RequestTimeout:               null,
                                                                 TransmissionRetryDelay:       null,
                                                                 MaxNumberOfRetries:           3,
                                                                 InternalBufferSize:           null,

                                                                 SecWebSocketProtocols:        null,
                                                                 NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                                 NextHopNetworkingNodeId:      ocppGateway.Id,
                                                                 RoutingNetworkingNodeIds:     [ NetworkingNode_Id.CSMS ],

                                                                 DisableWebSocketPings:        false,
                                                                 WebSocketPingEvery:           null,
                                                                 SlowNetworkSimulationDelay:   null,

                                                                 DisableMaintenanceTasks:      false,
                                                                 MaintenanceEvery:             null,

                                                                 LoggingPath:                  null,
                                                                 LoggingContext:               String.Empty,
                                                                 LogfileCreator:               null,
                                                                 HTTPLogger:                   null,
                                                                 DNSClient:                    null

                                                             );

            Assert.That(ocppLocalControllerConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Local Controller could not connect to OCPP Gateway: {ocppLocalControllerConnectResult.HTTPStatusCode}");


            ocppLocalController_OCPPWebSocketServer  = ocppLocalController.AttachWebSocketServer(

                                                          HTTPServiceName:              null,
                                                          IPAddress:                    null,
                                                          TCPPort:                      ocppLocalController_tcpPort,
                                                          Description:                  null,

                                                          RequireAuthentication:        true,
                                                          DisableWebSocketPings:        false,
                                                          WebSocketPingEvery:           null,
                                                          SlowNetworkSimulationDelay:   null,

                                                          ServerCertificateSelector:    null,
                                                          ClientCertificateValidator:   null,
                                                          LocalCertificateSelector:     null,
                                                          AllowedTLSProtocols:          null,
                                                          ClientCertificateRequired:    null,
                                                          CheckCertificateRevocation:   null,

                                                          ServerThreadNameCreator:      null,
                                                          ServerThreadPrioritySetter:   null,
                                                          ServerThreadIsBackground:     null,
                                                          ConnectionIdBuilder:          null,
                                                          ConnectionTimeout:            null,
                                                          MaxClientConnections:         null,

                                                          AutoStart:                    true

                                                      );

            #region Define signature policy

            ocppLocalController_keyPair = KeyPair.GenerateKeys()!;

            ocppLocalController.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                         KeyPair:                ocppLocalController_keyPair!,
                                                                         UserIdGenerator:        (signableMessage) => "lc001",
                                                                         DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Local Controller!"),
                                                                         TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppLocalController.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                         VerificationRuleActions.VerifyAll);

            #endregion

            #endregion

            #region Setup Energy Meter

            ocppEnergyMeter                      = new TestEnergyMeterNode(

                                                       Id:                          NetworkingNode_Id.Parse("em"),
                                                       VendorName:                  "GraphDefined",
                                                       Model:                       "vem1",
                                                       SerialNumber:                null,
                                                       FirmwareVersion:             null,
                                                       Description:                 I18NString.Create(Languages.en, "An OCPP Energy Meter for testing"),

                                                       SignaturePolicy:             null,
                                                       ForwardingSignaturePolicy:   null,

                                                       DisableSendHeartbeats:       true,
                                                       SendHeartbeatsEvery:         null,
                                                       DefaultRequestTimeout:       null,

                                                       DisableMaintenanceTasks:     false,
                                                       MaintenanceEvery:            null,
                                                       DNSClient:                   DNSClient

                                                   );

            var ocppEnergyMeterAuth              = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                               ocppEnergyMeter.Id,
                                                                                               "em12345678"
                                                                                           );

            var ocppEnergyMeterConnectResult     = await ocppEnergyMeter.ConnectWebSocketClient(

                                                             RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_tcpPort}"),
                                                             VirtualHostname:              null,
                                                             Description:                  null,
                                                             PreferIPv4:                   null,
                                                             RemoteCertificateValidator:   null,
                                                             LocalCertificateSelector:     null,
                                                             ClientCert:                   null,
                                                             TLSProtocol:                  null,
                                                             HTTPUserAgent:                null,
                                                             HTTPAuthentication:           ocppEnergyMeterAuth,
                                                             RequestTimeout:               null,
                                                             TransmissionRetryDelay:       null,
                                                             MaxNumberOfRetries:           3,
                                                             InternalBufferSize:           null,

                                                             SecWebSocketProtocols:        null,
                                                             NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                             NextHopNetworkingNodeId:      ocppLocalController.Id,
                                                             RoutingNetworkingNodeIds:     [ NetworkingNode_Id.CSMS ],

                                                             DisableWebSocketPings:        false,
                                                             WebSocketPingEvery:           null,
                                                             SlowNetworkSimulationDelay:   null,

                                                             DisableMaintenanceTasks:      false,
                                                             MaintenanceEvery:             null,

                                                             LoggingPath:                  null,
                                                             LoggingContext:               String.Empty,
                                                             LogfileCreator:               null,
                                                             HTTPLogger:                   null,
                                                             DNSClient:                    null

                                                         );

            Assert.That(ocppEnergyMeterConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Energy Meter could not connect to OCPP Local Controller: {ocppEnergyMeterConnectResult.HTTPStatusCode}");


            //ocppEnergyMeter_OCPPWebSocketServer  = ocppEnergyMeter.AttachWebSocketServer(

            //                                              HTTPServiceName:              null,
            //                                              IPAddress:                    null,
            //                                              TCPPort:                      ocppEnergyMeter_tcpPort,
            //                                              Description:                  null,

            //                                              RequireAuthentication:        true,
            //                                              DisableWebSocketPings:        false,
            //                                              WebSocketPingEvery:           null,
            //                                              SlowNetworkSimulationDelay:   null,

            //                                              ServerCertificateSelector:    null,
            //                                              ClientCertificateValidator:   null,
            //                                              LocalCertificateSelector:     null,
            //                                              AllowedTLSProtocols:          null,
            //                                              ClientCertificateRequired:    null,
            //                                              CheckCertificateRevocation:   null,

            //                                              ServerThreadNameCreator:      null,
            //                                              ServerThreadPrioritySetter:   null,
            //                                              ServerThreadIsBackground:     null,
            //                                              ConnectionIdBuilder:          null,
            //                                              ConnectionTimeout:            null,
            //                                              MaxClientConnections:         null,

            //                                              AutoStart:                    true

            //                                          );

            #region Define signature policy

            ocppEnergyMeter_keyPair = KeyPair.GenerateKeys()!;

            ocppEnergyMeter.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                     KeyPair:                ocppEnergyMeter_keyPair!,
                                                                     UserIdGenerator:        (signableMessage) => "em001",
                                                                     DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Energy Meter!"),
                                                                     TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppEnergyMeter.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                     VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup chargingStation1

            chargingStation1      = new TestChargingStationNode(

                                        Id:                         NetworkingNode_Id.Parse("cs1"),
                                        VendorName:                 "GraphDefined",
                                        Model:                      "vcs1",
                                        Description:                I18NString.Create(Languages.en, "The first charging station for testing"),
                                        SerialNumber:               "cs#1",
                                        FirmwareVersion:            "cs-fw v1.0",
                                        Modem:                       new Modem(
                                                                         ICCID:       "ICCID#1",
                                                                         IMSI:        "IMSI#1",
                                                                         CustomData:   null
                                                                     ),

                                        EVSEs:                       [
                                                                         new protocols.OCPPv2_1.CS.ChargingStationEVSE(
                                                                             Id:                  protocols.OCPPv2_1.EVSE_Id.Parse(0),
                                                                             AdminStatus:         OperationalStatus.Operative,
                                                                             MeterType:           "myMeter",
                                                                             MeterSerialNumber:   "Meter#1",
                                                                             MeterPublicKey:      "pubkey#1",
                                                                             Connectors:          [
                                                                                                      new protocols.OCPPv2_1.CS.ChargingStationConnector(
                                                                                                          Id:              Connector_Id.Parse(1),
                                                                                                          ConnectorType:   ConnectorType.cType2
                                                                                                      )
                                                                                                  ]
                                                                         )
                                                                     ],
                                        UplinkEnergyMeter:           null,

                                        DefaultRequestTimeout:       null,

                                        SignaturePolicy:             null,
                                        ForwardingSignaturePolicy:   null,

                                        DisableSendHeartbeats:       true,
                                        SendHeartbeatsEvery:         null,

                                        DisableMaintenanceTasks:     false,
                                        MaintenanceEvery:            null,

                                        CustomData:                  null,
                                        DNSClient:                   DNSClient

                                    );

            ocppLocalController.AllowedChargingStations.Add(chargingStation1.Id);

            var cs1Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation1.Id,
                                                                                "cs1_12345678"
                                                                            );

            var cs1ConnectResult  = await chargingStation1.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_tcpPort}"),
                                              VirtualHostname:              null,
                                              Description:                  null,
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           cs1Auth,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              //NextHopNetworkingNodeId:      ocppLocalController.Id,
                                              //RoutingNetworkingNodeIds:     [ NetworkingNode_Id.CSMS ],

                                              DisableWebSocketPings:        false,
                                              WebSocketPingEvery:           null,
                                              SlowNetworkSimulationDelay:   null,

                                              DisableMaintenanceTasks:      false,
                                              MaintenanceEvery:             null,

                                              LoggingPath:                  null,
                                              LoggingContext:               String.Empty,
                                              LogfileCreator:               null,
                                              HTTPLogger:                   null,
                                              DNSClient:                    null

                                          );

            Assert.That(cs1ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station #1 could not connect to OCPP Local Controller: {cs1ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            chargingStation1_keyPair = KeyPair.GenerateKeys()!;

            chargingStation1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation1_keyPair!,
                                                                      UserIdGenerator:        (signableMessage) => "cs001",
                                                                      DescriptionGenerator:   (signableMessage) => I18NString.Create("Just the 1st OCPP Charging Station!"),
                                                                      TimestampGenerator:     (signableMessage) => Timestamp.Now);

            chargingStation1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                      VerificationRuleActions.VerifyAll);

            #endregion



            var csms1_addLocation1Result  = await csms1_cso.AddChargingPool(

                                                      Id:                             protocols.WWCP.ChargingPool_Id.Parse("DE*GEF*P123"),
                                                      Name:                           I18NString.Create(Languages.en, "ChargingPool DE*GEF 123"),
                                                      Description:                    I18NString.Create(Languages.en, "Pool #1"),
                                                      //RemoteChargingPoolCreator:      cp => new LocalControllerAdapter(csms1, cp, ocppLocalController),

                                                      Address:                        null,
                                                      GeoLocation:                    null,
                                                      TimeZone:                       null,
                                                      OpeningTimes:                   null,
                                                      ChargingWhenClosed:             null,
                                                      Accessibility:                  null,
                                                      LocationLanguage:               null,
                                                      HotlinePhoneNumber:             null,

                                                      Brands:                         null,
                                                      MobilityRootCAs:                null,

                                                      InitialAdminStatus:             null,
                                                      InitialStatus:                  null,
                                                      MaxAdminStatusScheduleSize:     null,
                                                      MaxStatusScheduleSize:          null

                                                      //DataSource:                     null,
                                                      //LastChange:                     null,

                                                      //CustomData:                     null,
                                                      //InternalData:                   null,

                                                      //Configurator:                   null,
                                                      //RemoteChargingPoolCreator:      null,

                                                      //OnSuccess:                      null,
                                                      //OnError:                        null,

                                                      //SkipAddedNotifications:         null,
                                                      //AllowInconsistentOperatorIds:   null,
                                                      //EventTrackingId:                null,
                                                      //CurrentUserId:                  null

                                                  );

            p1 = csms1_addLocation1Result.Entity!;

            var csms1_addStation1Result   = await p1.AddChargingStation(

                                                      Id:                             protocols.WWCP.ChargingStation_Id.Parse("DE*GEF*S123*456"),
                                                      Name:                           I18NString.Create(Languages.en, "ChargingStation DE*GEF 123*456"),
                                                      Description:                    I18NString.Create(Languages.en, "Pool #1, Station #1"),
                                                      RemoteChargingStationCreator:   cs => new ChargingStationAdapter(csms1, cs, chargingStation1),

                                                      Address:                        null,
                                                      GeoLocation:                    null,
                                                      OpeningTimes:                   null,
                                                      ChargingWhenClosed:             null,
                                                      Accessibility:                  null,
                                                      LocationLanguage:               null,
                                                      PhysicalReference:              null,
                                                      HotlinePhoneNumber:             null,

                                                      AuthenticationModes:            null,
                                                      PaymentOptions:                 null,
                                                      Features:                       null,

                                                      ServiceIdentification:          null,
                                                      ModelCode:                      null,

                                                      Published:                      null,
                                                      Disabled:                       null,

                                                      Brands:                         null,
                                                      MobilityRootCAs:                null,

                                                      InitialAdminStatus:             null,
                                                      InitialStatus:                  null,
                                                      MaxAdminStatusScheduleSize:     null,
                                                      MaxStatusScheduleSize:          null

                                                      //DataSource:                     null,
                                                      //LastChange:                     null,

                                                      //CustomData:                     null,
                                                      //InternalData:                   null,

                                                      //Configurator:                   null,
                                                      //RemoteChargingStationCreator:   null,

                                                      //OnSuccess:                      null,
                                                      //OnError:                        null,

                                                      //SkipAddedNotifications:         null,
                                                      //AllowInconsistentOperatorIds:   null,
                                                      //EventTrackingId:                null,
                                                      //CurrentUserId:                  null

                                                  );

            s1 = csms1_addStation1Result.Entity!;

            var csms1_addEVSE1Result      = await s1.AddEVSE(
                                                      Id:                             protocols.WWCP.EVSE_Id.Parse("DE*GEF*E123*456*1"),
                                                      Name:                           I18NString.Create(Languages.en, "EVSE DE*GEF 123*456*1"),
                                                      Description:                    I18NString.Create(Languages.en, "Pool #1, Station #1, EVSE #1"),

                                                      PhotoURLs:                      null,
                                                      Brands:                         null,
                                                      MobilityRootCAs:                null,
                                                      OpenDataLicenses:               null,
                                                      ChargingModes:                  null,
                                                      ChargingTariffs:                null,
                                                      CurrentType:                    protocols.WWCP.CurrentTypes.DC,
                                                      AverageVoltage:                 Volt.  ParseV (600),
                                                      AverageVoltageRealTime:         null,
                                                      AverageVoltagePrognoses:        null,
                                                      MaxCurrent:                     Ampere.ParseA (500),
                                                      MaxCurrentRealTime:             null,
                                                      MaxCurrentPrognoses:            null,
                                                      MaxPower:                       Watt.  ParseKW(300),
                                                      MaxPowerRealTime:               null,
                                                      MaxPowerPrognoses:              null,
                                                      MaxCapacity:                    null,
                                                      MaxCapacityRealTime:            null,
                                                      MaxCapacityPrognoses:           null,
                                                      EnergyMix:                      null,
                                                      EnergyMixRealTime:              null,
                                                      EnergyMixPrognoses:             null,
                                                      EnergyMeter:                    null,
                                                      IsFreeOfCharge:                 null,
                                                      ChargingConnectors:             null,
                                                      ChargingSession:                null,

                                                      InitialAdminStatus:             null,
                                                      InitialStatus:                  null,
                                                      MaxAdminStatusScheduleSize:     null,
                                                      MaxStatusScheduleSize:          null,
                                                      LastStatusUpdate:               null

                                                      //DataSource:                     null,
                                                      //LastChange:                     null,
                                                      //
                                                      //CustomData:                     null,
                                                      //InternalData:                   null,
                                                      //
                                                      //Configurator:                   null,
                                                      //RemoteEVSECreator:              null,
                                                      //
                                                      //OnSuccess:                      null,
                                                      //OnError:                        null,
                                                      //
                                                      //SkipAddedNotifications:         null,
                                                      //AllowInconsistentOperatorIds:   null,
                                                      //EventTrackingId:                null,
                                                      //CurrentUserId:                  null

                                                  );

            e1 = csms1_addEVSE1Result.Entity!;

            #endregion

            #region Setup chargingStation2

            chargingStation2      = new TestChargingStationNode(

                                            Id:                         NetworkingNode_Id.Parse("cs2"),
                                            VendorName:                 "GraphDefined",
                                            Model:                      "vcs2",
                                            Description:                I18NString.Create(Languages.en, "The second charging station for testing"),
                                            SerialNumber:               "cs#2",
                                            FirmwareVersion:            "cs-fw v2.0",
                                            Modem:                       new Modem(
                                                                             ICCID:       "ICCID#2",
                                                                             IMSI:        "IMSI#2",
                                                                             CustomData:   null
                                                                         ),

                                            EVSEs:                       [
                                                                             new protocols.OCPPv2_1.CS.ChargingStationEVSE(
                                                                                 Id:                  protocols.OCPPv2_1.EVSE_Id.Parse(0),
                                                                                 AdminStatus:         OperationalStatus.Operative,
                                                                                 MeterType:           "myMeter",
                                                                                 MeterSerialNumber:   "Meter#2",
                                                                                 MeterPublicKey:      "pubkey#2",
                                                                                 Connectors:          [
                                                                                                          new protocols.OCPPv2_1.CS.ChargingStationConnector(
                                                                                                              Id:              Connector_Id.Parse(1),
                                                                                                              ConnectorType:   ConnectorType.cType2
                                                                                                          )
                                                                                                      ]
                                                                             )
                                                                         ],
                                            UplinkEnergyMeter:           null,

                                            DefaultRequestTimeout:       null,

                                            SignaturePolicy:             null,
                                            ForwardingSignaturePolicy:   null,

                                            DisableSendHeartbeats:       true,
                                            SendHeartbeatsEvery:         null,

                                            DisableMaintenanceTasks:     false,
                                            MaintenanceEvery:            null,

                                            CustomData:                  null,
                                            DNSClient:                   DNSClient

                                        );

            ocppLocalController.AllowedChargingStations.Add(chargingStation2.Id);

            var cs2Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation2.Id,
                                                                                "cs2_12345678"
                                                                            );

            var cs2ConnectResult  = await chargingStation2.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_tcpPort}"),
                                              VirtualHostname:              null,
                                              Description:                  null,
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           cs2Auth,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              //NextHopNetworkingNodeId:      ocppLocalController.Id,
                                              //RoutingNetworkingNodeIds:     [ NetworkingNode_Id.CSMS ],

                                              DisableWebSocketPings:        false,
                                              WebSocketPingEvery:           null,
                                              SlowNetworkSimulationDelay:   null,

                                              DisableMaintenanceTasks:      false,
                                              MaintenanceEvery:             null,

                                              LoggingPath:                  null,
                                              LoggingContext:               String.Empty,
                                              LogfileCreator:               null,
                                              HTTPLogger:                   null,
                                              DNSClient:                    null

                                          );

            Assert.That(cs2ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station #2 could not connect to OCPP Local Controller: {cs2ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            chargingStation2_keyPair = KeyPair.GenerateKeys()!;

            chargingStation2.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation2_keyPair!,
                                                                      UserIdGenerator:        (signableMessage) => "cs002",
                                                                      DescriptionGenerator:   (signableMessage) => I18NString.Create("Just the 2st OCPP Charging Station!"),
                                                                      TimestampGenerator:     (signableMessage) => Timestamp.Now);

            chargingStation2.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                      VerificationRuleActions.VerifyAll);

            #endregion

            #endregion

            #region Setup chargingStation3

            chargingStation3      = new TestChargingStationNode(

                                            Id:                         NetworkingNode_Id.Parse("cs3"),
                                            VendorName:                 "GraphDefined",
                                            Model:                      "vcs3",
                                            Description:                I18NString.Create(Languages.en, "The third charging station for testing"),
                                            SerialNumber:               "cs#3",
                                            FirmwareVersion:            "cs-fw v3.0",
                                            Modem:                       new Modem(
                                                                             ICCID:       "ICCID#3",
                                                                             IMSI:        "IMSI#3",
                                                                             CustomData:   null
                                                                         ),

                                            EVSEs:                       [
                                                                             new protocols.OCPPv2_1.CS.ChargingStationEVSE(
                                                                                 Id:                  protocols.OCPPv2_1.EVSE_Id.Parse(0),
                                                                                 AdminStatus:         OperationalStatus.Operative,
                                                                                 MeterType:           "myMeter",
                                                                                 MeterSerialNumber:   "Meter#3",
                                                                                 MeterPublicKey:      "pubkey#3",
                                                                                 Connectors:          [
                                                                                                          new protocols.OCPPv2_1.CS.ChargingStationConnector(
                                                                                                              Id:              Connector_Id.Parse(1),
                                                                                                              ConnectorType:   ConnectorType.cType2
                                                                                                          )
                                                                                                      ]
                                                                             )
                                                                         ],
                                            UplinkEnergyMeter:           null,

                                            DefaultRequestTimeout:       null,

                                            SignaturePolicy:             null,
                                            ForwardingSignaturePolicy:   null,

                                            DisableSendHeartbeats:       true,
                                            SendHeartbeatsEvery:         null,

                                            DisableMaintenanceTasks:     false,
                                            MaintenanceEvery:            null,

                                            CustomData:                  null,
                                            DNSClient:                   DNSClient

                                        );

            ocppLocalController.AllowedChargingStations.Add(chargingStation3.Id);

            var cs3Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation3.Id,
                                                                                "cs3_12345678"
                                                                            );

            var cs3ConnectResult  = await chargingStation3.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_tcpPort}"),
                                              VirtualHostname:              null,
                                              Description:                  null,
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           cs3Auth,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              //NextHopNetworkingNodeId:      ocppLocalController.Id,
                                              //RoutingNetworkingNodeIds:     [ NetworkingNode_Id.CSMS ],

                                              DisableWebSocketPings:        false,
                                              WebSocketPingEvery:           null,
                                              SlowNetworkSimulationDelay:   null,

                                              DisableMaintenanceTasks:      false,
                                              MaintenanceEvery:             null,

                                              LoggingPath:                  null,
                                              LoggingContext:               String.Empty,
                                              LogfileCreator:               null,
                                              HTTPLogger:                   null,
                                              DNSClient:                    null

                                          );

            Assert.That(cs3ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station #3 could not connect to OCPP Local Controller: {cs3ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            chargingStation3_keyPair = KeyPair.GenerateKeys()!;

            chargingStation3.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation3_keyPair!,
                                                                      UserIdGenerator:        (signableMessage) => "cs003",
                                                                      DescriptionGenerator:   (signableMessage) => I18NString.Create("Just the 3st OCPP Charging Station!"),
                                                                      TimestampGenerator:     (signableMessage) => Timestamp.Now);

            chargingStation3.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                      VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            //ToDo: Make use of the routing protocol vendor extensions!

            csms1.              OCPP.AddStaticRouting(ocppLocalController.Id,  ocppGateway.Id);
            csms1.              OCPP.AddStaticRouting(ocppEnergyMeter.Id,      ocppGateway.Id);
            csms1.              OCPP.AddStaticRouting(chargingStation1.Id,     ocppGateway.Id);
            csms1.              OCPP.AddStaticRouting(chargingStation2.Id,     ocppGateway.Id);
            csms1.              OCPP.AddStaticRouting(chargingStation3.Id,     ocppGateway.Id);

            csms2.              OCPP.AddStaticRouting(ocppLocalController.Id,  ocppGateway.Id);
            csms2.              OCPP.AddStaticRouting(ocppEnergyMeter.Id,      ocppGateway.Id);
            csms2.              OCPP.AddStaticRouting(chargingStation1.Id,     ocppGateway.Id);
            csms2.              OCPP.AddStaticRouting(chargingStation2.Id,     ocppGateway.Id);
            csms2.              OCPP.AddStaticRouting(chargingStation3.Id,     ocppGateway.Id);

            ocppGateway.        OCPP.AddStaticRouting(NetworkingNode_Id.CSMS,  csms1.Id);  // The default CSMS!
            ocppGateway.        OCPP.AddStaticRouting(ocppEnergyMeter.Id,      ocppLocalController.Id);
            ocppGateway.        OCPP.AddStaticRouting(chargingStation1.Id,     ocppLocalController.Id);
            ocppGateway.        OCPP.AddStaticRouting(chargingStation2.Id,     ocppLocalController.Id);
            ocppGateway.        OCPP.AddStaticRouting(chargingStation3.Id,     ocppLocalController.Id);

            ocppLocalController.OCPP.AddStaticRouting(csms1.Id,                ocppGateway.Id);
            ocppLocalController.OCPP.AddStaticRouting(csms2.Id,                ocppGateway.Id);


            ocppGateway.        OCPP.FORWARD.OnAnyJSONRequestFilter += (timestamp,
                                                                        sender,
                                                                        connection,
                                                                        request,
                                                                        cancellationToken) =>

                Task.FromResult(
                    request.NetworkPath.Source == chargingStation3.Id
                        ? ForwardingDecision.FORWARD(request, csms2.Id)
                        : ForwardingDecision.NEXT   (request)
                );

            #region OnBootNotification

            ocppGateway.OCPP.FORWARD.OnBootNotificationRequestFilter += (timestamp,
                                                                         sender,
                                                                         connection,
                                                                         request,
                                                                         cancellationToken) =>

                Task.FromResult(
                    request.NetworkPath.Source == chargingStation3.Id
                        ? ForwardingDecision<BootNotificationRequest, BootNotificationResponse>.FORWARD(request, csms2.Id)
                        : ForwardingDecision<BootNotificationRequest, BootNotificationResponse>.FORWARD(request)
                );

            #endregion

        }

        #endregion

        #region SetupEachTest()

        [SetUp]
        public async Task SetupEachTest()
        {

            Timestamp.Reset();

        }

        #endregion

        #region ShutdownEachTest()

        [TearDown]
        public void ShutdownEachTest()
        {

        }

        #endregion

        #region ShutdownOnce()

        [OneTimeTearDown]
        public void ShutdownOnce()
        {

        }

        #endregion


    }

}
