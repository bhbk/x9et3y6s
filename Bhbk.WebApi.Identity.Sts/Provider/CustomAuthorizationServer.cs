using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Interface;
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
        private IUnitOfWork _uow;

        public CustomAuthorizationServer(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public override Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.authorizeendpoint(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            return Task.FromResult<object>(null);
        }

        public override Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.authorizationendpointresponse(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            return Task.FromResult<object>(null);
        }

        public override Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.grantauthorizationcode(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            return Task.FromResult<object>(null);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.grantrefreshtoken(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            Guid clientID, audienceID;
            AppClient client;
            AppAudience audience;

            string clientValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrClientID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrClientID] : null;

            string audienceValue = context.Ticket.Properties.Dictionary.ContainsKey(BaseLib.Statics.AttrAudienceID)
                ? context.Ticket.Properties.Dictionary[BaseLib.Statics.AttrAudienceID] : null;

            if (string.IsNullOrEmpty(clientValue))
            {
                context.SetError("invalid_client_id", string.Format(BaseLib.Statics.MsgClientInvalid + " '{0}'", clientValue));
                return Task.FromResult<object>(null);
            }

            if (string.IsNullOrEmpty(audienceValue))
            {
                context.SetError("invalid_audience_id", string.Format(BaseLib.Statics.MsgAudienceInvalid + " '{0}'", audienceValue));
                return Task.FromResult<object>(null);
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();
            else
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

            if (client == null)
            {
                context.SetError("invalid_client_id", string.Format(BaseLib.Statics.MsgClientInvalid + " '{0}'", clientValue));
                return Task.FromResult<object>(null);
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceMgmt.LocalStore.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();
            else
                audience = _uow.AudienceMgmt.LocalStore.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

            if (audience == null)
            {
                context.SetError("invalid_audience_id", string.Format(BaseLib.Statics.MsgAudienceInvalid + " '{0}'", audienceValue));
                return Task.FromResult<object>(null);
            }

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
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            Guid clientID, audienceID;
            AppClient client;
            AppAudience audience;

            var postValues = await context.Request.ReadFormAsync() as IEnumerable<KeyValuePair<string, string[]>>;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientID).Value[0];
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceID).Value[0];

            if (string.IsNullOrEmpty(clientValue))
            {
                context.SetError("invalid_client_id", string.Format(BaseLib.Statics.MsgClientInvalid + " '{0}'", clientValue));
                return;
            }

            if (string.IsNullOrEmpty(audienceValue))
            {
                context.SetError("invalid_audience_id", string.Format(BaseLib.Statics.MsgAudienceInvalid + " '{0}'", audienceValue));
                return;
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Id == clientID && x.Enabled).SingleOrDefault();
            else
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Name == clientValue && x.Enabled).SingleOrDefault();

            if (client == null)
            {
                context.SetError("invalid_client_id", string.Format(BaseLib.Statics.MsgClientInvalid + " '{0}'", clientValue));
                return;
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = _uow.AudienceMgmt.LocalStore.Get(x => x.Id == audienceID && x.Enabled).SingleOrDefault();
            else
                audience = _uow.AudienceMgmt.LocalStore.Get(x => x.Name == audienceValue && x.Enabled).SingleOrDefault();

            if (audience == null)
            {
                context.SetError("invalid_audience_id", string.Format(BaseLib.Statics.MsgAudienceInvalid + " '{0}'", audienceValue));
                return;
            }

            //check that username has valid format...
            if (!FormatHelper.ValidateUsernameFormat(context.UserName))
            {
                context.SetError("invalid_user_id", string.Format(BaseLib.Statics.MsgUserInvalid + " '{0}'", context.UserName));
                return;
            }

            var user = await _uow.UserMgmt.FindByNameAsyncDeprecated(context.UserName);

            //check that user exists...
            if (user == null)
            {
                context.SetError("invalid_user_id", string.Format(BaseLib.Statics.MsgUserNotExist + " '{0}'", context.UserName));
                return;
            }

            //check that user is confirmed...
            else if (!user.EmailConfirmed)
            {
                context.SetError("invalid_user_id", string.Format(BaseLib.Statics.MsgUserUnconfirmed + " '{0}'", context.UserName));
                return;
            }

            //check that user is not locked...
            else if (await _uow.UserMgmt.IsLockedOutAsync(user.Id))
            {
                context.SetError("invalid_user_id", string.Format(BaseLib.Statics.MsgUserLocked + " '{0}'", context.UserName));
                return;
            }

            var providers = await _uow.UserMgmt.GetProvidersAsync(user.Id);

            //check that user has a provider to auth against...
            if (providers.Contains(BaseLib.Statics.ApiDefaultProvider) || providers.Where(x => x.StartsWith(BaseLib.Statics.ApiUnitTestProvider)).Any())
            {
                //check that password is valid...
                if (!await _uow.UserMgmt.CheckPasswordAsync(user, context.Password))
                {
                    await _uow.UserMgmt.AccessFailedAsync(user.Id);

                    context.SetError("invalid_user_id", string.Format(BaseLib.Statics.MsgUserInvalid + " '{0}'", context.UserName));
                    return;
                }
            }
            else
            {
                context.SetError("invalid_provider_id", string.Format(BaseLib.Statics.MsgProviderInvalid + " '{0}'", context.UserName));
                return;
            }

            var attrs = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { BaseLib.Statics.AttrClientID, client.Id.ToString().ToLower() },
                    { BaseLib.Statics.AttrAudienceID, audience.Id.ToString().ToLower() },
                    { BaseLib.Statics.AttrUserID, user.Id.ToString().ToLower() }
                });

            ClaimsIdentity claims = await _uow.UserMgmt.CreateIdentityAsync(user, "JWT");

            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            AuthenticationTicket ticket = new AuthenticationTicket(claims, attrs);

            await _uow.UserMgmt.ResetAccessFailedCountAsync(user.Id);

            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.tokenendpoint(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();

            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
                context.AdditionalResponseParameters.Add(property.Key, property.Value);

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //https://msdn.microsoft.com/en-us/library/microsoft.owin.security.oauth.oauthauthorizationserverprovider.validateclientauthentication(v=vs.113).aspx

            if (context == null)
                throw new ArgumentNullException();
            else
                context.OwinContext.Set<IUnitOfWork>(_uow);

            string contextID;
            string contextSecret;

            if (!context.TryGetBasicCredentials(out contextID, out contextSecret))
                context.TryGetFormCredentials(out contextID, out contextSecret);

            Guid clientID;
            AppClient client;

            if (string.IsNullOrEmpty(context.ClientId))
            {
                context.SetError("invalid_client_id", BaseLib.Statics.MsgClientInvalid);
                return Task.FromResult<object>(null);
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(context.ClientId, out clientID))
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Id.ToString() == context.ClientId && x.Enabled).SingleOrDefault();
            else
                client = _uow.ClientMgmt.LocalStore.Get(x => x.Name == context.ClientId && x.Enabled).SingleOrDefault();

            if (client == null)
            {
                context.SetError("invalid_client_id", string.Format(BaseLib.Statics.MsgClientInvalid + " '{0}'", context.ClientId));
                return Task.FromResult<object>(null);
            }

            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            Uri expectedUri = new Uri(context.Request.Uri, "/");

            if (expectedUri.AbsoluteUri == context.RedirectUri)
                context.Validated();
            else
                context.Validated();

            return Task.FromResult<object>(null);
        }
    }
}