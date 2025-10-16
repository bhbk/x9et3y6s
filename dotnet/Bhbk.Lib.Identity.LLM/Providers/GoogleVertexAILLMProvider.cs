using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;
using ProtoValue = Google.Protobuf.WellKnownTypes.Value;
using ProtoStruct = Google.Protobuf.WellKnownTypes.Struct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Type = Google.Cloud.AIPlatform.V1.Type;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class GoogleVertexAILLMProvider : ILLMProvider
    {
        private readonly PredictionServiceClient _client;
        private readonly VertexAISettings _settings;
        private readonly string _modelName;

        public string ProviderName => "GoogleVertexAI";

        public GoogleVertexAILLMProvider(IOptions<LLMProviderSettings> options)
        {
            _settings = options.Value.VertexAI;

            _client = new PredictionServiceClientBuilder
            {
                Endpoint = $"{_settings.Location}-aiplatform.googleapis.com"
            }.Build();

            _modelName = $"projects/{_settings.ProjectId}/locations/{_settings.Location}/publishers/google/models/{_settings.ModelName}";
        }

        public async Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(messages, tools);
            var response = await _client.GenerateContentAsync(request, cancellationToken);

            return ParseResponse(response);
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(messages, tools);

            var streamResponse = _client.StreamGenerateContent(request);
            var responseStream = streamResponse.GetResponseStream();

            int? totalInputTokens = null;
            int? totalOutputTokens = null;
            var functionCalls = new List<JObject>();

            await foreach (var response in responseStream)
            {
                if (response.UsageMetadata != null)
                {
                    totalInputTokens = response.UsageMetadata.PromptTokenCount;
                    totalOutputTokens = response.UsageMetadata.CandidatesTokenCount;
                }

                if (response.Candidates == null || response.Candidates.Count == 0)
                    continue;

                var candidate = response.Candidates[0];

                if (candidate.Content?.Parts == null)
                    continue;

                foreach (var part in candidate.Content.Parts)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        yield return new LLMStreamChunk
                        {
                            Type = "content_block_delta",
                            Content = part.Text,
                            IsComplete = false
                        };
                    }

                    if (part.FunctionCall != null)
                    {
                        var input = new JObject();
                        if (part.FunctionCall.Args != null)
                        {
                            foreach (var field in part.FunctionCall.Args.Fields)
                            {
                                input[field.Key] = ConvertProtoValueToJToken(field.Value);
                            }
                        }

                        functionCalls.Add(new JObject
                        {
                            ["id"] = Guid.NewGuid().ToString(),
                            ["name"] = part.FunctionCall.Name,
                            ["input"] = input
                        });
                    }
                }
            }

            if (functionCalls.Count > 0)
            {
                var toolCallsArray = new JArray(functionCalls);

                yield return new LLMStreamChunk
                {
                    Type = "tool_use",
                    Content = toolCallsArray.ToString(),
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
        }

        private GenerateContentRequest BuildRequest(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools)
        {
            var request = new GenerateContentRequest
            {
                Model = _modelName,
                GenerationConfig = new GenerationConfig
                {
                    MaxOutputTokens = _settings.MaxOutputTokens,
                    Temperature = _settings.Temperature
                }
            };

            // Extract and set system instruction
            var systemMessage = messages.FirstOrDefault(m => m.Role == "system");
            if (systemMessage != null)
            {
                request.SystemInstruction = new Content
                {
                    Parts = { new Part { Text = systemMessage.Content } }
                };
            }

            // Convert non-system messages to Contents
            foreach (var m in messages.Where(m => m.Role != "system"))
            {
                request.Contents.Add(ConvertMessage(m));
            }

            // Add tools if present
            if (tools != null && tools.Any())
            {
                var tool = new Tool();
                foreach (var t in tools)
                {
                    tool.FunctionDeclarations.Add(ConvertTool(t));
                }
                request.Tools.Add(tool);
            }

            return request;
        }

        private Content ConvertMessage(LLMMessage message)
        {
            var role = message.Role == "assistant" ? "MODEL" : "USER";
            var content = new Content { Role = role };

            if (!string.IsNullOrEmpty(message.Content))
            {
                content.Parts.Add(new Part { Text = message.Content });
            }

            if (message.ToolCalls != null)
            {
                foreach (var tc in message.ToolCalls)
                {
                    var args = new ProtoStruct();
                    var inputObj = tc["input"] as JObject;
                    if (inputObj != null)
                    {
                        foreach (var prop in inputObj.Properties())
                        {
                            args.Fields[prop.Name] = ConvertJTokenToProtoValue(prop.Value);
                        }
                    }

                    content.Parts.Add(new Part
                    {
                        FunctionCall = new FunctionCall
                        {
                            Name = tc["name"]?.ToString(),
                            Args = args
                        }
                    });
                }
            }

            if (message.ToolResults != null)
            {
                foreach (var tr in message.ToolResults)
                {
                    var responseStruct = new ProtoStruct();
                    responseStruct.Fields["content"] = ProtoValue.ForString(
                        tr["content"]?.ToString() ?? string.Empty);

                    content.Parts.Add(new Part
                    {
                        FunctionResponse = new FunctionResponse
                        {
                            Name = tr["tool_use_id"]?.ToString() ?? "unknown",
                            Response = responseStruct
                        }
                    });
                }
            }

            return content;
        }

        private FunctionDeclaration ConvertTool(MCPToolDefinition tool)
        {
            var declaration = new FunctionDeclaration
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = ConvertSchemaToOpenApi(tool.InputSchema)
            };

            return declaration;
        }

        private OpenApiSchema ConvertSchemaToOpenApi(JObject schema)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = Type.Object
            };

            if (schema["properties"] is JObject props)
            {
                foreach (var prop in props.Properties())
                {
                    var propDef = prop.Value as JObject;
                    var propSchema = new OpenApiSchema
                    {
                        Description = propDef?["description"]?.ToString()
                    };

                    var typeStr = propDef?["type"]?.ToString();
                    propSchema.Type = typeStr switch
                    {
                        "string" => Type.String,
                        "number" => Type.Number,
                        "integer" => Type.Integer,
                        "boolean" => Type.Boolean,
                        "array" => Type.Array,
                        _ => Type.String
                    };

                    if (propDef?["enum"] is JArray enumValues)
                    {
                        foreach (var e in enumValues)
                        {
                            propSchema.Enum.Add(e.ToString());
                        }
                    }

                    openApiSchema.Properties[prop.Name] = propSchema;
                }
            }

            if (schema["required"] is JArray required)
            {
                foreach (var r in required)
                {
                    openApiSchema.Required.Add(r.ToString());
                }
            }

            return openApiSchema;
        }

        private LLMResponse ParseResponse(GenerateContentResponse response)
        {
            var result = new LLMResponse
            {
                InputTokens = response.UsageMetadata?.PromptTokenCount ?? 0,
                OutputTokens = response.UsageMetadata?.CandidatesTokenCount ?? 0,
                StopReason = response.Candidates?.FirstOrDefault()?.FinishReason.ToString() ?? "stop"
            };

            var candidate = response.Candidates?.FirstOrDefault();
            if (candidate?.Content?.Parts == null)
                return result;

            var toolCalls = new JArray();

            foreach (var part in candidate.Content.Parts)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    result.Content = part.Text;
                }

                if (part.FunctionCall != null)
                {
                    var input = new JObject();
                    if (part.FunctionCall.Args != null)
                    {
                        foreach (var field in part.FunctionCall.Args.Fields)
                        {
                            input[field.Key] = ConvertProtoValueToJToken(field.Value);
                        }
                    }

                    toolCalls.Add(new JObject
                    {
                        ["id"] = Guid.NewGuid().ToString(),
                        ["name"] = part.FunctionCall.Name,
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

        private static ProtoValue ConvertJTokenToProtoValue(JToken token)
        {
            return token.Type switch
            {
                JTokenType.String => ProtoValue.ForString(token.ToString()),
                JTokenType.Integer => ProtoValue.ForNumber(token.Value<double>()),
                JTokenType.Float => ProtoValue.ForNumber(token.Value<double>()),
                JTokenType.Boolean => ProtoValue.ForBool(token.Value<bool>()),
                JTokenType.Null => ProtoValue.ForNull(),
                _ => ProtoValue.ForString(token.ToString())
            };
        }

        private static JToken ConvertProtoValueToJToken(ProtoValue value)
        {
            return value.KindCase switch
            {
                ProtoValue.KindOneofCase.StringValue => new JValue(value.StringValue),
                ProtoValue.KindOneofCase.NumberValue => new JValue(value.NumberValue),
                ProtoValue.KindOneofCase.BoolValue => new JValue(value.BoolValue),
                ProtoValue.KindOneofCase.NullValue => JValue.CreateNull(),
                _ => new JValue(value.ToString())
            };
        }
    }
}
