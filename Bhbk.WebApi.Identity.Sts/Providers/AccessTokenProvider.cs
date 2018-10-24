using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Core.Providers;
using Bhbk.Lib.Identity;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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
            #region v1 end-point

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Statics.AttrClientIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrUserPasswordIDV1)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrClientIDV1).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrAudienceIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || string.IsNullOrEmpty(audienceValue)
                    || !grantTypeValue.Equals(Statics.AttrUserPasswordIDV1)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgSysParamsInvalid }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientInvalid }, _serializer));
                }

                Guid audienceID;
                AppAudience audience;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(audienceValue, out audienceID))
                    audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
                else
                    audience = ioc.AudienceMgmt.FindByNameAsync(audienceValue).Result;

                if (audience == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgAudienceNotExist }, _serializer));
                }

                if (!audience.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgAudienceInvalid }, _serializer));
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserNotExist }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                }

                var loginList = ioc.UserMgmt.GetLoginsAsync(user).Result;
                var logins = ioc.LoginMgmt.Store.Get(x => loginList.Contains(x.Id.ToString()));

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Statics.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.LoginProvider.StartsWith(Statics.ApiUnitTestLogin1)).Any() && ioc.Status == ContextType.UnitTest))
                {
                    //check that password is valid...
                    if (!ioc.UserMgmt.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        ioc.UserMgmt.AccessFailedAsync(user).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                ioc.UserMgmt.AccessSuccessAsync(user).Wait();

                var access = JwtSecureProvider.CreateAccessTokenV1(ioc, client, audience, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV1(ioc, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user_id = user.Id.ToString(),
                    audience_id = audience.Id.ToString(),
                    client_id = client.Id.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                };

                //add activity entry for login...
                new ActivityProvider<AppActivity>(ioc.GetContext()).Commit(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = LoginType.GenerateAccessToken.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));

            }

            #endregion

            #region v1 end-point (where audience used as issuer too)

            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Statics.AttrClientIDV1)
                    && !context.Request.Form.ContainsKey(Statics.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Statics.AttrUserPasswordIDV1)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string audienceValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrClientIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserPasswordIDV1).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(audienceValue)
                    || !grantTypeValue.Equals(Statics.AttrUserPasswordIDV1)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgSysParamsInvalid }, _serializer));
                }

                var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();

                if (ioc == null)
                    throw new ArgumentNullException();

                //this is gross. will work because authorize filters backed by array of issuers, issuer keys and audiences.
                var client = ioc.ClientMgmt.Store.Get().First();

                Guid audienceID;
                AppAudience audience;

                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(audienceValue, out audienceID))
                    audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
                else
                    audience = ioc.AudienceMgmt.FindByNameAsync(audienceValue).Result;

                if (client == null
                    || audience == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled
                    || !audience.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientInvalid }, _serializer));
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserNotExist }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                }

                var loginList = ioc.UserMgmt.GetLoginsAsync(user).Result;
                var logins = ioc.LoginMgmt.Store.Get(x => loginList.Contains(x.Id.ToString()));

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Statics.ApiDefaultLogin).Any()
                    || (logins.Where(x => x.LoginProvider.StartsWith(Statics.ApiUnitTestLogin1)).Any() && ioc.Status == ContextType.UnitTest))
                {
                    //check that password is valid...
                    if (!ioc.UserMgmt.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        ioc.UserMgmt.AccessFailedAsync(user).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                ioc.UserMgmt.AccessSuccessAsync(user).Wait();

                var access = JwtSecureProvider.CreateAccessTokenV1(ioc, client, audience, user).Result;
                var refresh = JwtSecureProvider.CreateRefreshTokenV1(ioc, client, user).Result;

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user_id = user.Id.ToString(),
                    audience_id = audience.Id.ToString(),
                    client_id = client.Id.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
                };

                //add activity entry for login...
                new ActivityProvider<AppActivity>(ioc.GetContext()).Commit(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = LoginType.GenerateAccessToken.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));

            }

            #endregion

            #region v2 end-point

            //check if correct v2 path, method, content and params...
            if (context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Statics.AttrClientIDV2)
                    && context.Request.Form.ContainsKey(Statics.AttrAudienceIDV2)
                    && context.Request.Form.ContainsKey(Statics.AttrGrantTypeIDV2)
                    && context.Request.Form.ContainsKey(Statics.AttrUserIDV2)
                    && context.Request.Form.ContainsKey(Statics.AttrUserPasswordIDV2)))
            {
                var formValues = context.Request.ReadFormAsync().Result;

                string clientValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrClientIDV2).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrAudienceIDV2).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrGrantTypeIDV2).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserIDV2).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Statics.AttrUserPasswordIDV2).Value;

                //check for correct parameter format
                if (string.IsNullOrEmpty(clientValue)
                    || !grantTypeValue.Equals(Statics.AttrUserPasswordIDV2)
                    || string.IsNullOrEmpty(userValue)
                    || string.IsNullOrEmpty(passwordValue))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgSysParamsInvalid }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientNotExist }, _serializer));
                }

                if (!client.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgClientInvalid }, _serializer));
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserNotExist }, _serializer));
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
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                }

                var audienceList = ioc.UserMgmt.GetAudiencesAsync(user).Result;
                var audiences = new List<AppAudience>();

                //check if audience is single, multiple or undefined...
                if (string.IsNullOrEmpty(audienceValue))
                    audiences = ioc.AudienceMgmt.Store.Get(x => audienceList.Contains(x.Id.ToString())
                        && x.Enabled == true).ToList();
                else
                {
                    foreach (string entry in audienceValue.Split(","))
                    {
                        Guid audienceID;
                        AppAudience audience;

                        //check if identifier is guid. resolve to guid if not.
                        if (Guid.TryParse(entry.Trim(), out audienceID))
                            audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
                        else
                            audience = ioc.AudienceMgmt.FindByNameAsync(entry.Trim()).Result;

                        if (audience == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgAudienceNotExist }, _serializer));
                        }

                        if (!audience.Enabled
                            || !audienceList.Contains(audience.Id.ToString()))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgAudienceInvalid }, _serializer));
                        }

                        audiences.Add(audience);
                    }
                }

                var loginList = ioc.UserMgmt.GetLoginsAsync(user).Result;
                var logins = ioc.LoginMgmt.Store.Get(x => loginList.Contains(x.Id.ToString())).ToList();

                //check that login provider exists...
                if (loginList == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginNotExist }, _serializer));
                }

                //check if login provider is local...
                //check if login provider is transient for unit/integration test...
                else if (logins.Where(x => x.LoginProvider == Statics.ApiDefaultLogin).Any()
                    || (ioc.Status == ContextType.UnitTest && logins.Where(x => x.LoginProvider.StartsWith(Statics.ApiUnitTestLogin1)).Any()))
                {
                    //check that password is valid...
                    if (!ioc.UserMgmt.CheckPasswordAsync(user, passwordValue).Result)
                    {
                        //adjust counter(s) for login failure...
                        ioc.UserMgmt.AccessFailedAsync(user).Wait();

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgUserInvalid }, _serializer));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = Statics.MsgLoginInvalid }, _serializer));
                }

                //adjust counter(s) for login success...
                ioc.UserMgmt.AccessSuccessAsync(user).Wait();

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
                    ActivityType = LoginType.GenerateAccessToken.ToString(),
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