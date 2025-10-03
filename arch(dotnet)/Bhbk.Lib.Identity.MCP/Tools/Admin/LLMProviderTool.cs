using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bhbk.Lib.Identity.Data.EF.Models;

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class LLMProviderTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "llmproviders",
            Description = "Query LLM providers and their settings. Supports listing all providers with failover order, getting by ID with settings, searching by name, and counting total records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'search', 'settings'],
                        'description': 'The action to perform. list returns providers ordered by failover priority. settings returns config key-value pairs for a provider (secret values are masked).'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Provider ID (GUID) for get/settings actions'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for provider name'
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

        public LLMProviderTool(IUnitOfWork uow)
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
                        var total = _uow.LLMProviders.Get(x => true).Count();
                        return Task.FromResult(MCPToolResult.Ok(new JObject { ["total"] = total }));

                    case "list":
                        return Task.FromResult(ListProviders(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var providerId))
                            return Task.FromResult(MCPToolResult.Fail("Valid provider ID is required"));
                        return Task.FromResult(GetProvider(providerId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchProviders(query, skip, take));

                    case "settings":
                        var settingsId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(settingsId) || !Guid.TryParse(settingsId, out var settingsProviderId))
                            return Task.FromResult(MCPToolResult.Fail("Valid provider ID is required"));
                        return Task.FromResult(GetSettings(settingsProviderId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListProviders(int skip, int take)
        {
            var providers = _uow.LLMProviders.Get(x => true)
                .OrderBy(x => x.FailoverOrder)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.LLMProviders.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["providers"] = JArray.FromObject(providers, _serializer)
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetProvider(Guid id)
        {
            var provider = _uow.LLMProviders.Get(x => x.Id == id).FirstOrDefault();
            if (provider == null)
                return MCPToolResult.Fail($"LLM provider not found: {id}");

            var result = JObject.FromObject(provider, _serializer);
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchProviders(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var providers = _uow.LLMProviders.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery))
                .OrderBy(x => x.FailoverOrder)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = providers.Count,
                ["providers"] = JArray.FromObject(providers, _serializer)
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetSettings(Guid providerId)
        {
            var provider = _uow.LLMProviders.Get(x => x.Id == providerId).FirstOrDefault();
            if (provider == null)
                return MCPToolResult.Fail($"LLM provider not found: {providerId}");

            var settings = _uow.LLMProviderSettings.Get(x => x.ProviderId == providerId)
                .OrderBy(x => x.ConfigKey)
                .ToList();

            var settingsArray = new JArray();
            foreach (var s in settings)
            {
                settingsArray.Add(new JObject
                {
                    ["id"] = s.Id.ToString(),
                    ["configKey"] = s.ConfigKey,
                    ["configValue"] = s.IsSecret ? "********" : s.ConfigValue,
                    ["isSecret"] = s.IsSecret,
                });
            }

            var result = new JObject
            {
                ["providerId"] = providerId.ToString(),
                ["providerName"] = provider.Name,
                ["settings"] = settingsArray
            };

            return MCPToolResult.Ok(result);
        }
    }
}
