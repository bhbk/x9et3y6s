using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.User
{
    public class ProfileTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private readonly Guid _userId;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "profile",
            Description = "Get your profile information, roles, and claims. This tool only accesses your own data.",
            Scope = MCPScope.User,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['get', 'roles', 'claims', 'settings'],
                        'description': 'The action to perform'
                    }
                },
                'required': ['action']
            }")
        };

        public ProfileTool(IUnitOfWork uow, Guid userId)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userId = userId;
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();

            try
            {
                switch (action)
                {
                    case "get":
                        return Task.FromResult(GetProfile());

                    case "roles":
                        return Task.FromResult(GetRoles());

                    case "claims":
                        return Task.FromResult(GetClaims());

                    case "settings":
                        return Task.FromResult(GetSettings());

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult GetProfile()
        {
            var user = _uow.Users.Get(x => x.Id == _userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail("User not found");

            var result = JObject.FromObject(user, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetRoles()
        {
            var user = _uow.Users.Get(x => x.Id == _userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail("User not found");

            var roles = _uow.Users.GetRolesForUser(_userId);
            var result = new JObject
            {
                ["roles"] = JArray.FromObject(roles, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetClaims()
        {
            var user = _uow.Users.Get(x => x.Id == _userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail("User not found");

            // User claims are accessible via role assignments
            var result = new JObject
            {
                ["message"] = "User claims can be accessed via the user's role assignments"
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetSettings()
        {
            var settings = _uow.Settings.Get(x => x.UserId == _userId)
                .OrderBy(x => x.ConfigKey)
                .ToList();

            var result = new JObject
            {
                ["count"] = settings.Count,
                ["settings"] = JArray.FromObject(settings, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
