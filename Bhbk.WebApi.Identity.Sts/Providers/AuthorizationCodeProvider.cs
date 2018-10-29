﻿using Bhbk.Lib.Identity.Interfaces;
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
            if (context.Request.Path.Equals("/oauth2/v2/authorization", StringComparison.Ordinal)
                && context.Request.Method.Equals("GET")
                && (context.Request.Query.ContainsKey(Strings.AttrIssuerIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrUserIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrRedirectUriIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Query.ContainsKey(Strings.AttrAuthorizeCodeIDV2)))
            {
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
                    issuer = uow.IssuerRepo.GetAsync(issuerID).Result;
                else
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Name == issuerValue).Result).Single();

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

                Guid clientID;
                AppClient client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = uow.ClientRepo.GetAsync(clientID).Result;
                else
                    client = (uow.ClientRepo.GetAsync(x => x.Name == clientValue).Result).SingleOrDefault();

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

                //add all clients user is member of...
                var clientList = uow.CustomUserMgr.GetClientsAsync(user).Result;
                var clients = (uow.ClientRepo.GetAsync(x => clientList.Contains(x.Id.ToString())
                    && x.Enabled == true).Result).ToList();

                //check that redirect url is valid...
                if (!clients.Any(x => x.AppClientUri.Any(y => y.AbsoluteUri == redirectUriValue)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUriNotExist }, _serializer));
                }

                var access = JwtSecureProvider.CreateAccessTokenV2(uow, issuer, clients, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV2(uow, issuer, user).Result;

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
                uow.ActivityRepo.CreateAsync(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAuthorizationCodeV2.ToString(),
                    Created = DateTime.Now,
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
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysNotImplemented }, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
