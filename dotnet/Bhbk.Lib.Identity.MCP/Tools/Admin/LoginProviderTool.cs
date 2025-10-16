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
    public class LoginProviderTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "login-providers",
            Description = "Query external login providers.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'search'], 'description': 'The operation to perform. count returns total login providers. list returns a paginated list. get returns a single provider by ID. search finds providers by name or description.' },
                    'id': { 'type': 'string', 'description': 'Login provider ID (GUID) required for the get action.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against provider name and description.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public LoginProviderTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountLoginProviders());

                    case "list":
                        return Task.FromResult(ListLoginProviders(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var loginProviderId))
                            return Task.FromResult(MCPToolResult.Fail("Valid login provider ID is required"));
                        return Task.FromResult(GetLoginProvider(loginProviderId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchLoginProviders(query, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountLoginProviders()
        {
            var total = _uow.LoginProviders.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListLoginProviders(int skip, int take)
        {
            var loginProviders = _uow.LoginProviders.Get(x => true)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.LoginProviders.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["loginProviders"] = JArray.FromObject(loginProviders, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetLoginProvider(Guid id)
        {
            var loginProvider = _uow.LoginProviders.Get(x => x.Id == id).FirstOrDefault();
            if (loginProvider == null)
                return MCPToolResult.Fail($"Login provider not found: {id}");

            var result = JObject.FromObject(loginProvider, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchLoginProviders(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var loginProviders = _uow.LoginProviders.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = loginProviders.Count,
                ["loginProviders"] = JArray.FromObject(loginProviders, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
