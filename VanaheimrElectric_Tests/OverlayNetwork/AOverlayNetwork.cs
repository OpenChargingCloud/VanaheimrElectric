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
using cloud.charging.open.protocols.WWCP.NetworkingNode;
using cloud.charging.open.protocols.OCPP.WebSockets;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Charging infrastructure test defaults using an OCPP Overlay Network
    /// consisting of three Charging Stations, an OCPP Local Controller, an
    /// Energy Meter at the shared grid connection point, an OCPP Gateway
    /// and two Charging Station Management Systems.
    /// 
    /// The HTTP WebSocket connections are initiated in "normal order" from
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
        public OCPPWebSocketServer?         csms1_OCPPWebSocketServer;
        public KeyPair?                     csms1_keyPair;
        public RoamingNetwork?              csms1_roamingNetwork;
        public IChargingStationOperator?    csms1_cso;
        public IEMobilityProvider?          csms1_emp;
        public EMobilityServiceProvider?    csms1_remoteEMP;

        public TestCSMSNode?                csms2;
        public OCPPWebSocketServer?         csms2_OCPPWebSocketServer;
        public KeyPair?                     csms2_keyPair;
        public RoamingNetwork?              csms2_roamingNetwork;
        public IChargingStationOperator?    csms2_cso;
        public EMobilityServiceProvider?    csms2_emp;

        public TestGatewayNode?             ocppGateway1;
        public OCPPWebSocketServer?         ocppGateway_OCPPWebSocketServer;
        public KeyPair?                     ocppGateway_keyPair;

        public TestLocalControllerNode?     ocppLocalController1;
        public OCPPWebSocketServer?         ocppLocalController_OCPPWebSocketServer;
        public KeyPair?                     ocppLocalController_keyPair;

        public TestEnergyMeterNode?         gridEnergyMeter1;
        public KeyPair?                     gridEnergyMeter_keyPair;

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

                        Id:                             NetworkingNode_Id.Parse("csms1"),
                        VendorName:                     "GraphDefined",
                        Model:                          "vcsms1",
                        Description:                    I18NString.Create("Charging Station Management System #1 for testing"),

                        SignaturePolicy:                null,
                        ForwardingSignaturePolicy:      null,

                        //DisableSendHeartbeats:          true,
                        //SendHeartbeatsEvery:            null,
                        DefaultRequestTimeout:          null,

                        DisableMaintenanceTasks:        false,
                        MaintenanceEvery:               null,

                        HTTPAPI_EventLoggingDisabled:   true,

                        DNSClient:                      DNSClient

                    );

            csms1_OCPPWebSocketServer = csms1.AttachWebSocketServer(

                                            HTTPServiceName:              null,
                                            IPAddress:                    null,
                                            TCPPort:                      null,//csms1_tcpPort,
                                            Description:                  I18NString.Create("Charging Station Management System #1 WebSocket Server"),

                                            RequireAuthentication:        true,
                                            DisableWebSocketPings:        true,
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

            #region Add User Roles

            csms1.UserRoles.Add(
                new UserRole(

                    Id:                      ChargingStationSettings.UserRoles.Admin,
                    Description:             I18NString. Create("The admin user role for charging stations"),

                    KeyPairs:                [ ECCKeyPair.  ParsePrivateKey("ANqBTkO85kZZ44o1jT/Ygxa7JDtVOBUPBtXhtoPYWjgO")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            csms1.UserRoles.Add(
                new UserRole(

                    Id:                      ChargingStationSettings.UserRoles.User,
                    Description:             I18NString. Create("The default user role for charging stations"),

                    KeyPairs:                [ ECCKeyPair.  ParsePrivateKey("BtSha5ImqfBiNY53aGrU1cQ5hmQ9CheI79+EKJNKNeQ=")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            #endregion

            #region Define signature policy

            csms1_keyPair = ECCKeyPair.GenerateKeys()!;

            csms1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                           KeyPair:                csms1_keyPair!,
                                                           UserIdGenerator:        (signableMessage) => "csms1",
                                                           DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Charging Station Management System #1!"),
                                                           TimestampGenerator:     (signableMessage) => Timestamp.Now);

            //csms1.OCPP.SignaturePolicy.AddSigningRule     (SetVariablesRequest.DefaultJSONLDContext,
            //                                               KeyPair:                csms1.UserRoles.First()?.KeyPairs.First()!,
            //                                               UserIdGenerator:        (signableMessage) => "csms #1 admin",
            //                                               DescriptionGenerator:   (signableMessage) => I18NString.Create("The admin of OCPP Charging Station Management System #1!"),
            //                                               TimestampGenerator:     (signableMessage) => Timestamp.Now);

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

                               Destination:             SourceRouting.To(authorizeRequest.NetworkPath.Source),
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

                             Destination:               SourceRouting.To(authorizeRequest.NetworkPath.Source),
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

                        Id:                             NetworkingNode_Id.Parse("csms2"),
                        VendorName:                     "GraphDefined",
                        Model:                          "vcsms21",
                        Description:                    I18NString.Create("Charging Station Management System #2 for testing"),

                        SignaturePolicy:                null,
                        ForwardingSignaturePolicy:      null,

                        //DisableSendHeartbeats:          true,
                        //SendHeartbeatsEvery:            null,
                        DefaultRequestTimeout:          null,

                        DisableMaintenanceTasks:        false,
                        MaintenanceEvery:               null,

                        HTTPAPI_EventLoggingDisabled:   true,

                        DNSClient:                      DNSClient

                    );

            csms2_OCPPWebSocketServer = csms2.AttachWebSocketServer(

                                            HTTPServiceName:              null,
                                            IPAddress:                    null,
                                            TCPPort:                      null,//csms2_tcpPort,
                                            Description:                  I18NString.Create("Charging Station Management System #2 WebSocket Server"),

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

            csms2_keyPair = ECCKeyPair.GenerateKeys()!;

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

            ocppGateway1                     = new TestGatewayNode(

                                                   Id:                             NetworkingNode_Id.Parse("gw"),
                                                   VendorName:                     "GraphDefined",
                                                   Model:                          "vgw1",
                                                   Description:                    I18NString.Create("An OCPP Gateway for testing"),

                                                   SignaturePolicy:                null,
                                                   ForwardingSignaturePolicy:      null,

                                                   DisableSendHeartbeats:          true,
                                                   SendHeartbeatsEvery:            null,
                                                   DefaultRequestTimeout:          null,

                                                   DisableMaintenanceTasks:        false,
                                                   MaintenanceEvery:               null,

                                                   HTTPAPI_EventLoggingDisabled:   true,

                                                   DNSClient:                      DNSClient

                                               );

            #region Connect to CSMS1

            var ocppGatewayAuth1             = csms1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                             ocppGateway1.Id,
                                                                             "gw2csms1_12345678"
                                                                         );

            var ocppGatewayConnectResult1    = await ocppGateway1.ConnectOCPPWebSocketClient(

                                                   RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms1_OCPPWebSocketServer.IPPort}"),
                                                   VirtualHostname:              null,
                                                   Description:                  I18NString.Create("GW to CSMS1"),
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
                                                                             ocppGateway1.Id,
                                                                             "gw2csms2_12345678"
                                                                         );

            var ocppGatewayConnectResult2    = await ocppGateway1.ConnectOCPPWebSocketClient(

                                                   RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms2_OCPPWebSocketServer.IPPort}"),
                                                   VirtualHostname:              null,
                                                   Description:                  I18NString.Create("GW to CSMS2"),
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


            ocppGateway_OCPPWebSocketServer  = ocppGateway1.AttachWebSocketServer(

                                                   HTTPServiceName:              null,
                                                   IPAddress:                    null,
                                                   TCPPort:                      null,//ocppGateway_tcpPort,
                                                   Description:                  I18NString.Create("OCPP Gateway WebSocket Server"),

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

            ocppGateway_keyPair = ECCKeyPair.GenerateKeys()!;

            ocppGateway1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                 KeyPair:                ocppGateway_keyPair!,
                                                                 UserIdGenerator:        (signableMessage) => "gw001",
                                                                 DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Gateway!"),
                                                                 TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppGateway1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                 VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup Local Controller

            ocppLocalController1                     = new TestLocalControllerNode(

                                                           Id:                             NetworkingNode_Id.Parse("lc"),
                                                           VendorName:                     "GraphDefined",
                                                           Model:                          "vlc1",
                                                           SerialNumber:                   null,
                                                           SoftwareVersion:                null,
                                                           Modem:                          null,
                                                           Description:                    I18NString.Create("An OCPP Local Controller for testing"),

                                                           SignaturePolicy:                null,
                                                           ForwardingSignaturePolicy:      null,

                                                           DisableSendHeartbeats:          true,
                                                           SendHeartbeatsEvery:            null,
                                                           DefaultRequestTimeout:          null,

                                                           DisableMaintenanceTasks:        false,
                                                           MaintenanceEvery:               null,

                                                           HTTPAPI_EventLoggingDisabled:   true,

                                                           DNSClient:                      DNSClient

                                                       );

            var ocppLocalControllerAuth              = ocppGateway_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                           ocppLocalController1.Id,
                                                                                           "lc12345678"
                                                                                       );

            var ocppLocalControllerConnectResult     = await ocppLocalController1.ConnectOCPPWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppGateway_OCPPWebSocketServer.IPPort}"),
                                                                 VirtualHostname:              null,
                                                                 Description:                  I18NString.Create("LC to GW"),
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
                                                                 NextHopNetworkingNodeId:      ocppGateway1.Id,
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


            ocppLocalController_OCPPWebSocketServer  = ocppLocalController1.AttachWebSocketServer(

                                                          HTTPServiceName:              null,
                                                          IPAddress:                    null,
                                                          TCPPort:                      null,//ocppLocalController_tcpPort,
                                                          Description:                  I18NString.Create("OCPP Local Controller WebSocket Server"),

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

            ocppLocalController_keyPair = ECCKeyPair.GenerateKeys()!;

            ocppLocalController1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                         KeyPair:                ocppLocalController_keyPair!,
                                                                         UserIdGenerator:        (signableMessage) => "lc001",
                                                                         DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Local Controller!"),
                                                                         TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppLocalController1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                         VerificationRuleActions.VerifyAll);

            #endregion

            #endregion

            #region Setup Grid Energy Meter

            gridEnergyMeter1                     = new TestEnergyMeterNode(

                                                       Id:                             NetworkingNode_Id.Parse("em"),
                                                       VendorName:                     "GraphDefined",
                                                       Model:                          "vem1",
                                                       SerialNumber:                   null,
                                                       FirmwareVersion:                null,
                                                       Description:                    I18NString.Create("An OCPP Energy Meter for testing"),

                                                       SignaturePolicy:                null,
                                                       ForwardingSignaturePolicy:      null,

                                                       DisableSendHeartbeats:          true,
                                                       SendHeartbeatsEvery:            null,
                                                       DefaultRequestTimeout:          null,

                                                       DisableMaintenanceTasks:        false,
                                                       MaintenanceEvery:               null,

                                                       HTTPAPI_EventLoggingDisabled:   true,

                                                       DNSClient:                      DNSClient

                                                   );

            var gridEnergyMeterAuth              = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                               gridEnergyMeter1.Id,
                                                                                               "em12345678"
                                                                                           );

            var gridEnergyMeterConnectResult     = await gridEnergyMeter1.ConnectOCPPWebSocketClient(

                                                             RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_OCPPWebSocketServer.IPPort}"),
                                                             VirtualHostname:              null,
                                                             Description:                  I18NString.Create("EM to LC"),
                                                             PreferIPv4:                   null,
                                                             RemoteCertificateValidator:   null,
                                                             LocalCertificateSelector:     null,
                                                             ClientCert:                   null,
                                                             TLSProtocol:                  null,
                                                             HTTPUserAgent:                null,
                                                             HTTPAuthentication:           gridEnergyMeterAuth,
                                                             RequestTimeout:               null,
                                                             TransmissionRetryDelay:       null,
                                                             MaxNumberOfRetries:           3,
                                                             InternalBufferSize:           null,

                                                             SecWebSocketProtocols:        null,
                                                             NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                             NextHopNetworkingNodeId:      ocppLocalController1.Id,
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

            Assert.That(gridEnergyMeterConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Energy Meter could not connect to OCPP Local Controller: {gridEnergyMeterConnectResult.HTTPStatusCode}");


            //gridEnergyMeter_OCPPWebSocketServer  = gridEnergyMeter.AttachWebSocketServer(

            //                                              HTTPServiceName:              null,
            //                                              IPAddress:                    null,
            //                                              TCPPort:                      gridEnergyMeter_tcpPort,
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

            gridEnergyMeter_keyPair = ECCKeyPair.GenerateKeys()!;

            gridEnergyMeter1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                     KeyPair:                gridEnergyMeter_keyPair!,
                                                                     UserIdGenerator:        (signableMessage) => "em001",
                                                                     DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Energy Meter!"),
                                                                     TimestampGenerator:     (signableMessage) => Timestamp.Now);

            gridEnergyMeter1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                     VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup chargingStation1

            chargingStation1      = new TestChargingStationNode(

                                        Id:                            NetworkingNode_Id.Parse("cs1"),
                                        VendorName:                    "GraphDefined",
                                        Model:                         "vcs1",
                                        Description:                   I18NString.Create("The first charging station for testing"),
                                        SerialNumber:                  "cs#1",
                                        FirmwareVersion:               "cs-fw v1.0",

                                        EVSEs:                         [
                                                                            new EVSESpec(
                                                                                AdminStatus:         OperationalStatus.Operative,
                                                                                ConnectorTypes:      [ ConnectorType.sType2 ],
                                                                                MeterType:           "myMeter",
                                                                                MeterSerialNumber:   "Meter#1.1",
                                                                                MeterPublicKey:      "pubkey#1.1"
                                                                            ),
                                                                            new EVSESpec(
                                                                                AdminStatus:         OperationalStatus.Operative,
                                                                                ConnectorTypes:      [ ConnectorType.cCCS2 ],
                                                                                MeterType:           "myMeter",
                                                                                MeterSerialNumber:   "Meter#1.2",
                                                                                MeterPublicKey:      "pubkey#1.2"
                                                                            )
                                                                        ],
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#1",
                                                                            IMSI:        "IMSI#1",
                                                                            CustomData:   null
                                                                        ),

                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient
            );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation1.Id);

            var cs1Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation1.Id,
                                                                                "cs1_12345678"
                                                                            );

            var cs1ConnectResult  = await chargingStation1.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS1 to LC"),
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

            #region Register User Roles

            //var keys = ECCKeyPair.GenerateKeys()!;

            // {
            //   "private": "AMMLFGD82B7GFc7QQYNtB/1AaNFBdo/W5TQpAneLbCza",
            //   "public":  "BEvelX5aAzr77LsrEqlUKAq69XHpLT2I9rPuSflnWGGvM+BAROk9IAUEho5HvO7QyD9j374CcjbUfi6xGp0m9ig="
            // }

            // {
            //   "private": "ANqBTkO85kZZ44o1jT/Ygxa7JDtVOBUPBtXhtoPYWjgO",
            //   "public":  "BGFDuYqF2x8R4DUb0glpmRDgOpd9+197eQ1jOriP9PaWF013TSU5WedD4YePdEwTXDbdsLPucT8L/cFKTKqCCgQ="
            // }

            // {
            //   "private": "BtSha5ImqfBiNY53aGrU1cQ5hmQ9CheI79+EKJNKNeQ=",
            //   "public":  "BHP5kGwkiii3V7YS7XbG1MEAY9UmxTzo8iEBftaBcstf0xy3qLUhdmtL5DPqs5F9k2mvMZDPzhOQXP1UZlrvefY="
            // }

            chargingStation1.UserRoles.Add(
                new UserRole(

                    Id:                      UserRole_Id.Vendor,
                    Description:             I18NString. Create("The vendor user role for the charging station"),
                    KeyPairs:                [ ECCKeyPair.  ParsePublicKey("BEvelX5aAzr77LsrEqlUKAq69XHpLT2I9rPuSflnWGGvM+BAROk9IAUEho5HvO7QyD9j374CcjbUfi6xGp0m9ig=")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            chargingStation1.UserRoles.Add(
                new UserRole(

                    Id:                      UserRole_Id.Admin,
                    Description:             I18NString. Create("The admin user role for the charging station"),
                    KeyPairs:                [ ECCKeyPair.  ParsePublicKey("BGFDuYqF2x8R4DUb0glpmRDgOpd9+197eQ1jOriP9PaWF013TSU5WedD4YePdEwTXDbdsLPucT8L/cFKTKqCCgQ=")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            chargingStation1.UserRoles.Add(
                new UserRole(

                    Id:                      UserRole_Id.Support,
                    Description:             I18NString. Create("The support user role for the charging station"),
                    KeyPairs:                [ ECCKeyPair.  ParsePublicKey("BHP5kGwkiii3V7YS7XbG1MEAY9UmxTzo8iEBftaBcstf0xy3qLUhdmtL5DPqs5F9k2mvMZDPzhOQXP1UZlrvefY=")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            #endregion

            #region Define signature policy

            chargingStation1_keyPair = ECCKeyPair.GenerateKeys()!;

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
                                                      Name:                           I18NString.Create("ChargingPool DE*GEF 123"),
                                                      Description:                    I18NString.Create("Pool #1"),
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
                                                      Name:                           I18NString.Create("ChargingStation DE*GEF 123*456"),
                                                      Description:                    I18NString.Create("Pool #1, Station #1"),
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
                                                      Name:                           I18NString.Create("EVSE DE*GEF 123*456*1"),
                                                      Description:                    I18NString.Create("Pool #1, Station #1, EVSE #1"),

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

                                        Id:                             NetworkingNode_Id.Parse("cs2"),
                                        VendorName:                     "GraphDefined",
                                        Model:                          "vcs2",
                                        Description:                    I18NString.Create("The second charging station for testing"),
                                        SerialNumber:                   "cs#2",
                                        FirmwareVersion:                "cs-fw v2.0",

                                        EVSEs:                          [
                                                                            new EVSESpec(
                                                                                AdminStatus:         OperationalStatus.Operative,
                                                                                ConnectorTypes:      [ ConnectorType.sType2 ],
                                                                                MeterType:           "myMeter",
                                                                                MeterSerialNumber:   "Meter#2",
                                                                                MeterPublicKey:      "pubkey#2"
                                                                            )
                                                                        ],
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#2",
                                                                            IMSI:        "IMSI#2",
                                                                            CustomData:   null
                                                                        ),

                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient

                                    );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation2.Id);

            var cs2Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation2.Id,
                                                                                "cs2_12345678"
                                                                            );

            var cs2ConnectResult  = await chargingStation2.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS2 to LC"),
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

            chargingStation2_keyPair = ECCKeyPair.GenerateKeys()!;

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

                                        Id:                             NetworkingNode_Id.Parse("cs3"),
                                        VendorName:                     "GraphDefined",
                                        Model:                          "vcs3",
                                        Description:                    I18NString.Create("The third charging station for testing"),
                                        SerialNumber:                   "cs#3",
                                        FirmwareVersion:                "cs-fw v3.0",

                                        EVSEs:                          [
                                                                            new EVSESpec(
                                                                                AdminStatus:         OperationalStatus.Operative,
                                                                                ConnectorTypes:      [ ConnectorType.sType2 ],
                                                                                MeterType:           "myMeter",
                                                                                MeterSerialNumber:   "Meter#3",
                                                                                MeterPublicKey:      "pubkey#3"
                                                                            )
                                                                        ],
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#3",
                                                                            IMSI:        "IMSI#3",
                                                                            CustomData:   null
                                                                        ),

                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient

                                    );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation3.Id);

            var cs3Auth           = ocppLocalController_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                chargingStation3.Id,
                                                                                "cs3_12345678"
                                                                            );

            var cs3ConnectResult  = await chargingStation3.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS3 to LC"),
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

            chargingStation3_keyPair = ECCKeyPair.GenerateKeys()!;

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

            csms1.               Routing.AddOrUpdateStaticRouting(ocppLocalController1.Id,  ocppGateway1.Id);
            csms1.               Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppGateway1.Id);
            csms1.               Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppGateway1.Id);
            csms1.               Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppGateway1.Id);
            csms1.               Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppGateway1.Id);

            csms2.               Routing.AddOrUpdateStaticRouting(ocppLocalController1.Id,  ocppGateway1.Id);
            csms2.               Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppGateway1.Id);
            csms2.               Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppGateway1.Id);
            csms2.               Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppGateway1.Id);
            csms2.               Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppGateway1.Id);

            ocppGateway1.        Routing.AddOrUpdateStaticRouting(NetworkingNode_Id.CSMS,   csms1.Id);  // The default CSMS!
            ocppGateway1.        Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppLocalController1.Id);
            ocppGateway1.        Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppLocalController1.Id);
            ocppGateway1.        Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppLocalController1.Id);
            ocppGateway1.        Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppLocalController1.Id);

            ocppLocalController1.Routing.AddOrUpdateStaticRouting(csms1.Id,                 ocppGateway1.Id);
            ocppLocalController1.Routing.AddOrUpdateStaticRouting(csms2.Id,                 ocppGateway1.Id);


            ocppGateway1.        OCPP.FORWARD.OnAnyJSONRequestFilter += (timestamp,
                                                                         sender,
                                                                         connection,
                                                                         request,
                                                                         cancellationToken) =>

                Task.FromResult(
                    request.NetworkPath.Source == chargingStation3.Id
                        ? RequestForwardingDecision.FORWARD(request, SourceRouting.To(csms2.Id))
                        : RequestForwardingDecision.NEXT   (request)
                );

            #region OnBootNotification

            ocppGateway1.OCPP.FORWARD.OnBootNotificationRequestFilter += (timestamp,
                                                                         sender,
                                                                         connection,
                                                                         request,
                                                                         cancellationToken) =>

                Task.FromResult(
                    request.NetworkPath.Source == chargingStation3.Id
                        ? RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>.FORWARD(request, SourceRouting.To(csms2.Id))
                        : RequestForwardingDecision<BootNotificationRequest, BootNotificationResponse>.FORWARD(request)
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
