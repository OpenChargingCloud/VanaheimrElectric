﻿/*
 * Copyright (c) 2015-2023 GraphDefined GmbH
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
using org.GraphDefined.Vanaheimr.Aegir;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.Virtual;
using WWCP = cloud.charging.open.protocols.WWCP;

using cloud.charging.open.protocols.OCPI;
using cloud.charging.open.protocols.OCPIv2_1_1;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests
{

    /// <summary>
    /// OCPI tests in combination with a WWCP roaming network.
    /// Will test the WWCP-to-OCPI adapters.
    /// </summary>
    [TestFixture]
    public class RoamingTests : AChargingInfrastructure
    {

        #region Add_ChargingLocationsAndEVSEs_Test1()

        /// <summary>
        /// Add WWCP charging locations, stations and EVSEs.
        /// Validate that they had been sent to the OCPI module.
        /// Validate via HTTP that they are present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task Add_ChargingLocationsAndEVSEs_Test1()
        {

            if (graphDefinedCSO is not null &&
                cpoAdapter      is not null &&
                emsp1Adapter    is not null &&
                emsp2Adapter    is not null)
            {

                #region Add DE*GEF*POOL1

                // Will call OCPICSOAdapter.AddStaticData(ChargingPool, ...)!
                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id: ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name: I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description: I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address: new Address(

                                                                           Street: "Biberweg",
                                                                           PostalCode: "07749",
                                                                           City: I18NString.Create(Languages.de, "Jena"),
                                                                           Country: Country.Germany,

                                                                           HouseNumber: "18",
                                                                           FloorLevel: null,
                                                                           Region: null,
                                                                           PostalCodeSub: null,
                                                                           TimeZone: Time_Zone.TryParse("CET"),
                                                                           OfficialLanguages: null,
                                                                           Comment: null,

                                                                           CustomData: null,
                                                                           InternalData: null

                                                                       ),
                                                 GeoLocation: GeoCoordinate.Parse(50.93, 11.63),

                                                 OpeningTimes: null,
                                                 ChargingWhenClosed: true,

                                                 //EnergyMix
                                                 //RelatedLocations
                                                 //Directions
                                                 //Facilities
                                                 //Images
                                                 //LocationType

                                                 InitialAdminStatus: ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus: ChargingPoolStatusTypes.Available,

                                                 Configurator: chargingPool =>
                                                 {
                                                 }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);
                Assert.IsTrue(addChargingPoolResult1.IsSuccess);

                var chargingPool1 = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                if (chargingPool1 is not null)
                {

                    var ocpiLocation1 = cpoAdapter.CommonAPI.GetLocations().FirstOrDefault(location => location.Name == chargingPool1.Name.FirstText());
                    Assert.IsNotNull(ocpiLocation1);

                    if (ocpiLocation1 is not null)
                    {

                        Assert.AreEqual($"{chargingPool1.Address?.Street} {chargingPool1.Address?.HouseNumber}", ocpiLocation1.Address);
                        Assert.AreEqual(chargingPool1.Address?.City.FirstText(), ocpiLocation1.City);
                        Assert.AreEqual(chargingPool1.Address?.PostalCode, ocpiLocation1.PostalCode);
                        Assert.AreEqual(chargingPool1.Address?.TimeZone?.ToString(), ocpiLocation1.Timezone);

                        Assert.AreEqual(chargingPool1.GeoLocation, ocpiLocation1.Coordinates);

                        Assert.AreEqual(chargingPool1.ChargingWhenClosed, ocpiLocation1.ChargingWhenClosed);

                    }

                }

                Assert.AreEqual(1, cpoAdapter.CommonAPI.GetLocations().Count());

                #endregion

                #region Add DE*GEF*POOL2

                // Will call OCPICSOAdapter.AddStaticData(ChargingPool, ...)!
                var addChargingPoolResult2 = await graphDefinedCSO.AddChargingPool(

                                                 Id: ChargingPool_Id.Parse("DE*GEF*POOL2"),
                                                 Name: I18NString.Create(Languages.en, "Test pool #2"),
                                                 Description: I18NString.Create(Languages.en, "GraphDefined charging pool for tests #2"),

                                                 Address: new Address(

                                                                           Street: "Biber Weg",
                                                                           PostalCode: "07748",
                                                                           City: I18NString.Create(Languages.de, "Neu-Jena"),
                                                                           Country: Country.Germany,

                                                                           HouseNumber: "18b",
                                                                           FloorLevel: null,
                                                                           Region: null,
                                                                           PostalCodeSub: null,
                                                                           TimeZone: null,
                                                                           OfficialLanguages: null,
                                                                           Comment: null,

                                                                           CustomData: null,
                                                                           InternalData: null

                                                                       ),
                                                 GeoLocation: GeoCoordinate.Parse(50.94, 11.64),

                                                 OpeningTimes: null,
                                                 ChargingWhenClosed: false,

                                                 InitialAdminStatus: ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus: ChargingPoolStatusTypes.Available,

                                                 Configurator: chargingPool =>
                                                 {
                                                 }

                                             );

                Assert.IsNotNull(addChargingPoolResult2);
                Assert.IsTrue(addChargingPoolResult2.IsSuccess);

                var chargingPool2 = addChargingPoolResult2.ChargingPool;
                Assert.IsNotNull(chargingPool2);

                if (chargingPool2 is not null)
                {

                    var ocpiLocation2 = cpoAdapter.CommonAPI.GetLocations().FirstOrDefault(location => location.Name == chargingPool2.Name.FirstText());
                    Assert.IsNotNull(ocpiLocation2);

                    if (ocpiLocation2 is not null)
                    {

                        Assert.AreEqual($"{chargingPool2.Address?.Street} {chargingPool2.Address?.HouseNumber}", ocpiLocation2.Address);
                        Assert.AreEqual(chargingPool2.Address?.City.FirstText(), ocpiLocation2.City);
                        Assert.AreEqual(chargingPool2.Address?.PostalCode, ocpiLocation2.PostalCode);
                        Assert.AreEqual(chargingPool2.Address?.TimeZone?.ToString(), ocpiLocation2.Timezone);

                        Assert.AreEqual(chargingPool2.GeoLocation, ocpiLocation2.Coordinates);

                    }

                }

                Assert.AreEqual(2, cpoAdapter.CommonAPI.GetLocations().Count());

                #endregion


                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id: ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name: I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description: I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation: GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus: ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus: ChargingStationStatusTypes.Available,

                                                    Configurator: chargingStation =>
                                                    {
                                                    }

                                                );

                Assert.IsNotNull(addChargingStationResult1);
                Assert.IsTrue(addChargingStationResult1.IsSuccess);

                var chargingStation1 = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add DE*GEF*STATION*1*B

                var addChargingStationResult2 = await chargingPool1!.AddChargingStation(

                                                    Id: ChargingStation_Id.Parse("DE*GEF*STATION*1*B"),
                                                    Name: I18NString.Create(Languages.en, "Test station #1B"),
                                                    Description: I18NString.Create(Languages.en, "GraphDefined charging station for tests #1B"),

                                                    GeoLocation: GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus: ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus: ChargingStationStatusTypes.Available,

                                                    Configurator: chargingStation =>
                                                    {
                                                    }

                                                );

                Assert.IsNotNull(addChargingStationResult2);
                Assert.IsTrue(addChargingStationResult2.IsSuccess);

                var chargingStation2 = addChargingStationResult2.ChargingStation;
                Assert.IsNotNull(chargingStation2);

                #endregion

                #region Add DE*GEF*STATION*2*A

                var addChargingStationResult3 = await chargingPool2!.AddChargingStation(

                                                    Id: ChargingStation_Id.Parse("DE*GEF*STATION*2*A"),
                                                    Name: I18NString.Create(Languages.en, "Test station #2A"),
                                                    Description: I18NString.Create(Languages.en, "GraphDefined charging station for tests #2A"),

                                                    GeoLocation: GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus: ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus: ChargingStationStatusTypes.Available,

                                                    Configurator: chargingStation =>
                                                    {
                                                    }

                                                );

                Assert.IsNotNull(addChargingStationResult3);
                Assert.IsTrue(addChargingStationResult3.IsSuccess);

                var chargingStation3 = addChargingStationResult3.ChargingStation;
                Assert.IsNotNull(chargingStation3);

                #endregion


                #region Add EVSE DE*GEF*EVSE*1*A*1

                // Will call OCPICSOAdapter.AddStaticData(EVSE, ...)!
                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);
                Assert.IsTrue(addEVSE1Result1.IsSuccess);

                var evse1 = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                if (evse1 is not null)
                {

                    var ocpiLocation1 = cpoAdapter.CommonAPI.GetLocations().FirstOrDefault(location => location.Name == chargingPool1.Name.FirstText());
                    Assert.IsNotNull(ocpiLocation1);

                    var ocpiEVSE1 = ocpiLocation1!.FirstOrDefault(evse => evse.UId.ToString() == evse1.Id.ToString());
                    Assert.IsNotNull(ocpiEVSE1);

                    if (ocpiEVSE1 is not null)
                    {

                        Assert.AreEqual(ocpiEVSE1.EVSEId.ToString(), evse1.Id.ToString());

                    }

                }

                Assert.AreEqual(1, cpoAdapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).Count());

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*2

                var addEVSE1Result2 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*2"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A2"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A2"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result2);

                var evse2 = addEVSE1Result2.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*B*1

                var addEVSE1Result3 = await chargingStation2!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*B*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1B1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1B1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result3);

                var evse3 = addEVSE1Result3.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*2*A*1

                var addEVSE1Result4 = await chargingStation3!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*2*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #2A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #2A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result4);

                var evse4 = addEVSE1Result4.EVSE;
                Assert.IsNotNull(evse4);

                #endregion


                //ToDo: There seems to be a timing issue!
                await Task.Delay(500);


                #region Validate, that locations had been sent to the CPO OCPI module

                var allLocationsAtCPO = cpoAdapter.CommonAPI.GetLocations().ToArray();
                Assert.IsNotNull(allLocationsAtCPO);
                Assert.AreEqual(2, allLocationsAtCPO.Length);

                #endregion

                #region Validate, that EVSEs had been sent to the CPO OCPI module

                var allEVSEsAtCPO = cpoAdapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).ToArray();
                Assert.IsNotNull(allEVSEsAtCPO);
                Assert.AreEqual(4, allEVSEsAtCPO.Length);

                #endregion

                #region Validate, that both CPO locations have EVSEs

                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1.Id.ToString()), out var location1) && location1 is not null)
                    Assert.AreEqual(3, location1.EVSEs.Count());
                else
                    Assert.Fail("location1 was not found!");


                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool2.Id.ToString()), out var location2) && location2 is not null)
                    Assert.AreEqual(1, location2.EVSEs.Count());
                else
                    Assert.Fail("location2 was not found!");

                #endregion


                #region Validate, that EVSEs had been sent to the 1st e-mobility provider

                //var allEVSEsAtEMSP1  = emsp1Adapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).ToArray();
                //Assert.IsNotNull(allEVSEsAtEMSP1);
                //Assert.AreEqual (4, allEVSEsAtEMSP1.Length);

                #endregion

                #region Validate, that EVSEs had been sent to the 2nd e-mobility provider

                //var allEVSEsAtEMSP2  = emsp2Adapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).ToArray();
                //Assert.IsNotNull(allEVSEsAtEMSP2);
                //Assert.AreEqual (4, allEVSEsAtEMSP2.Length);

                #endregion


            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion

        #region Update_ChargingLocationsAndEVSEs_Test1()

        /// <summary>
        /// Add WWCP charging locations, stations and EVSEs and update their static data.
        /// Validate that they had been sent to the OCPI module.
        /// Validate via HTTP that they are present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task Update_ChargingLocationsAndEVSEs_Test1()
        {

            if (csoRoamingNetwork  is not null &&
                emp1RoamingNetwork is not null &&
                emp2RoamingNetwork is not null &&

                cpoAdapter         is not null &&

                graphDefinedCSO    is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                #region Add DE*GEF*POOL2

                var addChargingPoolResult2 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL2"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #2"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #2"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult2);

                var chargingPool2  = addChargingPoolResult2.ChargingPool;
                Assert.IsNotNull(chargingPool2);

                #endregion


                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add DE*GEF*STATION*1*B

                var addChargingStationResult2 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*B"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1B"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1B"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult2);

                var chargingStation2  = addChargingStationResult2.ChargingStation;
                Assert.IsNotNull(chargingStation2);

                #endregion

                #region Add DE*GEF*STATION*2*A

                var addChargingStationResult3 = await chargingPool2!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*2*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #2A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #2A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult3);

                var chargingStation3  = addChargingStationResult3.ChargingStation;
                Assert.IsNotNull(chargingStation3);

                #endregion


                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*2

                var addEVSE1Result2 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*2"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A2"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A2"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result2);

                var evse2     = addEVSE1Result2.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*B*1

                var addEVSE1Result3 = await chargingStation2!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*B*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1B1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1B1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result3);

                var evse3     = addEVSE1Result3.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*2*A*1

                var addEVSE1Result4 = await chargingStation3!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*2*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #2A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #2A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result4);

                var evse4     = addEVSE1Result4.EVSE;
                Assert.IsNotNull(evse4);

                #endregion


                //ToDo: There seems to be a timing issue!
                await Task.Delay(500);


                #region Validate, that locations had been sent to the CPO OCPI module

                var allLocations  = cpoAdapter.CommonAPI.GetLocations().ToArray();
                Assert.IsNotNull(allLocations);
                Assert.AreEqual (2, allLocations.Length);

                #endregion

                #region Validate, that EVSEs had been sent to the CPO OCPI module

                var allEVSEs      = cpoAdapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).ToArray();
                Assert.IsNotNull(allEVSEs);
                Assert.AreEqual (4, allEVSEs.Length);

                #endregion

                #region Validate, that both locations have EVSEs

                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1.Id.ToString()), out var location1) && location1 is not null)
                    Assert.AreEqual(3, location1.EVSEs.Count());
                else
                    Assert.Fail("location1 was not found!");


                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool2.Id.ToString()), out var location2) && location2 is not null)
                    Assert.AreEqual(1, location2.EVSEs.Count());
                else
                    Assert.Fail("location2 was not found!");

                #endregion



                #region Update Add DE*GEF*POOL2, DE*GEF*STATION*2*A, DE*GEF*POOL2

                var updatedPoolProperties     = new List<PropertyUpdateInfo<ChargingPool_Id>>();
                var updatedStationProperties  = new List<PropertyUpdateInfo<ChargingStation_Id>>();
                var updatedEVSEProperties     = new List<PropertyUpdateInfo<WWCP.EVSE_Id>>();

                #region Subscribe charging pool events

                chargingPool1!.OnPropertyChanged += (timestamp,
                                                     eventTrackingId,
                                                     chargingPool,
                                                     propertyName,
                                                     newValue,
                                                     oldValue,
                                                     dataSource) => {

                    updatedPoolProperties.Add(new PropertyUpdateInfo<ChargingPool_Id>((chargingPool as IChargingPool)!.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                chargingPool1!.OnDataChanged += (timestamp,
                                                 eventTrackingId,
                                                 chargingPool,
                                                 propertyName,
                                                 newValue,
                                                 oldValue,
                                                 dataSource) => {

                    updatedPoolProperties.Add(new PropertyUpdateInfo<ChargingPool_Id>(chargingPool.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                graphDefinedCSO.OnChargingPoolDataChanged += (timestamp,
                                                              eventTrackingId,
                                                              chargingPool,
                                                              propertyName,
                                                              newValue,
                                                              oldValue,
                                                              dataSource) => {

                    updatedPoolProperties.Add(new PropertyUpdateInfo<ChargingPool_Id>(chargingPool.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                csoRoamingNetwork.OnChargingPoolDataChanged  += (timestamp,
                                                              eventTrackingId,
                                                              chargingPool,
                                                              propertyName,
                                                              newValue,
                                                              oldValue,
                                                              dataSource) => {

                    updatedPoolProperties.Add(new PropertyUpdateInfo<ChargingPool_Id>(chargingPool.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                #endregion

                #region Subscribe charging station events

                chargingStation1!.OnPropertyChanged += (timestamp,
                                                        eventTrackingId,
                                                        chargingStation,
                                                        propertyName,
                                                        newValue,
                                                        oldValue,
                                                        dataSource) => {

                    updatedStationProperties.Add(new PropertyUpdateInfo<ChargingStation_Id>((chargingStation as IChargingStation)!.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                chargingStation1!.OnDataChanged += (timestamp,
                                                    eventTrackingId,
                                                    chargingStation,
                                                    propertyName,
                                                    newValue,
                                                    oldValue,
                                                    dataSource) => {

                    updatedStationProperties.Add(new PropertyUpdateInfo<ChargingStation_Id>(chargingStation.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                graphDefinedCSO.OnChargingStationDataChanged += (timestamp,
                                                                 eventTrackingId,
                                                                 chargingStation,
                                                                 propertyName,
                                                                 newValue,
                                                                 oldValue,
                                                                 dataSource) => {

                    updatedStationProperties.Add(new PropertyUpdateInfo<ChargingStation_Id>(chargingStation.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                csoRoamingNetwork.OnChargingStationDataChanged += (timestamp,
                                                                   eventTrackingId,
                                                                   chargingStation,
                                                                   propertyName,
                                                                   newValue,
                                                                   oldValue,
                                                                   dataSource) => {

                    updatedStationProperties.Add(new PropertyUpdateInfo<ChargingStation_Id>(chargingStation.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                #endregion

                #region Subscribe EVSE events

                evse1!.OnPropertyChanged += (timestamp,
                                             eventTrackingId,
                                             evse,
                                             propertyName,
                                             newValue,
                                             oldValue,
                                             dataSource) => {

                    updatedEVSEProperties.Add(new PropertyUpdateInfo<WWCP.EVSE_Id>((evse as IEVSE)!.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                evse1!.OnDataChanged += (timestamp,
                                         eventTrackingId,
                                         evse,
                                         propertyName,
                                         newValue,
                                         oldValue,
                                         dataSource) => {

                    updatedEVSEProperties.Add(new PropertyUpdateInfo<WWCP.EVSE_Id>(evse.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                graphDefinedCSO.OnEVSEDataChanged += (timestamp,
                                                      eventTrackingId,
                                                      evse,
                                                      propertyName,
                                                      newValue,
                                                      oldValue,
                                                      dataSource) => {

                    updatedEVSEProperties.Add(new PropertyUpdateInfo<WWCP.EVSE_Id>(evse.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                csoRoamingNetwork.OnEVSEDataChanged += (timestamp,
                                                        eventTrackingId,
                                                        evse,
                                                        propertyName,
                                                        newValue,
                                                        oldValue,
                                                        dataSource) => {

                    updatedEVSEProperties.Add(new PropertyUpdateInfo<WWCP.EVSE_Id>(evse.Id, propertyName, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                #endregion

                cpoAdapter.CommonAPI.OnLocationChanged += async (location) => { };
                cpoAdapter.CommonAPI.OnEVSEChanged     += async (evse)     => { };


                chargingPool1!.Name.       Set(Languages.en, "Test pool #1 (updated)");
                chargingPool1!.Description.Set(Languages.en, "GraphDefined charging pool for tests #1 (updated)");

                Assert.AreEqual(8, updatedPoolProperties.Count);
                Assert.AreEqual("Test pool #1 (updated)",                             graphDefinedCSO.GetChargingPoolById(chargingPool1!.Id)!.Name       [Languages.en]);
                Assert.AreEqual("GraphDefined charging pool for tests #1 (updated)",  graphDefinedCSO.GetChargingPoolById(chargingPool1!.Id)!.Description[Languages.en]);


                //ToDo: There seems to be a timing issue!
                await Task.Delay(500);


                cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1!.Id.ToString()), out var location);
                Assert.AreEqual("Test pool #1 (updated)",                             location.Name);
                //Assert.AreEqual("GraphDefined Charging Pool für Tests #1",            location!.Name); // Not mapped to OCPI!


                evse1.Name.Set(Languages.en, "Test EVSE #1A1 (updated)");















                //var updateChargingPoolResult2 = await graphDefinedCSO.UpdateChargingPool()


                //var updateChargingPoolResult2 = await chargingPool2.Address = new Address(

                //                                                                  Street:             "Biberweg",
                //                                                                  PostalCode:         "07749",
                //                                                                  City:               I18NString.Create(Languages.da, "Jena"),
                //                                                                  Country:            Country.Germany,

                //                                                                  HouseNumber:        "18",
                //                                                                  FloorLevel:         null,
                //                                                                  Region:             null,
                //                                                                  PostalCodeSub:      null,
                //                                                                  TimeZone:           null,
                //                                                                  OfficialLanguages:  null,
                //                                                                  Comment:            null,

                //                                                                  CustomData:         null,
                //                                                                  InternalData:       null

                //                                                              );

                //Assert.IsNotNull(addChargingPoolResult2);

                //var chargingPool2  = addChargingPoolResult2.ChargingPool;
                //Assert.IsNotNull(chargingPool2);

                #endregion



            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion

        #region Update_EVSEStatus_Test1()

        /// <summary>
        /// Add WWCP charging locations, stations and EVSEs and update the EVSE status.
        /// Validate that they had been sent to the OCPI module.
        /// Validate via HTTP that they are present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task Update_EVSEStatus_Test1()
        {

            if (csoRoamingNetwork  is not null &&
                emp1RoamingNetwork is not null &&
                emp2RoamingNetwork is not null &&

                graphDefinedCSO    is not null &&

                cpoAdapter         is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                #region Add DE*GEF*POOL2

                var addChargingPoolResult2 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL2"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #2"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #2"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult2);

                var chargingPool2  = addChargingPoolResult2.ChargingPool;
                Assert.IsNotNull(chargingPool2);

                #endregion


                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add DE*GEF*STATION*1*B

                var addChargingStationResult2 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*B"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1B"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1B"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult2);

                var chargingStation2  = addChargingStationResult2.ChargingStation;
                Assert.IsNotNull(chargingStation2);

                #endregion

                #region Add DE*GEF*STATION*2*A

                var addChargingStationResult3 = await chargingPool2!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*2*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #2A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #2A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult3);

                var chargingStation3  = addChargingStationResult3.ChargingStation;
                Assert.IsNotNull(chargingStation3);

                #endregion


                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*2

                var addEVSE1Result2 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*2"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A2"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A2"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result2);

                var evse2     = addEVSE1Result2.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*B*1

                var addEVSE1Result3 = await chargingStation2!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*B*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1B1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1B1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result3);

                var evse3     = addEVSE1Result3.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*2*A*1

                var addEVSE1Result4 = await chargingStation3!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*2*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #2A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #2A1"),

                                          ChargingConnectors:   new[] {
                                                                      new ChargingConnector(
                                                                          Id:             ChargingConnector_Id.Parse(1),
                                                                          Plug:           ChargingPlugTypes.Type2Connector_CableAttached,
                                                                          Lockable:       true,
                                                                          CableAttached:  true,
                                                                          CableLength:    Meter.Parse(3.0)
                                                                      )
                                                                  },

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result4);

                var evse4     = addEVSE1Result4.EVSE;
                Assert.IsNotNull(evse4);

                #endregion


                //ToDo: There seems to be a timing issue!
                await Task.Delay(500);


                #region Validate, that locations had been sent to the CPO OCPI module

                var allLocations  = cpoAdapter.CommonAPI.GetLocations().ToArray();
                Assert.IsNotNull(allLocations);
                Assert.AreEqual (2, allLocations.Length);

                #endregion

                #region Validate, that EVSEs had been sent to the CPO OCPI module

                var allEVSEs      = cpoAdapter.CommonAPI.GetLocations().SelectMany(location => location.EVSEs).ToArray();
                Assert.IsNotNull(allEVSEs);
                Assert.AreEqual (4, allEVSEs.Length);

                #endregion

                #region Validate, that both locations have EVSEs

                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1.Id.ToString()), out var location1) && location1 is not null)
                    Assert.AreEqual(3, location1.EVSEs.Count());
                else
                    Assert.Fail("location1 was not found!");


                if (cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool2.Id.ToString()), out var location2) && location2 is not null)
                    Assert.AreEqual(1, location2.EVSEs.Count());
                else
                    Assert.Fail("location2 was not found!");

                #endregion



                var evse1_UId = evse1!.Id.ToOCPI_EVSEUId();
                Assert.IsTrue(evse1_UId.HasValue);

                #region Subscribe WWCP EVSE events

                var updatedEVSEStatus         = new List<EVSEStatusUpdate>();

                evse1!.OnStatusChanged += (timestamp,
                                           eventTrackingId,
                                           evse,
                                           newValue,
                                           oldValue,
                                           dataSource) => {

                    updatedEVSEStatus.Add(new EVSEStatusUpdate(evse.Id, newValue, oldValue, dataSource));
                    return Task.CompletedTask;

                };

                graphDefinedCSO.OnEVSEStatusChanged += (timestamp,
                                                        eventTrackingId,
                                                        evse,
                                                        newEVSEStatus,
                                                        oldEVSEStatus,
                                                        dataSource) => {

                    updatedEVSEStatus.Add(new EVSEStatusUpdate(evse.Id, newEVSEStatus, oldEVSEStatus, dataSource));
                    return Task.CompletedTask;

                };

                csoRoamingNetwork.OnEVSEStatusChanged += (timestamp,
                                                          eventTrackingId,
                                                          evse,
                                                          newEVSEStatus,
                                                          oldEVSEStatus,
                                                          dataSource) => {

                    updatedEVSEStatus.Add(new EVSEStatusUpdate(evse.Id, newEVSEStatus, oldEVSEStatus, dataSource));
                    return Task.CompletedTask;

                };

                #endregion

                #region Subscribe OCPI EVSE events

                var updatedOCPIEVSEStatus = new List<StatusType>();

                cpoAdapter.CommonAPI.OnEVSEChanged += (evse) => {

                    updatedOCPIEVSEStatus.Add(evse.Status);
                    return Task.CompletedTask;

                };

                cpoAdapter.CommonAPI.OnEVSEStatusChanged += (timestamp,
                                                             evse,
                                                             newEVSEStatus,
                                                             oldEVSEStatus) => {

                    updatedOCPIEVSEStatus.Add(newEVSEStatus);
                    return Task.CompletedTask;

                };

                #endregion

                #region Update

                {
                    if (evse1_UId.HasValue &&
                        cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1!.Id.Suffix), out var location) && location is not null &&
                        location.TryGetEVSE(evse1_UId.Value, out var ocpiEVSE) && ocpiEVSE is not null)
                    {
                        Assert.AreEqual(StatusType.AVAILABLE, ocpiEVSE.Status);
                    }
                }



                evse1.SetStatus(EVSEStatusTypes.Charging);


                //ToDo: There seems to be a timing issue!
                await Task.Delay(500);


                Assert.AreEqual(3, updatedEVSEStatus.    Count);
                Assert.AreEqual(2, updatedOCPIEVSEStatus.Count);

                Assert.AreEqual(EVSEStatusTypes.Charging,  graphDefinedCSO.GetEVSEById(evse1!.Id).Status.Value);

                {
                    if (evse1_UId.HasValue &&
                        cpoAdapter.CommonAPI.TryGetLocation(Location_Id.Parse(chargingPool1!.Id.Suffix), out var location) && location is not null &&
                        location.TryGetEVSE(evse1_UId.Value, out var ocpiEVSE) && ocpiEVSE is not null)
                    {
                        Assert.AreEqual(StatusType.CHARGING, ocpiEVSE.Status);
                    }
                }

                #endregion


            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion


        #region AuthStarts_Test1()

        /// <summary>
        /// Send multiple authorization start requests.
        /// Validate that the authorization start request had been sent to the OCPI module.
        /// Validate via HTTP that the authorization start request is present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task AuthStarts_Test1()
        {

            if (csoRoamingNetwork      is not null &&
                emp1RoamingNetwork     is not null &&
                emp2RoamingNetwork     is not null &&

                cpoCPOAPI              is not null &&
                emsp1EMSPAPI           is not null &&
                emsp2EMSPAPI           is not null &&

                graphDefinedCSO        is not null &&
                graphDefinedEMP1Local  is not null &&
                graphDefinedEMP2Local  is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                #region Add DE*GEF*POOL2

                var addChargingPoolResult2 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL2"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #2"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #2"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult2);

                var chargingPool2  = addChargingPoolResult2.ChargingPool;
                Assert.IsNotNull(chargingPool2);

                #endregion


                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add DE*GEF*STATION*1*B

                var addChargingStationResult2 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*B"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1B"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1B"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult2);

                var chargingStation2  = addChargingStationResult2.ChargingStation;
                Assert.IsNotNull(chargingStation2);

                #endregion

                #region Add DE*GEF*STATION*2*A

                var addChargingStationResult3 = await chargingPool2!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*2*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #2A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #2A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult3);

                var chargingStation3  = addChargingStationResult3.ChargingStation;
                Assert.IsNotNull(chargingStation3);

                #endregion


                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*2

                var addEVSE1Result2 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*2"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A2"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A2"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result2);

                var evse2     = addEVSE1Result2.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*B*1

                var addEVSE1Result3 = await chargingStation2!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*B*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1B1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1B1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result3);

                var evse3     = addEVSE1Result3.EVSE;
                Assert.IsNotNull(evse2);

                #endregion

                #region Add EVSE DE*GEF*EVSE*2*A*1

                var addEVSE1Result4 = await chargingStation3!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*2*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #2A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #2A1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          Configurator:         evse => {
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result4);

                var evse4     = addEVSE1Result4.EVSE;
                Assert.IsNotNull(evse4);

                #endregion


                graphDefinedEMP1Local.AddAuth(AuthenticationToken.Parse("11223344"),
                                              AuthStartResult.    Authorized(AuthorizatorId:            graphDefinedEMP1Local.AuthId,
                                                                             ISendAuthorizeStartStop:   null,
                                                                             SessionId:                 null,
                                                                             EMPPartnerSessionId:       null,
                                                                             ContractId:                null,
                                                                             PrintedNumber:             null,
                                                                             ExpiryDate:                null,
                                                                             MaxkW:                     null,
                                                                             MaxkWh:                    null,
                                                                             MaxDuration:               null,
                                                                             ChargingTariffs:           null,
                                                                             ListOfAuthStopTokens:      null,
                                                                             ListOfAuthStopPINs:        null,

                                                                             ProviderId:                null,
                                                                             Description:               null,
                                                                             AdditionalInfo:            null,
                                                                             NumberOfRetries:           0,
                                                                             Runtime:                   null));

                graphDefinedEMP2Local.AddAuth(AuthenticationToken.Parse("55667788"),
                                              AuthStartResult.    Authorized(AuthorizatorId:            graphDefinedEMP2Local.AuthId,
                                                                             ISendAuthorizeStartStop:   null,
                                                                             SessionId:                 null,
                                                                             EMPPartnerSessionId:       null,
                                                                             ContractId:                null,
                                                                             PrintedNumber:             null,
                                                                             ExpiryDate:                null,
                                                                             MaxkW:                     null,
                                                                             MaxkWh:                    null,
                                                                             MaxDuration:               null,
                                                                             ChargingTariffs:           null,
                                                                             ListOfAuthStopTokens:      null,
                                                                             ListOfAuthStopPINs:        null,

                                                                             ProviderId:                null,
                                                                             Description:               null,
                                                                             AdditionalInfo:            null,
                                                                             NumberOfRetries:           0,
                                                                             Runtime:                   null));


                var authStartResult1 = await csoRoamingNetwork.AuthorizeStart(
                                                 LocalAuthentication: LocalAuthentication.FromAuthToken(AuthenticationToken.NewRandom7Bytes),
                                                 ChargingLocation:    ChargingLocation.   FromEVSEId   (evse1!.Id),
                                                 ChargingProduct:     ChargingProduct.    FromId       (ChargingProduct_Id.Parse("AC1"))
                                             );

                var authStartResult2 = await csoRoamingNetwork.AuthorizeStart(
                                                 LocalAuthentication: LocalAuthentication.FromAuthToken(AuthenticationToken.Parse("11223344")),
                                                 ChargingLocation:    ChargingLocation.   FromEVSEId   (evse1!.Id),
                                                 ChargingProduct:     ChargingProduct.    FromId       (ChargingProduct_Id.Parse("AC1"))
                                             );

                var authStartResult3 = await csoRoamingNetwork.AuthorizeStart(
                                                 LocalAuthentication: LocalAuthentication.FromAuthToken(AuthenticationToken.Parse("55667788")),
                                                 ChargingLocation:    ChargingLocation.   FromEVSEId   (evse1!.Id),
                                                 ChargingProduct:     ChargingProduct.    FromId       (ChargingProduct_Id.Parse("AC1"))
                                             );

                Assert.AreEqual(AuthStartResultTypes.NotAuthorized, authStartResult1.Result);
                Assert.AreEqual(AuthStartResultTypes.Authorized,    authStartResult2.Result);
                Assert.AreEqual(AuthStartResultTypes.Authorized,    authStartResult3.Result);

            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion

        #region SendChargeDetailRecord_Test1()

        /// <summary>
        /// Send a charge detail record.
        /// Validate that the charge detail record had been sent to the OCPI module.
        /// Validate via HTTP that the charge detail record is present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task SendChargeDetailRecord_Test1()
        {

            if (csoRoamingNetwork      is not null &&
                emp1RoamingNetwork     is not null &&
                emp2RoamingNetwork     is not null &&

                cpoCPOAPI              is not null &&
                emsp1EMSPAPI           is not null &&
                emsp2EMSPAPI           is not null &&

                graphDefinedCSO        is not null &&
                graphDefinedEMP1Local  is not null &&
                graphDefinedEMP2Local  is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          ChargingConnectors:   new[] {
                                                                    new ChargingConnector(
                                                                        Plug:            ChargingPlugTypes.Type2Connector_CableAttached,
                                                                        Lockable:        true,
                                                                        CableAttached:   true,
                                                                        CableLength:     Meter.Parse(4)
                                                                    )
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion


                graphDefinedEMP1Local.AddAuth(AuthenticationToken.Parse("11223344"),
                                              AuthStartResult.    Authorized(AuthorizatorId:            graphDefinedEMP1Local.AuthId,
                                                                             ISendAuthorizeStartStop:   null,
                                                                             SessionId:                 null,
                                                                             EMPPartnerSessionId:       null,
                                                                             ContractId:                null,
                                                                             PrintedNumber:             null,
                                                                             ExpiryDate:                null,
                                                                             MaxkW:                     null,
                                                                             MaxkWh:                    null,
                                                                             MaxDuration:               null,
                                                                             ChargingTariffs:           null,
                                                                             ListOfAuthStopTokens:      null,
                                                                             ListOfAuthStopPINs:        null,

                                                                             ProviderId:                null,
                                                                             Description:               null,
                                                                             AdditionalInfo:            null,
                                                                             NumberOfRetries:           0,
                                                                             Runtime:                   null));

                if (evse1                 is not null &&
                    evse1.ChargingStation is not null &&
                    evse1.ChargingPool    is not null &&
                    evse1.Operator        is not null)
                {

                    var sendCDRResult = await csoRoamingNetwork.SendChargeDetailRecord(
                                                  new ChargeDetailRecord(
                                                      Id:                           ChargeDetailRecord_Id.NewRandom,
                                                      SessionId:                    ChargingSession_Id.   NewRandom,
                                                      SessionTime:                  new StartEndDateTime(
                                                                                        Timestamp.Now - TimeSpan.FromHours(2),
                                                                                        Timestamp.Now - TimeSpan.FromMinutes(2)
                                                                                    ),
                                                      Duration:                     TimeSpan.FromMinutes(118),

                                                      ChargingConnector:            evse1.ChargingConnectors.First(),
                                                      //EVSE:                         evse1,                      // automagic!
                                                      //EVSEId:                       evse1.Id,                   // automagic!
                                                      //ChargingStation:              evse1.ChargingStation,      // automagic!
                                                      //ChargingStationId:            evse1.ChargingStation.Id,   // automagic!
                                                      //ChargingPool:                 evse1.ChargingPool,         // automagic!
                                                      //ChargingPoolId:               evse1.ChargingPool.Id,      // automagic!
                                                      //ChargingStationOperator:      evse1.Operator,             // automagic!
                                                      //ChargingStationOperatorId:    evse1.Operator.Id,          // automagic!

                                                      ChargingProduct:              ChargingProduct.FromId(ChargingProduct_Id.Parse("AC1")),
                                                      ChargingPrice:                null,

                                                      AuthenticationStart:          LocalAuthentication.FromAuthToken(AuthenticationToken.NewRandom7Bytes),
                                                      AuthenticationStop:           LocalAuthentication.FromAuthToken(AuthenticationToken.NewRandom7Bytes),
                                                      AuthMethodStart:              AuthMethod.AUTH_REQUEST,
                                                      AuthMethodStop:               AuthMethod.WHITELIST,
                                                      ProviderIdStart:              EMobilityProvider_Id.Parse("DE-GDF"),
                                                      ProviderIdStop:               EMobilityProvider_Id.Parse("DE-GD2"),

                                                      EMPRoamingProvider:           null,
                                                      EMPRoamingProviderId:         null,

                                                      Reservation:                  null,
                                                      ReservationId:                null,
                                                      ReservationTime:              null,
                                                      ReservationFee:               null,

                                                      ParkingSpaceId:               null,
                                                      ParkingTime:                  null,
                                                      ParkingFee:                   null,

                                                      EnergyMeterId:                null,
                                                      EnergyMeter:                  null,
                                                      EnergyMeteringValues:         null,
                                                      ConsumedEnergy:               null,
                                                      ConsumedEnergyFee:            null,

                                                      CustomData:                   null,
                                                      InternalData:                 null,

                                                      PublicKey:                    null,
                                                      Signatures:                   null));


                    Assert.AreEqual(SendCDRsResultTypes.Success, sendCDRResult.Result);

                }

            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion


        #region AuthStartStopCDR_Test1()

        /// <summary>
        /// Send an authorization start, authorization stop and a CDR request.
        /// Validate that the authorization start request had been sent to the OCPI module.
        /// Validate via HTTP that the authorization start request is present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task AuthStartStopCDR_Test1()
        {

            if (csoRoamingNetwork      is not null &&
                emp1RoamingNetwork     is not null &&
                emp2RoamingNetwork     is not null &&

                cpoCPOAPI              is not null &&
                emsp1EMSPAPI           is not null &&
                emsp2EMSPAPI           is not null &&

                graphDefinedCSO        is not null &&
                graphDefinedEMP1Local  is not null &&
                graphDefinedEMP2Local  is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          ChargingConnectors:   new[] {
                                                                    new ChargingConnector(
                                                                        Plug:            ChargingPlugTypes.Type2Connector_CableAttached,
                                                                        Lockable:        true,
                                                                        CableAttached:   true,
                                                                        CableLength:     Meter.Parse(4)
                                                                    )
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion


                graphDefinedEMP1Local.AddAuth(AuthenticationToken.Parse("11223344"),
                                              AuthStartResult.    Authorized(AuthorizatorId:            graphDefinedEMP1Local.AuthId,
                                                                             ISendAuthorizeStartStop:   null,
                                                                             SessionId:                 null,
                                                                             EMPPartnerSessionId:       null,
                                                                             ContractId:                null,
                                                                             PrintedNumber:             null,
                                                                             ExpiryDate:                null,
                                                                             MaxkW:                     null,
                                                                             MaxkWh:                    null,
                                                                             MaxDuration:               null,
                                                                             ChargingTariffs:           null,
                                                                             ListOfAuthStopTokens:      null,
                                                                             ListOfAuthStopPINs:        null,

                                                                             ProviderId:                null,
                                                                             Description:               null,
                                                                             AdditionalInfo:            null,
                                                                             NumberOfRetries:           0,
                                                                             Runtime:                   null));

                graphDefinedEMP2Local.AddAuth(AuthenticationToken.Parse("55667788"),
                                              AuthStartResult.    Authorized(AuthorizatorId:            graphDefinedEMP1Local.AuthId,
                                                                             ISendAuthorizeStartStop:   null,
                                                                             SessionId:                 null,
                                                                             EMPPartnerSessionId:       null,
                                                                             ContractId:                null,
                                                                             PrintedNumber:             null,
                                                                             ExpiryDate:                null,
                                                                             MaxkW:                     null,
                                                                             MaxkWh:                    null,
                                                                             MaxDuration:               null,
                                                                             ChargingTariffs:           null,
                                                                             ListOfAuthStopTokens:      null,
                                                                             ListOfAuthStopPINs:        null,

                                                                             ProviderId:                null,
                                                                             Description:               null,
                                                                             AdditionalInfo:            null,
                                                                             NumberOfRetries:           0,
                                                                             Runtime:                   null));


                if (evse1                 is not null &&
                    evse1.ChargingStation is not null &&
                    evse1.ChargingPool    is not null &&
                    evse1.Operator        is not null)
                {

                    var authTokenStart   = AuthenticationToken.Parse("11223344");
                    var chargingProduct  = ChargingProduct.FromId(ChargingProduct_Id.Parse("AC1"));
                    var authStartResult  = await csoRoamingNetwork.AuthorizeStart(
                                                     LocalAuthentication:  LocalAuthentication.FromAuthToken(authTokenStart),
                                                     ChargingLocation:     ChargingLocation.   FromEVSEId   (evse1!.Id),
                                                     ChargingProduct:      chargingProduct
                                                 );

                    Assert.AreEqual(AuthStartResultTypes.Authorized,  authStartResult.Result);

                    if (authStartResult           is not null &&
                        authStartResult.SessionId is not null)
                    {

                        Timestamp.TravelForwardInTime(TimeSpan.FromMinutes(5));

                        var authTokenStop    = AuthenticationToken.Parse("55667788");
                        var authStopResult   = await csoRoamingNetwork.AuthorizeStop(
                                                         SessionId:            authStartResult.SessionId.Value,
                                                         LocalAuthentication:  LocalAuthentication.FromAuthToken(authTokenStop)
                                                     );

                        Assert.AreEqual(AuthStopResultTypes.Authorized,  authStopResult.Result);


                        var startTS          = Timestamp.Now - TimeSpan.FromMinutes(5);
                        var stopTS           = Timestamp.Now - TimeSpan.FromMinutes(1);

                        var sendCDRResult    = await csoRoamingNetwork.SendChargeDetailRecord(
                                                         new ChargeDetailRecord(
                                                             Id:                          ChargeDetailRecord_Id.NewRandom,
                                                             SessionId:                   authStartResult.SessionId.Value,
                                                             SessionTime:                 new StartEndDateTime(
                                                                                              startTS,
                                                                                              stopTS
                                                                                          ),
                                                             Duration:                    TimeSpan.FromMinutes(4),

                                                             ChargingConnector:           evse1.ChargingConnectors.First(),
                                                             //EVSE:                        evse1,                      // automagic!
                                                             //EVSEId:                      evse1.Id,                   // automagic!
                                                             //ChargingStation:             evse1.ChargingStation,      // automagic!
                                                             //ChargingStationId:           evse1.ChargingStation.Id,   // automagic!
                                                             //ChargingPool:                evse1.ChargingPool,         // automagic!
                                                             //ChargingPoolId:              evse1.ChargingPool.Id,      // automagic!
                                                             //ChargingStationOperator:     evse1.Operator,             // automagic!
                                                             //ChargingStationOperatorId:   evse1.Operator.Id,          // automagic!

                                                             ChargingProduct:             chargingProduct,
                                                             ChargingPrice:               new Price(
                                                                                              1.23M,
                                                                                              VAT:       0.34M,
                                                                                              Currency:  org.GraphDefined.Vanaheimr.Illias.Currency.EUR
                                                                                          ),

                                                             AuthenticationStart:         LocalAuthentication.FromAuthToken(authTokenStart),
                                                             AuthenticationStop:          LocalAuthentication.FromAuthToken(authTokenStop),
                                                             AuthMethodStart:             AuthMethod.AUTH_REQUEST,
                                                             AuthMethodStop:              AuthMethod.AUTH_REQUEST,
                                                             ProviderIdStart:             authStartResult.ProviderId,
                                                             ProviderIdStop:              authStopResult. ProviderId,

                                                             EMPRoamingProvider:          null,
                                                             EMPRoamingProviderId:        null,

                                                             Reservation:                 null,
                                                             ReservationId:               null,
                                                             ReservationTime:             null,
                                                             ReservationFee:              null,

                                                             ParkingSpaceId:              null,
                                                             ParkingTime:                 new StartEndDateTime(
                                                                                              startTS,
                                                                                              stopTS
                                                                                          ),
                                                             ParkingFee:                  null,

                                                             EnergyMeterId:               evse1.EnergyMeter?.Id,
                                                             EnergyMeter:                 evse1.EnergyMeter,
                                                             EnergyMeteringValues:        new[] {
                                                                                              new EnergyMeteringValue(
                                                                                                  startTS,
                                                                                                  1334.034M,
                                                                                                  "1334.034",
                                                                                                  "..."
                                                                                              ),
                                                                                              new EnergyMeteringValue(
                                                                                                  stopTS,
                                                                                                  1451.241M,
                                                                                                  "1451.241",
                                                                                                  "..."
                                                                                              )
                                                                                          },
                                                             ConsumedEnergy:              null,
                                                             ConsumedEnergyFee:           null,

                                                             CustomData:                  null,
                                                             InternalData:                null,

                                                             PublicKey:                   null,
                                                             Signatures:                  null));

                        Assert.AreEqual(SendCDRsResultTypes.Success,  sendCDRResult.Result);

                    }
                    else
                        Assert.Fail("The authorize start failed!");

                }
                else
                    Assert.Fail("Faile to setup EVSE #1!");

            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion

        #region RemoteStartStopCDR_Test1()

        /// <summary>
        /// Send a remote start, remote stop and a CDR  request.
        /// Validate that the remote start request had been sent to the OCPI module.
        /// Validate via HTTP that the remote start request is present within the remote OCPI module.
        /// </summary>
        [Test]
        public async Task RemoteStartStopCDR_Test1()
        {

            if (csoRoamingNetwork      is not null &&
                emp1RoamingNetwork     is not null &&
                emp2RoamingNetwork     is not null &&

                cpoCPOAPI              is not null &&
                emsp1EMSPAPI           is not null &&
                emsp2EMSPAPI           is not null &&

                graphDefinedCSO        is not null &&
                graphDefinedEMP1Local  is not null &&
                graphDefinedEMP2Local  is not null &&

                ahzfPhone              is not null)
            {

                #region Add DE*GEF*POOL1

                var addChargingPoolResult1 = await graphDefinedCSO.AddChargingPool(

                                                 Id:                   ChargingPool_Id.Parse("DE*GEF*POOL1"),
                                                 Name:                 I18NString.Create(Languages.en, "Test pool #1"),
                                                 Description:          I18NString.Create(Languages.en, "GraphDefined charging pool for tests #1"),

                                                 Address:              new Address(

                                                                           Street:             "Biberweg",
                                                                           PostalCode:         "07749",
                                                                           City:               I18NString.Create(Languages.da, "Jena"),
                                                                           Country:            Country.Germany,

                                                                           HouseNumber:        "18",
                                                                           FloorLevel:         null,
                                                                           Region:             null,
                                                                           PostalCodeSub:      null,
                                                                           TimeZone:           null,
                                                                           OfficialLanguages:  null,
                                                                           Comment:            null,

                                                                           CustomData:         null,
                                                                           InternalData:       null

                                                                       ),
                                                 GeoLocation:          GeoCoordinate.Parse(50.93, 11.63),

                                                 InitialAdminStatus:   ChargingPoolAdminStatusTypes.Operational,
                                                 InitialStatus:        ChargingPoolStatusTypes.Available,

                                                 Configurator:         chargingPool => {
                                                                       }

                                             );

                Assert.IsNotNull(addChargingPoolResult1);

                var chargingPool1  = addChargingPoolResult1.ChargingPool;
                Assert.IsNotNull(chargingPool1);

                #endregion

                // OCPI does not have stations!

                #region Add DE*GEF*STATION*1*A

                var addChargingStationResult1 = await chargingPool1!.AddChargingStation(

                                                    Id:                   ChargingStation_Id.Parse("DE*GEF*STATION*1*A"),
                                                    Name:                 I18NString.Create(Languages.en, "Test station #1A"),
                                                    Description:          I18NString.Create(Languages.en, "GraphDefined charging station for tests #1A"),

                                                    GeoLocation:          GeoCoordinate.Parse(50.82, 11.52),

                                                    InitialAdminStatus:   ChargingStationAdminStatusTypes.Operational,
                                                    InitialStatus:        ChargingStationStatusTypes.Available,

                                                    Configurator:         chargingStation => {
                                                                          }

                                                );

                Assert.IsNotNull(addChargingStationResult1);

                var chargingStation1  = addChargingStationResult1.ChargingStation;
                Assert.IsNotNull(chargingStation1);

                #endregion

                #region Add EVSE DE*GEF*EVSE*1*A*1

                var addEVSE1Result1 = await chargingStation1!.AddVirtualEVSE(

                                          Id:                   WWCP.EVSE_Id.Parse("DE*GEF*EVSE*1*A*1"),
                                          Name:                 I18NString.Create(Languages.en, "Test EVSE #1A1"),
                                          Description:          I18NString.Create(Languages.en, "GraphDefined EVSE for tests #1A1"),

                                          InitialAdminStatus:   EVSEAdminStatusTypes.Operational,
                                          InitialStatus:        EVSEStatusTypes.Available,

                                          ChargingConnectors:   new[] {
                                                                    new ChargingConnector(
                                                                        Plug:            ChargingPlugTypes.Type2Connector_CableAttached,
                                                                        Lockable:        true,
                                                                        CableAttached:   true,
                                                                        CableLength:     Meter.Parse(4)
                                                                    )
                                                                }

                                      );

                Assert.IsNotNull(addEVSE1Result1);

                var evse1     = addEVSE1Result1.EVSE;
                Assert.IsNotNull(evse1);

                #endregion


                // Copy locations to the EMSPs
                foreach (var location in cpoCPOAPI.CommonAPI.GetLocations())
                {
                    await emsp1EMSPAPI.CommonAPI.AddLocation(location.Clone());
                    await emsp2EMSPAPI.CommonAPI.AddLocation(location.Clone());
                }


                if (evse1                 is not null &&
                    evse1.ChargingStation is not null &&
                    evse1.ChargingPool    is not null &&
                    evse1.Operator        is not null)
                {

                    var authenticationStart  = RemoteAuthentication.FromRemoteIdentification(EMobilityAccount_Id.Parse("DE-GDF-C12345678-X"));
                    var smartPhoneSessionId  = ChargingSession_Id.NewRandom;
                    var chargingProduct      = ChargingProduct.FromId(ChargingProduct_Id.Parse("AC1"));

                    var remoteStartResult    = await ahzfPhone.RemoteStart(new WWCP.MobilityProvider.RemoteStartRequest(
                                                                   ChargingLocation:       ChargingLocation.FromEVSEId(evse1.Id),
                                                                   ChargingProduct:        chargingProduct,
                                                                   ReservationId:          ChargingReservation_Id.Random(ChargingStationOperator_Id.Parse("DE*GEF")),
                                                                   ChargingSessionId:      smartPhoneSessionId,
                                                                   RemoteAuthentication:   authenticationStart)
                                                               );

                                               //await graphDefinedEMP1Local.RemoteStart(
                                               //          ChargingLocation:       ChargingLocation.FromEVSEId(evse1.Id),
                                               //          ChargingProduct:        chargingProduct,
                                               //          ReservationId:          ChargingReservation_Id.Random(ChargingStationOperator_Id.Parse("DE*GEF")),
                                               //          SessionId:              providerSessionId,
                                               //          RemoteAuthentication:   authenticationStart
                                               //      );

                    Assert.AreEqual(RemoteStartResultTypes.AsyncOperation, remoteStartResult.Result);


                    var ss1 = emp1RoamingNetwork.SessionsStore.ContainsKey(smartPhoneSessionId);
                    var ss2 = emp1RoamingNetwork.ChargingStationOperators.First().Contains(smartPhoneSessionId);




                    if (remoteStartResult                 is not null &&
                        remoteStartResult.ChargingSession is not null)
                    {

                        Timestamp.TravelForwardInTime(TimeSpan.FromMinutes(5));

                        var providerIdStop      = EMobilityProvider_Id.Parse("DE-GDF");
                        var authenticationStop  = RemoteAuthentication.FromRemoteIdentification(EMobilityAccount_Id.Parse("DE-GDF-C56781234-X"));

                        var remoteStopResult    = await emp1RoamingNetwork.RemoteStop(
                                                            SessionId:              remoteStartResult.ChargingSession.Id,
                                                            ReservationHandling:    ReservationHandling.Close,
                                                            ProviderId:             providerIdStop,
                                                            RemoteAuthentication:   authenticationStop
                                                        );


                        Timestamp.TravelForwardInTime(TimeSpan.FromSeconds(10));

                        var startTS          = Timestamp.Now - TimeSpan.FromMinutes(5);
                        var stopTS           = Timestamp.Now - TimeSpan.FromMinutes(1);

                        var sendCDRResult    = await csoRoamingNetwork.SendChargeDetailRecord(
                                                         new ChargeDetailRecord(
                                                             Id:                           ChargeDetailRecord_Id.NewRandom,
                                                             SessionId:                    remoteStartResult.ChargingSession.Id,
                                                             SessionTime:                  new StartEndDateTime(
                                                                                               Timestamp.Now - TimeSpan.FromMinutes(5),
                                                                                               Timestamp.Now - TimeSpan.FromMinutes(1)
                                                                                           ),
                                                             Duration:                     TimeSpan.FromMinutes(4),

                                                             ChargingConnector:           evse1.ChargingConnectors.First(),
                                                             //EVSE:                        evse1,                      // automagic!
                                                             //EVSEId:                      evse1.Id,                   // automagic!
                                                             //ChargingStation:             evse1.ChargingStation,      // automagic!
                                                             //ChargingStationId:           evse1.ChargingStation.Id,   // automagic!
                                                             //ChargingPool:                evse1.ChargingPool,         // automagic!
                                                             //ChargingPoolId:              evse1.ChargingPool.Id,      // automagic!
                                                             //ChargingStationOperator:     evse1.Operator,             // automagic!
                                                             //ChargingStationOperatorId:   evse1.Operator.Id,          // automagic!

                                                             ChargingProduct:             chargingProduct,
                                                             ChargingPrice:               new Price(
                                                                                              1.23M,
                                                                                              VAT:       0.34M,
                                                                                              Currency:  org.GraphDefined.Vanaheimr.Illias.Currency.EUR
                                                                                          ),

                                                             AuthenticationStart:         authenticationStart,
                                                             AuthenticationStop:          authenticationStop,
                                                             AuthMethodStart:             AuthMethod.AUTH_REQUEST,
                                                             AuthMethodStop:              AuthMethod.AUTH_REQUEST,
                                                             ProviderIdStart:             graphDefinedEMP1Local.Id,
                                                             ProviderIdStop:              providerIdStop,

                                                             EMPRoamingProvider:          null,
                                                             EMPRoamingProviderId:        null,

                                                             Reservation:                 null,
                                                             ReservationId:               null,
                                                             ReservationTime:             null,
                                                             ReservationFee:              null,

                                                             ParkingSpaceId:              null,
                                                             ParkingTime:                 new StartEndDateTime(
                                                                                              startTS,
                                                                                              stopTS
                                                                                          ),
                                                             ParkingFee:                  null,

                                                             EnergyMeterId:               evse1.EnergyMeter?.Id,
                                                             EnergyMeter:                 evse1.EnergyMeter,
                                                             EnergyMeteringValues:        new[] {
                                                                                              new EnergyMeteringValue(
                                                                                                  startTS,
                                                                                                  1334.034M,
                                                                                                  "1334.034",
                                                                                                  "..."
                                                                                              ),
                                                                                              new EnergyMeteringValue(
                                                                                                  stopTS,
                                                                                                  1451.241M,
                                                                                                  "1451.241",
                                                                                                  "..."
                                                                                              )
                                                                                          },
                                                             ConsumedEnergy:              null,
                                                             ConsumedEnergyFee:           null,

                                                             CustomData:                  null,
                                                             InternalData:                null,

                                                             PublicKey:                   null,
                                                             Signatures:                  null));

                        Assert.AreEqual(SendCDRsResultTypes.Success,  sendCDRResult.Result);


                    }
                    else
                        Assert.Fail("The remote start failed!");

                }
                else
                    Assert.Fail("Faile to setup EVSE #1!");

            }
            else
                Assert.Fail("Failed precondition(s)!");

        }

        #endregion


    }

}
