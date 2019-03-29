using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

/*
 * https://oauth.net/2/grant-types/authorization-code/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class AuthorizationCodeExtension
    {
        public static IApplicationBuilder UseAuthorizationCodeProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthorizationCodeProvider_Deprecate>();
        }
    }

    public class AuthorizationCodeProvider_Deprecate
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AuthorizationCodeProvider_Deprecate(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth2/v2/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(Strings.AttrIssuerIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrUserIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrRedirectUriIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrAuthorizeCodeIDV2)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var urlValues = context.Request.Query;

                string issuerValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV2).Value;
                string clientValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV2).Value;
                string userValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV2).Value;
                string redirectUriValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrRedirectUriIDV2).Value;
                string grantTypeValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string authorizationCodeValue = urlValues.FirstOrDefault(x => x.Key == Strings.AttrAuthorizeCodeIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
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

                Guid issuerID;
                AppIssuer issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
                else
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Name == issuerValue).Result).SingleOrDefault();

                if (issuer == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgIssuerNotExist }, _serializer));
                }

                if (!issuer.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgIssuerInvalid }, _serializer));
                }

                Guid userID;
                AppUser user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = (uow.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();
                else
                    user = (uow.UserRepo.GetAsync(x => x.Email == userValue).Result).SingleOrDefault();

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
                if (uow.UserRepo.IsLockedOutAsync(user.Id).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                }

                //check that payload can be decrypted and validated...
                if (!new ProtectProvider(uow.Situation.ToString()).ValidateAsync(user.SecurityStamp, authorizationCodeValue, user).Result)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserTokenInvalid }, _serializer));
                }

                var clientList = uow.UserRepo.GetClientsAsync(user.Id).Result;
                var clients = new List<AppClient>();

                //check if client is single, multiple or undefined...
                if (string.IsNullOrEmpty(clientValue))
                    clients = uow.ClientRepo.GetAsync(x => clientList.Contains(x)
                        && x.Enabled == true).Result.ToList();
                else
                {
                    foreach (string entry in clientValue.Split(","))
                    {
                        Guid clientID;
                        AppClient client;

                        //check if identifier is guid. resolve to guid if not.
                        if (Guid.TryParse(entry.Trim(), out clientID))
                            client = (uow.ClientRepo.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                        else
                            client = (uow.ClientRepo.GetAsync(x => x.Name == entry.Trim()).Result).SingleOrDefault();

                        if (client == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientNotExist }, _serializer));
                        }

                        if (!client.Enabled
                            || !clientList.Contains(client))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientInvalid }, _serializer));
                        }

                        clients.Add(client);
                    }
                }

                //check that redirect url is valid...
                var validUrl = false;

                foreach (var entry in clients)
                {
                    var urls = uow.ClientRepo.GetUriListAsync(entry.Id).Result;

                    if (urls.Any(x => x.AbsoluteUri == redirectUriValue))
                        validUrl = true;
                }

                if (!validUrl)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUriNotExist }, _serializer));
                }

                var access = JwtBuilder.CreateAccessTokenV2(uow, issuer, clients, user).Result;
                var refresh = JwtBuilder.CreateRefreshTokenV2(uow, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user = user.Id.ToString(),
                    client = clients.Select(x => x.Id.ToString()),
                    issuer = issuer.Id.ToString() + ":" + uow.IssuerRepo.Salt,
                };

                //add activity entry for login...
                uow.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    ActorId = user.Id,
                    ActivityType = LoginType.GenerateAuthorizationCodeV2.ToString(),
                    Immutable = false
                }).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(Strings.AttrIssuerIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrRedirectUriIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Query.ContainsKey(Strings.AttrAuthorizeCodeIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
