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
            Description = "Query authentication activity logs.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'byuser', 'search'], 'description': 'The operation to perform. count returns total activity records. list returns a paginated list sorted by most recent. get returns a single record by ID. byuser returns activity for a specific user. search finds records by login type or audience name.' },
                    'id': { 'type': 'string', 'description': 'Activity record ID (GUID) required for the get action.' },
                    'userId': { 'type': 'string', 'description': 'User ID (GUID) required for the byuser action.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against login type and audience name.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
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
