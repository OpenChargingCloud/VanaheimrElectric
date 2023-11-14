
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

using org.GraphDefined.Vanaheimr.Illias;


var secp256r1            = SecNamedCurves.GetByName("secp256r1");
var secp256r1Generator   = GeneratorUtilities.GetKeyPairGenerator("ECDH");
secp256r1Generator.Init(new ECKeyGenerationParameters(
                            new ECDomainParameters(
                                secp256r1.Curve,
                                secp256r1.G,
                                secp256r1.N,
                                secp256r1.H,
                                secp256r1.GetSeed()
                            ),
                            new SecureRandom()
                        ));

var secp256r1KeyPair     = secp256r1Generator.GenerateKeyPair();
var secp256r1PrivateKey  = (secp256r1KeyPair.Private as ECPrivateKeyParameters).D.ToByteArray().ToBase64();
var secp256r1PublicKey   = (secp256r1KeyPair.Public  as ECPublicKeyParameters). Q.GetEncoded(). ToBase64();


var secp521r1            = SecNamedCurves.GetByName("secp521r1");
var secp521r1Generator   = GeneratorUtilities.GetKeyPairGenerator("ECDH");
secp521r1Generator.Init(new ECKeyGenerationParameters(
                            new ECDomainParameters(
                                secp521r1.Curve,
                                secp521r1.G,
                                secp521r1.N,
                                secp521r1.H,
                                secp521r1.GetSeed()
                            ),
                            new SecureRandom()
                        ));

var secp521r1KeyPair     = secp521r1Generator.GenerateKeyPair();
var secp521r1PrivateKey  = (secp521r1KeyPair.Private as ECPrivateKeyParameters).D.ToByteArray().ToBase64();
var secp521r1PublicKey   = (secp521r1KeyPair.Public  as ECPublicKeyParameters). Q.GetEncoded(). ToBase64();


var xx = 23;

