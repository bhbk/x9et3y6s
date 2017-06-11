using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens;
using System.Linq;
using Thinktecture.IdentityModel.Tokens;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Provider
{
    public class CustomSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private string _issuer;
        private IUnitOfWork _uow;

        public CustomSecureDataFormat(string issuer, IUnitOfWork uow)
        {
            _issuer = issuer;
            _uow = uow;
        }

        public string Protect(AuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException();

            Guid clientID, audienceID;
            DateTimeOffset? issue, expire;
            AppClient client;
            AppAudience audience;

            string clientValue = ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            string audienceValue = ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (string.IsNullOrEmpty(clientValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            if (string.IsNullOrEmpty(audienceValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientRepository.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();
            else
                client = _uow.ClientRepository.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

            if (client == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceRepository.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();
            else
                audience = _uow.AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

            if (audience == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            string symmetricKey = audience.AudienceKey;
            var keyBytes = TextEncodings.Base64Url.Decode(symmetricKey);
            var signingKey = new HmacSigningCredentials(keyBytes);

            if (_uow.CustomConfigManager.Config.UnitTestAccessToken)
            {
                issue = _uow.CustomConfigManager.Config.UnitTestAccessTokenFakeUtcNow;
                expire = _uow.CustomConfigManager.Config.UnitTestAccessTokenFakeUtcNow.AddMinutes(_uow.CustomConfigManager.Config.DefaultAccessTokenLife);
            }
            else if (_uow.CustomConfigManager.Config.UnitTestRefreshToken)
            {
                issue = _uow.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow;
                expire = _uow.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow.AddMinutes(_uow.CustomConfigManager.Config.DefaultRefreshTokenLife);
            }
            else
            {
                issue = ticket.Properties.IssuedUtc;
                expire = ticket.Properties.ExpiresUtc;
            }

            var token = new JwtSecurityToken(_issuer, audience.Id.ToString().ToLower(), ticket.Identity.Claims, issue.Value.UtcDateTime, expire.Value.UtcDateTime, signingKey);
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException();

            Guid clientID, audienceID;
            AppClient client;
            AppAudience audience;
            SecurityToken token;

            var jwt = JObject.Parse(text);
            var clientValue = (string)jwt[BaseLib.Statics.AttrClientID];
            var audienceValue = (string)jwt[BaseLib.Statics.AttrAudienceID];

            if (string.IsNullOrEmpty(clientValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            if (string.IsNullOrEmpty(audienceValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientRepository.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();
            else
                client = _uow.ClientRepository.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

            if (client == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceRepository.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();
            else
                audience = _uow.AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

            if (audience == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            var keyByteArray = TextEncodings.Base64Url.Decode(audience.AudienceKey);
            var signingKey = new HmacSigningCredentials(keyByteArray);

            var details = new TokenValidationParameters
            {
                IssuerSigningKey = signingKey.SigningKey,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidAudience = audience.Id.ToString(),
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(text, details, out token);
            var identity = principal.Identities;

            return new AuthenticationTicket(identity.First(), new AuthenticationProperties());
        }
    }
}