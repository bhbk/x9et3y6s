using Azure;
using Azure.AI.OpenAI;
using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class AzureOpenAILLMProvider : ILLMProvider
    {
        private readonly ChatClient _chatClient;
        private readonly AzureOpenAISettings _settings;

        public string ProviderName => "AzureOpenAI";

        public AzureOpenAILLMProvider(IOptions<LLMProviderSettings> options)
        {
            _settings = options.Value.AzureOpenAI;

            var azureClient = new AzureOpenAIClient(
                new Uri(_settings.Endpoint),
                new ApiKeyCredential(_settings.ApiKey));

            _chatClient = azureClient.GetChatClient(_settings.DeploymentName);
        }

        public async Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            var chatMessages = ConvertMessages(messages);
            var chatOptions = BuildOptions(tools);

            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                chatMessages, chatOptions, cancellationToken);

            return ParseResponse(completion);
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatMessages = ConvertMessages(messages);
            var chatOptions = BuildOptions(tools);

            // Manually accumulate tool call fragments across streaming updates.
            // Each update may carry a partial tool call identified by Index.
            var toolCallAccumulator = new Dictionary<int, (string Id, string Name, StringBuilder Args)>();
            int? inputTokens = null;
            int? outputTokens = null;

            AsyncCollectionResult<StreamingChatCompletionUpdate> updates =
                _chatClient.CompleteChatStreamingAsync(chatMessages, chatOptions, cancellationToken);

            await foreach (var update in updates)
            {
                // Accumulate content deltas
                foreach (var contentPart in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(contentPart.Text))
                    {
                        yield return new LLMStreamChunk
                        {
                            Type = "content_block_delta",
                            Content = contentPart.Text,
                            IsComplete = false
                        };
                    }
                }

                // Accumulate tool call deltas
                foreach (var toolCallUpdate in update.ToolCallUpdates)
                {
                    var idx = toolCallUpdate.Index;

                    if (!toolCallAccumulator.ContainsKey(idx))
                    {
                        toolCallAccumulator[idx] = (
                            toolCallUpdate.ToolCallId ?? Guid.NewGuid().ToString(),
                            toolCallUpdate.FunctionName ?? string.Empty,
                            new StringBuilder());
                    }

                    var entry = toolCallAccumulator[idx];

                    if (!string.IsNullOrEmpty(toolCallUpdate.ToolCallId))
                        entry.Id = toolCallUpdate.ToolCallId;

                    if (!string.IsNullOrEmpty(toolCallUpdate.FunctionName))
                        entry.Name = toolCallUpdate.FunctionName;

                    if (toolCallUpdate.FunctionArgumentsUpdate != null)
                        entry.Args.Append(toolCallUpdate.FunctionArgumentsUpdate.ToString());

                    toolCallAccumulator[idx] = entry;
                }

                // Capture usage
                if (update.Usage != null)
                {
                    inputTokens = update.Usage.InputTokenCount;
                    outputTokens = update.Usage.OutputTokenCount;
                }
            }

            if (toolCallAccumulator.Count > 0)
            {
                var toolCallsArray = new JArray();
                foreach (var entry in toolCallAccumulator.Values)
                {
                    JObject input;
                    try
                    {
                        input = JObject.Parse(entry.Args.ToString());
                    }
                    catch
                    {
                        input = new JObject();
                    }

                    toolCallsArray.Add(new JObject
                    {
                        ["id"] = entry.Id,
                        ["name"] = entry.Name,
                        ["input"] = input
                    });
                }

                yield return new LLMStreamChunk
                {
                    Type = "tool_use",
                    Content = toolCallsArray.ToString(),
                    IsComplete = true,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens
                };
                yield break;
            }

            yield return new LLMStreamChunk
            {
                Type = "message_stop",
                Content = null,
                IsComplete = true,
                InputTokens = inputTokens,
                OutputTokens = outputTokens
            };
        }

        private IList<ChatMessage> ConvertMessages(IEnumerable<LLMMessage> messages)
        {
            var result = new List<ChatMessage>();

            foreach (var m in messages)
            {
                if (m.Role == "system")
                {
                    result.Add(new SystemChatMessage(m.Content));
                }
                else if (m.Role == "tool" && m.ToolResults != null)
                {
                    foreach (var tr in m.ToolResults)
                    {
                        result.Add(new ToolChatMessage(
                            tr["tool_use_id"]?.ToString(),
                            tr["content"]?.ToString()));
                    }
                }
                else if (m.Role == "assistant")
                {
                    if (m.ToolCalls != null && m.ToolCalls.Count > 0)
                    {
                        var toolCalls = new List<ChatToolCall>();
                        foreach (var tc in m.ToolCalls)
                        {
                            toolCalls.Add(ChatToolCall.CreateFunctionToolCall(
                                tc["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                                tc["name"]?.ToString(),
                                BinaryData.FromString(tc["input"]?.ToString() ?? "{}")));
                        }

                        var assistantMsg = new AssistantChatMessage(toolCalls);
                        if (!string.IsNullOrEmpty(m.Content))
                        {
                            assistantMsg.Content.Add(ChatMessageContentPart.CreateTextPart(m.Content));
                        }
                        result.Add(assistantMsg);
                    }
                    else
                    {
                        result.Add(new AssistantChatMessage(m.Content ?? string.Empty));
                    }
                }
                else
                {
                    result.Add(new UserChatMessage(m.Content ?? string.Empty));
                }
            }

            return result;
        }

        private ChatCompletionOptions BuildOptions(IEnumerable<MCPToolDefinition> tools)
        {
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _settings.MaxTokens,
                Temperature = _settings.Temperature
            };

            if (tools != null)
            {
                foreach (var tool in tools)
                {
                    options.Tools.Add(ChatTool.CreateFunctionTool(
                        tool.Name,
                        tool.Description,
                        BinaryData.FromString(tool.InputSchema.ToString())));
                }
            }

            return options;
        }

        private LLMResponse ParseResponse(ChatCompletion completion)
        {
            var result = new LLMResponse
            {
                InputTokens = completion.Usage?.InputTokenCount ?? 0,
                OutputTokens = completion.Usage?.OutputTokenCount ?? 0,
                StopReason = completion.FinishReason.ToString()
            };

            // Extract text content
            foreach (var part in completion.Content)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    result.Content = part.Text;
                }
            }

            // Extract tool calls
            if (completion.ToolCalls != null && completion.ToolCalls.Count > 0)
            {
                result.ToolCalls = ConvertToolCallsToJArray(completion.ToolCalls);
                result.RequiresToolExecution = true;
            }

            return result;
        }

        private JArray ConvertToolCallsToJArray(IReadOnlyList<ChatToolCall> toolCalls)
        {
            var array = new JArray();

            foreach (var tc in toolCalls)
            {
                JObject input;
                try
                {
                    input = JObject.Parse(tc.FunctionArguments?.ToString() ?? "{}");
                }
                catch
                {
                    input = new JObject();
                }

                array.Add(new JObject
                {
                    ["id"] = tc.Id,
                    ["name"] = tc.FunctionName,
                    ["input"] = input
                });
            }

            return array;
        }
    }
}
