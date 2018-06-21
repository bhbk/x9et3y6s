using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

//https://blogs.ibs.com/2017/12/19/token-based-auth-in-asp-net-core-2-part-2-refresh-tokens/

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class RefreshTokenV2Extension
    {
        public static IApplicationBuilder UseRefreshTokenV2Provider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RefreshTokenV2Provider>();
        }
    }

    public class RefreshTokenV2Provider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public RefreshTokenV2Provider(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            //check if correct path
            if (!context.Request.Path.Equals("/oauth/v2/refresh", StringComparison.Ordinal))
                return _next(context);

            //check if POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            //check for application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            //check for correct parameters
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrRefreshTokenIDV2))
                return _next(context);

            var postValues = context.Request.ReadFormAsync().Result;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV2).Value;
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceIDV2).Value;
            string grantTypeValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV2).Value;
            string refreshTokenValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrRefreshTokenIDV2).Value;

            //check for correct parameter format
            if (string.IsNullOrEmpty(clientValue)
                || !grantTypeValue.Equals(BaseLib.Statics.AttrRefreshTokenIDV2)
                || string.IsNullOrEmpty(refreshTokenValue))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSystemParametersInvalid }, _serializer));
            }

            var ioc = context.RequestServices.GetRequiredService<IIdentityContext>();

            if (ioc == null)
                throw new ArgumentNullException();

            var current = ioc.UserMgmt.FindRefreshTokenAsync(refreshTokenValue).Result;

            if (current == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
            }

            else if (current.IssuedUtc >= DateTime.UtcNow || current.ExpiresUtc <= DateTime.UtcNow)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalidToken }, _serializer));
            }

            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = ioc.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = ioc.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgClientInvalid }, _serializer));
            }

            var user = ioc.UserMgmt.FindByIdAsync(current.UserId.ToString()).Result;

            //check that user exists...
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is not locked...
            if (ioc.UserMgmt.IsLockedOutAsync(user).Result
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
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

                    if (audience == null
                        || !audience.Enabled
                        || !audienceList.Contains(audience.Id.ToString()))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgAudienceInvalid }, _serializer));
                    }

                    audiences.Add(audience);
                }
            }

            var access = JwtV2Helper.GenerateAccessToken(context, client, audiences, user).Result;
            var refresh = JwtV2Helper.GenerateRefreshToken(context, client, user).Result;

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
            new CustomActivityProvider<AppActivity>(ioc.GetContext()).Commit(new AppActivity()
            {
                Id = Guid.NewGuid(),
                ActorId = user.Id,
                ActivityType = ActivityType.StsRefresh.ToString(),
                Created = DateTime.Now,
                Immutable = false
            });

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
        }
    }
}
