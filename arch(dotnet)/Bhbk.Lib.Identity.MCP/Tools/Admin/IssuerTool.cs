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
    public class IssuerTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "issuers",
            Description = "Query and manage issuers (token providers). Supports listing, getting by ID, searching, viewing associated audiences, and counting total records. Use 'count' to get the total number without fetching records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'search', 'audiences'],
                        'description': 'The action to perform. Use count to get total records without fetching data.'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Issuer ID (GUID) for get/audiences actions'
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

        public IssuerTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountIssuers());

                    case "list":
                        return Task.FromResult(ListIssuers(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var issuerId))
                            return Task.FromResult(MCPToolResult.Fail("Valid issuer ID is required"));
                        return Task.FromResult(GetIssuer(issuerId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchIssuers(query, skip, take));

                    case "audiences":
                        var audId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(audId) || !Guid.TryParse(audId, out var audIssuerId))
                            return Task.FromResult(MCPToolResult.Fail("Valid issuer ID is required"));
                        return Task.FromResult(GetIssuerAudiences(audIssuerId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountIssuers()
        {
            var total = _uow.Issuers.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListIssuers(int skip, int take)
        {
            var issuers = _uow.Issuers.Get(x => true)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Issuers.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["issuers"] = JArray.FromObject(issuers, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetIssuer(Guid id)
        {
            var issuer = _uow.Issuers.Get(x => x.Id == id).FirstOrDefault();
            if (issuer == null)
                return MCPToolResult.Fail($"Issuer not found: {id}");

            var result = JObject.FromObject(issuer, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchIssuers(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var issuers = _uow.Issuers.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = issuers.Count,
                ["issuers"] = JArray.FromObject(issuers, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetIssuerAudiences(Guid issuerId)
        {
            var issuer = _uow.Issuers.Get(x => x.Id == issuerId).FirstOrDefault();
            if (issuer == null)
                return MCPToolResult.Fail($"Issuer not found: {issuerId}");

            var audiences = _uow.Audiences.Get(x => x.IssuerId == issuerId).ToList();
            var result = new JObject
            {
                ["issuerId"] = issuerId.ToString(),
                ["issuerName"] = issuer.Name,
                ["audiences"] = JArray.FromObject(audiences, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
