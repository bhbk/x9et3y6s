using AutoMapper;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

/*
 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
 */

/*
 * https://oauth.net/2/grant-types/refresh-token/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Middlewares
{
    public static class ResourceOwnerRefreshExtension
    {
        public static IApplicationBuilder UseRefreshTokenProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResourceOwnerRefresh_Deprecate>();
        }
    }

    public class ResourceOwnerRefresh_Deprecate
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public ResourceOwnerRefresh_Deprecate(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            var mapper = context.RequestServices.GetRequiredService<IMapper>();

            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v2/ropg-rt", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrRefreshTokenIDV2)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrIssuerIDV2).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrClientIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV2).Value;
                string refreshTokenValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrRefreshTokenIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || !grantTypeValue.Equals(RealConstants.AttrRefreshTokenIDV2, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(refreshTokenValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = MessageType.ParametersInvalid.ToString() }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IUoWService>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                tbl_Issuers issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = (uow.Issuers.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
                else
                    issuer = (uow.Issuers.GetAsync(x => x.Name == issuerValue).Result).SingleOrDefault();

                if (issuer == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuerValue}" }, _serializer));
                }
                else if (!issuer.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuerValue}" }, _serializer));
                }

                var refreshToken = (uow.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.RefreshValue == refreshTokenValue).ToLambda()).Result).SingleOrDefault();

                if (refreshToken == null
                    || refreshToken.ValidFromUtc >= DateTime.UtcNow
                    || refreshToken.ValidToUtc <= DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Token:{refreshTokenValue}" }, _serializer));
                }

                var user = (uow.Users.GetAsync(x => x.Id == refreshToken.UserId).Result).SingleOrDefault();

                //check that user exists...
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{refreshToken.UserId}" }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is not locked...
                if (uow.Users.IsLockedOutAsync(user.Id).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                var clientList = uow.Clients.GetAsync(new QueryExpression<tbl_Clients>()
                        .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda()).Result;
                var clients = new List<tbl_Clients>();

                //check if client is single, multiple or undefined...
                if (string.IsNullOrEmpty(clientValue))
                    clients = uow.Clients.GetAsync(x => clientList.Contains(x)
                        && x.Enabled == true).Result.ToList();
                else
                {
                    foreach (string entry in clientValue.Split(","))
                    {
                        Guid clientID;
                        tbl_Clients client;

                        //check if identifier is guid. resolve to guid if not.
                        if (Guid.TryParse(entry.Trim(), out clientID))
                            client = (uow.Clients.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                        else
                            client = (uow.Clients.GetAsync(x => x.Name == entry.Trim()).Result).SingleOrDefault();

                        if (client == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Client:{clientValue}" }, _serializer));
                        }

                        if (!client.Enabled
                            || !clientList.Contains(client))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Client:{clientValue}" }, _serializer));
                        }

                        clients.Add(client);
                    }
                }

                var rop = JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, clients, user).Result;
                var rt = JwtFactory.UserRefreshV2(uow, mapper, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    refresh_token = rt.RawData,
                    user = user.Id.ToString(),
                    client = clients.Select(x => x.Id.ToString()),
                    issuer = issuer.Id.ToString() + ":" + uow.Issuers.Salt,
                };

                //add activity entry...
                uow.Activities.CreateAsync(
                    mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                        Immutable = false
                    }));

                uow.CommitAsync();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/ropg-rt", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrRefreshTokenIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrIssuerIDV1).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV1).Value;
                string refreshTokenValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrRefreshTokenIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(RealConstants.AttrRefreshTokenIDV1, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(refreshTokenValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = MessageType.ParametersInvalid.ToString() }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IUoWService>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                tbl_Issuers issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = (uow.Issuers.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
                else
                    issuer = (uow.Issuers.GetAsync(x => x.Name == issuerValue).Result).SingleOrDefault();

                if (issuer == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuerValue}" }, _serializer));
                }
                else if (!issuer.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuerValue}" }, _serializer));
                }

                Guid clientID;
                tbl_Clients client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = (uow.Clients.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                else
                    client = (uow.Clients.GetAsync(x => x.Name == clientValue).Result).SingleOrDefault();

                if (client == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Client:{clientValue}" }, _serializer));
                }

                if (!client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Client:{clientValue}" }, _serializer));
                }

                var refreshToken = (uow.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.RefreshValue == refreshTokenValue).ToLambda()).Result).SingleOrDefault();

                if (refreshToken == null
                    || refreshToken.ValidFromUtc >= DateTime.UtcNow
                    || refreshToken.ValidToUtc <= DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Token:{refreshTokenValue}" }, _serializer));
                }

                var user = (uow.Users.GetAsync(x => x.Id == refreshToken.UserId).Result).SingleOrDefault();

                //check that user exists...
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{refreshToken.UserId}" }, _serializer));
                }

                //no context for auth exists yet... so set actor id same as user id...
                user.ActorId = user.Id;

                //check that user is not locked...
                if (uow.Users.IsLockedOutAsync(user.Id).Result
                    || !user.EmailConfirmed
                    || !user.PasswordConfirmed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                var rop = JwtFactory.UserResourceOwnerV1(uow, mapper, issuer, client, user).Result;
                var rt = JwtFactory.UserRefreshV1(uow, mapper, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    refresh_token = rt.RawData,
                    user_id = user.Id.ToString(),
                    client_id = client.Id.ToString(),
                    issuer_id = issuer.Id.ToString() + ":" + uow.Issuers.Salt,
                };

                //add activity entry...
                uow.Activities.CreateAsync(
                    mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserRefreshTokenV1.ToString(),
                        Immutable = false
                    }));

                uow.CommitAsync();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            return _next(context);
        }
    }
}
