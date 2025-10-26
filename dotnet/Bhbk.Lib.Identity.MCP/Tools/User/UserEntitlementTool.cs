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
    public class UserEntitlementTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private readonly Guid _userId;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "entitlements",
            Description = "View your entitlements and permissions.",
            Scope = MCPScope.User,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['list', 'types', 'scopes'], 'description': 'The operation to perform. list returns your entitlements. types returns available entitlement types. scopes returns available entitlement scopes.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public UserEntitlementTool(IUnitOfWork uow, Guid userId)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userId = userId;
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 50, 100);

            try
            {
                switch (action)
                {
                    case "list":
                        return Task.FromResult(ListEntitlements(skip, take));

                    case "types":
                        return Task.FromResult(ListTypes());

                    case "scopes":
                        return Task.FromResult(ListScopes());

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListEntitlements(int skip, int take)
        {
            var entitlements = _uow.UserEntitlements.Get(x => x.UserId == _userId)
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.UserEntitlements.Get(x => x.UserId == _userId).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["entitlements"] = JArray.FromObject(entitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult ListTypes()
        {
            var types = _uow.EntitlementTypes.Get(x => true)
                .OrderBy(x => x.SortOrder)
                .ToList();

            var result = new JObject
            {
                ["total"] = types.Count,
                ["types"] = JArray.FromObject(types, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult ListScopes()
        {
            var scopes = _uow.EntitlementScopes.Get(x => true)
                .OrderBy(x => x.SortOrder)
                .ToList();

            var result = new JObject
            {
                ["total"] = scopes.Count,
                ["scopes"] = JArray.FromObject(scopes, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
