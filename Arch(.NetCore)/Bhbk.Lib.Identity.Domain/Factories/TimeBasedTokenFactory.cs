using OtpNet;
using System.Text;

//https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class TimeBasedTokenFactory
    {
        private readonly int _length;
        private readonly int _expire;

        public TimeBasedTokenFactory(int length, int expire)
        {
            _length = length;
            _expire = expire;
        }

        public string Generate(string purpose, string entity)
        {
            byte[] secret = Encoding.Unicode.GetBytes(entity + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            return code.ComputeTotp();
        }

        public bool Validate(string purpose, string token, string entity)
        {
            byte[] secret = Encoding.Unicode.GetBytes(entity + purpose);
            Totp code = new Totp(secret, step: _expire, mode: OtpHashMode.Sha512, totpSize: _length);

            long timeStep;
            bool result = code.VerifyTotp(token, out timeStep);

            return result;
        }
    }
}
