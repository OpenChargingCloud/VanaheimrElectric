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
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Overlay Network Tests
    /// CSMS  --[GW]--[LC]-->  Charging Station
    /// </summary>
    [TestFixture]
    public class CSMS_2_ChargingStation_Tests : AOverlayNetwork
    {

        #region Common

        #region SendDataTransfer()

        /// <summary>
        /// Send BootNotification test.
        /// </summary>
        [Test]
        public async Task SendDataTransfer()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var dataTransferResponse1  = await csms1.TransferData(

                                                   Destination:         SourceRouting.To(chargingStation1.Id),
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

            var dataTransferResponse2  = await csms1.TransferData(

                                                   Destination:         SourceRouting.To(chargingStation1.Id),
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

            var dataTransferResponse3  = await csms1.TransferData(

                                                   Destination:         SourceRouting.To(chargingStation1.Id),
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

                Assert.That(dataTransferResponse1.Status,                                            Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse1.Data?.Type,                                        Is.EqualTo(JTokenType.String));
                Assert.That(dataTransferResponse1.Data?.ToString(),                                  Is.EqualTo("ataDtseT"));
                //StatusInfo

                Assert.That(dataTransferResponse2.Status,                                            Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse2.Data?.Type,                                        Is.EqualTo(JTokenType.Object));
                Assert.That(dataTransferResponse2.Data?.ToString(Newtonsoft.Json.Formatting.None),   Is.EqualTo("{\"test\":\"atad\"}"));
                //StatusInfo

                Assert.That(dataTransferResponse3.Status,                                            Is.EqualTo(DataTransferStatus.Accepted));
                Assert.That(dataTransferResponse3.Data?.Type,                                        Is.EqualTo(JTokenType.Array));
                Assert.That(dataTransferResponse3.Data?.ToString(Newtonsoft.Json.Formatting.None),   Is.EqualTo("[\"tset\",\"atad\"]"));
                //StatusInfo

            });

        }

        #endregion

        #endregion

        #region Charging

        #region Tariffs

        #region ChangeTransactionTariff1()

        /// <summary>
        /// Change the tariff of an ongoing transaction.
        /// </summary>
        [Test]
        public async Task ChangeTransactionTariff1()
        {

            #region Initial checks

            if (csms1                is null ||
                csms2                is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var response1 = await csms1.ChangeTransactionTariff(

                                      Destination:        SourceRouting.To(chargingStation1.Id),
                                      TransactionId:      Transaction_Id.Parse("1111-3333-5555-6666-7777"),
                                      Tariff:             new Tariff(

                                                              Id:               Tariff_Id.Parse("DE-GDF-T12345678"),
                                                              Currency:         Currency.EUR,
                                                              Energy:           new TariffEnergy(
                                                                                    [ new TariffEnergyPrice(0.51M, StepSize: WattHour.TryParseKWh(1)) ],
                                                                                    [ TaxRate.VAT(15)]
                                                                                ),
                                                              Description:      new MessageContents(
                                                                                    "0.53 / kWh",
                                                                                    Language_Id.EN
                                                                                ),
                                                              MinPrice:         null,
                                                              MaxPrice:         new Price(
                                                                                    ExcludingTaxes:  0.51M,
                                                                                    IncludingTaxes:  0.53M
                                                                                ),

                                                              SignKeys:         null,
                                                              SignInfos:        null,
                                                              Signatures:       null,

                                                              CustomData:       null

                                                          ),
                                      CustomData:         null,

                                      SignKeys:           null,
                                      SignInfos:          null,
                                      Signatures:         null,

                                      RequestId:          null,
                                      RequestTimestamp:   null,
                                      RequestTimeout:     null,
                                      EventTrackingId:    null

                                  );


            Assert.That(response1.Status, Is.EqualTo(TariffStatus.Accepted));


        }

        #endregion

        #region ClearTariffs1()

        /// <summary>
        /// Clear tariffs.
        /// </summary>
        [Test]
        public async Task ClearTariffs1()
        {

            #region Initial checks

            if (csms1                is null ||
                csms2                is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var response1 = await csms1.ClearTariffs(

                                      Destination:        SourceRouting.To(chargingStation1.Id),
                                      TariffIds:          [ Tariff_Id.New() ],
                                      TariffKind:         TariffKinds.DefaultTariff,
                                      CustomData:         null,

                                      SignKeys:           null,
                                      SignInfos:          null,
                                      Signatures:         null,

                                      RequestId:          null,
                                      RequestTimestamp:   null,
                                      RequestTimeout:     null,
                                      EventTrackingId:    null

                                  );


            Assert.That(response1.ClearTariffsResults.First().Status, Is.EqualTo(TariffStatus.Accepted));


        }

        #endregion

        #region GetTariffs1()

        /// <summary>
        /// Get tariffs.
        /// </summary>
        [Test]
        public async Task GetTariffs1()
        {

            #region Initial checks

            if (csms1                is null ||
                csms2                is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var response1 = await csms1.GetTariffs(

                                      Destination:        SourceRouting.To(chargingStation1.Id),
                                      EVSEId:             EVSE_Id.Parse(1),
                                      CustomData:         null,

                                      SignKeys:           null,
                                      SignInfos:          null,
                                      Signatures:         null,

                                      RequestId:          null,
                                      RequestTimestamp:   null,
                                      RequestTimeout:     null,
                                      EventTrackingId:    null

                                  );


            Assert.That(response1.Status,                              Is.EqualTo(TariffStatus.Accepted));
            //Assert.That(response1.TariffAssignments.First().TariffId,  Is.EqualTo(TariffStatus.Accepted));


        }

        #endregion

        #region SetDefaultTariff1()

        /// <summary>
        /// Set a default tariff.
        /// </summary>
        [Test]
        public async Task SetDefaultTariff1()
        {

            #region Initial checks

            if (csms1                is null ||
                csms2                is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var response1 = await csms1.SetDefaultTariff(

                                      Destination:        SourceRouting.To(chargingStation1.Id),
                                      EVSEId:             EVSE_Id.Parse(1),
                                      Tariff:             new Tariff(

                                                              Id:               Tariff_Id.Parse("DE-GDF-T12345678"),
                                                              Currency:         Currency.EUR,
                                                              Energy:           new TariffEnergy(
                                                                                    [ new TariffEnergyPrice(0.51M, StepSize: WattHour.TryParseKWh(1)) ],
                                                                                    [ TaxRate.VAT(15)]
                                                                                ),
                                                              Description:      new MessageContents(
                                                                                    "0.53 / kWh",
                                                                                    Language_Id.EN
                                                                                ),
                                                              MinPrice:         null,
                                                              MaxPrice:         new Price(
                                                                                    ExcludingTaxes:  0.51M,
                                                                                    IncludingTaxes:  0.53M
                                                                                ),

                                                              SignKeys:         null,
                                                              SignInfos:        null,
                                                              Signatures:       null,

                                                              CustomData:       null

                                                          ),
                                      CustomData:         null,

                                      SignKeys:           null,
                                      SignInfos:          null,
                                      Signatures:         null,

                                      RequestId:          null,
                                      RequestTimestamp:   null,
                                      RequestTimeout:     null,
                                      EventTrackingId:    null

                                  );


            Assert.That(response1.Status, Is.EqualTo(TariffStatus.Accepted));


        }

        #endregion

        #endregion


        // QRCodeScanned
        // RequestStartTransaction
        // GetTransactionStatus
        // GetCompositeSchedule
        // RequestStopTransaction


        #endregion

        #region DeviceModel

        #region Variables

        #region SetAndVerifyQRCodePaymentParameters()

        /// <summary>
        /// Set QR-Code payment parameters.
        /// </summary>
        [Test]
        public async Task SetAndVerifyQRCodePaymentParameters()
        {

            #region Initial checks

            if (csms1                is null ||
                csms2                is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1     is null ||
                chargingStation2     is null ||
                chargingStation3     is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion

            var qrCodeURLTemplate  = "https://example.org/qr/{TOPT}";
            var sharedSecret       = RandomExtensions.RandomString(16);

            #region SetVariables...

            var setVariablesResponse = await csms1.SetVariables(

                                                 Destination:        SourceRouting.To(chargingStation1.Id),
                                                 VariableData:       [
                                                                         new SetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.Enabled)
                                                                             ),
                                                                             "true"
                                                                         ),
                                                                         new SetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.URLTemplate)
                                                                             ),
                                                                             qrCodeURLTemplate
                                                                         ),
                                                                         new SetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.SharedSecret)
                                                                             ),
                                                                             sharedSecret
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


            Assert.That(setVariablesResponse.SetVariableResults.ElementAt(0).AttributeStatus,   Is.EqualTo(SetVariableStatus.Accepted));
            Assert.That(setVariablesResponse.SetVariableResults.ElementAt(1).AttributeStatus,   Is.EqualTo(SetVariableStatus.Accepted));
            Assert.That(setVariablesResponse.SetVariableResults.ElementAt(2).AttributeStatus,   Is.EqualTo(SetVariableStatus.Accepted));

            #endregion

            #region GetVariables...

            var getVariablesResponse = await csms1.GetVariables(

                                                 Destination:        SourceRouting.To(chargingStation1.Id),
                                                 VariableData:       [
                                                                         new GetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.Enabled)
                                                                             )
                                                                         ),
                                                                         new GetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.URLTemplate)
                                                                             )
                                                                         ),
                                                                         new GetVariableData(
                                                                             new Component(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr),
                                                                                 EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                             ),
                                                                             new Variable(
                                                                                 Name:  nameof(QRCodePaymentsCtrlr.SharedSecret)
                                                                             )
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


            Assert.That(getVariablesResponse.Results.ElementAt(0).AttributeStatus,   Is.EqualTo(GetVariableStatus.Accepted));
            Assert.That(getVariablesResponse.Results.ElementAt(1).AttributeStatus,   Is.EqualTo(GetVariableStatus.Accepted));
            Assert.That(getVariablesResponse.Results.ElementAt(2).AttributeStatus,   Is.EqualTo(GetVariableStatus.Accepted));


            Assert.That(getVariablesResponse.Results.ElementAt(0).AttributeValue,    Is.EqualTo("true"));
            Assert.That(getVariablesResponse.Results.ElementAt(1).AttributeValue,    Is.EqualTo(qrCodeURLTemplate));
            Assert.That(getVariablesResponse.Results.ElementAt(2).AttributeValue,    Is.EqualTo(sharedSecret));

            #endregion

            var evse               = chargingStation1.EVSEs.First()!;

            do
            {
                await Task.Delay(50);
            } while (evse.QRCodePaymentsURL is null);

            Assert.That(chargingStation1.EVSEs.First().QRCodePaymentsURL?[..23],     Is.EqualTo(qrCodeURLTemplate[..23]));

            #region GetReport...

            var reports = new ConcurrentList<ReportData>();

            csms1.OCPP.IN.OnNotifyReportRequestReceived += (timestamp, sender, connection, notifyReportRequest, ct) => {
                foreach (var report in notifyReportRequest.ReportData)
                    reports.TryAdd(report);
                return Task.CompletedTask;
            };

            var getReportResponse = await csms1.GetReport(

                                              Destination:          SourceRouting.To(chargingStation1.Id),
                                              GetReportRequestId:   1,
                                              ComponentCriteria:    [ ComponentCriteria.Enabled ],
                                              ComponentVariables:   [
                                                                        new ComponentVariable(
                                                                            new Component(
                                                                                Name:  nameof(QRCodePaymentsCtrlr),
                                                                                EVSE:  new EVSE(EVSE_Id.Parse(1))
                                                                            ),
                                                                            new Variable(
                                                                                Name:  nameof(QRCodePaymentsCtrlr.URLTemplate)
                                                                            )
                                                                        )
                                                                    ],
                                              CustomData:           null,

                                              SignKeys:             null,
                                              SignInfos:            null,
                                              Signatures:           null,

                                              RequestId:            null,
                                              RequestTimestamp:     null,
                                              RequestTimeout:       null,
                                              EventTrackingId:      null

                                          );

            //ToDo: Implement the charging station side!
            //Assert.That(getReportResponse.Status,Is.EqualTo(GenericDeviceModelStatus.Accepted));

            #endregion

        }

        #endregion

        #endregion

        #endregion

        #region Firmware

        #region SendReset1()

        /// <summary>
        /// Send Reset test.
        /// </summary>
        [Test]
        public async Task SendReset1()
        {

            #region Initial checks

            if (csms1               is null ||
                csms2               is null ||
                ocppGateway1         is null ||
                ocppLocalController1 is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
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

                    if (chargingStation1    is null)
                        Assert.Fail("The charging station 1 must not be null!");

                    if (chargingStation2    is null)
                        Assert.Fail("The charging station 2 must not be null!");

                    if (chargingStation3    is null)
                        Assert.Fail("The charging station 3 must not be null!");

                });

                return;

            }

            #endregion


            var response1 = await csms1.Reset(

                                      Destination:         SourceRouting.To(chargingStation1.Id),
                                      ResetType:           ResetType.OnIdle,
                                      CustomData:          null,

                                      SignKeys:            null,
                                      SignInfos:           null,
                                      Signatures:          null,

                                      RequestId:           null,
                                      RequestTimestamp:    null,
                                      RequestTimeout:      null,
                                      EventTrackingId:     null

                                  );


            Assert.That(response1.Status, Is.EqualTo(ResetStatus.Accepted));


        }

        #endregion

        #endregion


    }

}
