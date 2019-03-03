using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Primitives;
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

//https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
//https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class RefreshTokenExtension
    {
        public static IApplicationBuilder UseRefreshTokenProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RefreshTokenProvider_Deprecate>();
        }
    }

    public class RefreshTokenProvider_Deprecate
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public RefreshTokenProvider_Deprecate(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth2/v2/refresh", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrRefreshTokenIDV2)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV2).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string refreshTokenValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrRefreshTokenIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || !grantTypeValue.Equals(Strings.AttrRefreshTokenIDV2)
                    || string.IsNullOrEmpty(refreshTokenValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysParamsInvalid }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                IssuerModel issuer;

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

                var refreshToken = (uow.UserRepo.GetRefreshTokensAsync(x => x.ProtectedTicket == refreshTokenValue).Result).SingleOrDefault();

                if (refreshToken == null
                    || refreshToken.IssuedUtc >= DateTime.UtcNow
                    || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserTokenInvalid }, _serializer));
                }

                var user = (uow.UserRepo.GetAsync(x => x.Id == refreshToken.UserId).Result).SingleOrDefault();

                //check that user exists...
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is not locked...
                if (uow.UserRepo.IsLockedOutAsync(user.Id).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                }

                var clientList = uow.UserRepo.GetClientsAsync(user.Id).Result;
                var clients = new List<ClientModel>();

                //check if client is single, multiple or undefined...
                if (string.IsNullOrEmpty(clientValue))
                    clients = uow.ClientRepo.GetAsync(x => clientList.Contains(x.Id.ToString())
                        && x.Enabled == true).Result.ToList();
                else
                {
                    foreach (string entry in clientValue.Split(","))
                    {
                        Guid clientID;
                        ClientModel client;

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
                            || !clientList.Contains(client.Id.ToString()))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientInvalid }, _serializer));
                        }

                        clients.Add(client);
                    }
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
                    ActivityType = Enums.LoginType.GenerateRefreshTokenV2.ToString(),
                    Immutable = false
                }).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/refresh", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrRefreshTokenIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV1).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV1).Value;
                string refreshTokenValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrRefreshTokenIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(Strings.AttrRefreshTokenIDV1)
                    || string.IsNullOrEmpty(refreshTokenValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysParamsInvalid }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                IssuerModel issuer;

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

                Guid clientID;
                ClientModel client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = (uow.ClientRepo.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
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

                var refreshToken = (uow.UserRepo.GetRefreshTokensAsync(x => x.ProtectedTicket == refreshTokenValue).Result).SingleOrDefault();

                if (refreshToken == null
                    || refreshToken.IssuedUtc >= DateTime.UtcNow
                    || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserTokenInvalid }, _serializer));
                }

                var user = (uow.UserRepo.GetAsync(x => x.Id == refreshToken.UserId).Result).SingleOrDefault();

                //check that user exists...
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is not locked...
                if (uow.UserRepo.IsLockedOutAsync(user.Id).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                }

                var access = JwtBuilder.CreateAccessTokenV1(uow, issuer, client, user).Result;
                var refresh = JwtBuilder.CreateRefreshTokenV1(uow, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user_id = user.Id.ToString(),
                    client_id = client.Id.ToString(),
                    issuer_id = issuer.Id.ToString() + ":" + uow.IssuerRepo.Salt,
                };

                //add activity entry for login...
                uow.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateRefreshTokenV1.ToString(),
                    Immutable = false
                }).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
