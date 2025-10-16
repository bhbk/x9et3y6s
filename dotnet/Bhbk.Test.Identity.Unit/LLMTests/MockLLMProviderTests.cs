using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.LLM.Providers;
using Bhbk.Lib.Identity.MCP.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Test.Identity.Unit.LLMTests
{
    [Collection("LLMTests")]
    public class MockLLMProviderTests
    {
        [Fact]
        public async Task SendMessageAsync_ReturnsResponse()
        {
            var provider = new MockLLMProvider();
            var messages = new List<LLMMessage>
            {
                new LLMMessage { Role = "user", Content = "Hello" }
            };

            var response = await provider.SendMessageAsync(messages, null);

            response.Should().NotBeNull();
            response.Content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendMessageAsync_WithToolDefinitions_ReturnsResponse()
        {
            var provider = new MockLLMProvider();
            var messages = new List<LLMMessage>
            {
                new LLMMessage { Role = "user", Content = "List all users" }
            };
            var tools = new List<MCPToolDefinition>
            {
                new MCPToolDefinition
                {
                    Name = "user",
                    Description = "User management tool",
                    Scope = MCPScope.Admin
                }
            };

            var response = await provider.SendMessageAsync(messages, tools);

            response.Should().NotBeNull();
            // Mock provider should return some response
            (response.ToolCalls != null || response.Content != null).Should().BeTrue();
        }

        [Fact]
        public async Task StreamMessageAsync_ReturnsChunks()
        {
            var provider = new MockLLMProvider();
            var messages = new List<LLMMessage>
            {
                new LLMMessage { Role = "user", Content = "Tell me about identity" }
            };

            var chunks = new List<LLMStreamChunk>();
            await foreach (var chunk in provider.StreamMessageAsync(messages, null))
            {
                chunks.Add(chunk);
            }

            chunks.Should().NotBeEmpty();
            chunks.Should().Contain(c => c.Type == "content_block_delta");
            chunks.Should().Contain(c => c.IsComplete);
        }

        [Fact]
        public async Task StreamMessageAsync_EmitsCompleteAtEnd()
        {
            var provider = new MockLLMProvider();
            var messages = new List<LLMMessage>
            {
                new LLMMessage { Role = "user", Content = "Hello" }
            };

            var chunks = new List<LLMStreamChunk>();
            await foreach (var chunk in provider.StreamMessageAsync(messages, null))
            {
                chunks.Add(chunk);
            }

            var lastChunk = chunks.Last();
            lastChunk.IsComplete.Should().BeTrue();
        }

        [Fact]
        public async Task SendMessageAsync_TracksTokenUsage()
        {
            var provider = new MockLLMProvider();
            var messages = new List<LLMMessage>
            {
                new LLMMessage { Role = "user", Content = "Hello world" }
            };

            var response = await provider.SendMessageAsync(messages, null);

            response.InputTokens.Should().BeGreaterOrEqualTo(0);
            response.OutputTokens.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void ProviderName_ReturnsMock()
        {
            var provider = new MockLLMProvider();

            provider.ProviderName.Should().Be("Mock");
        }
    }
}
