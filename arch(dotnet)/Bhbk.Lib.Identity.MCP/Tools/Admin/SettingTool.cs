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
    public class SettingTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "settings",
            Description = "Query user settings. Supports listing all settings, getting by ID, filtering by user, searching by key or value, and counting total records. Use 'count' to get the total number without fetching records.",
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
                        'description': 'Setting ID (GUID) for get action'
                    },
                    'userId': {
                        'type': 'string',
                        'description': 'User ID (GUID) for byuser action'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for key or value'
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

        public SettingTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountSettings());

                    case "list":
                        return Task.FromResult(ListSettings(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var settingId))
                            return Task.FromResult(MCPToolResult.Fail("Valid setting ID is required"));
                        return Task.FromResult(GetSetting(settingId));

                    case "byuser":
                        var userId = parameters["userId"]?.ToString();
                        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetSettingsByUser(userGuid, skip, take));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchSettings(query, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountSettings()
        {
            var total = _uow.Settings.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListSettings(int skip, int take)
        {
            var settings = _uow.Settings.Get(x => true)
                .OrderBy(x => x.ConfigKey)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Settings.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["settings"] = JArray.FromObject(settings, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetSetting(Guid id)
        {
            var setting = _uow.Settings.Get(x => x.Id == id).FirstOrDefault();
            if (setting == null)
                return MCPToolResult.Fail($"Setting not found: {id}");

            var result = JObject.FromObject(setting, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetSettingsByUser(Guid userId, int skip, int take)
        {
            var user = _uow.Users.Get(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {userId}");

            var settings = _uow.Settings.Get(x => x.UserId == userId)
                .OrderBy(x => x.ConfigKey)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["userId"] = userId.ToString(),
                ["userName"] = user.UserName,
                ["count"] = settings.Count,
                ["settings"] = JArray.FromObject(settings, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchSettings(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var settings = _uow.Settings.Get(x =>
                    x.ConfigKey.ToLower().Contains(lowerQuery) ||
                    (x.ConfigValue != null && x.ConfigValue.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.ConfigKey)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = settings.Count,
                ["settings"] = JArray.FromObject(settings, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
