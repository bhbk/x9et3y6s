using Bhbk.Lib.Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Helpers
{
    //https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcore
    //https://ppolyzos.com/2017/10/30/add-jwt-bearer-authorization-to-swagger-and-asp-net-core/
    public class SwaggerHelper
    {
        public static void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer",
                new ApiKeyScheme
                {
                    Description = "Authorization header using the bearer scheme. Example: \"Authorization: Bearer {apiKey}\"",
                    Name = "Authorization",
                    In = "Header",
                    Type = "apiKey"
                });

            options.AddSecurityRequirement(
                new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "Bearer", new string[] { }
                    },
                });

            options.SwaggerDoc("v1",
                new Info
                {
                    Title = "API Help",
                    Version = "v1"
                });

            options.OperationFilter<SwaggerOperationFilter>();
        }

        public static void ConfigureSwagger(SwaggerOptions options)
        {
            //path of the end point. also update UI middleware...                
            options.RouteTemplate = "help/{documentName}/index.json";
            options.PreSerializeFilters.Add((doc, request) => doc.Host = request.Host.Value);
        }

        public static void ConfigureSwaggerUI(SwaggerUIOptions options)
        {
            //includes virtual directory if site is configured with one...
            options.RoutePrefix = "help";
            options.SwaggerEndpoint("v1/index.json", "API Help");
        }
    }
}
