using Microsoft.AspNetCore.Authentication;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bhbk.Lib.Identity.Helpers
{
    internal enum RsaKeyLength
    {
        Length2048Bits = 2048, Length4096Bits = 4096
    }

    public static class CryptoHelper
    {
        public static X509Certificate2 GenerateCertificate()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            var certificateGenerator = new X509V3CertificateGenerator();
            var issuerAttrs = new Hashtable();
            var subjectAttrs = new Hashtable();

            issuerAttrs.Add(X509Name.CN, Assembly.GetCallingAssembly().ToString());
            subjectAttrs.Add(X509Name.OU, Assembly.GetCallingAssembly().ToString());

            var issuerDetails = new X509Name(new ArrayList(issuerAttrs.Keys), issuerAttrs);
            var subjectDetails = new X509Name(new ArrayList(subjectAttrs.Keys), subjectAttrs);

            certificateGenerator.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random));
            certificateGenerator.SetSignatureAlgorithm(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id);
            certificateGenerator.SetIssuerDN(issuerDetails);
            certificateGenerator.SetSubjectDN(subjectDetails);
            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));

            var keyPairGenerator = new RsaKeyPairGenerator();
            var keyPairParams = new KeyGenerationParameters(random, (int)RsaKeyLength.Length2048Bits);

            keyPairGenerator.Init(keyPairParams);

            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            var issuerKeyPair = subjectKeyPair;

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            var certFlags = X509KeyStorageFlags.Exportable;
            var certificate = certificateGenerator.Generate(issuerKeyPair.Private, random);
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            //create certificate that holds public/private key
            var x509 = new X509Certificate2(certificate.GetEncoded());
            var secret = (Asn1Sequence)Asn1Object.FromByteArray(privateKeyInfo.PrivateKey.GetDerEncoded());

            if (secret.Count != 9)
                throw new PemException("malformed sequence in RSA private key");

            var rsaKey = new RsaPrivateKeyStructure(secret);
            var rsaParams = new RsaPrivateCrtKeyParameters(rsaKey.Modulus, rsaKey.PublicExponent, rsaKey.PrivateExponent,
                rsaKey.Prime1, rsaKey.Prime2, rsaKey.Exponent1, rsaKey.Exponent2, rsaKey.Coefficient);

            throw new NotImplementedException();

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaParams);

            return x509;
        }

        public static string GenerateRandomBase64(int length)
        {
            byte[] byteValue = new byte[length];
            RNGCryptoServiceProvider.Create().GetBytes(byteValue);

            return Base64UrlTextEncoder.Encode(byteValue);
        }

        public static string GenerateSHA256(string input)
        {
            HashAlgorithm algo = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] byteHash = algo.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
