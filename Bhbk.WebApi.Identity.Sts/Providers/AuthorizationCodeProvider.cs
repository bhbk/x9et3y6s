using Bhbk.Lib.Core.Providers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v2/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrUserIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrRedirectUriIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrAuthorizeCodeIDV2)))
            {
                var urlValues = context.Request.Query;

                string clientValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV2).Value;
                string userValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV2).Value;
                string redirectUriValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrRedirectUriIDV2).Value;
                string grantTypeValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string authorizationCodeValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrAuthorizeCodeIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(redirectUriValue) || !Uri.IsWellFormedUriString(redirectUriValue, UriKind.RelativeOrAbsolute)
                    || !grantTypeValue.Equals(Strings.AttrAuthorizeCodeIDV2)
                    || string.IsNullOrEmpty(authorizationCodeValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysParamsInvalid }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid clientID;
                AppClient client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = uow.ClientRepo.GetAsync(clientID).Result;
                else
                    client = (uow.ClientRepo.GetAsync(x => x.Name == clientValue).Result).Single();

                if (client == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientInvalid }, _serializer));
                }

                Guid userID;
                AppUser user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = uow.CustomUserMgr.FindByIdAsync(userID.ToString()).Result;
                else
                    user = uow.CustomUserMgr.FindByEmailAsync(userValue).Result;

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is confirmed...
                //check that user is not locked...
                if (uow.CustomUserMgr.IsLockedOutAsync(user).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                }

                //check that payload can be decrypted and validated...
                if (!new ProtectProvider(uow.Situation.ToString()).ValidateAsync(user.PasswordHash, authorizationCodeValue, user).Result)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalidToken }, _serializer));
                }

                //add all audiences user is member of...
                var audienceList = uow.CustomUserMgr.GetAudiencesAsync(user).Result;
                var audiences = (uow.AudienceRepo.GetAsync(x => audienceList.Contains(x.Id.ToString())
                    && x.Enabled == true).Result).ToList();

                //check that redirect url is valid...
                if (!audiences.Any(x => x.AppAudienceUri.Any(y => y.AbsoluteUri == redirectUriValue)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUriNotExist }, _serializer));
                }

                var access = JwtSecureProvider.CreateAccessTokenV2(uow, client, audiences, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV2(uow, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user = user.Id.ToString(),
                    audience = audiences.Select(x => x.Id.ToString()),
                    client = client.Id.ToString() + ":" + uow.ClientRepo.Salt,
                };

                //add activity entry for login...
                new ActivityProvider<AppActivity>(uow.Context).Commit(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAuthorizationCodeV2.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrRedirectUriIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrAuthorizeCodeIDV1)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
