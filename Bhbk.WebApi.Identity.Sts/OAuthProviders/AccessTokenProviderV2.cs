using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

//https://blogs.ibs.com/2017/12/12/token-based-authentication-using-asp-net-core-2-0/
//https://blogs.ibs.com/2017/12/19/token-based-auth-in-asp-net-core-2-part-2-refresh-tokens/

namespace Bhbk.WebApi.Identity.Sts.OAuthProviders
{
    public class AccessTokenProviderV2
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public AccessTokenProviderV2(RequestDelegate next)
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
            if (!context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal))
                return _next(context);

            //check if POST method
            if (!context.Request.Method.Equals("POST"))
                return _next(context);

            //check for application/x-www-form-urlencoded
            if (!context.Request.HasFormContentType)
                return _next(context);

            //check for correct parameter
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrUserIDV1)
                || !context.Request.Form.ContainsKey("password"))
                return _next(context);

            var postValues = context.Request.ReadFormAsync().Result;

            string clientValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrClientIDV2).Value;
            string audienceValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrAudienceIDV2).Value;
            string grantTypeValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrGrantTypeIDV2).Value;
            string userValue = postValues.FirstOrDefault(x => x.Key == BaseLib.Statics.AttrUserIDV1).Value;
            string passwordValue = postValues.FirstOrDefault(x => x.Key == "password").Value;

            //check for correct parameter format
            if (string.IsNullOrEmpty(clientValue)
                || string.IsNullOrEmpty(audienceValue)
                || !grantTypeValue.Equals("password")
                || string.IsNullOrEmpty(userValue)
                || string.IsNullOrEmpty(passwordValue))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_values", _serializer));
            }

            Guid clientID, audienceID;
            ClientModel client;
            AudienceModel audience;

            var ioc = context.RequestServices.GetService<IIdentityContext>();

            if (ioc == null)
                throw new ArgumentNullException();

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = ioc.ClientMgmt.FindByIdAsync(clientID).Result;
            else
                client = ioc.ClientMgmt.FindByNameAsync(clientValue).Result;

            if (client == null || !client.Enabled)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_client", _serializer));
            }

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(audienceValue, out audienceID))
                audience = ioc.AudienceMgmt.FindByIdAsync(audienceID).Result;
            else
                audience = ioc.AudienceMgmt.FindByNameAsync(audienceValue).Result;

            if (audience == null || !audience.Enabled)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_audience", _serializer));
            }

            var user = ioc.UserMgmt.FindByNameAsync(userValue).Result;

            //check that user exists...
            if (user == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            //check that user is confirmed...
            //check that user is not locked...
            else if (ioc.UserMgmt.IsLockedOutAsync(user.Id).Result
                || !user.EmailConfirmed)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
            }

            var logins = ioc.UserMgmt.GetLoginsAsync(user.Id).Result;

            //check that login provider exists...
            if (logins == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_login", _serializer));
            }

            //check if login provider is local...
            //check if login provider is transient for unit/integration test...
            else if (logins.Contains(BaseLib.Statics.ApiDefaultLogin)
                || (logins.Where(x => x.StartsWith(BaseLib.Statics.ApiUnitTestLogin)).Any() && ioc.ContextStatus == ContextType.UnitTest))
            {
                //check that password is valid...
                if (!ioc.UserMgmt.CheckPasswordAsync(ioc.UserMgmt.Store.Mf.Devolve.DoIt(user).Id, passwordValue).Result)
                {
                    ioc.UserMgmt.AccessFailedAsync(user.Id).Wait();

                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_user", _serializer));
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject("invalid_login", _serializer));
            }

            var access = JwtHelperV2.GenerateAccessToken(context, client, audience, user).Result;
            var refresh = JwtHelperV2.GenerateRefreshToken(context, client, audience, user).Result;

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                client = client.Id.ToString(),
                audience = audience.Id.ToString(),
                user = user.Id.ToString(),
                issued = access.begin,
                expires = access.end
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result, _serializer));
        }
    }
}