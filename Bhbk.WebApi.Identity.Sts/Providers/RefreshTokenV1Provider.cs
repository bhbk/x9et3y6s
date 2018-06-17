using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

//https://blogs.ibs.com/2017/12/19/token-based-auth-in-asp-net-core-2-part-2-refresh-tokens/

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class RefreshTokenV1Extension
    {
        public static IApplicationBuilder UseRefreshTokenV1Provider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RefreshTokenV1Provider>();
        }
    }

    public class RefreshTokenV1Provider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public RefreshTokenV1Provider(RequestDelegate next)
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
            if (!context.Request.Path.Equals("/oauth/v1/refresh", StringComparison.Ordinal))
                return _next(context);

            //check if POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            //check for application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            //check for correct parameters
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV1)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrRefreshTokenIDV1))
                return _next(context);

            var postValues = context.Request.ReadFormAsync().Result;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV1).Value;
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceIDV1).Value;
            string grantTypeValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV1).Value;
            string refreshTokenValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrRefreshTokenIDV1).Value;

            //check for correct parameter format
            if (string.IsNullOrEmpty(clientValue)
                || string.IsNullOrEmpty(audienceValue)
                || !grantTypeValue.Equals(BaseLib.Statics.AttrRefreshTokenIDV1)
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

            Guid audienceID;
            AppAudience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
            else
                audience = ioc.AudienceMgmt.FindByNameAsync(audienceValue).Result;

            if (audience == null || !audience.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgAudienceInvalid }, _serializer));
            }

            var user = ioc.UserMgmt.FindByIdAsync(current.UserId.ToString()).Result;

            //check that user exists...
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
            }

            //check that user is not locked...
            else if (ioc.UserMgmt.IsLockedOutAsync(user).Result
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgUserInvalid }, _serializer));
            }

            var access = JwtV1Helper.GenerateAccessToken(context, client, audience, user).Result;
            var refresh = JwtV1Helper.GenerateRefreshToken(context, client, user).Result;

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user_id = user.Id.ToString(),
                audience_id = audience.Id.ToString(),
                client_id = client.Id.ToString() + ":" + ioc.ClientMgmt.Store.Salt,
            };

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
        }
    }
}
