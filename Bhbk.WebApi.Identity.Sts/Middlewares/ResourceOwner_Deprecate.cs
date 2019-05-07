using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Helpers;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

/*
 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
 */

/*
 * https://oauth.net/2/grant-types/password/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Middlewares
{
    public static class ResourceOwnerExtension
    {
        public static IApplicationBuilder UseResourceOwnerMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResourceOwner_Deprecate>();
        }
    }

    public class ResourceOwner_Deprecate
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public ResourceOwner_Deprecate(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth2/v2/ropg", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrUserIDV2)
                    && context.Request.Form.ContainsKey(RealConstants.AttrResourceOwnerIDV2)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrIssuerIDV2).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrClientIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV2).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrUserIDV2).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrResourceOwnerIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || !grantTypeValue.Equals(RealConstants.AttrResourceOwnerIDV2, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = MessageType.ParametersInvalid.ToString() }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IUnitOfWork>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                tbl_Issuers issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
                else
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Name == issuerValue).Result).SingleOrDefault();

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

                Guid userID;
                tbl_Users user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = (uow.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();
                else
                    user = (uow.UserRepo.GetAsync(x => x.Email == userValue).Result).SingleOrDefault();

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{userValue}" }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                var clientList = uow.UserRepo.GetClientsAsync(user.Id).Result;
                var clients = new List<tbl_Clients>();

                //check if client is single, multiple or undefined...
                if (string.IsNullOrEmpty(clientValue))
                    clients = uow.ClientRepo.GetAsync(x => clientList.Contains(x)
                        && x.Enabled == true).Result.ToList();
                else
                {
                    foreach (string entry in clientValue.Split(","))
                    {
                        Guid clientID;
                        tbl_Clients client;

                        //check if identifier is guid. resolve to guid if not.
                        if (Guid.TryParse(entry.Trim(), out clientID))
                            client = (uow.ClientRepo.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                        else
                            client = (uow.ClientRepo.GetAsync(x => x.Name == entry.Trim()).Result).SingleOrDefault();

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

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result.ToList();

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Login for user:{user.Id}" }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name.Equals(RealConstants.ApiDefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                    || (logins.Where(x => x.Name.StartsWith(FakeConstants.ApiTestLogin, StringComparison.OrdinalIgnoreCase)).Any()
                        && uow.InstanceType == InstanceContext.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

                var rop = JwtFactory.UserResourceOwnerV2(uow, issuer, clients, user).Result;
                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    refresh_token = rt,
                    user = user.Id.ToString(),
                    client = clients.Select(x => x.Id.ToString()),
                    issuer = issuer.Id.ToString() + ":" + uow.IssuerRepo.Salt,
                };

                //add activity entry...
                uow.ActivityRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false
                    })).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
            }

            #endregion

            #region v1 end-point

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/ropg", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrResourceOwnerIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrIssuerIDV1).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrResourceOwnerIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(RealConstants.AttrResourceOwnerIDV1, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = MessageType.ParametersInvalid.ToString() }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IUnitOfWork>();

                if (uow == null)
                    throw new ArgumentNullException();

                Guid issuerID;
                tbl_Issuers issuer;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(issuerValue, out issuerID))
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
                else
                    issuer = (uow.IssuerRepo.GetAsync(x => x.Name == issuerValue).Result).SingleOrDefault();

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
                    client = (uow.ClientRepo.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                else
                    client = (uow.ClientRepo.GetAsync(x => x.Name == clientValue).Result).SingleOrDefault();

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

                Guid userID;
                tbl_Users user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = (uow.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();
                else
                    user = (uow.UserRepo.GetAsync(x => x.Email == userValue).Result).SingleOrDefault();

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{userValue}" }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Login for user:{user.Id}" }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name.Equals(RealConstants.ApiDefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                    || (logins.Where(x => x.Name.StartsWith(FakeConstants.ApiTestLogin, StringComparison.OrdinalIgnoreCase)).Any()
                        && uow.InstanceType == InstanceContext.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

                var rop = JwtFactory.UserResourceOwnerV1(uow, issuer, client, user).Result;
                var rt = JwtFactory.UserRefreshV1(uow, issuer, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    refresh_token = rt.RawData,
                    user_id = user.Id.ToString(),
                    client_id = client.Id.ToString(),
                    issuer_id = issuer.Id.ToString() + ":" + uow.IssuerRepo.Salt,
                };

                //add activity entry...
                uow.ActivityRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV1.ToString(),
                        Immutable = false
                    })).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));

            }

            #endregion

            #region v1 end-point (compatibility: issuer and client entities mixed. no issuer salt.)

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/ropg", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (!context.Request.Form.ContainsKey(RealConstants.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(RealConstants.AttrResourceOwnerIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == RealConstants.AttrResourceOwnerIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(RealConstants.AttrResourceOwnerIDV1, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = MessageType.ParametersInvalid.ToString() }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IUnitOfWork>();

                if (uow == null)
                    throw new ArgumentNullException();

                //check if issuer compatibility mode enabled.
                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer)).Result.Single();

                if (!bool.Parse(legacyIssuer.ConfigValue))
                    return _next(context);

                /*
                 * this is really gross but is needed for backward compatibility.
                 * 
                 * will work because identity backed authorize filters use array of issuers, issuer keys and clients. so basically 
                 * we just need any valid issuer defined in configuration on resource server side.
                 * 
                 */
                var issuer = uow.IssuerRepo.GetAsync().Result.First();

                Guid clientID;
                tbl_Clients client;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(clientValue, out clientID))
                    client = (uow.ClientRepo.GetAsync(x => x.Id == clientID).Result).SingleOrDefault();
                else
                    client = (uow.ClientRepo.GetAsync(x => x.Name == clientValue).Result).SingleOrDefault();

                if (issuer == null
                    || client == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuer.Id}" }, _serializer));
                }
                else if (!issuer.Enabled
                    || !client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Issuer:{issuer.Id}" }, _serializer));
                }

                Guid userID;
                tbl_Users user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = (uow.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();
                else
                    user = (uow.UserRepo.GetAsync(x => x.Email == userValue).Result).SingleOrDefault();

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{userValue}" }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"Login for user:{user.Id}" }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name.Equals(RealConstants.ApiDefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                    || (logins.Where(x => x.Name.StartsWith(FakeConstants.ApiTestLogin, StringComparison.OrdinalIgnoreCase)).Any()
                        && uow.InstanceType == InstanceContext.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = $"User:{user.Id}" }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

                var access = JwtFactory.UserResourceOwnerV1_Legacy(uow, issuer, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.RawData,
                    expires_in = (int)(new DateTimeOffset(access.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry...
                uow.ActivityRepo.CreateAsync(
                    uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV1Legacy.ToString(),
                        Immutable = false
                    })).Wait();

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