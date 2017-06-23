using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Provider
{
    public class CustomRefreshToken : IAuthenticationTokenProvider
    {
        private IUnitOfWork _uow;

        public CustomRefreshToken(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            Guid clientID, audienceID, userID;
            DateTime issue, expire;
            ClientModel.Model client;
            AudienceModel.Model audience;
            UserModel.Model user;

            string clientValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            if (string.IsNullOrEmpty(clientValue))
                return;

            string audienceValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (string.IsNullOrEmpty(audienceValue))
                return;

            string userValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrUserID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrUserID] : null;

            if (string.IsNullOrEmpty(userValue))
                return;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = await _uow.ClientMgmt.FindByIdAsync(clientID);
            else
                client = await _uow.ClientMgmt.FindByNameAsync(clientValue);

            if (client == null || !client.Enabled)
                return;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = await _uow.AudienceMgmt.FindByIdAsync(audienceID);
            else
                audience = await _uow.AudienceMgmt.FindByNameAsync(audienceValue);

            if (audience == null || !audience.Enabled)
                return;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = await _uow.UserMgmt.FindByIdAsync(userID);
            else
                user = await _uow.UserMgmt.FindByNameAsync(userValue);

            if (user == null)
                return;

            //check if user is confirmed...
            if (!user.EmailConfirmed)
                return;

            //check if user is locked...
            if (await _uow.UserMgmt.IsLockedOutAsync(user.Id))
                return;

            if (_uow.ContextStatus == ContextType.UnitTest
                && _uow.ConfigMgmt.Tweaks.UnitTestRefreshToken)
            {
                issue = _uow.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow;
                expire = _uow.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow.AddMinutes(_uow.ConfigMgmt.Tweaks.DefaultRefreshTokenLife);
            }
            else
            {
                issue = DateTime.UtcNow;
                expire = DateTime.UtcNow.AddMinutes(_uow.ConfigMgmt.Tweaks.DefaultRefreshTokenLife);
            }

            var token = new UserRefreshTokenModel.Create()
            {
                ClientId = client.Id,
                AudienceId = audience.Id,
                UserId = user.Id,
                ProtectedTicket = context.SerializeTicket(),
                IssuedUtc = issue,
                ExpiresUtc = expire
            };

            context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
            context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

            var result = await _uow.UserMgmt.AddRefreshTokenAsync(token);

            if (result.Succeeded)
                context.SetToken(token.ProtectedTicket);
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            Guid clientID, audienceID;
            ClientModel.Model client;
            AudienceModel.Model audience;

            var postValues = await context.Request.ReadFormAsync() as IEnumerable<KeyValuePair<string, string[]>>;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientID).Value[0];
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceID).Value[0];

            if (string.IsNullOrEmpty(clientValue))
                return;

            if (string.IsNullOrEmpty(audienceValue))
                return;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = await _uow.ClientMgmt.FindByIdAsync(clientID);
            else
                client = await _uow.ClientMgmt.FindByNameAsync(clientValue);

            if (client == null || !client.Enabled)
                return;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = await _uow.AudienceMgmt.FindByIdAsync(audienceID);
            else
                audience = await _uow.AudienceMgmt.FindByNameAsync(audienceValue);

            if (audience == null || !audience.Enabled)
                return;

            var token = await _uow.UserMgmt.FindRefreshTokenAsync(context.Token);

            if (token == null)
                return;

            else if (await _uow.UserMgmt.IsLockedOutAsync(token.UserId))
                return;

            var result = await _uow.UserMgmt.RemoveRefreshTokenByIdAsync(token.Id);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (token.IssuedUtc <= DateTime.UtcNow && token.ExpiresUtc >= DateTime.UtcNow)
                context.DeserializeTicket(token.ProtectedTicket);
        }
    }
}