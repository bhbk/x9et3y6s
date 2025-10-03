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
    public class QuoteTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "quotes",
            Description = "Query quote entries. Supports listing all quotes, getting by ID, and searching by quote text or author.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['list', 'get', 'search', 'current', 'count'],
                        'description': 'The action to perform: list=all quotes with total count, get=specific quote by id, search=find by quote or author, current=most recent quote, count=total number of quotes'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Quote ID (GUID) for get action'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for quote or author'
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

        public QuoteTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            if (string.IsNullOrEmpty(action))
                action = "list";
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 50, 100);

            try
            {
                switch (action)
                {
                    case "list":
                        return Task.FromResult(ListQuotes(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var quoteId))
                            return Task.FromResult(MCPToolResult.Fail("Valid quote ID is required"));
                        return Task.FromResult(GetQuote(quoteId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchQuotes(query, skip, take));

                    case "current":
                        return Task.FromResult(GetCurrentQuote());

                    case "count":
                        return Task.FromResult(CountQuotes());

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}. Valid actions are: list, get, search, current, count"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListQuotes(int skip, int take)
        {
            var quotes = _uow.Quotes.Get(x => true)
                .OrderByDescending(x => x.TssDate)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Quotes.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["quotes"] = JArray.FromObject(quotes, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetQuote(Guid id)
        {
            var quote = _uow.Quotes.Get(x => x.Id == id).FirstOrDefault();
            if (quote == null)
                return MCPToolResult.Fail($"Quote not found: {id}");

            var result = JObject.FromObject(quote, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchQuotes(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var quotes = _uow.Quotes.Get(x =>
                    x.Quote.ToLower().Contains(lowerQuery) ||
                    (x.Author != null && x.Author.ToLower().Contains(lowerQuery)))
                .OrderByDescending(x => x.TssDate)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = quotes.Count,
                ["quotes"] = JArray.FromObject(quotes, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult CountQuotes()
        {
            var total = _uow.Quotes.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult GetCurrentQuote()
        {
            // Get the most recent quote by date
            var quote = _uow.Quotes.Get(x => true)
                .OrderByDescending(x => x.TssDate)
                .FirstOrDefault();

            if (quote == null)
                return MCPToolResult.Fail("No quote found");

            var result = JObject.FromObject(quote, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
