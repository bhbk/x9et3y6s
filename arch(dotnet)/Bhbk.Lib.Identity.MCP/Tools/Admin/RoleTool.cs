using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class RoleTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "roles",
            Description = "Query and manage roles. Supports listing, getting by ID, searching, viewing users assigned to a role, and counting total records. Use 'count' to get the total number without fetching records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'search', 'users'],
                        'description': 'The action to perform. Use count to get total records without fetching data.'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Role ID (GUID) for get/users actions'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for name'
                    },
                    'skip': {
                        'type': 'integer',
                        'description': 'Number of records to skip (default: 0)'
                    },
                    'take': {
                        'type': 'integer',
                        'description': 'Number of records to take (default: 50, max: 100)'
                    }
                },
                'required': ['action']
            }")
        };

        public RoleTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
                    case "count":
                        return Task.FromResult(CountRoles());

                    case "list":
                        return Task.FromResult(ListRoles(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var roleId))
                            return Task.FromResult(MCPToolResult.Fail("Valid role ID is required"));
                        return Task.FromResult(GetRole(roleId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchRoles(query, skip, take));

                    case "users":
                        var usersId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(usersId) || !Guid.TryParse(usersId, out var usersRoleId))
                            return Task.FromResult(MCPToolResult.Fail("Valid role ID is required"));
                        return Task.FromResult(GetRoleUsers(usersRoleId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountRoles()
        {
            var total = _uow.Roles.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListRoles(int skip, int take)
        {
            var roles = _uow.Roles.Get(x => true)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Roles.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["roles"] = JArray.FromObject(roles, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetRole(Guid id)
        {
            var role = _uow.Roles.Get(x => x.Id == id).FirstOrDefault();
            if (role == null)
                return MCPToolResult.Fail($"Role not found: {id}");

            var result = JObject.FromObject(role, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchRoles(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var roles = _uow.Roles.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = roles.Count,
                ["roles"] = JArray.FromObject(roles, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetRoleUsers(Guid roleId)
        {
            var role = _uow.Roles.Get(x => x.Id == roleId).FirstOrDefault();
            if (role == null)
                return MCPToolResult.Fail($"Role not found: {roleId}");

            var users = _uow.Roles.GetUsersInRole(roleId);
            var result = new JObject
            {
                ["roleId"] = roleId.ToString(),
                ["roleName"] = role.Name,
                ["users"] = JArray.FromObject(users, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
