/*
 * Copyright (c) 2015-2023 GraphDefined GmbH
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

using System.Collections.Concurrent;

using NUnit.Framework;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Logging;

using cloud.charging.open.protocols.OCPI;
using cloud.charging.open.protocols.OCPIv2_1_1;
using cloud.charging.open.protocols.OCPIv2_1_1.HTTP;
using cloud.charging.open.protocols.OCPIv2_1_1.WebAPI;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.MobilityProvider;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// The charging infrastructure test defaults for Vanaheimr Electric.
    /// </summary>
    public abstract class AChargingInfrastructure
    {

        #region Data

        public          DNSClient                                      dnsClient;
        public          RoamingNetwork_Id                              RoamingNetworkPROD;

        public          ChargingNode?                                  OpenChargingCloudPKI_Node1;
        public          ChargingNode?                                  OpenChargingCloudPKI_Node2;
        public          ChargingNode?                                  OpenChargingCloudPKI_Node3;

        public          ChargingNode?                                  PTB_Node1;


        public          ChargingNode?                                  GraphDefinedMetering_Node1;

        public          ChargingNode?                                  GraphDefinedStationA1;
        public          ChargingNode?                                  GraphDefinedStationA2;
        public          ChargingNode?                                  GraphDefinedStationA3;

        public          ChargingNode?                                  GraphDefinedPoolB;
        public          ChargingNode?                                  GraphDefinedStationB1;
        public          ChargingNode?                                  GraphDefinedStationB2;
        public          ChargingNode?                                  GraphDefinedStationB3;

        public          ChargingNode?                                  GraphDefinedCSO_Node1;
        public          ChargingNode?                                  GraphDefinedCSO_Node2;
        public          ChargingNode?                                  GraphDefinedCSO_Node3;

        public          ChargingNode?                                  GraphDefinedEMP_Node1;
        public          ChargingNode?                                  GraphDefinedEMP_Node2;
        public          ChargingNode?                                  GraphDefinedEMP_Node3;

        public          ChargingNode?                                  Hubject_Node1;
        public          ChargingNode?                                  Hubject_Node2;
        public          ChargingNode?                                  Hubject_Node3;



        // old

        public          URL?                                           cpoVersionsAPIURL;
        public          URL?                                           emsp1VersionsAPIURL;
        public          URL?                                           emsp2VersionsAPIURL;

        protected       HTTPAPI?                                       cpoHTTPAPI;
        protected       CommonAPI?                                     cpoCommonAPI;
        protected       OCPIWebAPI?                                    cpoWebAPI;
        protected       CPOAPI?                                        cpoCPOAPI;
        protected       OCPICSOAdapter?                                cpoAdapter;
        protected       ConcurrentDictionary<DateTime, OCPIRequest>    cpoAPIRequestLogs;
        protected       ConcurrentDictionary<DateTime, OCPIResponse>   cpoAPIResponseLogs;

        protected       HTTPAPI?                                       emsp1HTTPAPI;
        protected       CommonAPI?                                     emsp1CommonAPI;
        protected       OCPIWebAPI?                                    emsp1WebAPI;
        protected       EMSPAPI?                                       emsp1EMSPAPI;
        protected       OCPIEMPAdapter?                                emsp1Adapter;
        protected       ConcurrentDictionary<DateTime, OCPIRequest>    emsp1APIRequestLogs;
        protected       ConcurrentDictionary<DateTime, OCPIResponse>   emsp1APIResponseLogs;

        protected       HTTPAPI?                                       emsp2HTTPAPI;
        protected       CommonAPI?                                     emsp2CommonAPI;
        protected       OCPIWebAPI?                                    emsp2WebAPI;
        protected       EMSPAPI?                                       emsp2EMSPAPI;
        protected       OCPIEMPAdapter?                                emsp2Adapter;
        protected       ConcurrentDictionary<DateTime, OCPIRequest>    emsp2APIRequestLogs;
        protected       ConcurrentDictionary<DateTime, OCPIResponse>   emsp2APIResponseLogs;

        protected const String                                         cpo_accessing_emsp1__token    = "cpo_accessing_emsp1++token";
        protected const String                                         cpo_accessing_emsp2__token    = "cpo_accessing_emsp2++token";

        protected const String                                         emsp1_accessing_cpo__token    = "emsp1_accessing_cpo++token";
        protected const String                                         emsp2_accessing_cpo__token    = "emsp2_accessing_cpo++token";

        protected const String                                         UnknownToken                  = "UnknownUnknownUnknownToken";

        protected const String                                         BlockedCPOToken               = "blocked-cpo";
        protected const String                                         BlockedEMSPToken              = "blocked-emsp";

        protected  RoamingNetwork?            csoRoamingNetwork;
        protected  IChargingStationOperator?  graphDefinedCSO;

        protected  VirtualEMobilityProvider?  graphDefinedEMP1Local;
        protected  RoamingNetwork?            emp1RoamingNetwork;
        protected  IEMobilityProvider?        graphDefinedEMP1;
        protected  EMobilityProviderAPI?      graphDefinedEMP1API;

        protected  VirtualEMobilityProvider?  graphDefinedEMP2Local;
        protected  RoamingNetwork?            emp2RoamingNetwork;
        protected  IEMobilityProvider?        graphDefinedEMP2;
        protected  EMobilityProviderAPI?      graphDefinedEMP2API;

        protected  VirtualSmartPhone?         ahzfPhone;
        protected  EVehicle?                  ahzfCar;

        #endregion

        #region Constructor(s)

        public AChargingInfrastructure()
        {

            this.dnsClient           = new();
            this.RoamingNetworkPROD  = RoamingNetwork_Id.Parse("PROD");

        }

        #endregion


        #region SetupOnce()

        [OneTimeSetUp]
        public void SetupOnce()
        {

            #region Open Charging Cloud PKI Service

            #region Node 1

            var OpenChargingCloudPKI_Node1_HTTPPort = 3001;

            OpenChargingCloudPKI_Node1  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node1"),
                                             Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 1 of 3"),
                                             Identity:         new CryptoKeyInfo(
                                                                   "BAFZihMJdtrB5ixV7guNQAoVY46QO/AI7xF/7DOiZh8kANGOKI+EvcLxVYSs6wO/mhAj8SvwmltQ/Cs2iFuEuGQ4FQFRGBf16cG5dM4jfS0LewqfeATjtmT9AjQgyJu41EDiqf8g/vogBO0NHBq9wGzlseasWlnRlbGvqzGr6EoVmA1C7A==",
                                                                   "ATvTmppPVADEbTCujGnIiq5pu2viKXh4wuIqR/tyEcJrILc0tBhzUAUbGPZmUyVPq1RTaVQE12p5fJc7kIETxfLe",
                                                                   CryptoKeyUsage.Identity
                                                               ),
                                             IdentityGroup:    new CryptoKeyInfo(
                                                                   "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                   "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                   CryptoKeyUsage.IdentityGroup
                                                               ),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node1_HTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                          );


            #endregion

            #region Node 2

            var OpenChargingCloudPKI_Node2_HTTPPort = 3002;

            OpenChargingCloudPKI_Node2  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node2"),
                                             Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 2 of 3"),
                                             Identity:         new CryptoKeyInfo(
                                                                   "BAEi3ZlHFUtzyav3FzbYpNzjHwnMGp2xCQPEK24hSlzYTl4AEkcxnPJzAGD/MQBnhCitsnLOKDACBWrwwGgbYwZxBwHB+dAD9NQgov/kAOphWP9ZzQXfyslPpRTZFHEu0ZJ4RSKDj8K53f1tUbMjdEZVMX2+HsAEeWr5E1Af7xWIUusd4g==",
                                                                   "ALX4MIq5wuUEKa0FoOgD0LyoTClxoYaFOBSoDGB88uok0uPmWXMxS+49A5sn5DuFghZRA6yfN17Qt6S/JZn8z2Hf",
                                                                   CryptoKeyUsage.Identity
                                                               ),
                                             IdentityGroup:    new CryptoKeyInfo(
                                                                   "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                   "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                   CryptoKeyUsage.IdentityGroup
                                                               ),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node2_HTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                          );


            #endregion

            #region Node 3

            var OpenChargingCloudPKI_Node3_HTTPPort = 3003;

            OpenChargingCloudPKI_Node3  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node3"),
                                             Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 3 of 3"),
                                             Identity:         new CryptoKeyInfo(
                                                                   "BAHg2Wzfj4Xm5DtJzCUIxu/niGxY2cO1hf3jh2azthT3Y+nip6BpcvWcPy+BR6Rj9h/qbgukAimfTqLNUsjDW709EgF+mtGy6JJAm3e1jxtRZcc//s+K7Q6Qw63zAnpnNIPJTHKgag6LY/VaP9a8hYJZOg5xwiCEH9Vkg9rIHziOYMHhiw==",
                                                                   "AcwWAmnwzfjsePCg9u02ustsaOR/916i8jrK6nv7mJmt9+t5gXg0XVm7UXIvGtoi8wB97LTY34RSK0q7vzSQPQVj",
                                                                   CryptoKeyUsage.Identity
                                                               ),
                                             IdentityGroup:    new CryptoKeyInfo(
                                                                   "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                   "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                   CryptoKeyUsage.IdentityGroup
                                                               ),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node3_HTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                          );


            #endregion

            #endregion

            #region PTB

            var PTB_Node1_HTTPPort = 3101;

            PTB_Node1  = new ChargingNode(
                             Name:             I18NString.Create(Languages.en, "PTB_Node1"),
                             Description:      I18NString.Create(Languages.de, "Physikalisch-Technische Bundesanstalt - Node 1 of 3"),
                             Identity:         new CryptoKeyInfo(
                                                   "BAHyie2ntlBNEJaiY0kFyGkTXf3uBL0gyQIqKQZmgwLec550jmzztm52CPUBQLB1wtxwIUzGobcmtbwk7SsHdBFalgDrCE8UQY2G6rD7TocQWs+0UOAMuTktUUO04JQUs2Ea1CSZ5YvfjVi0ZXC/y+Ui1tqMKtfW5SOLHJ8AkfhPrdSN8A==",
                                                   "AUU/uHl2kQ7abGQgJmulyxBZeLpY2YK346Bn6dnTTQelr9oYs0lW3pOJwDTcV/iUdGNDPCgOcqfITUuukVQRLU6W",
                                                   CryptoKeyUsage.Identity
                                               ),
                             IdentityGroup:    new CryptoKeyInfo(
                                                   "BAFOQI8mUNfqgPp7SuKF5oQ1CJK/CjHPR3g+2cc8w4rtuESjAYD+uHlQdA67/IZnoamZY+EMWWQzaoQ5W52c7PM3xAH3nYfJkKKhS2oFjPkXZDaAR81tSkJ0YLgr25tce9m0KD8iYBlb83sG/S8dOd2L25il9Q8SSOIwUbmVN1jONTlfIQ==",
                                                   "I59i1RwKSncsy/FY1Rb3PV0Ev9pj0HBae9EeU9Uau6ee3AgtZPGpmKRLwgVS8o7HyZA2ORLvbVvk1HJxuLBjloo=",
                                                   CryptoKeyUsage.IdentityGroup
                                               ),
                             DefaultHTTPAPI:   new HTTPAPI(
                                                   HTTPServerPort:  IPPort.Parse(PTB_Node1_HTTPPort),
                                                   DNSClient:       dnsClient,
                                                   Autostart:       true
                                               ),
                             DNSClient:        dnsClient
                          );

            #endregion

            #region GraphDefined Metering

            var GraphDefinedMetering_Node1_HTTPPort = 3201;

            GraphDefinedMetering_Node1 = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "PTB_Node1"),
                                             Description:      I18NString.Create(Languages.de, "Physikalisch-Technische Bundesanstalt - Node 1 of 3"),
                                             Identity:         new CryptoKeyInfo(
                                                                   "BAClhSA92s1KInjXUqZ8L89Y2y80fJnaQSyuBPCknjxeYQhuc+hCD5dDED+uOd4Ccms+xr/sJEIIKSljLdiaAdxkWQEJNzMJbIKgfyKfNjEYNphNYBKt0udayF5c/ZqHTO+oWAYY304ojjs9WAbPWx0CSB7qjHIRCWLgV1BTKE/naVQgBA==",
                                                                   "VAL7TXDRnNy9Mc7ENCtbHQ3EDCCOUuSiNgfyP6NEcumeh7kPAguhibCFuFqb6jzjn5XK6/fPv4B5JgQcgxilDVc=",
                                                                   CryptoKeyUsage.Identity
                                                               ),
                                             IdentityGroup:    new CryptoKeyInfo(
                                                                   "BAF6HcyIwIstkj7u9KzZz9dYfMPfxMdhqZyXT2G9ZSZxqfzCDU+N2sRfD8a63/LTHheD5opC5I/1AjbTX2t9abLnhAHq1vUuYNkWpOzmVHJWel6cnRgKVcqsS4z8XTImU8D45LgLfny1lFaGqBqp0B9gyfoOyvFxX4NlpgYMjFb1jdsOig==",
                                                                   "AYSbfXi+895sYLiiAHsxWsButKu4JurjDoDkZSA7g31V+SMs3MD0homVHIqYqbZqP5dS24fjsZQUvDv0c8kcrsHP",
                                                                   CryptoKeyUsage.IdentityGroup
                                                               ),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(GraphDefinedMetering_Node1_HTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                          );

            #endregion

            #region GraphDefined CSO

            var graphDefinedCSOHTTPPort = 3301;

            GraphDefinedCSO_Node1  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "GraphDefinedCSO_Node1"),
                                             Description:      I18NString.Create(Languages.en, "GraphDefined Charging Station Operator - Node 1 of 3"),
                                             Identity:         new CryptoKeyInfo(
                                                                   "BLMl4daDO/n+kf3Qz0kcBwG0cGxoDCGQNz/H6W21VL8cAq3l+vgJeNWR50MhoSyVOvI+x8YZRpoBdMl5AzKebaE=",
                                                                   "APKdkn4o9ozmwJ+WFxEJ6qxnsWf6p6Tghcy7aQThJLqq",
                                                                   CryptoKeyUsage.Identity,
                                                                   CryptoKeyType.SecP256r1
                                                               ),
                                             IdentityGroup:    new CryptoKeyInfo(
                                                                   "BKDXmC6yyP1OMlDPgsE67fsfII90cskSVkd993EtW+1/1TY5KJAGSpEsQTGdGruKH4sKDztqNbxXpUYicfYUzQ4=",
                                                                   "AOawdrgs0v4cQWmg4vokRzp2cxq+1V2ftr4oVTB+A6we",
                                                                   CryptoKeyUsage.IdentityGroup,
                                                                   CryptoKeyType.SecP256r1
                                                               ),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(graphDefinedCSOHTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                         );;

            Assert.IsNotNull(GraphDefinedCSO_Node1);

            GraphDefinedCSO_Node1.CreateNewRoamingNetwork(
                                      RoamingNetworkPROD,
                                      Name:                 I18NString.Create(Languages.en, "Roaming Network PROD"),
                                      Description:          I18NString.Create(Languages.en, "Roaming Network PROD"),
                                      InitialAdminStatus:   RoamingNetworkAdminStatusTypes.Operational,
                                      InitialStatus:        RoamingNetworkStatusTypes.Available
                                  );

            #region OICP v2.3

            #endregion

            #region OICP v2.3 p2p

            #endregion

            #region OCPI v2.2

            #endregion

            #region OCPI v2.1

            cpoVersionsAPIURL = URL.Parse($"http://127.0.0.1:{graphDefinedCSOHTTPPort}/ocpi/v2.1/versions");

            cpoCommonAPI         = new CommonAPI(

                                       OurBaseURL:                          URL.Parse($"http://127.0.0.1:{graphDefinedCSOHTTPPort}/ocpi/v2.1"),
                                       OurVersionsURL:                      cpoVersionsAPIURL.Value,
                                       OurBusinessDetails:                  new BusinessDetails(
                                                                                "GraphDefined CSO OCPI v2.1 Services",
                                                                                URL.Parse("https://www.graphdefined.com/cso")
                                                                            ),
                                       OurCountryCode:                      CountryCode.Parse("DE"),
                                       OurPartyId:                          Party_Id.   Parse("GEF"),
                                       OurRole:                             Roles.      CPO,

                                       HTTPServer:                          GraphDefinedCSO_Node1.DefaultHTTPAPI!.HTTPServer,

                                       AdditionalURLPathPrefix:             null,
                                       KeepRemovedEVSEs:                    null,
                                       LocationsAsOpenData:                 true,
                                       AllowDowngrades:                     null,
                                       Disable_RootServices:                false,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_CSO.log",
                                       LogfileCreator:                      null,
                                       DatabaseFilePath:                    null,
                                       RemotePartyDBFileName:               $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_RemoteParties_CPO.log",
                                       AssetsDBFileName:                    $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_Assets_CPO.log",
                                       Autostart:                           false

                                   );

            #endregion

            #endregion

            #region GraphDefined EMP

            var graphDefinedEMPHTTPPort = 3301;

            GraphDefinedEMP_Node1 = new ChargingNode(
                                        Name:             I18NString.Create(Languages.en, "GraphDefinedEMP_Node1"),
                                        Description:      I18NString.Create(Languages.en, "GraphDefined E-Mobility Provider - Node 1 of 3"),
                                        Identity:         new CryptoKeyInfo(
                                                              "BCphDksQSmKHJKMVcbPv8ABAYwM4oPz+lW47A5mS0V986jhZpjRo+a4bhhnBDtbvbWgObe0t5Z4fC63IxXG/Y8k=",
                                                              "PiE4dMglv6W5km72JIdPL2adDF3PdBuRaoitaX8+L8g=",
                                                              CryptoKeyUsage.Identity,
                                                              CryptoKeyType.SecP256r1
                                                          ),
                                        IdentityGroup:    new CryptoKeyInfo(
                                                              "BEycchG1xCCAbgIii15U6s4P9D91u5TdS66T/Qf2bnBAi83jilLce56W8p9B6f6PwQyj2Rjw0b7z228ZElSTDtM=",
                                                              "ALRCex+A7QwYU0rdlFrS6Q19vTXeccaGtzXkT66mt8Hh",
                                                              CryptoKeyUsage.IdentityGroup,
                                                              CryptoKeyType.SecP256r1
                                                          ),
                                        DefaultHTTPAPI:   new HTTPAPI(
                                                              HTTPServerPort:  IPPort.Parse(graphDefinedEMPHTTPPort),
                                                              DNSClient:       dnsClient,
                                                              Autostart:       true
                                                          ),
                                        DNSClient:        dnsClient
                                    );

            Assert.IsNotNull(GraphDefinedEMP_Node1);

            GraphDefinedEMP_Node1.CreateNewRoamingNetwork(
                                      RoamingNetworkPROD,
                                      Name:                 I18NString.Create(Languages.en, "Roaming Network PROD"),
                                      Description:          I18NString.Create(Languages.en, "Roaming Network PROD"),
                                      InitialAdminStatus:   RoamingNetworkAdminStatusTypes.Operational,
                                      InitialStatus:        RoamingNetworkStatusTypes.Available
                                  );

            #region OICP v2.3

            #endregion

            #region OICP v2.3 p2p

            #endregion

            #region OCPI v2.1

            emsp1VersionsAPIURL  = URL.Parse($"http://127.0.0.1:{graphDefinedEMPHTTPPort}/ocpi/v2.1/versions");

            emsp1CommonAPI       = new CommonAPI(

                                       OurBaseURL:                          URL.Parse($"http://127.0.0.1:{graphDefinedEMPHTTPPort}/ocpi/v2.1"),
                                       OurVersionsURL:                      emsp1VersionsAPIURL.Value,
                                       OurBusinessDetails:                  new BusinessDetails(
                                                                                "GraphDefined EMP OCPI v2.1 Services",
                                                                                URL.Parse("https://www.graphdefined.com/emp")
                                                                            ),
                                       OurCountryCode:                      CountryCode.Parse("DE"),
                                       OurPartyId:                          Party_Id.   Parse("GDF"),
                                       OurRole:                             Roles.      EMSP,

                                       HTTPServer:                          GraphDefinedEMP_Node1.DefaultHTTPAPI!.HTTPServer,

                                       AdditionalURLPathPrefix:             null,
                                       KeepRemovedEVSEs:                    null,
                                       LocationsAsOpenData:                 true,
                                       AllowDowngrades:                     null,
                                       Disable_RootServices:                false,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_EMSP1.log",
                                       LogfileCreator:                      null,
                                       DatabaseFilePath:                    null,
                                       RemotePartyDBFileName:               $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_RemoteParties_EMSP1.log",
                                       AssetsDBFileName:                    $"GraphDefined_OCPI{protocols.OCPIv2_1_1.Version.String}_Assets_EMSP1.log",
                                       Autostart:                           false

                                   );

            #endregion

            #region OCPI v2.2

            #endregion

            #endregion

            #region Hubject

            var hubjectHTTPPort = 3501;

            Hubject_Node1 = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "Hubject_Node1"),
                                             Description:      I18NString.Create(Languages.en, "Hubject Central EV Roaming Hub - Node 1 of 3"),
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(hubjectHTTPPort),
                                                                   DNSClient:       dnsClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        dnsClient
                                         );

            Assert.IsNotNull(Hubject_Node1);

            Hubject_Node1.CreateNewRoamingNetwork(
                              RoamingNetworkPROD,
                              Name:                 I18NString.Create(Languages.en, "Roaming Network PROD"),
                              Description:          I18NString.Create(Languages.en, "Roaming Network PROD"),
                              InitialAdminStatus:   RoamingNetworkAdminStatusTypes.Operational,
                              InitialStatus:        RoamingNetworkStatusTypes.Available
                          );

            #endregion


        }

        #endregion

        #region SetupEachTest()

        [SetUp]
        public async Task SetupEachTest()
        {


            Timestamp.Reset();

            #region Create cpo/emsp1/emsp2 HTTP API

            cpoHTTPAPI            = new HTTPAPI(
                                        HTTPServerPort:  IPPort.Parse(3301),
                                        Autostart:       true
                                    );

            emsp1HTTPAPI          = new HTTPAPI(
                                        HTTPServerPort:  IPPort.Parse(3401),
                                        Autostart:       true
                                    );

            emsp2HTTPAPI          = new HTTPAPI(
                                       HTTPServerPort:  IPPort.Parse(3402),
                                       Autostart:       true
                                    );

            Assert.IsNotNull(cpoHTTPAPI);
            Assert.IsNotNull(emsp1HTTPAPI);
            Assert.IsNotNull(emsp2HTTPAPI);

            #endregion

            #region Create cpo/emsp1/emsp2 OCPI Common API

            // Clean up log and databade directories...
            foreach (var filePath in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,
                                                                     HTTPAPI.DefaultHTTPAPI_LoggingPath),
                                                        $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_*.log"))
            {
                File.Delete(filePath);
            }


            cpoVersionsAPIURL    = URL.Parse("http://127.0.0.1:3301/ocpi/v2.1/versions");

            cpoCommonAPI         = new CommonAPI(

                                       OurBaseURL:                          URL.Parse("http://127.0.0.1:3301/ocpi/v2.1"),
                                       OurVersionsURL:                      cpoVersionsAPIURL.Value,
                                       OurBusinessDetails:                  new BusinessDetails(
                                                                                "GraphDefined CSO Services",
                                                                                URL.Parse("https://www.graphdefined.com/cso")
                                                                            ),
                                       OurCountryCode:                      CountryCode.Parse("DE"),
                                       OurPartyId:                          Party_Id.   Parse("GEF"),
                                       OurRole:                             Roles.      CPO,

                                       HTTPServer:                          cpoHTTPAPI.HTTPServer,

                                       AdditionalURLPathPrefix:             null,
                                       KeepRemovedEVSEs:                    null,
                                       LocationsAsOpenData:                 true,
                                       AllowDowngrades:                     null,
                                       Disable_RootServices:                false,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_CSO.log",
                                       LogfileCreator:                      null,
                                       DatabaseFilePath:                    null,
                                       RemotePartyDBFileName:               $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_RemoteParties_CPO.log",
                                       AssetsDBFileName:                    $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_Assets_CPO.log",
                                       Autostart:                           false

                                   );


            emsp1VersionsAPIURL  = URL.Parse("http://127.0.0.1:3401/ocpi/v2.1/versions");

            emsp1CommonAPI       = new CommonAPI(

                                       OurBaseURL:                          URL.Parse("http://127.0.0.1:3401/ocpi/v2.1"),
                                       OurVersionsURL:                      emsp1VersionsAPIURL.Value,
                                       OurBusinessDetails:                  new BusinessDetails(
                                                                                "GraphDefined EMSP #1 Services",
                                                                                URL.Parse("https://www.graphdefined.com/emsp1")
                                                                            ),
                                       OurCountryCode:                      CountryCode.Parse("DE"),
                                       OurPartyId:                          Party_Id.   Parse("GDF"),
                                       OurRole:                             Roles.      EMSP,

                                       HTTPServer:                          emsp1HTTPAPI.HTTPServer,

                                       AdditionalURLPathPrefix:             null,
                                       KeepRemovedEVSEs:                    null,
                                       LocationsAsOpenData:                 true,
                                       AllowDowngrades:                     null,
                                       Disable_RootServices:                false,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_EMSP1.log",
                                       LogfileCreator:                      null,
                                       DatabaseFilePath:                    null,
                                       RemotePartyDBFileName:               $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_RemoteParties_EMSP1.log",
                                       AssetsDBFileName:                    $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_Assets_EMSP1.log",
                                       Autostart:                           false

                                   );


            emsp2VersionsAPIURL  = URL.Parse("http://127.0.0.1:3402/ocpi/v2.1/versions");

            emsp2CommonAPI       = new CommonAPI(

                                       OurBaseURL:                          URL.Parse("http://127.0.0.1:3402/ocpi/v2.1"),
                                       OurVersionsURL:                      emsp2VersionsAPIURL.Value,
                                       OurBusinessDetails:                  new BusinessDetails(
                                                                                "GraphDefined EMSP #2 Services",
                                                                                URL.Parse("https://www.graphdefined.com/emsp2")
                                                                            ),
                                       OurCountryCode:                      CountryCode.Parse("DE"),
                                       OurPartyId:                          Party_Id.   Parse("GD2"),
                                       OurRole:                             Roles.      EMSP,

                                       HTTPServer:                          emsp2HTTPAPI.HTTPServer,

                                       AdditionalURLPathPrefix:             null,
                                       KeepRemovedEVSEs:                    null,
                                       LocationsAsOpenData:                 true,
                                       AllowDowngrades:                     null,
                                       Disable_RootServices:                false,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_EMSP2.log",
                                       LogfileCreator:                      null,
                                       DatabaseFilePath:                    null,
                                       RemotePartyDBFileName:               $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_RemoteParties_EMSP2.log",
                                       AssetsDBFileName:                    $"GraphDefined_OCPI{protocols.OCPIv2_2_1.Version.String}_Assets_EMSP2.log",
                                       Autostart:                           false

                                   );

            Assert.IsNotNull(cpoVersionsAPIURL);
            Assert.IsNotNull(emsp1VersionsAPIURL);
            Assert.IsNotNull(emsp2VersionsAPIURL);

            Assert.IsNotNull(cpoCommonAPI);
            Assert.IsNotNull(emsp1CommonAPI);
            Assert.IsNotNull(emsp2CommonAPI);

            #endregion

            #region Create cpo/emsp1/emsp2 OCPI WebAPI

            cpoWebAPI            = new OCPIWebAPI(
                                       HTTPServer:                          cpoHTTPAPI.HTTPServer,
                                       CommonAPI:                           cpoCommonAPI,
                                       OverlayURLPathPrefix:                HTTPPath.Parse("/ocpi/v2.1"),
                                       WebAPIURLPathPrefix:                 HTTPPath.Parse("/ocpi/v2.1/webapi"),
                                       BasePath:                            HTTPPath.Parse("/ocpi/v2.1"),
                                       HTTPRealm:                           "GraphDefined OCPI CPO WebAPI",
                                       HTTPLogins:                          new[] {
                                                                                new KeyValuePair<String, String>("a", "b")
                                                                            },
                                       HTMLTemplate:                        null,
                                       RequestTimeout:                      null
                                   );

            emsp1WebAPI          = new OCPIWebAPI(
                                       HTTPServer:                          emsp1HTTPAPI.HTTPServer,
                                       CommonAPI:                           emsp1CommonAPI,
                                       OverlayURLPathPrefix:                HTTPPath.Parse("/ocpi/v2.1"),
                                       WebAPIURLPathPrefix:                 HTTPPath.Parse("/ocpi/v2.1/webapi"),
                                       BasePath:                            HTTPPath.Parse("/ocpi/v2.1"),
                                       HTTPRealm:                           "GraphDefined OCPI EMSP #1 WebAPI",
                                       HTTPLogins:                          new[] {
                                                                                new KeyValuePair<String, String>("c", "d")
                                                                            },
                                       HTMLTemplate:                        null,
                                       RequestTimeout:                      null
                                   );

            emsp2WebAPI          = new OCPIWebAPI(
                                       HTTPServer:                          emsp2HTTPAPI.HTTPServer,
                                       CommonAPI:                           emsp2CommonAPI,
                                       OverlayURLPathPrefix:                HTTPPath.Parse("/ocpi/v2.1"),
                                       WebAPIURLPathPrefix:                 HTTPPath.Parse("/ocpi/v2.1/webapi"),
                                       BasePath:                            HTTPPath.Parse("/ocpi/v2.1"),
                                       HTTPRealm:                           "GraphDefined OCPI EMSP #2 WebAPI",
                                       HTTPLogins:                          new[] {
                                                                                new KeyValuePair<String, String>("e", "f")
                                                                            },
                                       HTMLTemplate:                        null,
                                       RequestTimeout:                      null
                                   );

            Assert.IsNotNull(cpoWebAPI);
            Assert.IsNotNull(emsp1WebAPI);
            Assert.IsNotNull(emsp2WebAPI);

            #endregion

            #region Create cpo CPOAPI & emsp1/emsp2 EMPAPI

            cpoCPOAPI            = new CPOAPI(

                                       CommonAPI:                           cpoCommonAPI,
                                       DefaultCountryCode:                  cpoCommonAPI.OurCountryCode,
                                       DefaultPartyId:                      cpoCommonAPI.OurPartyId,
                                       AllowDowngrades:                     null,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1/v2.1.1/cpo"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         null,
                                       LogfileCreator:                      null,
                                       Autostart:                           false

                                   );

            emsp1EMSPAPI         = new EMSPAPI(

                                       CommonAPI:                           emsp1CommonAPI,
                                       DefaultCountryCode:                  emsp1CommonAPI.OurCountryCode,
                                       DefaultPartyId:                      emsp1CommonAPI.OurPartyId,
                                       AllowDowngrades:                     null,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1/v2.1.1/emsp"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         null,
                                       LogfileCreator:                      null,
                                       Autostart:                           false

                                   );

            emsp2EMSPAPI         = new EMSPAPI(

                                       CommonAPI:                           emsp2CommonAPI,
                                       DefaultCountryCode:                  emsp2CommonAPI.OurCountryCode,
                                       DefaultPartyId:                      emsp2CommonAPI.OurPartyId,
                                       AllowDowngrades:                     null,

                                       HTTPHostname:                        null,
                                       ExternalDNSName:                     null,
                                       HTTPServiceName:                     null,
                                       BasePath:                            null,

                                       URLPathPrefix:                       HTTPPath.Parse("/ocpi/v2.1/v2.1.1/emsp"),
                                       APIVersionHashes:                    null,

                                       DisableMaintenanceTasks:             null,
                                       MaintenanceInitialDelay:             null,
                                       MaintenanceEvery:                    null,

                                       DisableWardenTasks:                  null,
                                       WardenInitialDelay:                  null,
                                       WardenCheckEvery:                    null,

                                       IsDevelopment:                       null,
                                       DevelopmentServers:                  null,
                                       DisableLogging:                      null,
                                       LoggingPath:                         null,
                                       LogfileName:                         null,
                                       LogfileCreator:                      null,
                                       Autostart:                           false

                                   );

            Assert.IsNotNull(cpoCPOAPI);
            Assert.IsNotNull(emsp1EMSPAPI);
            Assert.IsNotNull(emsp2EMSPAPI);

            #endregion

            #region Define and connect Remote Parties

            await cpoCommonAPI.AddRemoteParty  (CountryCode:                 emsp1CommonAPI.OurCountryCode,
                                                PartyId:                     emsp1CommonAPI.OurPartyId,
                                                Role:                        Roles.EMSP,
                                                BusinessDetails:             emsp1CommonAPI.OurBusinessDetails,

                                                AccessToken:                 AccessToken.Parse(emsp1_accessing_cpo__token),
                                                AccessStatus:                AccessStatus.ALLOWED,

                                                RemoteAccessToken:           AccessToken.Parse(cpo_accessing_emsp1__token),
                                                RemoteVersionsURL:           URL.Parse($"http://localhost:{emsp1HTTPAPI.HTTPServer.IPPorts.First()}/ocpi/v2.1/versions"),
                                                RemoteVersionIds:            new[] { protocols.OCPIv2_2_1.Version.Id },
                                                SelectedVersionId:           protocols.OCPIv2_2_1.Version.Id,
                                                AccessTokenBase64Encoding:   false,
                                                RemoteStatus:                RemoteAccessStatus.ONLINE,

                                                PartyStatus:                 PartyStatus.ENABLED);

            await cpoCommonAPI.AddRemoteParty  (CountryCode:                 emsp2CommonAPI.OurCountryCode,
                                                PartyId:                     emsp2CommonAPI.OurPartyId,
                                                Role:                        Roles.EMSP,
                                                BusinessDetails:             emsp2CommonAPI.OurBusinessDetails,
                                                AccessToken:                 AccessToken.Parse(emsp2_accessing_cpo__token),
                                                AccessStatus:                AccessStatus.ALLOWED,
                                                RemoteAccessToken:           AccessToken.Parse(cpo_accessing_emsp2__token),
                                                RemoteVersionsURL:           URL.Parse($"http://localhost:{emsp2HTTPAPI.HTTPServer.IPPorts.First()}/ocpi/v2.1/versions"),
                                                RemoteVersionIds:            new[] { protocols.OCPIv2_2_1.Version.Id },
                                                SelectedVersionId:           protocols.OCPIv2_2_1.Version.Id,
                                                AccessTokenBase64Encoding:   false,
                                                RemoteStatus:                RemoteAccessStatus.ONLINE,
                                                PartyStatus:                 PartyStatus.ENABLED);



            await emsp1CommonAPI.AddRemoteParty(CountryCode:                 cpoCommonAPI.OurCountryCode,
                                                PartyId:                     cpoCommonAPI.OurPartyId,
                                                Role:                        Roles.CPO,
                                                BusinessDetails:             cpoCommonAPI.OurBusinessDetails,

                                                AccessToken:                 AccessToken.Parse(cpo_accessing_emsp1__token),
                                                AccessStatus:                AccessStatus.ALLOWED,

                                                RemoteAccessToken:           AccessToken.Parse(emsp1_accessing_cpo__token),
                                                RemoteVersionsURL:           URL.Parse($"http://localhost:{cpoHTTPAPI.HTTPServer.IPPorts.First()}/ocpi/v2.1/versions"),
                                                RemoteVersionIds:            new[] { protocols.OCPIv2_2_1.Version.Id },
                                                SelectedVersionId:           protocols.OCPIv2_2_1.Version.Id,
                                                AccessTokenBase64Encoding:   false,
                                                RemoteStatus:                RemoteAccessStatus.ONLINE,

                                                PartyStatus:                 PartyStatus.ENABLED);


            await emsp2CommonAPI.AddRemoteParty(CountryCode:                 cpoCommonAPI.OurCountryCode,
                                                PartyId:                     cpoCommonAPI.OurPartyId,
                                                Role:                        Roles.CPO,
                                                BusinessDetails:             cpoCommonAPI.OurBusinessDetails,

                                                AccessToken:                 AccessToken.Parse(cpo_accessing_emsp2__token),
                                                AccessStatus:                AccessStatus.ALLOWED,

                                                RemoteAccessToken:           AccessToken.Parse(emsp2_accessing_cpo__token),
                                                RemoteVersionsURL:           URL.Parse($"http://localhost:{cpoHTTPAPI.HTTPServer.IPPorts.First()}/ocpi/v2.1/versions"),
                                                RemoteVersionIds:            new[] { protocols.OCPIv2_2_1.Version.Id },
                                                SelectedVersionId:           protocols.OCPIv2_2_1.Version.Id,
                                                AccessTokenBase64Encoding:   false,
                                                RemoteStatus:                RemoteAccessStatus.ONLINE,

                                                PartyStatus:                 PartyStatus.ENABLED);


            Assert.AreEqual(2, cpoCommonAPI.  RemoteParties.Count());
            Assert.AreEqual(1, emsp1CommonAPI.RemoteParties.Count());
            Assert.AreEqual(1, emsp2CommonAPI.RemoteParties.Count());

            Assert.AreEqual(2, File.ReadAllLines(cpoCommonAPI.  RemotePartyDBFileName).Length);
            Assert.AreEqual(1, File.ReadAllLines(emsp1CommonAPI.RemotePartyDBFileName).Length);
            Assert.AreEqual(1, File.ReadAllLines(emsp2CommonAPI.RemotePartyDBFileName).Length);

            #endregion

            #region Define blocked Remote Parties

            await cpoCommonAPI.AddRemoteParty  (CountryCode:       CountryCode.Parse("XX"),
                                                PartyId:           Party_Id.   Parse("BLE"),
                                                Role:              Roles.EMSP,
                                                BusinessDetails:   new BusinessDetails(
                                                                       "Blocked EMSP"
                                                                   ),
                                                AccessToken:       AccessToken.Parse(BlockedEMSPToken),
                                                AccessStatus:      AccessStatus.BLOCKED,
                                                PartyStatus:       PartyStatus. ENABLED);

            await emsp1CommonAPI.AddRemoteParty(CountryCode:       CountryCode.Parse("XX"),
                                                PartyId:           Party_Id.   Parse("BLC"),
                                                Role:              Roles.CPO,
                                                BusinessDetails:   new BusinessDetails(
                                                                       "Blocked CPO"
                                                                   ),
                                                AccessToken:       AccessToken.Parse(BlockedCPOToken),
                                                AccessStatus:      AccessStatus.BLOCKED,
                                                PartyStatus:       PartyStatus. ENABLED);

            await emsp2CommonAPI.AddRemoteParty(CountryCode:       CountryCode.Parse("XX"),
                                                PartyId:           Party_Id.   Parse("BLC"),
                                                Role:              Roles.CPO,
                                                BusinessDetails:   new BusinessDetails(
                                                                       "Blocked CPO"
                                                                   ),
                                                AccessToken:       AccessToken.Parse(BlockedCPOToken),
                                                AccessStatus:      AccessStatus.BLOCKED,
                                                PartyStatus:       PartyStatus. ENABLED);


            Assert.AreEqual(3, cpoCommonAPI.  RemoteParties.Count());
            Assert.AreEqual(2, emsp1CommonAPI.RemoteParties.Count());
            Assert.AreEqual(2, emsp2CommonAPI.RemoteParties.Count());

            #endregion

            #region Defined API loggers

            // CPO
            cpoAPIRequestLogs     = new ConcurrentDictionary<DateTime, OCPIRequest>();
            cpoAPIResponseLogs    = new ConcurrentDictionary<DateTime, OCPIResponse>();

            cpoCPOAPI.CPOAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                      (loggingPath, context, logEventName, request) => {
                                                          cpoAPIRequestLogs. TryAdd(Timestamp.Now, request);
                                                          return Task.CompletedTask;
                                                      });

            cpoCPOAPI.CPOAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                      (loggingPath, context, logEventName, request, response) => {
                                                          cpoAPIResponseLogs.TryAdd(Timestamp.Now, response);
                                                          return Task.CompletedTask;
                                                      });

            cpoCPOAPI.CPOAPILogger?.Debug("all", LogTargets.Debug);

            cpoCommonAPI.ClientConfigurations.Description = (remotePartyId) => $"CPO Client for {remotePartyId}";



            // EMSP #1
            emsp1APIRequestLogs   = new ConcurrentDictionary<DateTime, OCPIRequest>();
            emsp1APIResponseLogs  = new ConcurrentDictionary<DateTime, OCPIResponse>();

            emsp1EMSPAPI.EMSPAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                          (loggingPath, context, logEventName, request) => {
                                                              emsp1APIRequestLogs. TryAdd(Timestamp.Now, request);
                                                              return Task.CompletedTask;
                                                          });

            emsp1EMSPAPI.EMSPAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                          (loggingPath, context, logEventName, request, response) => {
                                                              emsp1APIResponseLogs.TryAdd(Timestamp.Now, response);
                                                              return Task.CompletedTask;
                                                          });

            emsp1EMSPAPI.EMSPAPILogger?.Debug("all", LogTargets.Debug);

            emsp1CommonAPI.ClientConfigurations.Description = (remotePartyId) => $"EMSP #1 Client for {remotePartyId}";



            // EMSP #2
            emsp2APIRequestLogs   = new ConcurrentDictionary<DateTime, OCPIRequest>();
            emsp2APIResponseLogs  = new ConcurrentDictionary<DateTime, OCPIResponse>();

            emsp2EMSPAPI.EMSPAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                          (loggingPath, context, logEventName, request) => {
                                                              emsp2APIRequestLogs. TryAdd(Timestamp.Now, request);
                                                              return Task.CompletedTask;
                                                          });

            emsp2EMSPAPI.EMSPAPILogger?.RegisterLogTarget(LogTargets.Debug,
                                                          (loggingPath, context, logEventName, request, response) => {
                                                              emsp2APIResponseLogs.TryAdd(Timestamp.Now, response);
                                                              return Task.CompletedTask;
                                                          });

            emsp2EMSPAPI.EMSPAPILogger?.Debug("all", LogTargets.Debug);

            emsp2CommonAPI.ClientConfigurations.Description = (remotePartyId) => $"EMSP #2 Client for {remotePartyId}";

            #endregion

            #region Create cso/emp1/emp2 roaming network

            csoRoamingNetwork    = new RoamingNetwork(
                                       Id:                  RoamingNetwork_Id.Parse("test_cso"),
                                       Name:                I18NString.Create(Languages.en, "CSO EV Roaming Test Network"),
                                       Description:         I18NString.Create(Languages.en, "The EV roaming test network at the charging station operator"),
                                       InitialAdminStatus:  RoamingNetworkAdminStatusTypes.Operational,
                                       InitialStatus:       RoamingNetworkStatusTypes.Available
                                   );

            emp1RoamingNetwork   = new RoamingNetwork(
                                       Id:                  RoamingNetwork_Id.Parse("test_emp1"),
                                       Name:                I18NString.Create(Languages.en, "EV Roaming Test Network EMP1"),
                                       Description:         I18NString.Create(Languages.en, "The EV roaming test network at the 1st e-mobility provider"),
                                       InitialAdminStatus:  RoamingNetworkAdminStatusTypes.Operational,
                                       InitialStatus:       RoamingNetworkStatusTypes.Available
                                   );

            emp2RoamingNetwork   = new RoamingNetwork(
                                       Id:                  RoamingNetwork_Id.Parse("test_emp2"),
                                       Name:                I18NString.Create(Languages.en, "EV Roaming Test Network EMP2"),
                                       Description:         I18NString.Create(Languages.en, "The EV roaming test network at the 2nd e-mobility provider"),
                                       InitialAdminStatus:  RoamingNetworkAdminStatusTypes.Operational,
                                       InitialStatus:       RoamingNetworkStatusTypes.Available
                                   );

            Assert.IsNotNull(csoRoamingNetwork);
            Assert.IsNotNull(emp1RoamingNetwork);
            Assert.IsNotNull(emp2RoamingNetwork);

            #endregion

            #region Create graphDefinedCSO / graphDefinedEMP1 / graphDefinedEMP2

            var csoResult          = await csoRoamingNetwork.CreateChargingStationOperator(
                                                                 Id:                   ChargingStationOperator_Id.Parse("DE*GEF"),
                                                                 Name:                 I18NString.Create(Languages.en, "GraphDefined CSO"),
                                                                 Description:          I18NString.Create(Languages.en, "GraphDefined CSO Services"),
                                                                 InitialAdminStatus:   ChargingStationOperatorAdminStatusTypes.Operational,
                                                                 InitialStatus:        ChargingStationOperatorStatusTypes.Available
                                                             );

            Assert.IsTrue   (csoResult.IsSuccess);
            Assert.IsNotNull(csoResult.ChargingStationOperator);

            graphDefinedCSO        = csoResult.ChargingStationOperator;



            var emp1result         = await emp1RoamingNetwork.CreateEMobilityProvider(

                                                                  Id:                               EMobilityProvider_Id.Parse("DE-GDF"),
                                                                  Name:                             I18NString.Create(Languages.en, "GraphDefined EMP #1"),
                                                                  Description:                      I18NString.Create(Languages.en, "GraphDefined EMP #1 Services"),
                                                                  InitialAdminStatus:               EMobilityProviderAdminStatusTypes.Operational,
                                                                  InitialStatus:                    EMobilityProviderStatusTypes.Available,

                                                                  RemoteEMobilityProviderCreator:   eMobilityProvider => new VirtualEMobilityProvider(
                                                                                                                             EMobilityProvider_Id.Parse("DE-GDF"),
                                                                                                                             eMobilityProvider.RoamingNetwork
                                                                                                                         )

                                                              );

            Assert.IsTrue   (emp1result.IsSuccess);
            Assert.IsNotNull(emp1result.EMobilityProvider);

            graphDefinedEMP1       = emp1result.EMobilityProvider;
            graphDefinedEMP1Local  = graphDefinedEMP1?.RemoteEMobilityProvider as VirtualEMobilityProvider;


            var emp2result         = await emp2RoamingNetwork.CreateEMobilityProvider(

                                                                  Id:                               EMobilityProvider_Id.Parse("DE-GD2"),
                                                                  Name:                             I18NString.Create(Languages.en, "GraphDefined EMP #2"),
                                                                  Description:                      I18NString.Create(Languages.en, "GraphDefined EMP #2 Services"),
                                                                  InitialAdminStatus:               EMobilityProviderAdminStatusTypes.Operational,
                                                                  InitialStatus:                    EMobilityProviderStatusTypes.Available,

                                                                  RemoteEMobilityProviderCreator:   eMobilityProvider => new VirtualEMobilityProvider(
                                                                                                                             EMobilityProvider_Id.Parse("DE-GD2"),
                                                                                                                             eMobilityProvider.RoamingNetwork
                                                                                                                         )

                                                              );

            Assert.IsTrue   (emp2result.IsSuccess);
            Assert.IsNotNull(emp2result.EMobilityProvider);

            graphDefinedEMP2       = emp2result.EMobilityProvider;
            graphDefinedEMP2Local  = graphDefinedEMP2?.RemoteEMobilityProvider as VirtualEMobilityProvider;
            graphDefinedEMP2Local?.StartAPI(HTTPServerPort: IPPort.Parse(3501));

            #endregion

            #region Create cpo/emsp1/emsp2 adapter

            Assert.IsNotNull(cpoCPOAPI);
            Assert.IsNotNull(emsp1EMSPAPI);
            Assert.IsNotNull(emsp2EMSPAPI);

            if (cpoCPOAPI    is not null &&
                emsp1EMSPAPI is not null &&
                emsp2EMSPAPI is not null)
            {

                cpoAdapter           = csoRoamingNetwork.CreateOCPIv2_1_1_CSOAdapter(

                                           Id:                                  EMPRoamingProvider_Id.Parse("OCPIv2.1_CSO_" + this.csoRoamingNetwork.Id),
                                           Name:                                I18NString.Create(Languages.de, "OCPI v2.1 CSO"),
                                           Description:                         I18NString.Create(Languages.de, "OCPI v2.1 CSO Roaming"),

                                           CPOAPI:                              cpoCPOAPI,

                                           CustomEVSEIdConverter:               null,
                                           CustomEVSEConverter:                 null,
                                           CustomEVSEStatusUpdateConverter:     null,
                                           CustomChargeDetailRecordConverter:   null,

                                           IncludeEVSEIds:                      null,
                                           IncludeEVSEs:                        null,
                                           IncludeChargingPoolIds:              null,
                                           IncludeChargingPools:                null,
                                           ChargeDetailRecordFilter:            null,

                                           ServiceCheckEvery:                   null,
                                           StatusCheckEvery:                    null,
                                           CDRCheckEvery:                       null,

                                           DisablePushData:                     true,
                                           DisablePushStatus:                   true,
                                           DisableAuthentication:               false,
                                           DisableSendChargeDetailRecords:      false

                                       );

                emsp1Adapter          = emp1RoamingNetwork.CreateOCPIv2_1_EMPAdapter(

                                           Id:                                  CSORoamingProvider_Id.Parse("OCPIv2.1_EMP1_" + this.emp1RoamingNetwork.Id),
                                           Name:                                I18NString.Create(Languages.de, "OCPI v2.1 EMP1"),
                                           Description:                         I18NString.Create(Languages.de, "OCPI v2.1 EMP1 Roaming"),

                                           EMSPAPI:                             emsp1EMSPAPI,

                                           CustomEVSEIdConverter:               null,
                                           CustomEVSEConverter:                 null,
                                           CustomEVSEStatusUpdateConverter:     null,
                                           CustomChargeDetailRecordConverter:   null,

                                           IncludeEVSEIds:                      null,
                                           IncludeEVSEs:                        null,
                                           IncludeChargingPoolIds:              null,
                                           IncludeChargingPools:                null,
                                           ChargeDetailRecordFilter:            null,

                                           ServiceCheckEvery:                   null,
                                           StatusCheckEvery:                    null,
                                           CDRCheckEvery:                       null,

                                           DisablePushData:                     true,
                                           DisablePushStatus:                   true,
                                           DisableAuthentication:               false,
                                           DisableSendChargeDetailRecords:      false

                                       );

                emsp2Adapter          = emp2RoamingNetwork.CreateOCPIv2_1_EMPAdapter(

                                           Id:                                  CSORoamingProvider_Id.Parse("OCPIv2.1_EMP2_" + this.emp1RoamingNetwork.Id),
                                           Name:                                I18NString.Create(Languages.de, "OCPI v2.1 EMP2"),
                                           Description:                         I18NString.Create(Languages.de, "OCPI v2.1 EMP2 Roaming"),

                                           EMSPAPI:                             emsp2EMSPAPI,

                                           CustomEVSEIdConverter:               null,
                                           CustomEVSEConverter:                 null,
                                           CustomEVSEStatusUpdateConverter:     null,
                                           CustomChargeDetailRecordConverter:   null,

                                           IncludeEVSEIds:                      null,
                                           IncludeEVSEs:                        null,
                                           IncludeChargingPoolIds:              null,
                                           IncludeChargingPools:                null,
                                           ChargeDetailRecordFilter:            null,

                                           ServiceCheckEvery:                   null,
                                           StatusCheckEvery:                    null,
                                           CDRCheckEvery:                       null,

                                           DisablePushData:                     true,
                                           DisablePushStatus:                   true,
                                           DisableAuthentication:               false,
                                           DisableSendChargeDetailRecords:      false

                                       );

                Assert.IsNotNull(cpoAdapter);
                Assert.IsNotNull(emsp1Adapter);
                Assert.IsNotNull(emsp2Adapter);

            }

            #endregion


            ahzfPhone = new VirtualSmartPhone();
            ahzfPhone?.Connect(URL.Parse("http://127.0.0.1:3501"));

        }

        #endregion

        #region ShutdownEachTest()

        [TearDown]
        public void ShutdownEachTest()
        {

            cpoHTTPAPI?.  Shutdown();
            emsp1HTTPAPI?.Shutdown();
            emsp2HTTPAPI?.Shutdown();

            if (cpoCommonAPI is not null)
                File.Delete(Path.Combine(cpoCommonAPI.  LoggingPath, cpoCommonAPI.  LogfileName));

            if (emsp1CommonAPI is not null)
                File.Delete(Path.Combine(emsp1CommonAPI.LoggingPath, emsp1CommonAPI.LogfileName));

            if (emsp2CommonAPI is not null)
                File.Delete(Path.Combine(emsp2CommonAPI.LoggingPath, emsp2CommonAPI.LogfileName));

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
