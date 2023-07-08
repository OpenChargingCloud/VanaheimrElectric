
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using org.GraphDefined.Vanaheimr.Illias;

var ecParameters  = SecNamedCurves.GetByName("secp256r1");
var generator     = GeneratorUtilities.GetKeyPairGenerator("ECDH");
generator.Init(new ECKeyGenerationParameters(new ECDomainParameters(ecParameters.Curve,
                                                                    ecParameters.G,
                                                                    ecParameters.N,
                                                                    ecParameters.H,
                                                                    ecParameters.GetSeed()),
                                             new SecureRandom()));

var keyPair     = generator.GenerateKeyPair();
var privateKey  = (keyPair.Private as ECPrivateKeyParameters).D.ToByteArray().ToBase64();
var publicKey   = (keyPair.Public  as ECPublicKeyParameters). Q.GetEncoded(). ToBase64();



var xx = 23;

