using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Test.Identity.Unit.MCPTests
{
    [Collection("MCPTests")]
    public class MCPToolRegistryTests
    {
        [Fact]
        public void Register_AddsToolToRegistry()
        {
            var registry = new MCPToolRegistry();
            var tool = new MockTool("test-tool", MCPScope.Admin);

            registry.Register(tool);

            registry.GetTools().Should().ContainSingle();
        }

        [Fact]
        public void GetTool_ReturnsTool_WhenExists()
        {
            var registry = new MCPToolRegistry();
            var tool = new MockTool("test-tool", MCPScope.Admin);
            registry.Register(tool);

            var result = registry.GetTool("test-tool");

            result.Should().NotBeNull();
            result.Definition.Name.Should().Be("test-tool");
        }

        [Fact]
        public void GetTool_ReturnsNull_WhenNotExists()
        {
            var registry = new MCPToolRegistry();

            var result = registry.GetTool("non-existent");

            result.Should().BeNull();
        }

        [Fact]
        public void GetToolsByScope_ReturnsToolsForScope()
        {
            var registry = new MCPToolRegistry();
            var adminTool = new MockTool("admin-tool", MCPScope.Admin);
            var userTool = new MockTool("user-tool", MCPScope.User);
            registry.Register(adminTool);
            registry.Register(userTool);

            var adminTools = registry.GetToolsByScope(MCPScope.Admin);
            var userTools = registry.GetToolsByScope(MCPScope.User);

            adminTools.Should().HaveCount(1);
            adminTools.Should().Contain(t => t.Definition.Name == "admin-tool");
            userTools.Should().HaveCount(1);
            userTools.Should().Contain(t => t.Definition.Name == "user-tool");
        }

        [Fact]
        public void GetTools_ReturnsAllRegisteredTools()
        {
            var registry = new MCPToolRegistry();
            registry.Register(new MockTool("tool1", MCPScope.Admin));
            registry.Register(new MockTool("tool2", MCPScope.User));
            registry.Register(new MockTool("tool3", MCPScope.Admin));

            var all = registry.GetTools();

            all.Should().HaveCount(3);
        }

        [Fact]
        public void GetDefinitions_ReturnsAllDefinitions()
        {
            var registry = new MCPToolRegistry();
            registry.Register(new MockTool("tool1", MCPScope.Admin));
            registry.Register(new MockTool("tool2", MCPScope.User));

            var definitions = registry.GetDefinitions();

            definitions.Should().HaveCount(2);
            definitions.Should().Contain(d => d.Name == "tool1");
            definitions.Should().Contain(d => d.Name == "tool2");
        }

        [Fact]
        public void GetDefinitionsByScope_FiltersCorrectly()
        {
            var registry = new MCPToolRegistry();
            registry.Register(new MockTool("admin-tool", MCPScope.Admin));
            registry.Register(new MockTool("user-tool", MCPScope.User));

            var adminDefs = registry.GetDefinitionsByScope(MCPScope.Admin);
            var userDefs = registry.GetDefinitionsByScope(MCPScope.User);

            adminDefs.Should().HaveCount(1);
            adminDefs.Should().Contain(d => d.Name == "admin-tool");
            userDefs.Should().HaveCount(1);
            userDefs.Should().Contain(d => d.Name == "user-tool");
        }

        [Fact]
        public void GetTool_IsCaseInsensitive()
        {
            var registry = new MCPToolRegistry();
            registry.Register(new MockTool("MyTool", MCPScope.Admin));

            registry.GetTool("mytool").Should().NotBeNull();
            registry.GetTool("MYTOOL").Should().NotBeNull();
            registry.GetTool("MyTool").Should().NotBeNull();
        }

        [Fact]
        public void Register_OverwritesExistingTool()
        {
            var registry = new MCPToolRegistry();
            var tool1 = new MockTool("test-tool", MCPScope.Admin);
            var tool2 = new MockTool("test-tool", MCPScope.User);

            registry.Register(tool1);
            registry.Register(tool2);

            registry.GetTools().Should().HaveCount(1);
            registry.GetTool("test-tool").Definition.Scope.Should().Be(MCPScope.User);
        }

        private class MockTool : IMCPTool
        {
            public MCPToolDefinition Definition { get; }

            public MockTool(string name, MCPScope scope)
            {
                Definition = new MCPToolDefinition
                {
                    Name = name,
                    Description = $"Mock tool: {name}",
                    Scope = scope,
                    InputSchema = new JObject()
                };
            }

            public Task<MCPToolResult> ExecuteAsync(JObject parameters)
            {
                return Task.FromResult(MCPToolResult.Ok(new JObject { ["message"] = "Mock execution" }));
            }
        }
    }
}
