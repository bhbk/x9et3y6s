using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class SchemaTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private readonly MCPSchemaService _schemaService;

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "schema",
            Description = "Get database schema information. Supports listing all entity schemas or getting a specific entity schema. Use this to understand the data model.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['list', 'get'],
                        'description': 'The action to perform'
                    },
                    'entity': {
                        'type': 'string',
                        'description': 'Entity name for get action (e.g., User, Audience, Issuer, Role, Claim)'
                    }
                },
                'required': ['action']
            }")
        };

        public SchemaTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _schemaService = new MCPSchemaService(uow);
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();

            try
            {
                switch (action)
                {
                    case "list":
                        return Task.FromResult(ListSchemas());

                    case "get":
                        var entity = parameters["entity"]?.ToString();
                        if (string.IsNullOrEmpty(entity))
                            return Task.FromResult(MCPToolResult.Fail("Entity name is required"));
                        return Task.FromResult(GetSchema(entity));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListSchemas()
        {
            var schemas = _schemaService.GetAllEntitySchemas();
            var result = new JObject
            {
                ["count"] = schemas.Count,
                ["schemas"] = schemas
            };

            return MCPToolResult.Ok(result);
        }

        private MCPToolResult GetSchema(string entityName)
        {
            var schema = _schemaService.GetEntitySchema(entityName);
            if (schema == null)
                return MCPToolResult.Fail($"Entity schema not found: {entityName}");

            return MCPToolResult.Ok(schema);
        }
    }
}
