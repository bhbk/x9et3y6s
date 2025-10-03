using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class AWSBedrockLLMProvider : ILLMProvider
    {
        private readonly AmazonBedrockRuntimeClient _client;
        private readonly AWSBedrockSettings _settings;

        public string ProviderName => "AWSBedrock";

        public AWSBedrockLLMProvider(IOptions<LLMProviderSettings> options)
        {
            _settings = options.Value.AWSBedrock;

            var region = RegionEndpoint.GetBySystemName(_settings.Region);
            _client = new AmazonBedrockRuntimeClient(region);
        }

        public async Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(messages, tools);
            var response = await _client.ConverseAsync(request, cancellationToken);

            return ParseResponse(response);
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // For now, use non-streaming and yield chunks
            // TODO: Implement proper streaming when SDK API is better understood
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
            if (!string.IsNullOrEmpty(response.Content))
            {
                var words = response.Content.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    yield return new LLMStreamChunk
                    {
                        Type = "content_block_delta",
                        Content = words[i] + (i < words.Length - 1 ? " " : ""),
                        IsComplete = false
                    };
                }
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

        private ConverseRequest BuildRequest(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools)
        {
            var request = new ConverseRequest
            {
                ModelId = _settings.ModelName,
                Messages = messages.Where(m => m.Role != "system").Select(ConvertMessage).ToList(),
                InferenceConfig = new InferenceConfiguration
                {
                    MaxTokens = _settings.MaxTokens
                }
            };

            // Add system message if present
            var systemMessage = messages.FirstOrDefault(m => m.Role == "system");
            if (systemMessage != null)
            {
                request.System = new List<SystemContentBlock>
                {
                    new SystemContentBlock { Text = systemMessage.Content }
                };
            }

            // Add tools if present
            if (tools != null && tools.Any())
            {
                request.ToolConfig = new ToolConfiguration
                {
                    Tools = tools.Select(ConvertTool).ToList()
                };
            }

            return request;
        }

        private Message ConvertMessage(LLMMessage message)
        {
            var role = message.Role == "assistant" ? ConversationRole.Assistant : ConversationRole.User;

            var content = new List<ContentBlock>();

            if (!string.IsNullOrEmpty(message.Content))
            {
                content.Add(new ContentBlock { Text = message.Content });
            }

            if (message.ToolCalls != null)
            {
                foreach (var tc in message.ToolCalls)
                {
                    var inputObj = tc["input"] as JObject ?? new JObject();
                    var inputDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(inputObj.ToString());
                    var inputDoc = Amazon.Runtime.Documents.Document.FromObject(inputDict);

                    content.Add(new ContentBlock
                    {
                        ToolUse = new ToolUseBlock
                        {
                            ToolUseId = tc["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Name = tc["name"]?.ToString(),
                            Input = inputDoc
                        }
                    });
                }
            }

            if (message.ToolResults != null)
            {
                foreach (var result in message.ToolResults)
                {
                    content.Add(new ContentBlock
                    {
                        ToolResult = new ToolResultBlock
                        {
                            ToolUseId = result["tool_use_id"]?.ToString(),
                            Content = new List<ToolResultContentBlock>
                            {
                                new ToolResultContentBlock
                                {
                                    Text = result["content"]?.ToString()
                                }
                            }
                        }
                    });
                }
            }

            return new Message
            {
                Role = role,
                Content = content
            };
        }

        private Tool ConvertTool(MCPToolDefinition tool)
        {
            var schemaDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(tool.InputSchema.ToString());
            var doc = Amazon.Runtime.Documents.Document.FromObject(schemaDict);

            return new Tool
            {
                ToolSpec = new ToolSpecification
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    InputSchema = new ToolInputSchema
                    {
                        Json = doc
                    }
                }
            };
        }

        private LLMResponse ParseResponse(ConverseResponse response)
        {
            var result = new LLMResponse
            {
                InputTokens = (int)(response.Usage?.InputTokens ?? 0),
                OutputTokens = (int)(response.Usage?.OutputTokens ?? 0),
                StopReason = response.StopReason?.Value
            };

            var toolCalls = new JArray();

            foreach (var block in response.Output?.Message?.Content ?? new List<ContentBlock>())
            {
                if (!string.IsNullOrEmpty(block.Text))
                {
                    result.Content = block.Text;
                }

                if (block.ToolUse != null)
                {
                    var input = new JObject();

                    // Convert Document to JObject
                    try
                    {
                        var json = JsonConvert.SerializeObject(block.ToolUse.Input);
                        if (!string.IsNullOrEmpty(json) && json != "null")
                        {
                            input = JObject.Parse(json);
                        }
                    }
                    catch
                    {
                        // If conversion fails, use empty object
                    }

                    toolCalls.Add(new JObject
                    {
                        ["id"] = block.ToolUse.ToolUseId,
                        ["name"] = block.ToolUse.Name,
                        ["input"] = input
                    });
                }
            }

            if (toolCalls.Count > 0)
            {
                result.ToolCalls = toolCalls;
                result.RequiresToolExecution = true;
            }

            return result;
        }
    }
}
