using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
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

            string clientValue = ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            if (string.IsNullOrEmpty(clientValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            string audienceValue = ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (string.IsNullOrEmpty(audienceValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            Guid clientID, audienceID;
            DateTimeOffset? issue, expire;
            ClientModel client;
            AudienceModel audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = _uow.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceMgmt.FindByIdAsync(audienceID).Result;
            else
                audience = _uow.AudienceMgmt.FindByNameAsync(audienceValue).Result;

            if (audience == null || !audience.Enabled)
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            string symmetricKey = audience.AudienceKey;
            var keyBytes = TextEncodings.Base64Url.Decode(symmetricKey);
            var signingKey = new HmacSigningCredentials(keyBytes);

            if (_uow.ContextStatus == ContextType.UnitTest
                && _uow.ConfigMgmt.Tweaks.UnitTestAccessToken)
            {
                issue = _uow.ConfigMgmt.Tweaks.UnitTestAccessTokenFakeUtcNow;
                expire = _uow.ConfigMgmt.Tweaks.UnitTestAccessTokenFakeUtcNow.AddMinutes(_uow.ConfigMgmt.Tweaks.DefaultAccessTokenLife);
            }
            else if (_uow.ContextStatus == ContextType.UnitTest
                && _uow.ConfigMgmt.Tweaks.UnitTestRefreshToken)
            {
                issue = _uow.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow;
                expire = _uow.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow.AddMinutes(_uow.ConfigMgmt.Tweaks.DefaultRefreshTokenLife);
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

            var jwt = JObject.Parse(text);
            var clientValue = (string)jwt[BaseLib.Statics.AttrClientID];
            var audienceValue = (string)jwt[BaseLib.Statics.AttrAudienceID];

            if (string.IsNullOrEmpty(clientValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            if (string.IsNullOrEmpty(audienceValue))
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            Guid clientID, audienceID;
            ClientModel client;
            AudienceModel audience;
            SecurityToken token;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = _uow.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
                throw new ArgumentNullException(BaseLib.Statics.MsgClientInvalid);

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceMgmt.FindByIdAsync(audienceID).Result;
            else
                audience = _uow.AudienceMgmt.FindByNameAsync(audienceValue).Result;

            if (audience == null || !audience.Enabled)
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