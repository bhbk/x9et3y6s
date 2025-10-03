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
    public class UserQuoteTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "quotes",
            Description = "Get quote entries. Use 'current' for the most recent quote, or 'get' with an id to retrieve a specific quote by its GUID.",
            Scope = MCPScope.User,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['current', 'get'],
                        'description': 'The action to perform: current=most recent quote, get=specific quote by id'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Quote ID (GUID) — required for the get action'
                    }
                },
                'required': ['action']
            }")
        };

        public UserQuoteTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            if (string.IsNullOrEmpty(action))
                action = "current";

            try
            {
                switch (action)
                {
                    case "current":
                        return Task.FromResult(GetCurrentQuote());

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var quoteId))
                            return Task.FromResult(MCPToolResult.Fail("Valid quote ID is required for the get action"));
                        return Task.FromResult(GetQuote(quoteId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}. Valid actions are: current, get"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult GetCurrentQuote()
        {
            var quote = _uow.Quotes.Get(x => true)
                .OrderByDescending(x => x.TssDate)
                .FirstOrDefault();

            if (quote == null)
                return MCPToolResult.Fail("No quote found");

            var result = JObject.FromObject(quote, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
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
    }
}
