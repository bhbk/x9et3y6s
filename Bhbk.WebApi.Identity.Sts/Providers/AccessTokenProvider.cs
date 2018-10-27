using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

//https://blogs.ibs.com/2017/12/12/token-based-authentication-using-asp-net-core-2-0/

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class AccessTokenExtension
    {
        public static IApplicationBuilder UseAccessTokenProvider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AccessTokenProvider>();
        }
    }

    public class AccessTokenProvider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AccessTokenProvider(RequestDelegate next)
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
            if (context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrAudienceIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV2)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV2)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV2).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrAudienceIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV2).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV2).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var audienceList = uow.CustomUserMgr.GetAudiencesAsync(user).Result;
                var audiences = new List<AppAudience>();

                //check if audience is single, multiple or undefined...
                if (string.IsNullOrEmpty(audienceValue))
                    audiences = uow.AudienceRepo.GetAsync(x => audienceList.Contains(x.Id.ToString())
                        && x.Enabled == true).Result.ToList();
                else
                {
                    foreach (string entry in audienceValue.Split(","))
                    {
                        Guid audienceID;
                        AppAudience audience;

                        //check if identifier is guid. resolve to guid if not.
                        if (Guid.TryParse(entry.Trim(), out audienceID))
                            audience = uow.AudienceRepo.GetAsync(audienceID).Result;
                        else
                            audience = (uow.AudienceRepo.GetAsync(x => x.Name == entry.Trim()).Result).SingleOrDefault();

                        if (audience == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgAudienceNotExist }, _serializer));
                        }

                        if (!audience.Enabled
                            || !audienceList.Contains(audience.Id.ToString()))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgAudienceInvalid }, _serializer));
                        }

                        audiences.Add(audience);
                    }
                }

                var loginList = uow.CustomUserMgr.GetLoginsAsync(user).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x.Id.ToString())).Result.ToList();

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Strings.ApiDefaultLogin).Any()
                    || (uow.Situation == ContextType.UnitTest && logins.Where(x => x.LoginProvider.StartsWith(Strings.ApiUnitTestLogin1)).Any()))
                {
                    //check that password is valid...
                    if (!uow.CustomUserMgr.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.CustomUserMgr.AccessFailedAsync(user).Wait();

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
                uow.CustomUserMgr.AccessSuccessAsync(user).Wait();

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
                uow.ActivityRepo.CreateAsync(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAccessTokenV2.ToString(),
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

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV1)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV1).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrAudienceIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || string.IsNullOrEmpty(audienceValue)
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

                Guid audienceID;
                AppAudience audience;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(audienceValue, out audienceID))
                    audience = uow.AudienceRepo.GetAsync(audienceID).Result;
                else
                    audience = (uow.AudienceRepo.GetAsync(x => x.Name == audienceValue).Result).SingleOrDefault();

                if (audience == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgAudienceNotExist }, _serializer));
                }

                if (!audience.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgAudienceInvalid }, _serializer));
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var loginList = uow.CustomUserMgr.GetLoginsAsync(user).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x.Id.ToString())).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Strings.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.LoginProvider.StartsWith(Strings.ApiUnitTestLogin1)).Any() && uow.Situation == ContextType.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.CustomUserMgr.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.CustomUserMgr.AccessFailedAsync(user).Wait();

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
                uow.CustomUserMgr.AccessSuccessAsync(user).Wait();

                var access = JwtSecureProvider.CreateAccessTokenV1(uow, client, audience, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV1(uow, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user_id = user.Id.ToString(),
                    audience_id = audience.Id.ToString(),
                    client_id = client.Id.ToString() + ":" + uow.ClientRepo.Salt,
                };

                //add activity entry for login...
                uow.ActivityRepo.CreateAsync(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                }).Wait();

                uow.CommitAsync().Wait();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));

            }

            #endregion

            #region v1 end-point (compatibility: audience and issuer entities mixed. no issuer salt.)

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Strings.AttrClientIDV1)
                    && !context.Request.Form.ContainsKey(Strings.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Strings.AttrUserPasswordIDV1)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string audienceValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Strings.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(audienceValue)
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
                if(!uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    return _next(context);

                /*
                 * this is really gross but is needed for backward compatibility.
                 * 
                 * will work because identity backed authorize filters use array of issuers, issuer keys and audiences. so basically 
                 * we just need any valid issuer defined in configuration on resource server side.
                 * 
                 */
                var client = uow.ClientRepo.GetAsync().Result.First();

                Guid audienceID;
                AppAudience audience;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(audienceValue, out audienceID))
                    audience = uow.AudienceRepo.GetAsync(audienceID).Result;
                else
                    audience = (uow.AudienceRepo.GetAsync(x => x.Name == audienceValue).Result).SingleOrDefault();

                if (client == null
                    || audience == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled
                    || !audience.Enabled)
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgUserNotExist }, _serializer));
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

                var loginList = uow.CustomUserMgr.GetLoginsAsync(user).Result;
                var logins = uow.LoginRepo.GetAsync(x => loginList.Contains(x.Id.ToString())).Result;

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Strings.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Strings.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.LoginProvider.StartsWith(Strings.ApiUnitTestLogin1)).Any() && uow.Situation == ContextType.UnitTest))
                {
                    //check that password is valid...
                    if (!uow.CustomUserMgr.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        uow.CustomUserMgr.AccessFailedAsync(user).Wait();

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
                uow.CustomUserMgr.AccessSuccessAsync(user).Wait();

                var access = JwtSecureProvider.CreateAccessTokenV1CompatibilityMode(uow, client, audience, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                uow.ActivityRepo.CreateAsync(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1CompatibilityMode.ToString(),
                    Created = DateTime.Now,
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