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
    public class SessionTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private readonly Guid _userId;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "sessions",
            Description = "View your active sessions, refresh tokens, and authentication activity. This tool only accesses your own data.",
            Scope = MCPScope.User,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['refreshes', 'activity'],
                        'description': 'The action to perform'
                    },
                    'skip': {
                        'type': 'integer',
                        'description': 'Number of records to skip (default: 0)'
                    },
                    'take': {
                        'type': 'integer',
                        'description': 'Number of records to take (default: 20, max: 50)'
                    }
                },
                'required': ['action']
            }")
        };

        public SessionTool(IUnitOfWork uow, Guid userId)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userId = userId;
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 20, 50);

            try
            {
                switch (action)
                {
                    case "refreshes":
                        return Task.FromResult(GetRefreshTokens(skip, take));

                    case "activity":
                        return Task.FromResult(GetActivity(skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult GetRefreshTokens(int skip, int take)
        {
            var refreshes = _uow.Refreshes.Get(x => x.UserId == _userId)
                .OrderByDescending(x => x.IssuedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Refreshes.Get(x => x.UserId == _userId).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["refreshTokens"] = JArray.FromObject(refreshes, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetActivity(int skip, int take)
        {
            var activities = _uow.AuthActivity.Get(x => x.UserId == _userId)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.AuthActivity.Get(x => x.UserId == _userId).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["activities"] = JArray.FromObject(activities, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
