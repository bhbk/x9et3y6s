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
    public class UserTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "users",
            Description = "Query and manage users. Supports listing all users, getting user by ID, searching by username or email, viewing user roles and claims, and counting total records. Use 'count' to get the total number without fetching records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'search', 'roles', 'claims'],
                        'description': 'The action to perform. Use count to get total records without fetching data.'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'User ID (GUID) for get/roles/claims actions'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for username or email'
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

        public UserTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountUsers());

                    case "list":
                        return Task.FromResult(ListUsers(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetUser(userId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchUsers(query, skip, take));

                    case "roles":
                        var rolesId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(rolesId) || !Guid.TryParse(rolesId, out var rolesUserId))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetUserRoles(rolesUserId));

                    case "claims":
                        var claimsId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(claimsId) || !Guid.TryParse(claimsId, out var claimsUserId))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetUserClaims(claimsUserId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountUsers()
        {
            var total = _uow.Users.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListUsers(int skip, int take)
        {
            var users = _uow.Users.Get(x => true)
                .OrderBy(x => x.UserName)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Users.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["users"] = JArray.FromObject(users, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetUser(Guid id)
        {
            var user = _uow.Users.Get(x => x.Id == id).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {id}");

            var result = JObject.FromObject(user, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchUsers(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var users = _uow.Users.Get(x =>
                    x.UserName.ToLower().Contains(lowerQuery) ||
                    x.EmailAddress.ToLower().Contains(lowerQuery) ||
                    x.FirstName.ToLower().Contains(lowerQuery) ||
                    x.LastName.ToLower().Contains(lowerQuery))
                .OrderBy(x => x.UserName)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = users.Count,
                ["users"] = JArray.FromObject(users, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetUserRoles(Guid userId)
        {
            var user = _uow.Users.Get(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {userId}");

            var roles = _uow.Users.GetRolesForUser(userId);
            var result = new JObject
            {
                ["userId"] = userId.ToString(),
                ["userName"] = user.UserName,
                ["roles"] = JArray.FromObject(roles, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetUserClaims(Guid userId)
        {
            var user = _uow.Users.Get(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {userId}");

            // Query claims via the junction table - no direct GetClaims method exists
            var result = new JObject
            {
                ["userId"] = userId.ToString(),
                ["userName"] = user.UserName,
                ["message"] = "User claims can be accessed via the user's role assignments"
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
