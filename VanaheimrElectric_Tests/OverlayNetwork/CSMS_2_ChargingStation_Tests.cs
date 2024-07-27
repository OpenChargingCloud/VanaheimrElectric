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
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;
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
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
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

                                      DestinationId:       chargingStation1.Id,
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
                ocppGateway         is null ||
                ocppLocalController is null ||
                chargingStation1    is null ||
                chargingStation2    is null ||
                chargingStation3    is null)
            {

                Assert.Multiple(() => {

                    if (csms1               is null)
                        Assert.Fail("The csms 1 must not be null!");

                    if (csms2               is null)
                        Assert.Fail("The csms 2 must not be null!");

                    if (ocppGateway         is null)
                        Assert.Fail("The gateway must not be null!");

                    if (ocppLocalController is null)
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

                                                   DestinationId:       chargingStation1.Id,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                "TestData",
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse2  = await csms1.TransferData(

                                                   DestinationId:       chargingStation1.Id,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                JSONObject.Create(new JProperty("test", "data")),
                                                   CustomData:          null,

                                                   SignKeys:            null,
                                                   SignInfos:           null,
                                                   Signatures:          null,

                                                   RequestId:           null,
                                                   RequestTimestamp:    null,
                                                   RequestTimeout:      null,
                                                   EventTrackingId:     null

                                               );

            var dataTransferResponse3  = await csms1.TransferData(

                                                   DestinationId:       chargingStation1.Id,
                                                   VendorId:            Vendor_Id. GraphDefined,
                                                   MessageId:           Message_Id.GraphDefined_TestMessage,
                                                   Data:                new JArray("test", "data"),
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



    }

}
