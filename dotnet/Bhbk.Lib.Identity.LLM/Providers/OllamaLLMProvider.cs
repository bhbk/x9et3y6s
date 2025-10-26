using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class OllamaLLMProvider : ILLMProvider
    {
        private readonly OllamaApiClient _client;
        private readonly OllamaSettings _settings;
        private readonly ILogger<OllamaLLMProvider> _logger;

        public string ProviderName => "Ollama";

        /* DI constructor — ASP.NET Core injects ILogger<T> automatically */
        public OllamaLLMProvider(IOptions<LLMProviderSettings> options, ILogger<OllamaLLMProvider> logger)
        {
            _settings = options.Value.Ollama;
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds)
            };

            if (!string.IsNullOrEmpty(_settings.ApiKey))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            _client = new OllamaApiClient(httpClient);
            _logger = logger ?? NullLogger<OllamaLLMProvider>.Instance;
        }

        /* Test constructor — supply a pre-configured HttpClient (e.g. with a mocked handler) */
        public OllamaLLMProvider(IOptions<LLMProviderSettings> options, HttpClient httpClient)
        {
            _settings = options.Value.Ollama;
            httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");

            if (!string.IsNullOrEmpty(_settings.ApiKey))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            _client = new OllamaApiClient(httpClient);
            _logger = NullLogger<OllamaLLMProvider>.Instance;
        }

        public async Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            var toolList = tools?.ToList();
            var request = new ChatRequest
            {
                Model = _settings.ModelName,
                Messages = ConvertMessages(messages),
                Tools = toolList?.Count > 0 ? ConvertTools(toolList) : null,
                Stream = false,
                Options = new RequestOptions
                {
                    NumPredict = _settings.MaxTokens,
                    Temperature = _settings.Temperature
                }
            };

            ChatResponseStream lastChunk = null;
            int? inputTokens = null;
            int? outputTokens = null;
            string doneReason = null;

            try
            {
                await foreach (var chunk in _client.ChatAsync(request, cancellationToken))
                {
                    if (chunk == null) continue;
                    lastChunk = chunk;

                    if (chunk is ChatDoneResponseStream done)
                    {
                        inputTokens = done.PromptEvalCount;
                        outputTokens = done.EvalCount;
                        doneReason = done.DoneReason;
                    }
                }
            }
            catch (OllamaException ex)
            {
                throw new HttpRequestException("Ollama API request failed.", ex);
            }

            return ParseResponse(lastChunk, inputTokens, outputTokens, doneReason);
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var toolList = tools?.ToList();
            var request = new ChatRequest
            {
                Model = _settings.ModelName,
                Messages = ConvertMessages(messages),
                Tools = toolList?.Count > 0 ? ConvertTools(toolList) : null,
                Stream = true,
                Options = new RequestOptions
                {
                    NumPredict = _settings.MaxTokens,
                    Temperature = _settings.Temperature
                }
            };

            _logger.LogInformation("[Ollama] stream started, model={Model}", _settings.ModelName);

            int? totalInputTokens = null;
            int? totalOutputTokens = null;

            var enumerator = _client.ChatAsync(request, cancellationToken).GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    ChatResponseStream chunk;
                    try
                    {
                        if (!await enumerator.MoveNextAsync())
                            break;
                        chunk = enumerator.Current;
                    }
                    catch (OllamaException ex)
                    {
                        throw new HttpRequestException("Ollama API request failed.", ex);
                    }

                    if (chunk == null)
                        continue;

                    _logger.LogDebug("[Ollama] chunk done={Done} content={Content}",
                        chunk.Done, chunk.Message?.Content);

                    if (!chunk.Done && !string.IsNullOrEmpty(chunk.Message?.Content))
                    {
                        yield return new LLMStreamChunk
                        {
                            Type = "content_block_delta",
                            Content = chunk.Message.Content,
                            IsComplete = false
                        };
                    }
                    else if (chunk.Done)
                    {
                        if (chunk is ChatDoneResponseStream done)
                        {
                            totalInputTokens = done.PromptEvalCount;
                            totalOutputTokens = done.EvalCount;
                        }

                        _logger.LogInformation(
                            "[Ollama] done=true content={Content} has_tool_calls={HasToolCalls}",
                            chunk.Message?.Content,
                            chunk.Message?.ToolCalls?.Any() == true);

                        if (chunk.Message?.ToolCalls?.Any() == true)
                        {
                            yield return new LLMStreamChunk
                            {
                                Type = "tool_use",
                                Content = ConvertToolCallsToJson(chunk.Message.ToolCalls),
                                IsComplete = true,
                                InputTokens = totalInputTokens,
                                OutputTokens = totalOutputTokens
                            };
                            yield break;
                        }

                        yield return new LLMStreamChunk
                        {
                            Type = "message_stop",
                            Content = null,
                            IsComplete = true,
                            InputTokens = totalInputTokens,
                            OutputTokens = totalOutputTokens
                        };
                        yield break;
                    }
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            // Fallback if stream ends without a done=true chunk
            yield return new LLMStreamChunk
            {
                Type = "message_stop",
                Content = null,
                IsComplete = true,
                InputTokens = totalInputTokens,
                OutputTokens = totalOutputTokens
            };
        }

        private IList<Message> ConvertMessages(IEnumerable<LLMMessage> messages)
        {
            var result = new List<Message>();

            foreach (var m in messages)
            {
                if (m.ToolResults != null)
                {
                    // Each tool result becomes a separate "tool" role message
                    foreach (var tr in m.ToolResults)
                    {
                        result.Add(new Message
                        {
                            Role = ChatRole.Tool,
                            Content = tr["content"]?.ToString() ?? string.Empty
                        });
                    }
                }
                else if (m.ToolCalls != null)
                {
                    var toolCalls = new List<Message.ToolCall>();
                    foreach (var tc in m.ToolCalls)
                    {
                        var inputObj = tc["input"] as JObject ?? new JObject();
                        var args = inputObj.ToObject<Dictionary<string, object>>();

                        toolCalls.Add(new Message.ToolCall
                        {
                            Function = new Message.Function
                            {
                                Name = tc["name"]?.ToString(),
                                Arguments = args
                            }
                        });
                    }

                    result.Add(new Message
                    {
                        Role = m.Role == "assistant" ? ChatRole.Assistant : ChatRole.User,
                        Content = m.Content ?? string.Empty,
                        ToolCalls = toolCalls
                    });
                }
                else
                {
                    var role = m.Role switch
                    {
                        "system" => ChatRole.System,
                        "user" => ChatRole.User,
                        "tool" => ChatRole.Tool,
                        _ => ChatRole.Assistant
                    };

                    result.Add(new Message
                    {
                        Role = role,
                        Content = m.Content ?? string.Empty
                    });
                }
            }

            return result;
        }

        private IEnumerable<Tool> ConvertTools(IEnumerable<MCPToolDefinition> tools)
        {
            return tools.Select(tool =>
            {
                var schema = tool.InputSchema;
                var properties = new Dictionary<string, Property>();

                if (schema["properties"] is JObject props)
                {
                    foreach (var prop in props.Properties())
                    {
                        var propDef = prop.Value as JObject;
                        properties[prop.Name] = new Property
                        {
                            Type = propDef?["type"]?.ToString(),
                            Description = propDef?["description"]?.ToString(),
                            Enum = (propDef?["enum"] as JArray)?.Select(e => e.ToString()).ToList()
                        };
                    }
                }

                var required = (schema["required"] as JArray)?.Select(r => r.ToString()).ToArray();

                return new Tool
                {
                    Function = new Function
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = new Parameters
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                };
            });
        }

        private LLMResponse ParseResponse(ChatResponseStream lastChunk, int? inputTokens, int? outputTokens, string doneReason)
        {
            var result = new LLMResponse
            {
                InputTokens = inputTokens ?? 0,
                OutputTokens = outputTokens ?? 0,
                StopReason = doneReason ?? "stop"
            };

            if (lastChunk?.Message != null)
            {
                result.Content = lastChunk.Message.Content;

                if (lastChunk.Message.ToolCalls?.Any() == true)
                {
                    result.ToolCalls = JArray.Parse(ConvertToolCallsToJson(lastChunk.Message.ToolCalls));
                    result.RequiresToolExecution = true;
                }
            }

            return result;
        }

        private string ConvertToolCallsToJson(IEnumerable<Message.ToolCall> toolCalls)
        {
            var result = new JArray();

            foreach (var tc in toolCalls)
            {
                JObject input;
                if (tc.Function?.Arguments != null)
                {
                    // Arguments deserialized by System.Text.Json may contain JsonElement values
                    var json = JsonSerializer.Serialize(tc.Function.Arguments);
                    input = JObject.Parse(json);
                }
                else
                {
                    input = new JObject();
                }

                result.Add(new JObject
                {
                    ["id"] = tc.Id ?? Guid.NewGuid().ToString(),
                    ["name"] = tc.Function?.Name,
                    ["input"] = input
                });
            }

            return result.ToString();
        }
    }
}
