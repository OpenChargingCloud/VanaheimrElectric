﻿/*
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
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Logging;

using cloud.charging.open.protocols.OCPI;
using cloud.charging.open.protocols.OCPIv2_1_1;
using cloud.charging.open.protocols.OCPIv2_1_1.HTTP;
using cloud.charging.open.protocols.OCPIv2_1_1.WebAPI;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.MobilityProvider;
using cloud.charging.open.protocols.WWCP.PKI;
using cloud.charging.open.protocols.WWCP.SMM;
using cloud.charging.open.protocols.WWCP.CSM;
using cloud.charging.open.protocols.WWCP.MCL;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// The charging infrastructure test defaults for Vanaheimr Electric.
    /// </summary>
    public abstract class AChargingInfrastructure
    {

        #region Data

        public          DNSClient                                      DNSClient;
        public          RoamingNetwork_Id                              RoamingNetworkPROD;

        public          OpenChargingCloudPKI_CA?                       OCCPKICA;
        public          ChargingNode?                                  OpenChargingCloudPKI_Node1;
        public          ChargingNode?                                  OpenChargingCloudPKI_Node2;
        public          ChargingNode?                                  OpenChargingCloudPKI_Node3;

        public          MeteringCalibrationLawAgency?                  PTB;
        public          ChargingNode?                                  PTB_Node1;

        public          MeteringCalibrationLawAgency?                  VDE;
        public          ChargingNode?                                  VDE_Node1;


        public          SmartMeterManufacturer?                        GraphDefinedSEM;
        public          SmartMeterModel?                               GraphDefinedSEM_M1;
        public          SmartMeterDevice?                              GraphDefinedSEM_M1_0001;
        public          ChargingNode?                                  GraphDefinedSEM_Node1;

        public          ChargingStationManufacturer?                   GraphDefinedCSM;
        public          ChargingNode?                                  GraphDefinedCSM_Node1;


        public          ChargingNode?                                  GraphDefinedCSO_Node1;
        public          ChargingNode?                                  GraphDefinedCSO_Node2;
        public          ChargingNode?                                  GraphDefinedCSO_Node3;

        public          ChargingNode?                                  GraphDefined_ChargingStation_A1;
        public          ChargingNode?                                  GraphDefined_ChargingStation_A2;
        public          ChargingNode?                                  GraphDefined_ChargingStation_A3;

        public          ChargingNode?                                  GraphDefined_LocalController_B;
        public          ChargingNode?                                  GraphDefined_UpstreamEnergyMeter_B;
        public          ChargingNode?                                  GraphDefined_ChargingStation_B1;
        public          ChargingNode?                                  GraphDefined_ChargingStation_B2;
        public          ChargingNode?                                  GraphDefined_ChargingStation_B3;

        public          ChargingNode?                                  GraphDefined_PaymentTerminal_C;
        public          ChargingNode?                                  GraphDefined_ChargingStation_C1;
        public          ChargingNode?                                  GraphDefined_ChargingStation_C2;
        public          ChargingNode?                                  GraphDefined_ChargingStation_C3;


        public          ChargingNode?                                  GraphDefinedEMP_Node1;
        public          ChargingNode?                                  GraphDefinedEMP_Node2;
        public          ChargingNode?                                  GraphDefinedEMP_Node3;


        public          ChargingNode?                                  Hubject_Node1;
        public          ChargingNode?                                  Hubject_Node2;
        public          ChargingNode?                                  Hubject_Node3;

        public          ChargingNode?                                  Gireve_Node1;
        public          ChargingNode?                                  Gireve_Node2;
        public          ChargingNode?                                  Gireve_Node3;

        public          NationalContactPoint?                          Leitstelle;
        public          ChargingNode?                                  Leitstelle_Node1;

        public          VirtualSmartPhone?                             ahzfPhone;
        public          EVehicle?                                      ahzfCar;



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

        #endregion

        #region Constructor(s)

        public AChargingInfrastructure()
        {

            this.DNSClient           = new();
            this.RoamingNetworkPROD  = RoamingNetwork_Id.Parse("PROD");

        }

        #endregion


        #region SetupOnce()

        [OneTimeSetUp]
        public void SetupOnce()
        {

            var OpenChargingCloudPKI_Node1_HTTPPort   = 3001;
            var OpenChargingCloudPKI_Node2_HTTPPort   = 3002;
            var OpenChargingCloudPKI_Node3_HTTPPort   = 3003;

            var PTB_Node1_HTTPPort                    = 3101;
            var VDE_Node1_HTTPPort                    = 3111;

            var GraphDefinedSEM_Node1_HTTPPort        = 3201;

            var GraphDefinedCSM_Node1_HTTPPort        = 3301;

            var GraphDefinedCSO_Node1_HTTPPort        = 3401;

            var GraphDefinedEMP_Node1_HTTPPort        = 3501;

            var Hubject_Node1_HTTPPort                = 3601;
            var Gireve_Node1_HTTPPort                 = 3611;

            var Leitstelle_Node1_HTTPPort             = 3701;


            var notBefore = Timestamp.Now - TimeSpan.FromDays(1);
            var notAfter  = notBefore     + TimeSpan.FromDays(365);


            #region Open Charging Cloud PKI Service

            // Open Charging Cloud brings order to the galaxy ;)

            OCCPKICA                    = new OpenChargingCloudPKI_CA(
                                              Name:             I18NString.Create(Languages.en, "OpenChargingCloud PKI Test CA"),
                                              Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure Test Certification Authority"),
                                              CryptoKeys:       new[] {
                                                                    //ToDo: Create a "Quorum 2 of 3" certificate!
                                                                    new SecP521r1Keys(
                                                                        PublicKey:   "BAHJHVXPl5pM3uIvimkDvhEqQv3fQVfIvyEGwG9ER9qsuil2bKVEYTurJjH5c51kiOoM3F+zcgfjszCHyDMzoJ09bAHp4Wgow7gKlEJrug3DZk2lQURg82IFvcmVoS/PNci52L0Xlx6gLmKKaYXp8BpwwYDY+VrOdoeE+sbWOBg8nkiXQg==",
                                                                        PrivateKey:  "AOF/+/sO1vcmbiOeuzm9ap5llq1Uzw4tjBpl8d2jiUEL4UhUMy3enx6OKdFeKb/uJZ7YJaPrutWXUeRQ4wLE8lAr",
                                                                        KeyUsage:    WWCPCryptoKeyUsage.Identity,
                                                                        NotBefore:   notBefore,
                                                                        NotAfter:    notAfter
                                                                    ),
                                                                    new SecP521r1Keys(
                                                                        PublicKey:   "BACiZ5ny6sL+GGQlgNMBNiQAnpfy9Eay4e+zFfTVmQUJ8F3f8PSysgS/MMCz75wK6MNZSVtfId5oIoDNf1Ye++siCgHeDTBU6sMQC0XdMdXMUqYB2xuM9mK3hk0naHoq/poR0C5qbWF2a2II/oS5psSF2msWCaBR4nlmYCbtXSk9zSSvsA==",
                                                                        PrivateKey:  "Abww+cx4+tMxLNAUyEMripnJr5lE8i/bsDbH9Jm38au7FskEn1W/W8UxUpxSSqgC1FOtfvz/hI6CKFcueG4gHKEz",
                                                                        KeyUsage:    WWCPCryptoKeyUsage.Identity,
                                                                        NotBefore:   notBefore,
                                                                        NotAfter:    notAfter
                                                                    ),
                                                                    new SecP521r1Keys(
                                                                        PublicKey:   "BABsuWebS95+a2pB7wZglFjg/uraplxor1U6Fag/k2sbPacZlEtDeU0AlwDG//fA8cdyxufu5LpiwgdJUSPdrO2xWwButQwDXrwTLY+oXQwL/DFzMjIAEQhVPu4Mp9h5ChJ5cQYhRK1VVj2//6Mfr7xoA+ZYxksRIUOtoMaptFTTyi8hOw==",
                                                                        PrivateKey:  "AbKoRYT/FBsUDCX2ikTf4JlHkoVDryljx1jcJJLcdbD/HygmaIBlPnA5kIkDJRtXdVHxiYR9xKlXekYeqcohvKdB",
                                                                        KeyUsage:    WWCPCryptoKeyUsage.Identity,
                                                                        NotBefore:   notBefore,
                                                                        NotAfter:    notAfter
                                                                    )
                                                                }
                                          );

            var registerNetworkingNodesKey = OCCPKICA.SignKey(new SecP521r1Keys(
                                                                  PublicKey:   "BABUB3itslWidVRI4EaS4Apf1QQNX6+LbYMJkiYddVSu+s05lDQlrotOX9y3IMsvqB1U2m0pQZvZYqN4+Y/YUxkZOwEAVcgnbaFijzVt8bEwol8AStpEABeZvv9GMiBcUgi62LVaxNcQWy0mmP6gJ4N10TSdbhHiusCRQxnE+GHNw4mclA==",
                                                                  PrivateKey:  "AYaza+xoMmqOtYJRoLIh4S2OiM5Dv6RUs7LT1gmqWNzG8Y/eO0nNiYmPEjCtLrVPbddpyCocbLfquyGBxUWgDDKC",
                                                                  KeyUsage:    WWCPCryptoKeyUsage.RegisterNetworkingNodes,
                                                                  NotBefore:   notBefore,
                                                                  NotAfter:    notAfter
                                                              ));

            OCCPKICA.SignKey(new SecP521r1Keys(
                                 PublicKey:   "BAAZHRUmGHyRy4gJ27KnLoxjJiwS/2lXgxFziuIT7Re5ThNDJcXPbNK8sPmJfavYyOMtZxetIIBNAgD6BHJJZTh00gH0xumUDMxzYp4VsGhAWEuFpcw5s5wDCSb0ZuVRsbPXcr6RriMCCEPeuLdCG/2ErMZ5O41WELOGd6d10qqR1w+8bA==",
                                 PrivateKey:  "AWzcywdo3SSotXMyW0UzoIZsT5l12tXXVJMp7bB0f3SW6goVeYWUUiIYtZmyorFgqouXWx8qnCQ0GyRh9BGXPOTi",
                                 KeyUsage:    WWCPCryptoKeyUsage.RegisterMeteringCalibrationLawAgencies,
                                 NotBefore:   notBefore,
                                 NotAfter:    notAfter
                             ));

            OCCPKICA.SignKey(new SecP521r1Keys(
                                 PublicKey:   "BADA8CcMmGFcpeYoHyrbTMI+qBNCsdPfpAeX5Gou3un9iHEjONwu+jBPbdJL3w6hjv1ZTEsKi01bCwzqHcOVwFOQWQAocK20b/1BbAJAsVV50QiVn/7ePX8rlhIzbDavEbMEkVjOd5Vl/34Pro1U2Q+gs3AP881RBFBEvMLQA/4tlei3yA==",
                                 PrivateKey:  "AKSIT5TjWzKc4zYD4XYxVjWv1vCMVdzKSpdg2a6D9anohzienL8qKZVeQbDlUBNJiyPwI1TOtIurzPu/47c0YrTW",
                                 KeyUsage:    WWCPCryptoKeyUsage.RegisterSmartMeterManufacturers,
                                 NotBefore:   notBefore,
                                 NotAfter:    notAfter
                             ));

            OCCPKICA.SignKey(new SecP521r1Keys(
                                 PublicKey:   "BADoSdjEEH2rYGf5G1OvJJB85nCAG3eQouUpAJwi/GIXIW8FkTAV1ZXk0U8aV2S8sSblEwT9czhirhWR21yBNoMs6AAJKVWEGzyVaqNOQoH+efbKZGml8jN/H0Z7a4oPjL5HgTv4CeUYc23pWdqdF4Ff/ZKDvU2GV/6DShnx+Fpg++vl1Q==",
                                 PrivateKey:  "AXq8YQpgKuu78v3PMy3bV0DpETF1+G7rnn0n0pMxCQewhiVIKPo5GgduN/SB7fZ3BvWBV9luONB9n0jafW6A6c3T",
                                 KeyUsage:    WWCPCryptoKeyUsage.RegisterChargingStationManufacturers,
                                 NotBefore:   notBefore,
                                 NotAfter:    notAfter
                             ));


            #region Node 1

            OpenChargingCloudPKI_Node1  = new ChargingNode(
                                              Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node1"),
                                              Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 1 of 3"),
                                              Identities:       new[] {
                                                                    new SecP521r1Keys(
                                                                        PublicKey:   "BAFZihMJdtrB5ixV7guNQAoVY46QO/AI7xF/7DOiZh8kANGOKI+EvcLxVYSs6wO/mhAj8SvwmltQ/Cs2iFuEuGQ4FQFRGBf16cG5dM4jfS0LewqfeATjtmT9AjQgyJu41EDiqf8g/vogBO0NHBq9wGzlseasWlnRlbGvqzGr6EoVmA1C7A==",
                                                                        PrivateKey:  "ATvTmppPVADEbTCujGnIiq5pu2viKXh4wuIqR/tyEcJrILc0tBhzUAUbGPZmUyVPq1RTaVQE12p5fJc7kIETxfLe",
                                                                        KeyUsage:    WWCPCryptoKeyUsage.Identity,
                                                                        NotBefore:   notBefore,
                                                                        NotAfter:    notAfter
                                                                    )
                                                                },
                                              IdentityGroups:   new[] {
                                                                    new SecP521r1Keys(
                                                                        PublicKey:   "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                        PrivateKey:  "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                        KeyUsage:    WWCPCryptoKeyUsage.IdentityGroup,
                                                                        NotBefore:   notBefore,
                                                                        NotAfter:    notAfter
                                                                    )
                                                                },
                                              DefaultHTTPAPI:   new HTTPAPI(
                                                                    HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node1_HTTPPort),
                                                                    DNSClient:       DNSClient,
                                                                    Autostart:       true
                                                                ),
                                              DNSClient:        DNSClient
                                          );


            #endregion

            #region Node 2

            OpenChargingCloudPKI_Node2  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node2"),
                                             Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 2 of 3"),
                                             Identities:       new[] {
                                                                   new SecP521r1Keys(
                                                                       "BAEi3ZlHFUtzyav3FzbYpNzjHwnMGp2xCQPEK24hSlzYTl4AEkcxnPJzAGD/MQBnhCitsnLOKDACBWrwwGgbYwZxBwHB+dAD9NQgov/kAOphWP9ZzQXfyslPpRTZFHEu0ZJ4RSKDj8K53f1tUbMjdEZVMX2+HsAEeWr5E1Af7xWIUusd4g==",
                                                                       "ALX4MIq5wuUEKa0FoOgD0LyoTClxoYaFOBSoDGB88uok0uPmWXMxS+49A5sn5DuFghZRA6yfN17Qt6S/JZn8z2Hf",
                                                                       WWCPCryptoKeyUsage.Identity,
                                                                       notBefore,
                                                                       notAfter
                                                                   )
                                                               },
                                             IdentityGroups:   new[] {
                                                                   new SecP521r1Keys(
                                                                       "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                       "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                       WWCPCryptoKeyUsage.IdentityGroup,
                                                                       notBefore,
                                                                       notAfter
                                                                   )
                                                               },
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node2_HTTPPort),
                                                                   DNSClient:       DNSClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        DNSClient
                                          );


            #endregion

            #region Node 3

            OpenChargingCloudPKI_Node3  = new ChargingNode(
                                             Name:             I18NString.Create(Languages.en, "OpenChargingCloudPKI_Node3"),
                                             Description:      I18NString.Create(Languages.en, "Open Charging Cloud Public Key Infrastructure - Node 3 of 3"),
                                             Identities:       new[] {
                                                                   new SecP521r1Keys(
                                                                       "BAHg2Wzfj4Xm5DtJzCUIxu/niGxY2cO1hf3jh2azthT3Y+nip6BpcvWcPy+BR6Rj9h/qbgukAimfTqLNUsjDW709EgF+mtGy6JJAm3e1jxtRZcc//s+K7Q6Qw63zAnpnNIPJTHKgag6LY/VaP9a8hYJZOg5xwiCEH9Vkg9rIHziOYMHhiw==",
                                                                       "AcwWAmnwzfjsePCg9u02ustsaOR/916i8jrK6nv7mJmt9+t5gXg0XVm7UXIvGtoi8wB97LTY34RSK0q7vzSQPQVj",
                                                                       WWCPCryptoKeyUsage.Identity,
                                                                       notBefore,
                                                                       notAfter
                                                                   )
                                                               },
                                             IdentityGroups:   new[] {
                                                                   new SecP521r1Keys(
                                                                       "BACJyOXVYqYRHi0Tkt9PEcUR1UZj4NJGpxiv0d4ysOZhto/wrDa3Et9QNg+sTxDkhQZ0hiVN3n4O3Uz2Q+ijjL4dpQH0GsKj5siTcEtUnRUUihIYNhRn42x0iG5N96ObTCeMqM85TaBahGTySrxOEMRx0gWOJ0MBKnprJpsunmsdHvt9Kw==",
                                                                       "ASwnHt2QKA4JEyK9J8Pd25+icdoYuqJcABeGANFgXw/XmZcdnBtFGtEvQJfXFpsKiNaFYcZH6MSw8sTHqLbOPsOg",
                                                                       WWCPCryptoKeyUsage.IdentityGroup,
                                                                       notBefore,
                                                                       notAfter
                                                                   )
                                                               },
                                             DefaultHTTPAPI:   new HTTPAPI(
                                                                   HTTPServerPort:  IPPort.Parse(OpenChargingCloudPKI_Node3_HTTPPort),
                                                                   DNSClient:       DNSClient,
                                                                   Autostart:       true
                                                               ),
                                             DNSClient:        DNSClient
                                          );


            #endregion

            #endregion

            #region Metering Calibration Law Agencies

            // Metering Calibration Law Agencies review and sign smart meters and charging stations,
            // their software and their production process.

            // MCAs register their identity and their certificates within the Open Charging Cloud.


            #region PTB

            PTB        = new MeteringCalibrationLawAgency(
                             Id:               MeteringCalibrationLawAgency_Id.Parse("PTB"),
                             Name:             I18NString.Create(Languages.de, "Physikalisch-Technische Bundesanstalt"),
                             Description:      I18NString.Create(Languages.de, "Die Physikalisch-Technische Bundesanstalt ist das nationale Metrologieinstitut der Bundesrepublik Deutschland"),
                             CryptoKeys:       new[] {
                                                   new SecP521r1Keys(
                                                       "BABXbnt69Dr5+X9T04Y//nhP8UP7c97BSuCn+FX4ZlzwZ0fyMQTnTcZ9ulQ6v4u32XzWB4BFPD0w4oWQDlwOd5o93gFeNd5gmYh31vu2Tv7hqfMfg6H8D+tuU0TSL2d2AdxHnvGEFPBzPVYWmR25zKFeijYUQF998CHrywz/reMCgX++Vw==",
                                                       "AYE6HKxDNg1FSZj9zgCMmSnBZP6VWMCOOO2PTbd/g/LatCACPWRGqsOUv0IGzlOU5Pi+nGysnL/TnxgI+mt/MU7F",
                                                       WWCPCryptoKeyUsage.Identity,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BAHT6heRMQ6Rwgnr2KC9jAd64xgx/qyrlfTyClHi7YCR7KvmZEOpF2PZJiTQYVjdgwDvG8xjoQDkvkrjwVxNDDMXLAFES3DOoZxFvsjdtCOOi1bVLN9/lww+FM4cX8Nd4YwVfC+Pita4WPhYVgbltAEvFS5sLd1+H7C4LhOWCZa7NOqWpg==",
                                                       "OZBe1/wwIz//Y5PCHBdY2McG57k60p79+s58ZftvdRon2GJvWQ1uARkbiFnrEiD4LVuBCOHSxGekn5AFMyGNMag=",
                                                       WWCPCryptoKeyUsage.Signature,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BABvBdJ+Rn09fnGAz8romSS5pRF9Y9LQl60/sFFkxDORgaEt4WalFNOs/zWJqR7ARxMo5loT9i0XtoOvSYT8ySaSCgC+Ma1zqp1u06vVSDHCoLuL+qCvsRYQsueDkaPswDlgdEIlQpGBGMeKGJxwLEmNEJcrU9OD233EC6NA6EpE2XFevw==",
                                                       "AbEM0M0d4w+YuT/DguS/RAR6AgCKyT0bZGRkQ19kWWw+Mvde46x6HEP2AL5TNeyLhaQ0RcFoZVshLn4mCmuXW2cn",
                                                       WWCPCryptoKeyUsage.MeasuringInstrumentsDirective,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BAGqwpw1G2OGnqJgazPuclh6m0GzbeLHZNggZ314RcfmfFWEXLcz53wm0/moZwp/fvY+OJESKJYLsCPTtUEcVn68eQDO3HVM1kx3nOEEyjYsRai8CfUWcnVFwjk1qeZvxI748Pw2wqw+JRsylIeFZtQMy05kX52P6dp3ba10WWHJeRdHLQ==",
                                                       "ARoDAWvjiZgwlbxmHPSizaMXgRVC/nrKxQpbQpsxJAcNiHLRRaQdk/E6ofdh9qspJPf+OeMUZ4ktxeKqN+MInLZj",
                                                       WWCPCryptoKeyUsage.GermanCalibrationLaw_TypeApproval,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BABZDc4qWIAtxqvAw8OimnkTz+lliJqzhT2oJjXRm3n01MV0Z9EKHbKToFpycN56xBy754/2oWmntHgDqYYR3ubANwH1UOmbF/VFpn9zMJ71Y4hnYgRT+MUyBUK2yBsKRHYIyBBj6l/XhpAHYtDnEjaAbnk7FUHJUXA2arlTqCgeL72dgQ==",
                                                       "AO0vNjU/Pp+P88qVLPzwCAvZz4lnvgY7gO2/hgE3ce7hDlBxJ2o8O6X+k7ZpcfwPGq4defsU4zxzvBj2vz322scm",
                                                       WWCPCryptoKeyUsage.GermanCalibrationLaw_QualityAssurance,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               }
                         );

            PTB_Node1  = new ChargingNode(
                             Name:             I18NString.Create(Languages.en, "PTB_Node1"),
                             Description:      I18NString.Create(Languages.de, "Physikalisch-Technische Bundesanstalt - Node 1 of 3"),
                             Identities:       new[] {
                                                   new SecP521r1Keys(
                                                       "BAHyie2ntlBNEJaiY0kFyGkTXf3uBL0gyQIqKQZmgwLec550jmzztm52CPUBQLB1wtxwIUzGobcmtbwk7SsHdBFalgDrCE8UQY2G6rD7TocQWs+0UOAMuTktUUO04JQUs2Ea1CSZ5YvfjVi0ZXC/y+Ui1tqMKtfW5SOLHJ8AkfhPrdSN8A==",
                                                       "AUU/uHl2kQ7abGQgJmulyxBZeLpY2YK346Bn6dnTTQelr9oYs0lW3pOJwDTcV/iUdGNDPCgOcqfITUuukVQRLU6W",
                                                       WWCPCryptoKeyUsage.Identity,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               },
                             IdentityGroups:   new[] {
                                                   new SecP521r1Keys(
                                                       "BAFOQI8mUNfqgPp7SuKF5oQ1CJK/CjHPR3g+2cc8w4rtuESjAYD+uHlQdA67/IZnoamZY+EMWWQzaoQ5W52c7PM3xAH3nYfJkKKhS2oFjPkXZDaAR81tSkJ0YLgr25tce9m0KD8iYBlb83sG/S8dOd2L25il9Q8SSOIwUbmVN1jONTlfIQ==",
                                                       "I59i1RwKSncsy/FY1Rb3PV0Ev9pj0HBae9EeU9Uau6ee3AgtZPGpmKRLwgVS8o7HyZA2ORLvbVvk1HJxuLBjloo=",
                                                       WWCPCryptoKeyUsage.IdentityGroup,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               },
                             DefaultHTTPAPI:   new HTTPAPI(
                                                   HTTPServerPort:  IPPort.Parse(PTB_Node1_HTTPPort),
                                                   DNSClient:       DNSClient,
                                                   Autostart:       true
                                               ),
                             DNSClient:        DNSClient
                         );

            #endregion

            #region VDE

            VDE        = new MeteringCalibrationLawAgency(
                             Id:               MeteringCalibrationLawAgency_Id.Parse("VDE"),
                             Name:             I18NString.Create(Languages.de, "Verband der Elektrotechnik Elektronik Informationstechnik"),
                             Description:      I18NString.Create(Languages.de, "VDE Verband der Elektrotechnik Elektronik Informationstechnik e. V."),
                             CryptoKeys:       new[] {
                                                   new SecP521r1Keys(
                                                       "BAArNbJfoQAo2gNPLp7dPFVW39pWs1+z3CG/BYcjTuCWocU6Mj6kdbTfe9fKvS5NzoD870NZWk4IEwhENnt7hvQRHQB75RuVdGOxXNhkfXUDHawlhjjuPwrssrP4OqaQgM+Ah0aZ+tztpK+cCluph8gMAdpyQuNVFYTV2NnGww/7B0wpWQ==",
                                                       "AaG3cKLbLJpFFt3+dnPVz6L7Hdw0GHpGianqAFCCFCimY4alqHGLN6allSoXW2LoW7uVjZMutK73zXLm0x3CQKTv",
                                                       WWCPCryptoKeyUsage.Identity,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BAEzDxT6qHDTJqfWjbZZC9F1FPr0s3GyJvcJb91wii8lr8sbJ9pIgyh6eFhIQ2PTKEGlqJHnDAkf4ty/2V6gvFYpPAF5JFMnhFmlo9/M2xnOKZ2ovkRyQRQeUqOu0YygOvKa96xa6aTXgJr+92GMNg0nlx1E8tPaD+VT0jCPaW0UZk/xhA==",
                                                       "dW0YZxkrCmhVcd9tkRTAyZ13rb7Yb0+J304jA67DocKD9qpWHaC0mnCwe7W9lr/oJYcaNzjFvmzetHjfyNU7aPY=",
                                                       WWCPCryptoKeyUsage.Signature,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BABwKA4OTHsENYfSUWNfBkNcJ5PcM7vdnaYa9JkGy4viwCuqKDf8X3u/Z7dFUj5g+kexpCyozlbuzdQLpi9JmxFMugBxMDF3eqUWnBB/NBc4rWOC1LiKBYNJ1J0DFJljtE8Vunnmbarw39ncA8VOjzecpsMfjPlDUcDgvh/NKeCbiaf4Tw==",
                                                       "AOauw6htnOUIHGqtHr8kAedfm8Taw6VHP0hRHlRtMJM8jwdwPrlH4gvaZA/3JbnvUtSUH1kvN/veyiMic8ITj+kE",
                                                       WWCPCryptoKeyUsage.MeasuringInstrumentsDirective,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BAEGs8vF+l4rFGaCRm6pnnIHU9AAZBD42/98d5R+XMNiLjWmnO/p+GUnBf2QPrrsVQA6YXP648vGKu6ac5OS/Ye4+wG84/mFSmTGtG9VkQPa5aEN+7HBS7kLNHjqed4QFNQMpsgeuB04+WTlw63Gm0sejOcy5mZ9ApwWRynzbuORX61djw==",
                                                       "AP35EuWEP5kq7dQGv2cyzCbcU0aSV4fr4DmBoxVspF9UFwxf5P9J6bj/rJ/rj5iiFmoRTg0D5WU67i4Bkdzok6JD",
                                                       WWCPCryptoKeyUsage.GermanCalibrationLaw_TypeApproval,
                                                       notBefore,
                                                       notAfter
                                                   ),
                                                   new SecP521r1Keys(
                                                       "BABi6MPJx8GjK7eWvF9ofRVq0PRwJlzKj27NwrUug2w2DoFB+UO+H1JHfHOyFdG2K8wy7oSM2RIxNchX/ASKgRYUIgB7g9eoYy+fgulsjuYP9fe7DiPe3xyVWIrNjeLazGWOM+B4exbZYmIMnzVCUeGAhR8KwZili/CsMSNxq+S/nOJntg==",
                                                       "AIMt6YtKd6CXUDiOUrYfvOEJoo1LFrOJ+vmSrD19vy4WqzNjrYDeMRLIApaIdg8AS9WEbyqm0l+9OXEkzDrWREom",
                                                       WWCPCryptoKeyUsage.GermanCalibrationLaw_QualityAssurance,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               }
                         );

            VDE_Node1  = new ChargingNode(
                             Name:             I18NString.Create(Languages.en, "VDE_Node1"),
                             Description:      I18NString.Create(Languages.de, "Verband der Elektrotechnik Elektronik Informationstechnik - Node 1 of 3"),
                             Identities:       new[] {
                                                   new SecP521r1Keys(
                                                       "BAEjk2LHaXC3bHewphY2VtxEKmBuUd6FiW/6h52IKdra2vcPEjR7piR2CvActMppLGtaEaz29bXksutwYG6y2IsDHQC2BqesLANOtj0lj80zI1NC6JylHN7xm+mkVs+X4REltPhJG44Xp5z1vKvmPvyAR9o/9X/LKv3oXbE85m+MIAaIig==",
                                                       "AaTKr2YNbG0rOsmX/oSVfFZ1+MT1Ovad6CxcWkPwri3BvjXCFDa1qGmgOuPie2LiX8DL3fV56iLl22hK0559/6UK",
                                                       WWCPCryptoKeyUsage.Identity,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               },
                             IdentityGroups:   new[] {
                                                   new SecP521r1Keys(
                                                       "BAF8nLf89yliV7MlxnBw5btyzY/+dcKAvyeX4GTfD2k0QQI8qdMd+wlRVQ3DaYcJ9L7m6ZR05EPegUGz9otFB7GnNwGULhlImqBf5h7j0qvPXXcittez40EQsSL6pVW9Y4gyMz1ktlZIM1wHY+kochH7zf5Lc5SQKhPWnral9QdER6b1zg==",
                                                       "APsSJ++EGD7OECmxBsIeqXZp1QEP5aKmKHv1Kl1b8q3M2mBrQic6ZClQgqhOnca1N5mLXKF0/rSRdDVnTf33w1Uo",
                                                       WWCPCryptoKeyUsage.IdentityGroup,
                                                       notBefore,
                                                       notAfter
                                                   )
                                               },
                             DefaultHTTPAPI:   new HTTPAPI(
                                                   HTTPServerPort:  IPPort.Parse(VDE_Node1_HTTPPort),
                                                   DNSClient:       DNSClient,
                                                   Autostart:       true
                                               ),
                             DNSClient:        DNSClient
                          );

            #endregion


            #region Eichamt

            #endregion

            #endregion

            #region Smart Meter Manufacturers

            // Smart Meter Manufacturers produce smart energy meters:
            //
            //   1. Those smart energy meters have a model template.
            //      Those model templates are signed by metering calibration law agencies.
            //
            //   2. Individual smart energy meters are:
            //      2.1. Signed by the manufacturer AND a metering calibration law agency.
            //      2.2. The metering calibration law agency signes a certificate allowing the manufacturer to do it on their own.
            //
            //   3. Firmware updates are signed by:
            //      3.1. The manufacturer, when the calibration law is not affected.
            //      3.2. The manufacturer AND a metering calibration law agency, when those affect the calibration law.
            //
            //   4. Transparency Software for smart energy meters are signed by metering calibration law agencies.
            //

            // SMMs register their identity and their certificates within the Open Charging Cloud.

            #region GraphDefined SEM

            GraphDefinedSEM          = new SmartMeterManufacturer(
                                           Id:               SmartMeterManufacturer_Id.Parse("GraphDefinedSEM"),
                                           Name:             I18NString.Create(Languages.en, "GraphDefined Smart Energy Metering"),
                                           CryptoKeys:       new[] {
                                                                 new SecP521r1Keys(
                                                                     "BAHFKMrHD0Gu8finZockYrUjC3sHWnnHzwBQoNWZYmriuyT+F33k+36vZP9ZqZjAvBMerJbcEeEM3ulVPAIt1CLFVAFaTqZm3JHpre2I1Rkneh/TVvf3pEo72rEaY4/D3KdE8i6kQnPGDnOdp6DAfDMRm0kRmCmGe9Dj/tKa4oqw/JndUw==",
                                                                     "aqOnZ+ucfOyF0/xPJiPSlFqEdS51Z21vTuimbU0PW8pi3pV23ThdP9Zms9JcX9mNqmC4vLcDoRJdzxOVH0GdCUE=",
                                                                     WWCPCryptoKeyUsage.Identity,
                                                                     notBefore,
                                                                     notAfter
                                                                 ),
                                                                 new SecP521r1Keys(
                                                                     "BACcr9r7JizaJMdMZwCpg58sE+yr+tr9qCnXJ5+VjcparB2xlwQhRBZ69urpSgku5kYU06o/0JxLC/TCTjLUId/H+QAI5K6FHfzytdWRG1uOI0yAL1ZuWH/KTXGKrAOn59phEyzmXYwltJKgq7ux5pS2MMA+3bKwoHFIP8eeKqR2BqkM5Q==",
                                                                     "AaCYLMYwo4aw6zt2rPlRvdVfF/WQoMfIyuk+S4XJOuN1MwwtzgozjLzs4GG46kUbZAjqN90vdOHA+M/RDE/UZ50s",
                                                                     WWCPCryptoKeyUsage.Signature,
                                                                     notBefore,
                                                                     notAfter
                                                                 )
                                                             }
                                       );

            GraphDefinedSEM_M1       = new SmartMeterModel(
                                           Id:               SmartMeterModel_Id.Parse("GraphDefinedSEM_M1"),
                                           Name:             I18NString.Create(Languages.en, "GraphDefined Smart Energy Metering - Smart Meter Model M1")
                                       );

            GraphDefinedSEM_M1_0001  = new SmartMeterDevice(
                                           Id:               SmartMeterDevice_Id.Parse("GraphDefinedSEM_M1_0001"),
                                           Name:             I18NString.Create(Languages.en, "GraphDefined Smart Energy Metering - Smart Meter M1 0001"),
                                           Identities:       new CryptoKeyInfo[] {
                                                                 new SecP521r1Keys(
                                                                     "BADwHOMkplbgGAzHAxmDHl600L2a4CYJ5S629DVYxAQy7ELVUvCCu4wMivwa3A+A2YNr32zF92NPd85LwyYO4Tb44AEH5NNbSMaWYpuHyC9pKZ8NjbDtPg5YcjO/7oLWXh0vcVeKFNeWdi0Rc370ucknSmsY71Sgj1TyFNIFHSXYqXAYbA==",
                                                                     "AJsJ8RtjQihyxVL/iRwyh8eWJhzByoF9xqc9eWG7cbdqtPB7TvtvnRlL4zDn8N4YFzyaNSyPTLFAXE48vBKTbaOY",
                                                                     WWCPCryptoKeyUsage.Identity,
                                                                     notBefore,
                                                                     notAfter
                                                                 ),
                                                                 new SecP256r1Keys(
                                                                     "BKr3Np0dgN9MH6ANP/2L3Ubl9r49bQkJsslA/GH2tf7B0gqODH+tcz+/0TDvw90QWoW6N1Z/rhcr33WBTQ9+rf0=",
                                                                     "XLdToBdLCjSlxQmSDv6Sw4NLOi3JaCuI8dJVpHTHBaU=",
                                                                     WWCPCryptoKeyUsage.Signature,
                                                                     notBefore,
                                                                     notAfter
                                                                 )
                                                             }
                                       );

            GraphDefinedSEM_Node1    = new ChargingNode(
                                           Name:             I18NString.Create(Languages.en, "GraphDefinedSEM_Node1"),
                                           Description:      I18NString.Create(Languages.de, "GraphDefined Smart Energy Metering - Node 1 of 3"),
                                           Identities:       new[] {
                                                                 new SecP521r1Keys(
                                                                     "BAClhSA92s1KInjXUqZ8L89Y2y80fJnaQSyuBPCknjxeYQhuc+hCD5dDED+uOd4Ccms+xr/sJEIIKSljLdiaAdxkWQEJNzMJbIKgfyKfNjEYNphNYBKt0udayF5c/ZqHTO+oWAYY304ojjs9WAbPWx0CSB7qjHIRCWLgV1BTKE/naVQgBA==",
                                                                     "VAL7TXDRnNy9Mc7ENCtbHQ3EDCCOUuSiNgfyP6NEcumeh7kPAguhibCFuFqb6jzjn5XK6/fPv4B5JgQcgxilDVc=",
                                                                     WWCPCryptoKeyUsage.Identity,
                                                                     notBefore,
                                                                     notAfter
                                                                 )
                                                             },
                                           IdentityGroups:   new[] {
                                                                 new SecP521r1Keys(
                                                                     "BAF6HcyIwIstkj7u9KzZz9dYfMPfxMdhqZyXT2G9ZSZxqfzCDU+N2sRfD8a63/LTHheD5opC5I/1AjbTX2t9abLnhAHq1vUuYNkWpOzmVHJWel6cnRgKVcqsS4z8XTImU8D45LgLfny1lFaGqBqp0B9gyfoOyvFxX4NlpgYMjFb1jdsOig==",
                                                                     "AYSbfXi+895sYLiiAHsxWsButKu4JurjDoDkZSA7g31V+SMs3MD0homVHIqYqbZqP5dS24fjsZQUvDv0c8kcrsHP",
                                                                     WWCPCryptoKeyUsage.IdentityGroup,
                                                                     notBefore,
                                                                     notAfter
                                                                 )
                                                             },
                                           DefaultHTTPAPI:   new HTTPAPI(
                                                                 HTTPServerPort:  IPPort.Parse(GraphDefinedSEM_Node1_HTTPPort),
                                                                 DNSClient:       DNSClient,
                                                                 Autostart:       true
                                                             ),
                                           DNSClient:        DNSClient
                                        );

            #endregion

            #endregion

            #region Charging Station Manufacturers

            // Charging Station Manufacturers produce charging stations:
            //
            //   1. Those charging stations have a model template.
            //      Those model templates are signed by metering calibration law agencies.
            //
            //   2. Individual charging stations are:
            //      2.1. Signed by the manufacturer AND a metering calibration law agency.
            //      2.2. The metering calibration law agency signes a certificate allowing the manufacturer to do it on their own.
            //
            //   3. Firmware updates are signed by:
            //      3.1. The manufacturer, when the calibration law is not affected.
            //      3.2. The manufacturer AND a metering calibration law agency, when those affect the calibration law.
            //
            //   4. Transparency Software for charging stations are signed by metering calibration law agencies.
            //
            // Note: Charging Stations can reuse an existing smart energy meter, or be their own smart energy meter.
            //       The prefered way is to encapsulate everything related to energy metering into a smart energy meter.

            // CSMs register their identity and their certificates within the Open Charging Cloud.


            #region GraphDefined CSM

            GraphDefinedCSM        = new ChargingStationManufacturer(
                                         Id:               ChargingStationManufacturer_Id.Parse("GraphDefinedCSM"),
                                         Name:             I18NString.Create(Languages.en, "GraphDefined Charging Station Manufacturing"),
                                         CryptoKeys:       new[] {
                                                               new SecP521r1Keys(
                                                                   "BAAStI+niDMflUP0x82HHlss6az/yAZ0Z/P3EQZjUvfCWvxlg69qDMmpgW1WovOTw9XZshB1JIf3dxikqEEAXfGyRAFQj6ofJFwbeVYzo8w0/0+IlPSlErQR6FDmWHvWR6ouuoP1R1m0BDdhpeCp9z1KlYl/qIDsivuB2Qzy682SUUnO8Q==",
                                                                   "AZ7Ef+Hqn1+TZtWilHB/d7jKTGtN6kx9IwA2946tLbal8kR95K2aBJ1iCw5MPMZzVdAO8oEARidtfFbDYcfqpZGe",
                                                                   WWCPCryptoKeyUsage.Identity,
                                                                   notBefore,
                                                                   notAfter
                                                               )
                                                           }
                                     );

            GraphDefinedCSM_Node1  = new ChargingNode(
                                         Name:             I18NString.Create(Languages.en, "GraphDefinedCSM_Node1"),
                                         Description:      I18NString.Create(Languages.de, "GraphDefined Charging Station Manufacturing - Node 1 of 3"),
                                         Identities:       new[] {
                                                               new SecP521r1Keys(
                                                                   "BAB7om+G9sLoccGkXrc2ak3QKcobuDmPvfu/dzXsLbWbrm3RU/xoO8TB5HmuKsBxAwWH9IBIZNn6ndNZ93U0B+ViwQCpNq48nL1ZCpBWh/jaSNmdW9T1JSOhzHTWzc7CR9/7+Yn5HqQtS3Md0uPURbskX7rR7R3WS2PK9xqdVOyYKvgltQ==",
                                                                   "APvu7tPDN/XVNvQLR6ZCevFgvaOQl02KhfK3GkSPByJE6uqm0gmzd3CPS56mQBc9dlDjZlRUFT3gJX4UQUT2B6W3",
                                                                   WWCPCryptoKeyUsage.Identity,
                                                                   notBefore,
                                                                   notAfter
                                                               )
                                                           },
                                         IdentityGroups:   new[] {
                                                               new SecP521r1Keys(
                                                                   "BACZW58CNfw/orPAY77TmYU+m346s6gWywQEU32co6LXryxRKY1owu9PrgHF2xDL305JBCul60ivagAduznmIWKrXQDjSk6KMwve1lOtp6Q/3rMUmhoWteRdwj8ZjLqlZh6mlhYg+cwxD2SHYZ6MMhniEofkMdJl3be1/KuC2lKuHpvF7A==",
                                                                   "AUT+bzHvlhMjnmsa+Qb6zPPD0J3lInMy80sb1vIdG5inEqYUGlOQ9ZIbRv60rJdTAPk7VhPgSeW4PJmIJcOQGgdD",
                                                                   WWCPCryptoKeyUsage.IdentityGroup,
                                                                   notBefore,
                                                                   notAfter
                                                               )
                                                           },
                                         DefaultHTTPAPI:   new HTTPAPI(
                                                               HTTPServerPort:  IPPort.Parse(GraphDefinedCSM_Node1_HTTPPort),
                                                               DNSClient:       DNSClient,
                                                               Autostart:       true
                                                           ),
                                         DNSClient:        DNSClient
                                      );

            #endregion

            #endregion

            #region Charging Station Operators

            // Charging Station Operators run charging stations at locations.
            // Multiple charging stations are grouped into a charging pool and may have a local controller.
            // At a charging location there can be additional equipment like an upstream energy meter or a payment terminal.
            // Charging Station Operators are directly connected to EMPs or indirectly via EV roaming hubs.

            // CSOs register their identity and their server certificates within the Open Charging Cloud.
            // CSOs send static location data and statistics to EMPs, roaming hubs and national contact points.
            // CSOs send real-time EVSE status data, real-time charging session data, CDRs, and energy data to EMPs or roaming hubs.
            // CSOs may send (statistical) charging session data to national contact points.
            // CSOs send real-time authorization requests of RFID UIDs and ISO 15118 eMAIds (legacy, privacy problem).

            #region GraphDefined CSO

            GraphDefinedCSO_Node1 = new ChargingNode(
                                        Name:             I18NString.Create(Languages.en, "GraphDefinedCSO_Node1"),
                                        Description:      I18NString.Create(Languages.en, "GraphDefined Charging Station Operator - Node 1 of 3"),
                                        Identities:       new[] {
                                                              new SecP256r1Keys(
                                                                  "BLMl4daDO/n+kf3Qz0kcBwG0cGxoDCGQNz/H6W21VL8cAq3l+vgJeNWR50MhoSyVOvI+x8YZRpoBdMl5AzKebaE=",
                                                                  "APKdkn4o9ozmwJ+WFxEJ6qxnsWf6p6Tghcy7aQThJLqq",
                                                                  WWCPCryptoKeyUsage.Identity,
                                                                  notBefore,
                                                                  notAfter
                                                              )
                                                          },
                                        IdentityGroups:   new[] {
                                                              new SecP256r1Keys(
                                                                  "BKDXmC6yyP1OMlDPgsE67fsfII90cskSVkd993EtW+1/1TY5KJAGSpEsQTGdGruKH4sKDztqNbxXpUYicfYUzQ4=",
                                                                  "AOawdrgs0v4cQWmg4vokRzp2cxq+1V2ftr4oVTB+A6we",
                                                                  WWCPCryptoKeyUsage.IdentityGroup,
                                                                  notBefore,
                                                                  notAfter
                                                              )
                                                          },
                                        DefaultHTTPAPI:   new HTTPAPI(
                                                              HTTPServerPort:  IPPort.Parse(GraphDefinedCSO_Node1_HTTPPort),
                                                              DNSClient:       DNSClient,
                                                              Autostart:       true
                                                          ),
                                        DNSClient:        DNSClient
                                    );

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

            cpoVersionsAPIURL = URL.Parse($"http://127.0.0.1:{GraphDefinedCSO_Node1_HTTPPort}/ocpi/v2.1/versions");

            cpoCommonAPI         = new CommonAPI(

                                       OurBaseURL:                          URL.Parse($"http://127.0.0.1:{GraphDefinedCSO_Node1_HTTPPort}/ocpi/v2.1"),
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

            #endregion

            #region E-Mobility Providers

            // E-Mobility Providers allow their customers to charge at all charging stations connected via EV roaming.
            // E-Mobility Providers are directly connected to CSOs or indirectly via EV roaming hubs.

            // EMPs register their identity and their server certificates within the Open Charging Cloud.
            // EMPs give RFID cards to EV drivers (legacy, privacy problem).
            // EMPs send reservations and remote starts to CSOs or roaming hubs.
            // EMPs sign ev driver identity certificates and charging tickets (WWCP).


            #region GraphDefined EMP

            GraphDefinedEMP_Node1 = new ChargingNode(
                                        Name:             I18NString.Create(Languages.en, "GraphDefinedEMP_Node1"),
                                        Description:      I18NString.Create(Languages.en, "GraphDefined E-Mobility Provider - Node 1 of 3"),
                                        Identities:       new[] {
                                                              new SecP256r1Keys(
                                                                  "BCphDksQSmKHJKMVcbPv8ABAYwM4oPz+lW47A5mS0V986jhZpjRo+a4bhhnBDtbvbWgObe0t5Z4fC63IxXG/Y8k=",
                                                                  "PiE4dMglv6W5km72JIdPL2adDF3PdBuRaoitaX8+L8g=",
                                                                  WWCPCryptoKeyUsage.Identity,
                                                                  notBefore,
                                                                  notAfter
                                                              )
                                                          },
                                        IdentityGroups:   new[] {
                                                              new SecP256r1Keys(
                                                                  "BEycchG1xCCAbgIii15U6s4P9D91u5TdS66T/Qf2bnBAi83jilLce56W8p9B6f6PwQyj2Rjw0b7z228ZElSTDtM=",
                                                                  "ALRCex+A7QwYU0rdlFrS6Q19vTXeccaGtzXkT66mt8Hh",
                                                                  WWCPCryptoKeyUsage.IdentityGroup,
                                                                  notBefore,
                                                                  notAfter
                                                              )
                                                          },
                                        DefaultHTTPAPI:   new HTTPAPI(
                                                              HTTPServerPort:  IPPort.Parse(GraphDefinedEMP_Node1_HTTPPort),
                                                              DNSClient:       DNSClient,
                                                              Autostart:       true
                                                          ),
                                        DNSClient:        DNSClient
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

            emsp1VersionsAPIURL  = URL.Parse($"http://127.0.0.1:{GraphDefinedEMP_Node1_HTTPPort}/ocpi/v2.1/versions");

            emsp1CommonAPI       = new CommonAPI(

                                       OurBaseURL:                          URL.Parse($"http://127.0.0.1:{GraphDefinedEMP_Node1_HTTPPort}/ocpi/v2.1"),
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

            #endregion

            #region EV Roaming Hubs

            // EV Roaming Hubs connect CSOs and EMPs, try to simplify communication and to improve data
            // and IT service quality in e-mobility. In reality they are often causing problems.

            // EV Roaming Hubs register their identity and their server certificates within the Open Charging Cloud.


            #region Hubject (OICP)

            Hubject_Node1 = new ChargingNode(
                                Name:             I18NString.Create(Languages.en, "Hubject_Node1"),
                                Description:      I18NString.Create(Languages.en, "Hubject Central EV Roaming Hub - Node 1 of 3"),
                                Identities:       new[] {
                                                      new SecP521r1Keys(
                                                          "BAD8/yuP2rfOhTasqruUkfjs9Q79HkClX3rfbZfXmo/GicuSPYdR0u7FghdgXzmT5/uPSG7IXreExRJbBJ6wHHZA1wBhi9ykAJ1iyKppWbfLTvWYuTlxXjfBLUABbB0eLgJQrM7v0TAPwSXSSbAOATQWLP3dX1bqeHk7GQVXZQYFSef34g==",
                                                          "ZN8c3h0ndlbs2kam8sYeZCfgX0JdUXJhZ7oOuSr3Xc/Z7+wWby6VXgLyJXlTFSXCnx/4X24VwOjYaY54wF1W2Ks=",
                                                          WWCPCryptoKeyUsage.Identity,
                                                          notBefore,
                                                          notAfter
                                                      )
                                                  },
                                IdentityGroups:   new[] {
                                                      new SecP521r1Keys(
                                                          "BACmzmoboAAvcjgXpamKZJF+mfJLbsT+StFa7IrcLgmaVflZ8sRltTxgc0hdnNDvvqlbJ3yCbsYDlsRkWrCqzXWP7AFvinp8W/KjR7+JlrabtwzqeHs97dRX/LXlpa+swSpGc9ThiE/HJwzjBvqt+adTRONQdUipUi2p5yiNqNo4zRDe/A==",
                                                          "Abei2hQuiz5Dd6guUfT5R4CUdCF2jJhpl1VJAd3kKRBhKy/WZPMA4zO8gjJ4te6pg1C3LoAG5ZwgW6sHVizajzwV",
                                                          WWCPCryptoKeyUsage.IdentityGroup,
                                                          notBefore,
                                                          notAfter
                                                      )
                                                  },
                                DefaultHTTPAPI:   new HTTPAPI(
                                                      HTTPServerPort:  IPPort.Parse(Hubject_Node1_HTTPPort),
                                                      DNSClient:       DNSClient,
                                                      Autostart:       true
                                                  ),
                                DNSClient:        DNSClient
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

            #region Gireve (OCPI)

            Gireve_Node1 = new ChargingNode(
                               Name:             I18NString.Create(Languages.en, "Gireve_Node1"),
                               Description:      I18NString.Create(Languages.en, "Gireve Central EV Roaming Hub - Node 1 of 3"),
                               Identities:       new[] {
                                                     new SecP521r1Keys(
                                                         "BADdccXxF+XWPajp7EbLSJZ/LJY0kgBr3rKRpF8e5snWkjGHFKRLvIIUVzKRBOerPgucjIaq23dGkWJSgyG/2hG13AENa3IwGHrR6GbvpCim8JrKIcGdZOX7gaQRw4mZHu21EZvtl7vewlcO+7Hklm90++/4/TJRldGACZ2gLYL7gz0pFA==",
                                                         "AS9YuNgq0yY9rbO/z2fH6gEo+YBWCjwRMhWq0BomPoFS1rZf8lJG/NLAe+3gkcilz+4YoeELFoOHSTnR07qGeXuX",
                                                         WWCPCryptoKeyUsage.Identity,
                                                         notBefore,
                                                         notAfter
                                                     )
                                                 },
                               IdentityGroups:   new[] {
                                                     new SecP521r1Keys(
                                                         "BACI/FAAWdKEKOYlZAUEKNPV3y/QLSv3prerXMRoWiWAqq1ClzbUuGnPYIMr6Jy6Qpo0fpyU52HusQ5y0BqPWVdgUQAb+sswTGcu1M+A4ygB6sOOU0YWnGGjCEFRbWOMbb5KJn4r7o3UCItmOUov7In2PKAj54WKbpH8ZleDS6/wnjyHoQ==",
                                                         "FjoJyQD2e0cKrcv4qD+7jULWfgCKkJYYXL3KFkyEE4+COKCPdgwEQPjQy3narDHmmxzsgCxYRNMiMqet9rLbPIA=",
                                                         WWCPCryptoKeyUsage.IdentityGroup,
                                                         notBefore,
                                                         notAfter
                                                     )
                                                 },
                               DefaultHTTPAPI:   new HTTPAPI(
                                                     HTTPServerPort:  IPPort.Parse(Gireve_Node1_HTTPPort),
                                                     DNSClient:       DNSClient,
                                                     Autostart:       true
                                                 ),
                               DNSClient:        DNSClient
                           );

            Assert.IsNotNull(Gireve_Node1);

            Gireve_Node1.CreateNewRoamingNetwork(
                             RoamingNetworkPROD,
                             Name:                 I18NString.Create(Languages.en, "Roaming Network PROD"),
                             Description:          I18NString.Create(Languages.en, "Roaming Network PROD"),
                             InitialAdminStatus:   RoamingNetworkAdminStatusTypes.Operational,
                             InitialStatus:        RoamingNetworkStatusTypes.Available
                         );

            #endregion

            #endregion

            #region National Contact Points

            // National Contact Points collect charging location data and statistics esp. about errors.
            // They also might collect informations about charging sessions (non real time), but this might conflict with data privacy laws.

            // NCPs register their identity and their server certificates within the Open Charging Cloud.

            #region Leitstelle Elektromobilität

            Leitstelle        = new NationalContactPoint(
                                    Id:               NationalContactPoint_Id.Parse("DE"),
                                    Name:             I18NString.Create(Languages.en, "Leitstelle Elektromobilität"),
                                    Description:      I18NString.Create(Languages.en, "Leitstelle Elektromobilität Deutschland"),
                                    CryptoKeys:       new[] {
                                                          new SecP521r1Keys(
                                                              "BADBAkQtES7KC9pWzaQQZFgWINmYRDxUzYzwYCc5Qug6F9i7rXB+5dmNoJGV+ay9LM5nDPoF2kSB+4h96POldZb5/gFQH7R5T94WFQFGfeL2fBL9M7QYKYkBw4FwvyA7E1y/ag3XhgdK7A9eoQqOy4kTkEaxgUa9CdZh4rQO1kSup64kPQ==",
                                                              "AOQlqe8KsK8lEBb975TC3CujMHUY6F0R40EGuyElt8yih/1qo9gqqn2qsDVMGdxaIjcYmKTLPCXVjVVW7W20Uazy",
                                                              WWCPCryptoKeyUsage.Identity,
                                                              notBefore,
                                                              notAfter
                                                          )
                                                      }
                                );

            Leitstelle_Node1  = new ChargingNode(
                                    Name:             I18NString.Create(Languages.en, "LeitstelleElektromobilität_Node1"),
                                    Description:      I18NString.Create(Languages.en, "Leitstelle Elektromobilität Deutschland - Node 1 of 3"),
                                    Identities:       new[] {
                                                          new SecP521r1Keys(
                                                              "BAFqS+zv2Q/nhKWGMY06g9PGsLOu2v24riOvi3WaL5djnx4Y7QoY9foRPOQyBbbvM6v9dsDDILt93X4cHbQj2NE1KgAlvnwnvMEdMqJAv94iqwZvIN9Mqva/y8yntHktRmhMXC0UtsXH9gN8FOUAwrgaeZMCQEN1Lv8S15+L9PvlcOnq0w==",
                                                              "AYo7JRM88IACoKGnyendgFJbm1bPZlQfrksaR9UEEsXyXQR4DMiP96j4OIb6LnzgeK91KxVdSvAsePbDa3apVKao",
                                                              WWCPCryptoKeyUsage.Identity,
                                                              notBefore,
                                                              notAfter
                                                          )
                                                      },
                                    IdentityGroups:   new[] {
                                                          new SecP521r1Keys(
                                                              "BAD2xjS3DzoZ59vdND49xOB1jobmHfztFWI1m1N1U1Jj4x+jcr093sG1lTZscZem49tg75AUsH7NfUFI15mzyPeN9wHWugHzRbCXx46zP9ybMlAFTL2/riRRykbp3ZD3c+EfHJ+ViEu7azxLu4XiXNs/OH9P0HTv2E8Sf3O46ZgieTlwzQ==",
                                                              "AeiVNgKk+kaWSdmMaPmFLyTmq8u+PpWm8DpbVttjffmERrFPJME1KbXWvav0kFWEOQr+xKePTamBM6UOWrVa2zwc",
                                                              WWCPCryptoKeyUsage.IdentityGroup,
                                                              notBefore,
                                                              notAfter
                                                          )
                                                      },
                                    DefaultHTTPAPI:   new HTTPAPI(
                                                          HTTPServerPort:  IPPort.Parse(Leitstelle_Node1_HTTPPort),
                                                          DNSClient:       DNSClient,
                                                          Autostart:       true
                                                      ),
                                    DNSClient:        DNSClient
                                );

            Assert.IsNotNull(Leitstelle_Node1);

            #endregion

            #endregion

            #region EV Manufacturers

            #endregion

            #region EVs

            // EV drivers may have one or multiple electric vehicles holding identity certificates and charging tickets
            // or legacy ISO 15118 certificates (privacy problem!).
            // EVs may communicate with EV OEM backends, EMP backends or directly with charging stations.
            // EVs can influence smart charging processes.

            #endregion

            #region Smartphones

            // EV drivers may have one or multiple smartphones holding identity certificates and charging tickets.
            // Smartphones may communicate with EMP backends or directly with charging stations.
            // Smartphones can influence smart charging processes.

            #endregion

            #region RFID cards

            // EV drivers may have one or multiple RFID cards having and "unique identifier", which is abused for
            // EV driver authentication when the RFID card is swipped in front of a charging station.

            #endregion

            // Parking Operators

            // Payment Providers

            // Bank Cards

            #region Grid Operators (DSOs et.al)

            // Grid Operators may send information about available energy to CSOs or directly to local controllers or charging stations.
            // Grid Operators may receive energy prognoses data from CSOs based to smart charging schedules.
            // Dokumentationspflichten (2 Jahre, redundante Speicherung, ..., hohe Konventionalstrafen, @Bundesnetzagentur)

            // Ortsnetzstation
            // Ortsnetztrafo
            // Ortsnetztrafoabgang

            // FNN-Steuerbox (EEBus, KNX)

            // Statistische Daten


            // Smart MicroGrid

            #endregion

            // Smart Energy Providers

            // Home Energy Management System Manufacturers

            // Home Energy Management Systems

            // Smart Cities


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
