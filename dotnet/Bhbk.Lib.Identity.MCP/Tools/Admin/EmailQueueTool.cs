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
    public class EmailQueueTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "emailqueue",
            Description = "Query the email notification queue.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['count', 'list', 'get', 'search', 'bystatus'], 'description': 'The operation to perform. count returns total queued emails. list returns a paginated list. get returns a single entry by ID. search finds entries by recipient or subject. bystatus filters entries by delivery status.' },
                    'id': { 'type': 'string', 'description': 'Email queue entry ID (GUID) required for the get action.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against recipient address and subject.' },
                    'status': { 'type': 'string', 'enum': ['pending', 'cancelled', 'delivered'], 'description': 'Delivery status filter for the bystatus action.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public EmailQueueTool(IUnitOfWork uow)
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
                        return Task.FromResult(CountEmails());

                    case "list":
                        return Task.FromResult(ListEmails(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var emailId))
                            return Task.FromResult(MCPToolResult.Fail("Valid email queue item ID is required"));
                        return Task.FromResult(GetEmail(emailId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchEmails(query, skip, take));

                    case "bystatus":
                        var status = parameters["status"]?.ToString()?.ToLower();
                        if (string.IsNullOrEmpty(status))
                            return Task.FromResult(MCPToolResult.Fail("Status filter is required"));
                        return Task.FromResult(GetEmailsByStatus(status, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult CountEmails()
        {
            var total = _uow.EmailQueue.Get(x => true).Count();
            return MCPToolResult.Ok(new JObject { ["total"] = total });
        }

        private MCPToolResult ListEmails(int skip, int take)
        {
            var emails = _uow.EmailQueue.Get(x => true)
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.EmailQueue.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["emails"] = JArray.FromObject(emails, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetEmail(Guid id)
        {
            var email = _uow.EmailQueue.Get(x => x.Id == id).FirstOrDefault();
            if (email == null)
                return MCPToolResult.Fail($"Email queue item not found: {id}");

            var result = JObject.FromObject(email, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchEmails(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var emails = _uow.EmailQueue.Get(x =>
                    x.ToEmail.ToLower().Contains(lowerQuery) ||
                    (x.ToDisplay != null && x.ToDisplay.ToLower().Contains(lowerQuery)) ||
                    (x.FromEmail != null && x.FromEmail.ToLower().Contains(lowerQuery)) ||
                    (x.Subject != null && x.Subject.ToLower().Contains(lowerQuery)))
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = emails.Count,
                ["emails"] = JArray.FromObject(emails, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetEmailsByStatus(string status, int skip, int take)
        {
            var emails = status switch
            {
                "pending" => _uow.EmailQueue.Get(x => !x.IsCancelled && x.Delivered == null),
                "cancelled" => _uow.EmailQueue.Get(x => x.IsCancelled),
                "delivered" => _uow.EmailQueue.Get(x => x.Delivered != null && !x.IsCancelled),
                _ => null
            };

            if (emails == null)
                return MCPToolResult.Fail($"Unknown status: {status}. Use pending, cancelled, or delivered.");

            var total = emails.Count();
            var paged = emails
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
                ["emails"] = JArray.FromObject(paged, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
