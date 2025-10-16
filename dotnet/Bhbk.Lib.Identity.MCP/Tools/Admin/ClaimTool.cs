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
    public class ClaimTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "claims",
            Description = "Query claims (user attributes).",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'search'], 'description': 'The operation to perform. count returns total claims. list returns a paginated list. get returns a single claim by ID. search finds claims by type or value.' },
                    'id': { 'type': 'string', 'description': 'Claim ID (GUID) required for the get action.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against claim type and value.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public ClaimTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountClaims());

                    case "list":
                        return Task.FromResult(ListClaims(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var claimId))
                            return Task.FromResult(MCPToolResult.Fail("Valid claim ID is required"));
                        return Task.FromResult(GetClaim(claimId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchClaims(query, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountClaims()
        {
            var total = _uow.Claims.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListClaims(int skip, int take)
        {
            var claims = _uow.Claims.Get(x => true)
                .OrderBy(x => x.Type)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Claims.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["claims"] = JArray.FromObject(claims, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetClaim(Guid id)
        {
            var claim = _uow.Claims.Get(x => x.Id == id).FirstOrDefault();
            if (claim == null)
                return MCPToolResult.Fail($"Claim not found: {id}");

            var result = JObject.FromObject(claim, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchClaims(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var claims = _uow.Claims.Get(x =>
                    x.Type.ToLower().Contains(lowerQuery) ||
                    (x.Value != null && x.Value.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.Type)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = claims.Count,
                ["claims"] = JArray.FromObject(claims, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
