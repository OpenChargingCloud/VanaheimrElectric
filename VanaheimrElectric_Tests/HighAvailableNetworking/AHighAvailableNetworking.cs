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
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

using cloud.charging.open.protocols.OCPP;
using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
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
    /// consisting of a single Charging Stations, three OCPP Local Controllers,
    /// an and a Charging Station Management Systems.
    /// 
    /// The HTTP WebSocket connections are initiated in "normal order" from
    /// the Charging Station to the Local Controllers and finally to the CSMS.
    /// 
    /// Between the Charging Station and the Local Controllers the "normal"
    /// OCPP transport JSON array is used, but the Charging Station opens
    /// three connections to three different Local Controllers. Between the
    /// Local Controllers and the CSMS the OCPP Overlay Network transport
    /// is used.
    /// 
    /// This test should show how the OCPP Overlay Network can be used to
    /// build a highly available charging infrastructure using multiple
    /// redundant connections between Charging Stations and the CSMSs.
    /// 
    ///        🡕 [lc1] ⭨
    /// [cs] ──→ [lc2] ━━━► [csms]
    ///        ⭨ [lc3] 🡕
    /// </summary>
    public abstract class AHighAvailableNetworking
    {

        #region Data

        public String                       RFIDUID1  = "11-22-33-44-55-66-77";
        public String                       RFIDUID2  = "AA-BB-CC-55-DD-EE-FF";

        public TestCSMSNode?                csms;
        public OCPPWebSocketServer?         csms_OCPPWebSocketServer;
        public KeyPair?                     csms_keyPair;
        public RoamingNetwork?              csms_roamingNetwork;
        public IChargingStationOperator?    csms_cso;
        public IEMobilityProvider?          csms_emp;
        public EMobilityServiceProvider?    csms_remoteEMP;


        public TestLocalControllerNode?     ocppLocalController1;
        public OCPPWebSocketServer?         ocppLocalController1_OCPPWebSocketServer;
        public KeyPair?                     ocppLocalController_keyPair1;

        public TestLocalControllerNode?     ocppLocalController2;
        public OCPPWebSocketServer?         ocppLocalController2_OCPPWebSocketServer;
        public KeyPair?                     ocppLocalController_keyPair2;

        public TestLocalControllerNode?     ocppLocalController3;
        public OCPPWebSocketServer?         ocppLocalController3_OCPPWebSocketServer;
        public KeyPair?                     ocppLocalController_keyPair3;

        public TestChargingStationNode?     chargingStation;
        public KeyPair?                     chargingStation1_keyPair;
        public IChargingPool?               p1;
        public IChargingStation?            s1;
        public IEVSE?                       e1;

        public DNSClient                    DNSClient;

        #endregion

        #region Constructor(s)

        public AHighAvailableNetworking()
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

            #region Setup Charging Station Management Systems

            csms = new TestCSMSNode(

                       Id:                             NetworkingNode_Id.Parse("csms"),
                       VendorName:                     "GraphDefined",
                       Model:                          "vcsms",
                       Description:                    I18NString.Create("Charging Station Management System for h/a testing"),

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

            csms_OCPPWebSocketServer = csms.AttachWebSocketServer(

                                           HTTPServiceName:              null,
                                           IPAddress:                    null,
                                           TCPPort:                      null,
                                           Description:                  I18NString.Create("Charging Station Management System WebSocket Server"),

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

            csms.UserRoles.Add(
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

            csms.UserRoles.Add(
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

            csms_keyPair = ECCKeyPair.GenerateKeys()!;

            csms.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                          KeyPair:                csms_keyPair!,
                                                          UserIdGenerator:        (signableMessage) => "csms",
                                                          DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Charging Station Management System!"),
                                                          TimestampGenerator:     (signableMessage) => Timestamp.Now);

            //csms.OCPP.SignaturePolicy.AddSigningRule     (SetVariablesRequest.DefaultJSONLDContext,
            //                                              KeyPair:                csms.UserRoles.First()?.KeyPairs.First()!,
            //                                              UserIdGenerator:        (signableMessage) => "csms #1 admin",
            //                                              DescriptionGenerator:   (signableMessage) => I18NString.Create("The admin of OCPP Charging Station Management System #1!"),
            //                                              TimestampGenerator:     (signableMessage) => Timestamp.Now);

            csms.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                          VerificationRuleActions.VerifyAll);

            #endregion


            csms_roamingNetwork          = new RoamingNetwork(
                                               RoamingNetwork_Id.Parse("PROD"),
                                               I18NString.Create("Default EV Roaming Network")
                                           );

            var csms_addCSOResult        = await csms_roamingNetwork.CreateChargingStationOperator(
                                                     Id:                                     ChargingStationOperator_Id.Parse("DE*GEF"),
                                                     Name:                                   I18NString.Create("GraphDefined CSO"),
                                                     Description:                            I18NString.Create("GraphDefined CSO Node 1")
                                                     //RemoteChargingStationOperatorCreator:   cso => new ChargingStationOperatorAdapter(csms, cso)
                                                 );

            csms_cso = csms_addCSOResult.Entity!;





            csms.OCPP.IN.RemoveAllEventHandlers(nameof(csms.OCPP.IN.OnAuthorize));

            csms.OCPP.IN.OnAuthorize += async (timestamp, sender, connection, authorizeRequest, ct) => {

                var cs               = authorizeRequest.NetworkPath.Source;

                var authStartResult  = await csms_roamingNetwork.AuthorizeStart(
                                                 LocalAuthentication:   LocalAuthentication.FromAuthToken(
                                                                            AuthenticationToken.Parse(
                                                                                authorizeRequest.IdToken.Value
                                                                            )
                                                                        ),
                                                 ChargingLocation:      null,
                                                 ChargingProduct:       null,
                                                 SessionId:             null,
                                                 CPOPartnerSessionId:   null,
                                                 OperatorId:            csms_cso.Id,

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
                                                            //HasChargingTariff:     null,
                                                            GroupIdToken:          null,
                                                            Language1:             null,
                                                            Language2:             null,
                                                            PersonalMessage:       null,
                                                            CustomData:            null
                                                        ),
                               CertificateStatus:       null,
                               //AllowedEnergyTransfer:   null,
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
                             //AllowedEnergyTransfer:     null,
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


            var csms_addEMPResult1   = await csms_roamingNetwork.CreateEMobilityServiceProvider(
                                                  Id:            EMobilityProvider_Id.Parse("DE-GDF"),
                                                  Name:          I18NString.Create("GraphDefined EMP"),
                                                  Description:   I18NString.Create("GraphDefined EMP Node 1")
                                              );

            csms_emp                 = csms_addEMPResult1.Entity!;
            csms_remoteEMP           = csms_emp.RemoteEMobilityProvider as EMobilityServiceProvider;

            csms_remoteEMP?.AddToken(
                LocalAuthentication.FromAuthToken(
                    AuthenticationToken.ParseHEX(RFIDUID1)
                ),
                TokenAuthorizationResultType.Authorized
            );

            #endregion


            #region Setup Local Controller 1

            ocppLocalController1                     = new TestLocalControllerNode(

                                                           Id:                             NetworkingNode_Id.Parse("lc1"),
                                                           VendorName:                     "GraphDefined",
                                                           Model:                          "vlc1",
                                                           SerialNumber:                   null,
                                                           SoftwareVersion:                null,
                                                           Modem:                          null,
                                                           Description:                    I18NString.Create("An OCPP Local Controller #1 for testing"),

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

            var ocppLocalControllerAuth1             = csms_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                    ocppLocalController1.Id,
                                                                                    "lc12345678"
                                                                                );

            var ocppLocalController1ConnectResult    = await ocppLocalController1.ConnectOCPPWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms_OCPPWebSocketServer.IPPort}"),
                                                                 VirtualHostname:              null,
                                                                 Description:                  I18NString.Create("LC1 to GW"),
                                                                 PreferIPv4:                   null,
                                                                 RemoteCertificateValidator:   null,
                                                                 LocalCertificateSelector:     null,
                                                                 ClientCert:                   null,
                                                                 TLSProtocol:                  null,
                                                                 HTTPUserAgent:                null,
                                                                 HTTPAuthentication:           ocppLocalControllerAuth1,
                                                                 RequestTimeout:               null,
                                                                 TransmissionRetryDelay:       null,
                                                                 MaxNumberOfRetries:           3,
                                                                 InternalBufferSize:           null,

                                                                 SecWebSocketProtocols:        null,
                                                                 NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                                 NextHopNetworkingNodeId:      csms.Id,
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

            Assert.That(ocppLocalController1ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Local Controller #1 could not connect to the CSMS: {ocppLocalController1ConnectResult.HTTPStatusCode}");


            ocppLocalController1_OCPPWebSocketServer  = ocppLocalController1.AttachWebSocketServer(

                                                            HTTPServiceName:              null,
                                                            IPAddress:                    null,
                                                            TCPPort:                      null,
                                                            Description:                  I18NString.Create("OCPP Local Controller #1 WebSocket Server"),

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

            ocppLocalController_keyPair1 = ECCKeyPair.GenerateKeys()!;

            ocppLocalController1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                          KeyPair:                ocppLocalController_keyPair1!,
                                                                          UserIdGenerator:        (signableMessage) => "lc001",
                                                                          DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Local Controller #1!"),
                                                                          TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppLocalController1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                          VerificationRuleActions.VerifyAll);

            #endregion

            #endregion

            #region Setup Local Controller 2

            ocppLocalController2                     = new TestLocalControllerNode(

                                                           Id:                             NetworkingNode_Id.Parse("lc2"),
                                                           VendorName:                     "GraphDefined",
                                                           Model:                          "vlc2",
                                                           SerialNumber:                   null,
                                                           SoftwareVersion:                null,
                                                           Modem:                          null,
                                                           Description:                    I18NString.Create("An OCPP Local Controller #2 for testing"),

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

            var ocppLocalControllerAuth2             = csms_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                    ocppLocalController2.Id,
                                                                                    "lc22345678"
                                                                                );

            var ocppLocalController2ConnectResult    = await ocppLocalController2.ConnectOCPPWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms_OCPPWebSocketServer.IPPort}"),
                                                                 VirtualHostname:              null,
                                                                 Description:                  I18NString.Create("LC2 to GW"),
                                                                 PreferIPv4:                   null,
                                                                 RemoteCertificateValidator:   null,
                                                                 LocalCertificateSelector:     null,
                                                                 ClientCert:                   null,
                                                                 TLSProtocol:                  null,
                                                                 HTTPUserAgent:                null,
                                                                 HTTPAuthentication:           ocppLocalControllerAuth2,
                                                                 RequestTimeout:               null,
                                                                 TransmissionRetryDelay:       null,
                                                                 MaxNumberOfRetries:           3,
                                                                 InternalBufferSize:           null,

                                                                 SecWebSocketProtocols:        null,
                                                                 NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                                 NextHopNetworkingNodeId:      csms.Id,
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

            Assert.That(ocppLocalController2ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Local Controller #2 could not connect to the CSMS: {ocppLocalController2ConnectResult.HTTPStatusCode}");


            ocppLocalController2_OCPPWebSocketServer  = ocppLocalController2.AttachWebSocketServer(

                                                            HTTPServiceName:              null,
                                                            IPAddress:                    null,
                                                            TCPPort:                      null,
                                                            Description:                  I18NString.Create("OCPP Local Controller #2 WebSocket Server"),

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

            ocppLocalController_keyPair2 = ECCKeyPair.GenerateKeys()!;

            ocppLocalController2.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                          KeyPair:                ocppLocalController_keyPair2!,
                                                                          UserIdGenerator:        (signableMessage) => "lc002",
                                                                          DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Local Controller #2!"),
                                                                          TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppLocalController2.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                          VerificationRuleActions.VerifyAll);

            #endregion

            #endregion

            #region Setup Local Controller 3

            ocppLocalController3                     = new TestLocalControllerNode(

                                                           Id:                             NetworkingNode_Id.Parse("lc3"),
                                                           VendorName:                     "GraphDefined",
                                                           Model:                          "vlc3",
                                                           SerialNumber:                   null,
                                                           SoftwareVersion:                null,
                                                           Modem:                          null,
                                                           Description:                    I18NString.Create("An OCPP Local Controller #3 for testing"),

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

            var ocppLocalControllerAuth3             = csms_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                    ocppLocalController3.Id,
                                                                                    "lc33345678"
                                                                                );

            var ocppLocalController3ConnectResult    = await ocppLocalController3.ConnectOCPPWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{csms_OCPPWebSocketServer.IPPort}"),
                                                                 VirtualHostname:              null,
                                                                 Description:                  I18NString.Create("LC3 to GW"),
                                                                 PreferIPv4:                   null,
                                                                 RemoteCertificateValidator:   null,
                                                                 LocalCertificateSelector:     null,
                                                                 ClientCert:                   null,
                                                                 TLSProtocol:                  null,
                                                                 HTTPUserAgent:                null,
                                                                 HTTPAuthentication:           ocppLocalControllerAuth3,
                                                                 RequestTimeout:               null,
                                                                 TransmissionRetryDelay:       null,
                                                                 MaxNumberOfRetries:           3,
                                                                 InternalBufferSize:           null,

                                                                 SecWebSocketProtocols:        null,
                                                                 NetworkingMode:               NetworkingMode.OverlayNetwork,
                                                                 NextHopNetworkingNodeId:      csms.Id,
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

            Assert.That(ocppLocalController3ConnectResult.HTTPStatusCode.Code, Is.EqualTo(101), $"OCPP Local Controller #3 could not connect to the CSMS: {ocppLocalController3ConnectResult.HTTPStatusCode}");


            ocppLocalController3_OCPPWebSocketServer  = ocppLocalController3.AttachWebSocketServer(

                                                            HTTPServiceName:              null,
                                                            IPAddress:                    null,
                                                            TCPPort:                      null,
                                                            Description:                  I18NString.Create("OCPP Local Controller #3 WebSocket Server"),

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

            ocppLocalController_keyPair3 = ECCKeyPair.GenerateKeys()!;

            ocppLocalController3.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                          KeyPair:                ocppLocalController_keyPair3!,
                                                                          UserIdGenerator:        (signableMessage) => "lc003",
                                                                          DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Local Controller #3!"),
                                                                          TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppLocalController3.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                          VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup chargingStation1

            chargingStation      = new TestChargingStationNode(

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

            ocppLocalController1.AllowedChargingStations.Add(chargingStation.Id);

            var csAuth1           = ocppLocalController1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation.Id,
                                                                                 "cs1a_12345678"
                                                                             );

            var csAuth2           = ocppLocalController2_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation.Id,
                                                                                 "cs1b_12345678"
                                                                             );

            var csAuth3           = ocppLocalController3_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation.Id,
                                                                                 "cs1c_12345678"
                                                                             );


            var csConnectResult1  = await chargingStation.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController1_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS1 to LC1"),
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           csAuth1,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              NextHopNetworkingNodeId:      ocppLocalController1.Id,
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

            Assert.That(csConnectResult1.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station could not connect to OCPP Local Controller #1: {csConnectResult1.HTTPStatusCode}");


            var csConnectResult2  = await chargingStation.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController2_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS1 to LC2"),
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           csAuth2,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              NextHopNetworkingNodeId:      ocppLocalController2.Id,
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

            Assert.That(csConnectResult2.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station could not connect to OCPP Local Controller #2: {csConnectResult2.HTTPStatusCode}");


            var csConnectResult3  = await chargingStation.ConnectOCPPWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController3_OCPPWebSocketServer.IPPort}"),
                                              VirtualHostname:              null,
                                              Description:                  I18NString.Create("CS1 to LC3"),
                                              PreferIPv4:                   null,
                                              RemoteCertificateValidator:   null,
                                              LocalCertificateSelector:     null,
                                              ClientCert:                   null,
                                              TLSProtocol:                  null,
                                              HTTPUserAgent:                null,
                                              HTTPAuthentication:           csAuth3,
                                              RequestTimeout:               null,
                                              TransmissionRetryDelay:       null,
                                              MaxNumberOfRetries:           3,
                                              InternalBufferSize:           null,

                                              SecWebSocketProtocols:        null,
                                              NetworkingMode:               null,
                                              NextHopNetworkingNodeId:      ocppLocalController3.Id,
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

            Assert.That(csConnectResult3.HTTPStatusCode.Code, Is.EqualTo(101), $"Charging Station could not connect to OCPP Local Controller #3: {csConnectResult3.HTTPStatusCode}");

            #endregion


            #region Register User Roles

            //var ur1 = ECCKeyPair.GenerateKeys()!;
            //var ur2 = ECCKeyPair.GenerateKeys()!;

            // {
            //   "private": "ANqBTkO85kZZ44o1jT/Ygxa7JDtVOBUPBtXhtoPYWjgO",
            //   "public":  "BGFDuYqF2x8R4DUb0glpmRDgOpd9+197eQ1jOriP9PaWF013TSU5WedD4YePdEwTXDbdsLPucT8L/cFKTKqCCgQ="
            // }

            // {
            //   "private": "BtSha5ImqfBiNY53aGrU1cQ5hmQ9CheI79+EKJNKNeQ=",
            //   "public":  "BHP5kGwkiii3V7YS7XbG1MEAY9UmxTzo8iEBftaBcstf0xy3qLUhdmtL5DPqs5F9k2mvMZDPzhOQXP1UZlrvefY="
            // }

            chargingStation.UserRoles.Add(
                new UserRole(

                    Id:                      UserRole_Id.Parse ("admin"),
                    Description:             I18NString. Create("The admin user role for the charging station"),
                    KeyPairs:                [ ECCKeyPair.  ParsePublicKey("BGFDuYqF2x8R4DUb0glpmRDgOpd9+197eQ1jOriP9PaWF013TSU5WedD4YePdEwTXDbdsLPucT8L/cFKTKqCCgQ=")! ],

                    //ComponentAccessRights:   null,

                    //SignerName:              null,
                    //Description:             null,
                    //Timestamp:               null,

                    CustomData:              null

                )
            );

            chargingStation.UserRoles.Add(
                new UserRole(

                    Id:                      UserRole_Id.Parse ("user"),
                    Description:             I18NString. Create("The default user role for the charging station"),
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

            chargingStation.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation1_keyPair!,
                                                                      UserIdGenerator:        (signableMessage) => "cs001",
                                                                      DescriptionGenerator:   (signableMessage) => I18NString.Create("Just the 1st OCPP Charging Station!"),
                                                                      TimestampGenerator:     (signableMessage) => Timestamp.Now);

            chargingStation.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                      VerificationRuleActions.VerifyAll);

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
