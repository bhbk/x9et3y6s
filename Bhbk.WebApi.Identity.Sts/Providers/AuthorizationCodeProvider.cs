using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class AuthorizationCodeExtension
    {
        public static IApplicationBuilder UseAuthorizationCodeProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthorizationCodeProvider>();
        }
    }

    public class AuthorizationCodeProvider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeProvider(RequestDelegate next)
        {
            _next = next;

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            #region v1 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrRedirectUriIDV1)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrAuthorizeCodeIDV1)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v2/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrUserIDV2)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrRedirectUriIDV2)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || context.Request.Query.ContainsKey(BaseLib.Statics.AttrAuthorizeCodeIDV2)))
            {
                var urlValues = context.Request.Query;

                string clientValue = urlValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV2).Value;
                string userValue = urlValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrUserIDV2).Value;
                string redirectUriValue = urlValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrRedirectUriIDV2).Value;
                string grantTypeValue = urlValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV2).Value;
                string authorizationCodeValue = urlValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAuthorizeCodeIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(redirectUriValue) || !Uri.IsWellFormedUriString(redirectUriValue, UriKind.RelativeOrAbsolute)
                    || !grantTypeValue.Equals(BaseLib.Statics.AttrAuthorizeCodeIDV2)
                    || string.IsNullOrEmpty(authorizationCodeValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSysParamsInvalid }, _serializer));
                }

                var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();

                if (ioc == null)
                    throw new ArgumentNullException();

                Guid clientID;
                AppClient client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = ioc.ClientMgmt.FindByIdAsync(clientID).Result;
                else
                    client = ioc.ClientMgmt.FindByNameAsync(clientValue).Result;

                if (client == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgClientInvalid }, _serializer));
                }

                Guid userID;
                AppUser user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = ioc.UserMgmt.FindByIdAsync(userID.ToString()).Result;
                else
                    user = ioc.UserMgmt.FindByEmailAsync(userValue).Result;

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is confirmed...
                //check that user is not locked...
                if (ioc.UserMgmt.IsLockedOutAsync(user).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
                }

                //check that payload can be decrypted and validated...
                if (!new ProtectProvider(ioc.Status.ToString()).ValidateAsync(user.PasswordHash, authorizationCodeValue, user).Result)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalidToken }, _serializer));
                }

                //add all audiences user is member of...
                var audienceList = ioc.UserMgmt.GetAudiencesAsync(user).Result;
                var audiences = ioc.AudienceMgmt.Store.Get(x => audienceList.Contains(x.Id.ToString())
                    && x.Enabled == true).ToList();

                //check that redirect url is valid...
                if (!audiences.Any(x => x.AppAudienceUri.Any(y => y.AbsoluteUri == redirectUriValue)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUriNotExist }, _serializer));
                }

                var access = JwtSecureProvider.CreateAccessTokenV2(ioc, client, audiences, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV2(ioc, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user = user.Id.ToString(),
                    audience = audiences.Select(x => x.Id.ToString()),
                    client = client.Id.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                };

                //add activity entry for login...
                new ActivityProvider<AppActivity>(ioc.GetContext()).Commit(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = ActivityType.StsAuthorizationCode.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
