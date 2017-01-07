using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Provider
{
    public class CustomAuthorizationServer : OAuthAuthorizationServerProvider
    {
        public CustomAuthorizationServer() { }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.grantrefreshtoken(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();

            Guid clientID;
            AppClient client;

            string clientValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            if (!Guid.TryParse(clientValue, out clientID))
            {
                //check if guid used for client. resolve guid from name if not.
                client = context.OwinContext.GetUserManager<IUnitOfWork>().ClientRepository.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

                if (client == null)
                {
                    context.SetError("invalid_client_id", string.Format("Invalid client '{0}'", context.ClientId));
                    return Task.FromResult<object>(null);
                }
            }
            else
                client = context.OwinContext.GetUserManager<IUnitOfWork>().ClientRepository.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();

            Guid audienceID;
            AppAudience audience;

            string audienceValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (!Guid.TryParse(audienceValue, out audienceID))
            {
                //check if guid used for client. resolve guid from name if not.
                audience = context.OwinContext.GetUserManager<IUnitOfWork>().AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

                if (audience == null)
                {
                    context.SetError("invalid_audience_id", string.Format("Invalid audience '{0}'", context.ClientId));
                    return Task.FromResult<object>(null);
                }
            }
            else
                audience = context.OwinContext.GetUserManager<IUnitOfWork>().AudienceRepository.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();

            var claims = new ClaimsIdentity(context.Ticket.Identity);
            var ticket = new AuthenticationTicket(claims, context.Ticket.Properties);

            context.Validated(ticket);
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.grantresourceownercredentials(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();

            var data = await context.Request.ReadFormAsync() as IEnumerable<KeyValuePair<string, string[]>>;

            //check that username has valid format...
            if (!FormatHelper.ValidateUsernameFormat(context.UserName))
            {
                context.SetError("invalid_user_id", string.Format("Invalid user '{0}'", context.UserName));
                return;
            }

            var user = await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.FindByEmailAsync(context.UserName);

            //check that user exists...
            if (user == null)
            {
                context.SetError("invalid_user_id", string.Format("Invalid user '{0}'", context.UserName));
                return;
            }
            //check that password is valid...
            else if (!await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.CheckPasswordAsync(user, context.Password))
            {
                await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.AccessFailedAsync(user.Id);

                context.SetError("invalid_user_id", string.Format("Invalid user '{0}'", context.UserName));
                return;
            }

            Guid clientID;
            string clientValue = data.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientID).Value[0];

            //check if guid used for client. resolve guid from name if not.
            if (!Guid.TryParse(clientValue, out clientID))
            {
                var client = context.OwinContext.GetUserManager<IUnitOfWork>().ClientRepository.Get(x => x.Name == context.ClientId && x.Enabled).SingleOrDefault();

                if (client != null)
                    clientID = client.Id;
            }

            if (clientID == null)
            {
                await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.AccessFailedAsync(user.Id);

                context.SetError("invalid_client_id", string.Format("Invalid client '{0}'", clientValue));
                return;
            }

            Guid audienceID;
            string audienceValue = data.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceID).Value[0];

            //check if guid used for audience. resolve guid from name if not.
            if (!Guid.TryParse(audienceValue, out audienceID))
            {
                var audience = context.OwinContext.GetUserManager<IUnitOfWork>().AudienceRepository.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

                if (audience != null)
                    audienceID = audience.Id;
            }

            if (audienceID == null)
            {
                await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.AccessFailedAsync(user.Id);

                context.SetError("invalid_audience_id", string.Format("Invalid audience '{0}'", audienceValue));
                return;
            }
            
            var attrs = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { BaseLib.Statics.AttrClientID, clientID.ToString().ToLower() },
                    { BaseLib.Statics.AttrAudienceID, audienceID.ToString().ToLower() },
                    { BaseLib.Statics.AttrUserID, user.Email }
                });

            ClaimsIdentity claims = await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.CreateIdentityAsync(user, "JWT");
            claims.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            AuthenticationTicket ticket = new AuthenticationTicket(claims, attrs);

            await context.OwinContext.GetUserManager<IUnitOfWork>().CustomUserManager.ResetAccessFailedCountAsync(user.Id);

            context.Validated(ticket);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.validateclientauthentication(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();

            string contextID;
            string contextSecret;

            if (!context.TryGetBasicCredentials(out contextID, out contextSecret))
                context.TryGetFormCredentials(out contextID, out contextSecret);

            if (context.ClientId == null)
            {
                context.SetError("invalid_client_id", string.Format("Invalid client '{0}'", context.ClientId));
                return Task.FromResult<object>(null);
            }

            Guid clientID;
            AppClient client;

            if (!Guid.TryParse(context.ClientId, out clientID))
            {
                //check if guid used for client. resolve guid from name if not.
                client = context.OwinContext.GetUserManager<IUnitOfWork>().ClientRepository.Get(x => x.Name == context.ClientId && x.Enabled).SingleOrDefault();

                if (client == null)
                    context.SetError("invalid_client_id", string.Format("Invalid client '{0}'", context.ClientId));
            }
            else
                client = context.OwinContext.GetUserManager<IUnitOfWork>().ClientRepository.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();

            if (client == null)
            {
                context.SetError("invalid_client_id", string.Format("Invalid client '{0}'", context.ClientId));
                return Task.FromResult<object>(null);
            }

            context.Validated();
            return Task.FromResult<object>(null);
        }
    }
}