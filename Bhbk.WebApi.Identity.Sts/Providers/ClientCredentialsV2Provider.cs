using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Providers
{
    public static class ClientCredentialsV2Extension
    {
        public static IApplicationBuilder UseClientCredentialsV2Provider(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ClientCredentialsV2Provider>();
        }
    }

    public class ClientCredentialsV2Provider
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public ClientCredentialsV2Provider(RequestDelegate next)
        {
            _next = next;

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            //check for correct parameters
            if (!context.Request.Form.ContainsKey(BaseLib.Statics.AttrClientIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrAudienceIDV2)
                || !context.Request.Form.ContainsKey(BaseLib.Statics.AttrGrantTypeIDV2)
                || !context.Request.Form.ContainsKey("client_secret"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = BaseLib.Statics.MsgSystemParametersInvalid }, _serializer));
            }

            return _next(context);
        }
    }
}
