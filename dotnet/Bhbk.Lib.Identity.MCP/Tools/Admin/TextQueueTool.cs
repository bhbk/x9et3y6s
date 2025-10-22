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
    public class TextQueueTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "textqueue",
            Description = "Query the text/SMS notification queue.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'search', 'bystatus'], 'description': 'The operation to perform. count returns total queued texts. list returns a paginated list. get returns a single entry by ID. search finds entries by recipient phone number. bystatus filters entries by delivery status.' },
                    'id': { 'type': 'string', 'description': 'Text queue entry ID (GUID) required for the get action.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against recipient phone number.' },
                    'status': { 'type': 'string', 'enum': ['pending', 'cancelled', 'delivered'], 'description': 'Delivery status filter for the bystatus action.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public TextQueueTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountTexts());

                    case "list":
                        return Task.FromResult(ListTexts(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var textId))
                            return Task.FromResult(MCPToolResult.Fail("Valid text queue item ID is required"));
                        return Task.FromResult(GetText(textId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchTexts(query, skip, take));

                    case "bystatus":
                        var status = parameters["status"]?.ToString()?.ToLower();
                        if (string.IsNullOrEmpty(status))
                            return Task.FromResult(MCPToolResult.Fail("Status filter is required"));
                        return Task.FromResult(GetTextsByStatus(status, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountTexts()
        {
            var total = _uow.TextQueue.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListTexts(int skip, int take)
        {
            var texts = _uow.TextQueue.Get(x => true)
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.TextQueue.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["texts"] = JArray.FromObject(texts, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetText(Guid id)
        {
            var text = _uow.TextQueue.Get(x => x.Id == id).FirstOrDefault();
            if (text == null)
                return MCPToolResult.Fail($"Text queue item not found: {id}");

            var result = JObject.FromObject(text, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchTexts(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var texts = _uow.TextQueue.Get(x =>
                    x.ToPhoneNumber.ToLower().Contains(lowerQuery) ||
                    (x.FromPhoneNumber != null && x.FromPhoneNumber.ToLower().Contains(lowerQuery)) ||
                    (x.Body != null && x.Body.ToLower().Contains(lowerQuery)))
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = texts.Count,
                ["texts"] = JArray.FromObject(texts, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetTextsByStatus(string status, int skip, int take)
        {
            var texts = status switch
            {
                "pending" => _uow.TextQueue.Get(x => !x.IsCancelled && x.Delivered == null),
                "cancelled" => _uow.TextQueue.Get(x => x.IsCancelled),
                "delivered" => _uow.TextQueue.Get(x => x.Delivered != null && !x.IsCancelled),
                _ => null
            };

            if (texts == null)
                return MCPToolResult.Fail($"Unknown status: {status}. Use pending, cancelled, or delivered.");

            var total = texts.Count();
            var paged = texts
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["status"] = status,
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["texts"] = JArray.FromObject(paged, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
