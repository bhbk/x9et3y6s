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
    public class AuthActivityTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "authactivity",
            Description = "Query authentication activity logs. Supports listing recent activity, getting by ID, filtering by user, searching by login type, and counting total records. Use 'count' to get the total number without fetching records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'byuser', 'search'],
                        'description': 'The action to perform. Use count to get total records without fetching data.'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Activity ID (GUID) for get action'
                    },
                    'userId': {
                        'type': 'string',
                        'description': 'User ID (GUID) for byuser action'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for login type'
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

        public AuthActivityTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountActivity());

                    case "list":
                        return Task.FromResult(ListActivity(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var activityId))
                            return Task.FromResult(MCPToolResult.Fail("Valid activity ID is required"));
                        return Task.FromResult(GetActivity(activityId));

                    case "byuser":
                        var userId = parameters["userId"]?.ToString();
                        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetActivityByUser(userGuid, skip, take));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchActivity(query, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountActivity()
        {
            var total = _uow.AuthActivity.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListActivity(int skip, int take)
        {
            var activities = _uow.AuthActivity.Get(x => true)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.AuthActivity.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["activities"] = JArray.FromObject(activities, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetActivity(Guid id)
        {
            var activity = _uow.AuthActivity.Get(x => x.Id == id).FirstOrDefault();
            if (activity == null)
                return MCPToolResult.Fail($"Activity not found: {id}");

            var result = JObject.FromObject(activity, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetActivityByUser(Guid userId, int skip, int take)
        {
            var user = _uow.Users.Get(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {userId}");

            var activities = _uow.AuthActivity.Get(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["userId"] = userId.ToString(),
                ["userName"] = user.UserName,
                ["count"] = activities.Count,
                ["activities"] = JArray.FromObject(activities, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchActivity(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var activities = _uow.AuthActivity.Get(x =>
                    x.LoginType.ToLower().Contains(lowerQuery) ||
                    (x.LocalEndpoint != null && x.LocalEndpoint.ToLower().Contains(lowerQuery)) ||
                    (x.RemoteEndpoint != null && x.RemoteEndpoint.ToLower().Contains(lowerQuery)))
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = activities.Count,
                ["activities"] = JArray.FromObject(activities, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
