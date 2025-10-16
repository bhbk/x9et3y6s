using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.MCP.Services
{
    public class MCPToolRegistry
    {
        private readonly Dictionary<string, IMCPTool> _tools = new Dictionary<string, IMCPTool>(StringComparer.OrdinalIgnoreCase);

        public void Register(IMCPTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            if (string.IsNullOrWhiteSpace(tool.Definition?.Name))
                throw new ArgumentException("Tool must have a valid name", nameof(tool));

            _tools[tool.Definition.Name] = tool;
        }

        public IMCPTool GetTool(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _tools.TryGetValue(name, out var tool);
            return tool;
        }

        public IReadOnlyList<IMCPTool> GetTools()
        {
            return _tools.Values.ToList().AsReadOnly();
        }

        public IReadOnlyList<IMCPTool> GetToolsByScope(MCPScope scope)
        {
            return _tools.Values
                .Where(t => t.Definition.Scope == scope)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<MCPToolDefinition> GetDefinitions()
        {
            return _tools.Values
                .Select(t => t.Definition)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<MCPToolDefinition> GetDefinitionsByScope(MCPScope scope)
        {
            return _tools.Values
                .Where(t => t.Definition.Scope == scope)
                .Select(t => t.Definition)
                .ToList()
                .AsReadOnly();
        }
    }
}
