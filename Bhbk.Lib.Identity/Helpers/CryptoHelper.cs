using Microsoft.AspNetCore.Authentication;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Serilog;
using System;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bhbk.Lib.Identity.Helpers
{
    public enum RsaKeyLength
    {
        Bits1024 = 1024,
        Bits2048 = 2048,
        Bits4096 = 4096
    }

    public class CryptoHelper
    {
        //https://svrooij.nl/2018/04/generate-x509certificate2-in-csharp/
        public static X509Certificate2 CreateX509Certificate(RsaKeyLength length)
        {
            var issuerName = Assembly.GetEntryAssembly().GetName().Name;
            var subjectName = Assembly.GetEntryAssembly().GetName().Name;
            var issuerAttrs = new Hashtable();
            var subjectAttrs = new Hashtable();

            var randomGenerator = new CryptoApiRandomGenerator();
            var randomNumber = new SecureRandom(randomGenerator);

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), randomNumber);

            issuerAttrs.Add(X509Name.CN, issuerName);
            subjectAttrs.Add(X509Name.OU, subjectName);

            var issuerDetails = new X509Name(new ArrayList(issuerAttrs.Keys), issuerAttrs);
            var subjectDetails = new X509Name(new ArrayList(subjectAttrs.Keys), subjectAttrs);
            var x509generator = new X509V3CertificateGenerator();

            x509generator.SetSerialNumber(serialNumber);
            x509generator.SetIssuerDN(issuerDetails);
            x509generator.SetSubjectDN(subjectDetails);
            x509generator.SetNotBefore(DateTime.UtcNow.Date);
            x509generator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));

            var keyPairGenerator = new RsaKeyPairGenerator();
            var keyPairParams = new KeyGenerationParameters(randomNumber, (int)length);

            keyPairGenerator.Init(keyPairParams);

            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            x509generator.SetPublicKey(subjectKeyPair.Public);

            var issuerKeyPair = subjectKeyPair;
            var signatureGenerator = new Asn1SignatureFactory("SHA256WithRSA", issuerKeyPair.Private);
            var x509certificate = x509generator.Generate(signatureGenerator);

            var pkcsStream = new System.IO.MemoryStream();
            var pkcsStore = new Pkcs12StoreBuilder().Build();
            var exportPass = Guid.NewGuid().ToString("x");

            pkcsStore.SetKeyEntry($"{subjectName}_key", 
                new AsymmetricKeyEntry(subjectKeyPair.Private), 
                new[] { new X509CertificateEntry(x509certificate) });
            pkcsStore.Save(pkcsStream, exportPass.ToCharArray(), randomNumber);

            var result = new X509Certificate2(pkcsStream.ToArray(), exportPass, X509KeyStorageFlags.Exportable);
            var msg = $"The issuer \"{result.Issuer}\" created x509 certificate with thumbprint \"{result.Thumbprint}\" and subject \"{result.Subject}\"";

            Log.Information(msg);

            return result;
        }

        public static string CreateRandomBase64(int length)
        {
            byte[] byteValue = new byte[length];
            RNGCryptoServiceProvider.Create().GetBytes(byteValue);

            return Base64UrlTextEncoder.Encode(byteValue);
        }

        public static string CreateRandomNumberAsString(int length)
        {
            var randomNumber = new Random();
            var result = string.Empty;

            for (int i = 0; i < length; i++)
                result = String.Concat(result, randomNumber.Next(10).ToString());

            return result;
        }

        public static string CreateSHA256(string input)
        {
            HashAlgorithm algo = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] byteHash = algo.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
