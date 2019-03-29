using Bhbk.Lib.Core.Primitives.Enums;
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
 * https://oauth.net/2/grant-types/password/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class AccessTokenExtension
    {
        public static IApplicationBuilder UseAccessTokenProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AccessTokenProvider_Deprecate>();
        }
    }

    public class AccessTokenProvider_Deprecate
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AccessTokenProvider_Deprecate(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth2/v2/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV2)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV2).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV2).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || !grantTypeValue.Equals(Strings.AttrUserPasswordIDV2)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result.ToList();

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                    || (uow.Situation == ExecutionType.UnitTest && logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin1)).Any()))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

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
                    ActivityType = LoginType.GenerateAccessTokenV2.ToString(),
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
            if (context.Request.Path.Equals("/oauth2/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrIssuerIDV1).Value;
                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(issuerValue)
                    || string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(Strings.AttrUserPasswordIDV1)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
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

                Guid clientID;
                AppClient client;

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

                Guid userID;
                AppUser user;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(userValue, out userID))
                    user = (uow.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();
                else
                    user = (uow.UserRepo.GetAsync(x => x.Email == userValue).Result).SingleOrDefault();

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin1)).Any() && uow.Situation == ExecutionType.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

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
                    ActivityType = LoginType.GenerateAccessTokenV1.ToString(),
                    Immutable = false
                }).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));

            }

            #endregion

            #region v1 end-point (compatibility: issuer and client entities mixed. no issuer salt.)

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (!context.Request.Form.ContainsKey(Strings.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV1)))
            {
                //logic below ported from middleware to controller so open api (swagger) can do its job easier...
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(Strings.AttrUserPasswordIDV1)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgSysParamsInvalid }, _serializer));
                }

                var uow = context.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();

                if (uow == null)
                    throw new ArgumentNullException();

                //check if issuer compatibility mode enabled.
                if (!uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
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
                AppClient client;

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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgIssuerNotExist }, _serializer));
                }

                if (!issuer.Enabled
                    || !client.Enabled)
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var loginList = uow.UserRepo.GetLoginsAsync(user.Id).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x)).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin1)).Any() && uow.Situation == ExecutionType.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.UserRepo.CheckPasswordAsync(user.Id, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.UserRepo.AccessFailedAsync(user.Id).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                uow.UserRepo.AccessSuccessAsync(user.Id).Wait();

                var access = JwtBuilder.CreateAccessTokenV1Legacy(uow, issuer, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                uow.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    ActorId = user.Id,
                    ActivityType = LoginType.GenerateAccessTokenV1Legacy.ToString(),
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