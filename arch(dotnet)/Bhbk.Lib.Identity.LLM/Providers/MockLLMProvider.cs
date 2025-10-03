using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class MockLLMProvider : ILLMProvider
    {
        public string ProviderName => "Mock";

        public Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            var lastMessage = messages.LastOrDefault();
            var userContent = lastMessage?.Content ?? string.Empty;

            // Check if tools are available and simulate tool calls based on keywords
            if (tools != null && tools.Any())
            {
                var toolsList = tools.ToList();

                // Simulate tool calls for specific keywords
                if (userContent.ToLower().Contains("user") && toolsList.Any(t => t.Name == "users"))
                {
                    return Task.FromResult(new LLMResponse
                    {
                        Content = null,
                        RequiresToolExecution = true,
                        ToolCalls = new JArray
                        {
                            new JObject
                            {
                                ["id"] = "mock_call_1",
                                ["name"] = "users",
                                ["input"] = new JObject
                                {
                                    ["action"] = "list",
                                    ["take"] = 10
                                }
                            }
                        },
                        InputTokens = 100,
                        OutputTokens = 50,
                        StopReason = "tool_use"
                    });
                }

                if (userContent.ToLower().Contains("audience") && toolsList.Any(t => t.Name == "audiences"))
                {
                    return Task.FromResult(new LLMResponse
                    {
                        Content = null,
                        RequiresToolExecution = true,
                        ToolCalls = new JArray
                        {
                            new JObject
                            {
                                ["id"] = "mock_call_2",
                                ["name"] = "audiences",
                                ["input"] = new JObject
                                {
                                    ["action"] = "list",
                                    ["take"] = 10
                                }
                            }
                        },
                        InputTokens = 100,
                        OutputTokens = 50,
                        StopReason = "tool_use"
                    });
                }
            }

            // Default response without tool calls
            return Task.FromResult(new LLMResponse
            {
                Content = $"[Mock Response] I received your message: \"{userContent}\". This is a simulated response from the Mock LLM Provider. In a real deployment, this would be handled by AWS Bedrock.",
                RequiresToolExecution = false,
                ToolCalls = null,
                InputTokens = 100,
                OutputTokens = 75,
                StopReason = "end_turn"
            });
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await SendMessageAsync(messages, tools, cancellationToken);

            if (response.RequiresToolExecution)
            {
                yield return new LLMStreamChunk
                {
                    Type = "tool_use",
                    Content = response.ToolCalls?.ToString(),
                    IsComplete = true,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens
                };
                yield break;
            }

            // Simulate streaming by yielding chunks
            var words = response.Content.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                await Task.Delay(50, cancellationToken);

                yield return new LLMStreamChunk
                {
                    Type = "content_block_delta",
                    Content = words[i] + (i < words.Length - 1 ? " " : ""),
                    IsComplete = false
                };
            }

            yield return new LLMStreamChunk
            {
                Type = "message_stop",
                Content = null,
                IsComplete = true,
                InputTokens = response.InputTokens,
                OutputTokens = response.OutputTokens
            };
        }
    }
}
