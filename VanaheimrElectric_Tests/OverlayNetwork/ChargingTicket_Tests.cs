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

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using org.GraphDefined.Vanaheimr.Illias;

using cloud.charging.open.protocols.OCPPv2_1;
using cloud.charging.open.protocols.OCPPv2_1.CS;
using cloud.charging.open.protocols.OCPPv2_1.CSMS;
using cloud.charging.open.protocols.OCPPv2_1.WebSockets;
using cloud.charging.open.protocols.OCPPv2_1.NetworkingNode;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.NetworkingNode;
using cloud.charging.open.protocols.WWCP.WebSockets;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Mail;

#endregion

namespace cloud.charging.open.vanaheimr.electric.UnitTests.OverlayNetwork
{

    /// <summary>
    /// Charging Ticket
    /// </summary>
    [TestFixture]
    public class ChargingTicket_Tests : AOverlayNetwork
    {

        #region SignupUserTest1()

        /// <summary>
        /// Signup User Test.
        /// </summary>
        [Test]
        public async Task SignupUserTest1()
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


            var userKeys1                   = new[] {
                                                  ECCKeyPair.GenerateKeys(CryptoAlgorithm.Secp256r1)!,
                                                  ECCKeyPair.GenerateKeys(CryptoAlgorithm.Secp384r1)!,
                                                  ECCKeyPair.GenerateKeys(CryptoAlgorithm.Secp521r1)!
                                              };

            var userRegistration            = new RegisterEMobilityAccountData(
                                                  Username:                   "John Doe",
                                                  Password:                   "veryS3CUR3!",
                                                  EMailAddress:               SimpleEMailAddress.Parse("john@doe.net"),
                                                  PublicKeys:                 userKeys1.Select(keyPair => keyPair.ToECCPublicKey()),
                                                  Signatures:                 null,
                                                  AdditionalEMailAddresses:   [ SimpleEMailAddress.Parse("john2@doe.net") ],
                                                  PhoneNumber:                PhoneNumber.Parse("555-12345678-23")
                                              );

            // {
            //   "@id":                "bca59101-a1a0-957d-9bfd-f0ef8fc6ba64",
            //   "@context":           "https://open.charging.cloud/context/wwcp/registerEMobilityAccountData",
            //   "creationTimestamp":  "2024-08-31T00:01:28.226Z",
            //   "username":           "John Doe",
            //   "password":           "veryS3CUR3!",
            //   "eMailAddress":       "john@doe.net",
            //   "publicKeys": [
            //     {
            //       "value":     "BLhfTk9HKHbxGrcqByxgNS+4BSo/oMd+MFs0kcOkFKo5gEcVWfnwBr0j9/PtKiE4cFuD3aHL/A0eMr+lbZgyG3w="
            //     },
            //     {
            //       "value":     "BK9hLBbmmzKzM+a+7B5ZOZ0kSmUJK3+RhOAL24xJBkASQRrutMXnZRWaCU5iLo+6xuulft2ban4eq9gnRCpm6Jd+hlntLLc+/kgkDMNjPST3mRTdNX6KNm/TM+k9N/oIqg==",
            //       "algorithm": "secp384r1"
            //     },
            //     {
            //       "value":     "BACRk3LQ4LjYnBfJ1NEqFSV1Gq2eIkh3xTcXbS+nJWNxG+OLS+dF/TVZSTjPrti+HkzXUQl5H6FQ4UhxyF4GIrfphQGRcH7+AFWLGtKWlTc74eNG5N7R15naQpdo+/2r57waedAvKoYTV5IvKMCEY58Ry6C6+sslpMLiK0P1pX9xNGslHw==",
            //       "algorithm": "secp521r1"
            //     }
            //   ],
            //   "additionalEMailAddresses": [
            //     "john2@doe.net"
            //   ],
            //   "phoneNumber": "555-12345678-23"
            // }
            var userRegistrationJSON        = userRegistration.ToJSON();

            var jsonPublicKeys              = userRegistrationJSON["publicKeys"] as JArray;
            Assert.That(jsonPublicKeys,                                     Is.Not.Null);

            if (jsonPublicKeys is not null)
            {

                Assert.That(jsonPublicKeys.Count,                           Is.EqualTo(3));

                var publicKey1 = jsonPublicKeys[0] as JObject;
                var publicKey2 = jsonPublicKeys[1] as JObject;
                var publicKey3 = jsonPublicKeys[2] as JObject;

                Assert.That(publicKey1, Is.Not.Null);
                Assert.That(publicKey2, Is.Not.Null);
                Assert.That(publicKey3, Is.Not.Null);

                Assert.That(publicKey1?["value"]?.Value<String>(),          Is.Not.Null);
                Assert.That(publicKey2?["value"]?.Value<String>(),          Is.Not.Null);
                Assert.That(publicKey3?["value"]?.Value<String>(),          Is.Not.Null);

                Assert.That(publicKey1?["algorithm"]?.Value<String>(),      Is.Null);
                Assert.That(publicKey2?["algorithm"]?.Value<String>(),      Is.EqualTo(CryptoAlgorithm.Secp384r1.ToString()));
                Assert.That(publicKey3?["algorithm"]?.Value<String>(),      Is.EqualTo(CryptoAlgorithm.Secp521r1.ToString()));

            }


            var signedUserRegistration      = userRegistration.Sign(userKeys1);
            var signedUserRegistrationJSON  = signedUserRegistration.ToJSON();

            Assert.That(signedUserRegistration.Signatures.Count(),          Is.EqualTo(3));


            var jsonSignatures              = signedUserRegistrationJSON["signatures"] as JArray;
            Assert.That(jsonSignatures,                                     Is.Not.Null);

            if (jsonSignatures is not null)
            {

                Assert.That(jsonSignatures.Count,                           Is.EqualTo(3));

                var signature1 = jsonSignatures[0] as JObject;
                var signature2 = jsonSignatures[1] as JObject;
                var signature3 = jsonSignatures[2] as JObject;

                Assert.That(signature1, Is.Not.Null);
                Assert.That(signature2, Is.Not.Null);
                Assert.That(signature3, Is.Not.Null);

                Assert.That(signature1?["keyId"]?.Value<String>(),          Is.Not.Null);
                Assert.That(signature2?["keyId"]?.Value<String>(),          Is.Not.Null);
                Assert.That(signature3?["keyId"]?.Value<String>(),          Is.Not.Null);

                Assert.That(signature1?["value"]?.Value<String>(),          Is.Not.Null);
                Assert.That(signature2?["value"]?.Value<String>(),          Is.Not.Null);
                Assert.That(signature3?["value"]?.Value<String>(),          Is.Not.Null);

                Assert.That(signature1?["algorithm"]?.Value<String>(),      Is.Null);
                Assert.That(signature2?["algorithm"]?.Value<String>(),      Is.EqualTo(CryptoAlgorithm.Secp384r1.ToString()));
                Assert.That(signature3?["algorithm"]?.Value<String>(),      Is.EqualTo(CryptoAlgorithm.Secp521r1.ToString()));

            }

            #region Verify

            var parserResult        = RegisterEMobilityAccountData.TryParse(signedUserRegistrationJSON, out var parsedSignedUserRegistrationJSON, out var errorResponse2);
            var verificationResult  = parsedSignedUserRegistrationJSON.Verify(signedUserRegistrationJSON, RegisterEMobilityAccountData.DefaultJSONLDContext, out var errorResponse3);

            Assert.That(verificationResult, Is.True);

            #endregion




        }

        #endregion

    }

}
