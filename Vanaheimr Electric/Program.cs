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

using System.Diagnostics;

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

namespace cloud.charging.open.vanaheimr.electric
{

    //using Org.BouncyCastle.Asn1.Sec;
    //using Org.BouncyCastle.Security;
    //using Org.BouncyCastle.Crypto.Parameters;

    //using org.GraphDefined.Vanaheimr.Illias;


    //var secp256r1            = SecNamedCurves.GetByName("secp256r1");
    //var secp256r1Generator   = GeneratorUtilities.GetKeyPairGenerator("ECDH");
    //secp256r1Generator.Init(new ECKeyGenerationParameters(
    //                            new ECDomainParameters(
    //                                secp256r1.Curve,
    //                                secp256r1.G,
    //                                secp256r1.N,
    //                                secp256r1.H,
    //                                secp256r1.GetSeed()
    //                            ),
    //                            new SecureRandom()
    //                        ));

    //var secp256r1KeyPair     = secp256r1Generator.GenerateKeyPair();
    //var secp256r1PrivateKey  = (secp256r1KeyPair.Private as ECPrivateKeyParameters).D.ToByteArray().ToBase64();
    //var secp256r1PublicKey   = (secp256r1KeyPair.Public  as ECPublicKeyParameters). Q.GetEncoded(). ToBase64();


    //var secp521r1            = SecNamedCurves.GetByName("secp521r1");
    //var secp521r1Generator   = GeneratorUtilities.GetKeyPairGenerator("ECDH");
    //secp521r1Generator.Init(new ECKeyGenerationParameters(
    //                            new ECDomainParameters(
    //                                secp521r1.Curve,
    //                                secp521r1.G,
    //                                secp521r1.N,
    //                                secp521r1.H,
    //                                secp521r1.GetSeed()
    //                            ),
    //                            new SecureRandom()
    //                        ));

    //var secp521r1KeyPair     = secp521r1Generator.GenerateKeyPair();
    //var secp521r1PrivateKey  = (secp521r1KeyPair.Private as ECPrivateKeyParameters).D.ToByteArray().ToBase64();
    //var secp521r1PublicKey   = (secp521r1KeyPair.Public  as ECPublicKeyParameters). Q.GetEncoded(). ToBase64();


    public class Program
    {

        #region Data

        private static          TestCSMSNode?                csms1;
        private static readonly IPPort                       csms1_httpAPI_tcpPort                 = IPPort. Parse(5010);
        private static readonly IPPort                       csms1_wss_tcpPort                     = IPPort. Parse(5011);
        private static          OCPPWebSocketServer?         csms1_OCPPWebSocketServer;
        private static readonly KeyPair                      csms1_keyPair                         = KeyPair.ParsePrivateKey("dtJJ1dZeOE9caar/kWdcH1EVC4Yu/S+nGbmDNvEBD1E=")!;
        private static          RoamingNetwork?              csms1_roamingNetwork;
        private static          IChargingStationOperator?    csms1_cso;
        private static          IEMobilityProvider?          csms1_emp;
        private static          EMobilityServiceProvider?    csms1_remoteEMP;

        private static          TestCSMSNode?                csms2;
        private static readonly IPPort                       csms2_httpAPI_tcpPort                 = IPPort. Parse(5020);
        private static readonly IPPort                       csms2_wss_tcpPort                     = IPPort. Parse(5021);
        private static          OCPPWebSocketServer?         csms2_OCPPWebSocketServer;
        private static readonly KeyPair                      csms2_keyPair                         = KeyPair.ParsePrivateKey("Da5Ero3vg9IumqNh+r9TEA7aSHxRccX7eds0VIgLXJs=")!;
        private static          RoamingNetwork?              csms2_roamingNetwork;
        private static          IChargingStationOperator?    csms2_cso;
        private static          EMobilityServiceProvider?    csms2_emp;


        private static          TestGatewayNode?             ocppGateway1;
        private static readonly IPPort                       ocppGateway1_httpAPI_tcpPort          = IPPort. Parse(6000);
        private static readonly IPPort                       ocppGateway1_wss_tcpPort              = IPPort. Parse(6001);
        private static          OCPPWebSocketServer?         ocppGateway1_OCPPWebSocketServer;
        private static readonly KeyPair                      ocppGateway1_keyPair                  = KeyPair.ParsePrivateKey("B2PTBabFjeRCQzgkw1C4DycqrgmWw4ysolqhZTOw4k4=")!;


        private static          TestLocalControllerNode?     ocppLocalController1;
        private static readonly IPPort                       ocppLocalController1_httpAPI_tcpPort  = IPPort. Parse(7000);
        private static readonly IPPort                       ocppLocalController1_wss_tcpPort      = IPPort. Parse(7001);
        private static          OCPPWebSocketServer?         ocppLocalController1_OCPPWebSocketServer;
        private static readonly KeyPair                      ocppLocalController1_keyPair          = KeyPair.ParsePrivateKey("H7k24uNZuvrU4dfsgZBBb0aJHRlGBDLsdGWRMSvTQz4=")!;

        private static          TestEnergyMeterNode?         gridEnergyMeter1;
        private static readonly IPPort                       gridEnergyMeter1_httpAPI_tcpPort      = IPPort. Parse(8000);
        //private static readonly IPPort                       gridEnergyMeter1_tcpPort             = IPPort. Parse(8001);
        private static readonly KeyPair                      gridEnergyMeter1_keyPair              = KeyPair.ParsePrivateKey("UsDGEskGmd0ErWJIREnp8qBeN3NakOhQkvM+Ma9dsK0=")!;


        private static          TestChargingStationNode?     chargingStation1;
        private static readonly IPPort                       chargingStation1_httpAPI_tcpPort      = IPPort. Parse(9010);
        //private static readonly IPPort                       chargingStation1_tcpPort             = IPPort. Parse(9011);
        private static readonly KeyPair                      chargingStation1_keyPair              = KeyPair.ParsePrivateKey("B3T71DARe4dsmqcOcTmqijqcNiSN/4Svsq92ghWute0=")!;
        private static          IChargingPool?               p1;
        private static          IChargingStation?            s1;
        private static          IEVSE?                       e1;

        private static          TestChargingStationNode?     chargingStation2;
        private static readonly IPPort                       chargingStation2_httpAPI_tcpPort      = IPPort. Parse(9020);
        //private static readonly IPPort                       chargingStation2_tcpPort             = IPPort. Parse(9022);
        private static readonly KeyPair                      chargingStation2_keyPair              = KeyPair.ParsePrivateKey("V0losL4KyPjTprMfmP8k/v3nLFRHM5RBGeF0AdTrDLQ=")!;

        private static          TestChargingStationNode?     chargingStation3;
        private static readonly IPPort                       chargingStation3_httpAPI_tcpPort      = IPPort. Parse(9030);
        //private static readonly IPPort                       chargingStation3_tcpPort             = IPPort. Parse(9033);
        private static readonly KeyPair                      chargingStation3_keyPair              = KeyPair.ParsePrivateKey("AJJzOpCMYy5KCPk0uPFmxBVJUNXmK3f1Twnvgnxvts/F")!;

        private static          DNSClient?                   DNSClient;

        #endregion


        /// <summary>
        /// The Vanaheimr Electric example application.
        /// </summary>
        /// <param name="Arguments">Command line arguments</param>
        public async static Task Main(String[] Arguments)
        {

            #region Write processId to pid file

            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "pid"),
                              Environment.ProcessId.ToString());

            #endregion

            #region Interactive process?

            var interactive = ApplicationRunType.GetRunType() == ApplicationRunTypes.MonoConsole ||
                              ApplicationRunType.GetRunType() == ApplicationRunTypes.WindowsConsole;

            if (interactive)
                Console.WriteLine(ApplicationRunType.GetRunType() + " running on " + Environment.MachineName);

            #endregion

            #region Get API version hashes

            var versionHash_Styx                  = (Arguments.Length > 0) ? Arguments[0] : "";
            var versionHash_Hermod                = (Arguments.Length > 1) ? Arguments[1] : "";
            var versionHash_WWCPCore              = (Arguments.Length > 2) ? Arguments[2] : "";
            var versionHash_OCPP                  = (Arguments.Length > 3) ? Arguments[3] : "";

            #endregion

            #region Debug to Console/file

            var debugFile = new TextWriterTraceListener("debug.log");

            var debugTargets = new[] {
                debugFile,
                new TextWriterTraceListener(Console.Out)
            };

            Trace.Listeners.AddRange(debugTargets);

            #endregion



            #region Setup Charging Station Management System 1

            csms1 = new TestCSMSNode(

                        Id:                             NetworkingNode_Id.Parse("csms1"),
                        VendorName:                     "GraphDefined",
                        Model:                          "vcsms1",
                        Description:                    I18NString.Create("Charging Station Management System #1 for testing"),

                        SignaturePolicy:                null,
                        ForwardingSignaturePolicy:      null,

                        DisableSendHeartbeats:          true,
                        SendHeartbeatsEvery:            null,
                        DefaultRequestTimeout:          null,

                        DisableMaintenanceTasks:        false,
                        MaintenanceEvery:               null,

                        HTTPAPI_Disabled:               false,
                        HTTPAPI_Port:                   csms1_httpAPI_tcpPort,
                        HTTPAPI_EventLoggingDisabled:   true,

                        DNSClient:                      DNSClient

                    );

            csms1_OCPPWebSocketServer = csms1.AttachWebSocketServer(

                                            HTTPServiceName:              "Charging Station Management System #1",
                                            IPAddress:                    null,
                                            TCPPort:                      csms1_wss_tcpPort,
                                            Description:                  I18NString.Create("Charging Station Management System #1 Web Socket Server"),

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

            //csms1_remoteEMP?.AddToken(
            //    LocalAuthentication.FromAuthToken(
            //        AuthenticationToken.ParseHEX(RFIDUID1)
            //    ),
            //    TokenAuthorizationResultType.Authorized
            //);

            #endregion

            #region Setup Charging Station Management System 2

            csms2 = new TestCSMSNode(

                        Id:                             NetworkingNode_Id.Parse("csms2"),
                        VendorName:                     "GraphDefined",
                        Model:                          "vcsms21",
                        Description:                    I18NString.Create("Charging Station Management System #2 for testing"),

                        SignaturePolicy:                null,
                        ForwardingSignaturePolicy:      null,

                        DisableSendHeartbeats:          true,
                        SendHeartbeatsEvery:            null,
                        DefaultRequestTimeout:          null,

                        DisableMaintenanceTasks:        false,
                        MaintenanceEvery:               null,

                        HTTPAPI_Disabled:               false,
                        HTTPAPI_Port:                   csms2_httpAPI_tcpPort,
                        HTTPAPI_EventLoggingDisabled:   true,

                        DNSClient:                      DNSClient

                    );

            csms2_OCPPWebSocketServer = csms2.AttachWebSocketServer(

                                            HTTPServiceName:              "Charging Station Management System #2",
                                            IPAddress:                    null,
                                            TCPPort:                      csms2_wss_tcpPort,
                                            Description:                  I18NString.Create("Charging Station Management System #2 Web Socket Server"),

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

                                                   HTTPAPI_Disabled:               false,
                                                   HTTPAPI_Port:                   ocppGateway1_httpAPI_tcpPort,
                                                   HTTPAPI_EventLoggingDisabled:   true,

                                                   DNSClient:                      DNSClient

                                               );

            #region Connect to CSMS1

            var ocppGatewayAuth1             = csms1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                             ocppGateway1.Id,
                                                                             "gw2csms1_12345678"
                                                                         );

            var ocppGatewayConnectResult1    = await ocppGateway1.ConnectWebSocketClient(

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

            if (ocppGatewayConnectResult1.HTTPStatusCode.Code != 101)
                throw new Exception($"OCPP Gateway could not connect to CSMS #1: {ocppGatewayConnectResult1.HTTPStatusCode}");

            #endregion

            #region Connect to CSMS2

            var ocppGatewayAuth2             = csms2_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                             ocppGateway1.Id,
                                                                             "gw2csms2_12345678"
                                                                         );

            var ocppGatewayConnectResult2    = await ocppGateway1.ConnectWebSocketClient(

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

                                                   DisableWebSocketPings:        true,
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

            if (ocppGatewayConnectResult2.HTTPStatusCode.Code != 101)
                throw new Exception($"OCPP Gateway could not connect to CSMS #2: {ocppGatewayConnectResult2.HTTPStatusCode}");

            #endregion


            ocppGateway1_OCPPWebSocketServer = ocppGateway1.AttachWebSocketServer(

                                                   HTTPServiceName:              "OCPP Gateway",
                                                   IPAddress:                    null,
                                                   TCPPort:                      ocppGateway1_wss_tcpPort,
                                                   Description:                  I18NString.Create("OCPP Gateway Web Socket Server"),

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

            ocppGateway1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                 KeyPair:                ocppGateway1_keyPair,
                                                                 UserIdGenerator:        (signableMessage) => "gw001",
                                                                 DescriptionGenerator:   (signableMessage) => I18NString.Create("Just an OCPP Gateway!"),
                                                                 TimestampGenerator:     (signableMessage) => Timestamp.Now);

            ocppGateway1.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                 VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup Local Controller

            ocppLocalController1                      = new TestLocalControllerNode(

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

                                                           HTTPAPI_Disabled:               false,
                                                           HTTPAPI_Port:                   ocppLocalController1_httpAPI_tcpPort,
                                                           HTTPAPI_EventLoggingDisabled:   true,

                                                           DNSClient:                      DNSClient

                                                       );

            var ocppLocalControllerAuth              = ocppGateway1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                           ocppLocalController1.Id,
                                                                                           "lc12345678"
                                                                                       );

            var ocppLocalControllerConnectResult     = await ocppLocalController1.ConnectWebSocketClient(

                                                                 RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppGateway1_OCPPWebSocketServer.IPPort}"),
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

            if (ocppLocalControllerConnectResult.HTTPStatusCode.Code != 101)
                throw new Exception($"OCPP Local Controller could not connect to OCPP Gateway: {ocppLocalControllerConnectResult.HTTPStatusCode}");


            ocppLocalController1_OCPPWebSocketServer  = ocppLocalController1.AttachWebSocketServer(

                                                          HTTPServiceName:              "OCPP Local Controller",
                                                          IPAddress:                    null,
                                                          TCPPort:                      ocppLocalController1_wss_tcpPort,
                                                          Description:                  I18NString.Create("OCPP Local Controller Web Socket Server"),

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

            

            ocppLocalController1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                         KeyPair:                ocppLocalController1_keyPair,
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

                                                       HTTPAPI_Disabled:               false,
                                                       HTTPAPI_Port:                   gridEnergyMeter1_httpAPI_tcpPort,
                                                       HTTPAPI_EventLoggingDisabled:   true,

                                                       DNSClient:                      DNSClient

                                                   );

            var gridEnergyMeter1Auth             = ocppLocalController1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                                gridEnergyMeter1.Id,
                                                                                                "em12345678"
                                                                                            );

            var gridEnergyMeter1ConnectResult    = await gridEnergyMeter1.ConnectWebSocketClient(

                                                             RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController1_OCPPWebSocketServer.IPPort}"),
                                                             VirtualHostname:              null,
                                                             Description:                  I18NString.Create("EM to LC"),
                                                             PreferIPv4:                   null,
                                                             RemoteCertificateValidator:   null,
                                                             LocalCertificateSelector:     null,
                                                             ClientCert:                   null,
                                                             TLSProtocol:                  null,
                                                             HTTPUserAgent:                null,
                                                             HTTPAuthentication:           gridEnergyMeter1Auth,
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

            if (gridEnergyMeter1ConnectResult.HTTPStatusCode.Code != 101)
                throw new Exception($"OCPP Energy Meter could not connect to OCPP Local Controller: {gridEnergyMeter1ConnectResult.HTTPStatusCode}");


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

            gridEnergyMeter1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                     KeyPair:                gridEnergyMeter1_keyPair,
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
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#1",
                                                                            IMSI:        "IMSI#1",
                                                                            CustomData:   null
                                                                        ),

                                        EVSEs:                          [
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
                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_Disabled:               false,
                                        HTTPAPI_Port:                   chargingStation1_httpAPI_tcpPort,
                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient

                                    );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation1.Id);

            var cs1Auth           = ocppLocalController1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation1.Id,
                                                                                 "cs1_12345678"
                                                                             );

            var cs1ConnectResult  = await chargingStation1.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController1_OCPPWebSocketServer.IPPort}"),
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

            if (cs1ConnectResult.HTTPStatusCode.Code != 101)
                throw new Exception($"Charging Station #1 could not connect to OCPP Local Controller: {cs1ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            

            chargingStation1.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation1_keyPair,
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
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#2",
                                                                            IMSI:        "IMSI#2",
                                                                            CustomData:   null
                                                                        ),

                                        EVSEs:                          [
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
                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_Disabled:               false,
                                        HTTPAPI_Port:                   chargingStation2_httpAPI_tcpPort,
                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient

                                    );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation2.Id);

            var cs2Auth           = ocppLocalController1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation2.Id,
                                                                                 "cs2_12345678"
                                                                             );

            var cs2ConnectResult  = await chargingStation2.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController1_OCPPWebSocketServer.IPPort}"),
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

            if (cs2ConnectResult.HTTPStatusCode.Code != 101)
                throw new Exception($"Charging Station #2 could not connect to OCPP Local Controller: {cs2ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            chargingStation2.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation2_keyPair,
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
                                        Modem:                          new Modem(
                                                                            ICCID:       "ICCID#3",
                                                                            IMSI:        "IMSI#3",
                                                                            CustomData:   null
                                                                        ),

                                        EVSEs:                          [
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
                                        UplinkEnergyMeter:              null,

                                        DefaultRequestTimeout:          null,

                                        SignaturePolicy:                null,
                                        ForwardingSignaturePolicy:      null,

                                        DisableSendHeartbeats:          true,
                                        SendHeartbeatsEvery:            null,

                                        DisableMaintenanceTasks:        false,
                                        MaintenanceEvery:               null,

                                        HTTPAPI_Disabled:               false,
                                        HTTPAPI_Port:                   chargingStation3_httpAPI_tcpPort,
                                        HTTPAPI_EventLoggingDisabled:   true,

                                        CustomData:                     null,
                                        DNSClient:                      DNSClient

                                    );

            ocppLocalController1.AllowedChargingStations.Add(chargingStation3.Id);

            var cs3Auth           = ocppLocalController1_OCPPWebSocketServer.AddOrUpdateHTTPBasicAuth(
                                                                                 chargingStation3.Id,
                                                                                 "cs3_12345678"
                                                                             );

            var cs3ConnectResult  = await chargingStation3.ConnectWebSocketClient(

                                              RemoteURL:                    URL.Parse($"ws://127.0.0.1:{ocppLocalController1_OCPPWebSocketServer.IPPort}"),
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

            if (cs3ConnectResult.HTTPStatusCode.Code != 101)
                throw new Exception($"Charging Station #3 could not connect to OCPP Local Controller: {cs3ConnectResult.HTTPStatusCode}");

            #region Define signature policy

            chargingStation3.OCPP.SignaturePolicy.AddSigningRule     (JSONContext.OCPP.Any,
                                                                      KeyPair:                chargingStation3_keyPair,
                                                                      UserIdGenerator:        (signableMessage) => "cs003",
                                                                      DescriptionGenerator:   (signableMessage) => I18NString.Create("Just the 3st OCPP Charging Station!"),
                                                                      TimestampGenerator:     (signableMessage) => Timestamp.Now);

            chargingStation3.OCPP.SignaturePolicy.AddVerificationRule(JSONContext.OCPP.Any,
                                                                      VerificationRuleActions.VerifyAll);

            #endregion

            #endregion


            #region Setup static OCPP Routing

            csms1.               OCPP.Routing.AddOrUpdateStaticRouting(ocppLocalController1.Id,  ocppGateway1.Id);
            csms1.               OCPP.Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppGateway1.Id);
            csms1.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppGateway1.Id);
            csms1.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppGateway1.Id);
            csms1.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppGateway1.Id);

            csms2.               OCPP.Routing.AddOrUpdateStaticRouting(ocppLocalController1.Id,  ocppGateway1.Id);
            csms2.               OCPP.Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppGateway1.Id);
            csms2.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppGateway1.Id);
            csms2.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppGateway1.Id);
            csms2.               OCPP.Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppGateway1.Id);

            ocppGateway1.        OCPP.Routing.AddOrUpdateStaticRouting(NetworkingNode_Id.CSMS,   csms1.Id);  // The default CSMS!
            ocppGateway1.        OCPP.Routing.AddOrUpdateStaticRouting(gridEnergyMeter1.Id,      ocppLocalController1.Id);
            ocppGateway1.        OCPP.Routing.AddOrUpdateStaticRouting(chargingStation1.Id,      ocppLocalController1.Id);
            ocppGateway1.        OCPP.Routing.AddOrUpdateStaticRouting(chargingStation2.Id,      ocppLocalController1.Id);
            ocppGateway1.        OCPP.Routing.AddOrUpdateStaticRouting(chargingStation3.Id,      ocppLocalController1.Id);

            ocppLocalController1.OCPP.Routing.AddOrUpdateStaticRouting(csms1.Id,                 ocppGateway1.Id);
            ocppLocalController1.OCPP.Routing.AddOrUpdateStaticRouting(csms2.Id,                 ocppGateway1.Id);

            #endregion


            #region Wait for key 'Q' pressed... and quit.

            Console.WriteLine();
            Console.WriteLine("finished...");

            if (!interactive)
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);

            var command = String.Empty;

            do
            {

                command = Console.ReadLine()?.ToLower();

            } while (command != "q");

            foreach (var debugListener in Trace.Listeners)
                (debugListener as TextWriterTraceListener)?.Flush();

            debugFile.Flush();

            #endregion

            #region Shutdown everything

            Console.WriteLine("Shutting down...");

            await csms1_OCPPWebSocketServer.               Shutdown(Wait: true);
            await csms2_OCPPWebSocketServer.               Shutdown(Wait: true);
            await ocppGateway1_OCPPWebSocketServer.        Shutdown(Wait: true);
            await ocppLocalController1_OCPPWebSocketServer.Shutdown(Wait: true);

            #endregion

        }

    }

}
