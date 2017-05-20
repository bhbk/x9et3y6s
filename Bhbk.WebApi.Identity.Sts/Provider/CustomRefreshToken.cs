using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Infrastructure;
using System;
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

            Guid clientID;
            AppClient client;

            string clientValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            if (!Guid.TryParse(clientValue, out clientID))
            {
                //check if guid used for client. resolve guid from name if not.
                client = _uow.ClientRepository.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

                if (client == null)
                    throw new ArgumentNullException();
            }
            else
                client = _uow.ClientRepository.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();

            Guid audienceID;
            AppAudience audience;

            string audienceValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (!Guid.TryParse(audienceValue, out audienceID))
            {
                //check if guid used for client. resolve guid from name if not.
                audience = _uow.AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

                if (audience == null)
                    throw new ArgumentNullException();
            }
            else
                audience = _uow.AudienceRepository.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();

            string emailValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrUserID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrUserID] : null;

            if (emailValue == null)
                throw new ArgumentNullException();

            var user = await _uow.CustomUserManager.FindByEmailAsync(emailValue);
            var tokenID = Guid.NewGuid();

            AppUserToken token = new AppUserToken()
            {
                Id = tokenID,
                UserId = user.Id,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(60)),
                AudienceId = audience.Id
            };

            context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
            context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

            token.ProtectedTicket = context.SerializeTicket();

            var result = await _uow.CustomUserManager.AddRefreshTokenAsync(token);

            if (result.Succeeded)
                context.SetToken(tokenID.ToString());
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

            var valid = await _uow.CustomUserManager.FindRefreshTokenAsync(context.Token);

            if (valid != null)
            {
                context.DeserializeTicket(valid.ProtectedTicket);

                if (valid.IssuedUtc >= DateTime.UtcNow || valid.ExpiresUtc <= DateTime.UtcNow)
                    await _uow.CustomUserManager.RemoveRefreshTokenAsync(context.Token);
            }
        }
    }
}