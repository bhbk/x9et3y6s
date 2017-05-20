using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.IdentityModel.Tokens;
using System.Linq;
using Thinktecture.IdentityModel.Tokens;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Provider
{
    public class CustomSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private IUnitOfWork _uow;
        private string _issuer;

        public CustomSecureDataFormat(string issuer, IUnitOfWork uow)
        {
            _issuer = issuer;
            _uow = uow;
        }

        public string Protect(AuthenticationTicket ticket)
        {
            Guid audienceID;
            AppAudience audience;

            if (ticket == null)
                throw new ArgumentNullException();

            //this value could be a uniqueidentifier or a human readable value...
            string audienceValue = ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID) 
                ? ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (audienceValue == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            else if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceRepository.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();

            else
                audience = _uow.AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

            if (audience == null)
                throw new ArgumentNullException(BaseLib.Statics.MsgAudienceInvalid);

            else
            {
                string symmetricKey = audience.AudienceKey;
                var keyBytes = TextEncodings.Base64Url.Decode(symmetricKey);
                var signingKey = new HmacSigningCredentials(keyBytes);
                var issue = ticket.Properties.IssuedUtc;
                var expire = ticket.Properties.ExpiresUtc;
                var token = new JwtSecurityToken(_issuer, audience.Id.ToString().ToLower(), ticket.Identity.Claims, issue.Value.UtcDateTime, expire.Value.UtcDateTime, signingKey);
                var handler = new JwtSecurityTokenHandler();

                return handler.WriteToken(token);
            }
        }

        public AuthenticationTicket Unprotect(string text)
        {
            throw new NotImplementedException();
        }
    }
}