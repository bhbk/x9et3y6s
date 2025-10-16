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
    public class AudienceTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "audiences",
            Description = "Query audiences (OAuth2 clients) and their roles.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'search', 'roles'], 'description': 'The operation to perform. count returns total audiences. list returns a paginated list. get returns a single audience by ID. search finds audiences by name or description. roles returns roles assigned to an audience.' },
                    'id': { 'type': 'string', 'description': 'Audience ID (GUID) required for get and roles actions.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against audience name and description.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public AudienceTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountAudiences());

                    case "list":
                        return Task.FromResult(ListAudiences(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var audienceId))
                            return Task.FromResult(MCPToolResult.Fail("Valid audience ID is required"));
                        return Task.FromResult(GetAudience(audienceId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchAudiences(query, skip, take));

                    case "roles":
                        var rolesId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(rolesId) || !Guid.TryParse(rolesId, out var rolesAudienceId))
                            return Task.FromResult(MCPToolResult.Fail("Valid audience ID is required"));
                        return Task.FromResult(GetAudienceRoles(rolesAudienceId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountAudiences()
        {
            var total = _uow.Audiences.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListAudiences(int skip, int take)
        {
            var audiences = _uow.Audiences.Get(x => true)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Audiences.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["audiences"] = JArray.FromObject(audiences, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetAudience(Guid id)
        {
            var audience = _uow.Audiences.Get(x => x.Id == id).FirstOrDefault();
            if (audience == null)
                return MCPToolResult.Fail($"Audience not found: {id}");

            var result = JObject.FromObject(audience, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchAudiences(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var audiences = _uow.Audiences.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = audiences.Count,
                ["audiences"] = JArray.FromObject(audiences, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetAudienceRoles(Guid audienceId)
        {
            var audience = _uow.Audiences.Get(x => x.Id == audienceId).FirstOrDefault();
            if (audience == null)
                return MCPToolResult.Fail($"Audience not found: {audienceId}");

            var roles = _uow.Roles.Get(x => x.AudienceId == audienceId).ToList();
            var result = new JObject
            {
                ["audienceId"] = audienceId.ToString(),
                ["audienceName"] = audience.Name,
                ["roles"] = JArray.FromObject(roles, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
