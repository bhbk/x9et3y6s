using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

/*
 * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
 */

namespace Bhbk.WebApi.Identity.Sts.Middlewares
{
    public static class SandboxMiddlewareExtension
    {
        public static IApplicationBuilder UseSandboxMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SandboxMiddleware>();
        }
    }

    public class SandboxMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializer;

        public SandboxMiddleware(RequestDelegate next)
        {
            _next = next;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            //check if correct v1 path, method, content and params...
            if (context.Request.Path.Equals("/oauth2/v1/sandbox", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType
                && (context.Request.Form.ContainsKey(Constants.AttrIssuerIDV1)
                    && context.Request.Form.ContainsKey(Constants.AttrAudienceIDV1)
                    && context.Request.Form.ContainsKey(Constants.AttrGrantTypeIDV1)
                    && context.Request.Form.ContainsKey(Constants.AttrUserIDV1)
                    && context.Request.Form.ContainsKey(Constants.AttrResourceOwnerIDV1)))
            {
                throw new InvalidOperationException();

                var formValues = context.Request.ReadFormAsync().Result;

                string issuerValue = formValues.FirstOrDefault(x => x.Key == Constants.AttrIssuerIDV1).Value;
                string audienceValue = formValues.FirstOrDefault(x => x.Key == Constants.AttrAudienceIDV1).Value;
                string grantTypeValue = formValues.FirstOrDefault(x => x.Key == Constants.AttrGrantTypeIDV1).Value;
                string userValue = formValues.FirstOrDefault(x => x.Key == Constants.AttrUserIDV1).Value;
                string passwordValue = formValues.FirstOrDefault(x => x.Key == Constants.AttrResourceOwnerIDV1).Value;

                var uow = context.RequestServices.GetRequiredService<IUnitOfWork>();
            }

            return _next(context);
        }
    }
}