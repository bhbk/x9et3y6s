using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Services
{
    public class LLMOrchestrator : ILLMOrchestrator
    {
        private readonly ILLMProvider _provider;
        private readonly IMCPToolContext _toolContext;
        private readonly ConversationService _conversationService;
        private const int MaxToolIterations = 5;

        public LLMOrchestrator(
            ILLMProvider provider,
            IMCPToolContext toolContext,
            ConversationService conversationService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _toolContext = toolContext ?? throw new ArgumentNullException(nameof(toolContext));
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        }

        public async Task<LLMResponse> ProcessMessageAsync(
            Guid conversationId,
            string userMessage,
            CancellationToken cancellationToken = default)
        {
            _conversationService.AddMessage(conversationId, "user", userMessage);

            // Build message history
            var messages = _conversationService.BuildMessageHistory(conversationId).ToList();

            // Add system prompt
            var systemPrompt = BuildSystemPrompt();
            messages.Insert(0, LLMMessage.System(systemPrompt));

            // Get tool definitions
            var tools = _toolContext.Tools.Select(t => t.Definition).ToList();

            // Process with tool loop
            LLMResponse response = null;
            int iterations = 0;

            while (iterations < MaxToolIterations)
            {
                iterations++;

                response = await _provider.SendMessageAsync(messages, tools, cancellationToken);

                if (!response.RequiresToolExecution)
                    break;

                // Execute tools
                var (toolResults, _) = await ExecuteToolsAsync(response.ToolCalls, conversationId, cancellationToken);

                // Save assistant message with tool calls
                _conversationService.AddMessage(
                    conversationId,
                    "assistant",
                    null,
                    toolCalls: response.ToolCalls?.ToString(),
                    inputTokens: response.InputTokens,
                    outputTokens: response.OutputTokens);

                // Save tool results as tool message
                _conversationService.AddMessage(
                    conversationId,
                    "tool",
                    null,
                    toolResults: toolResults.ToString());

                // Rebuild messages for next iteration
                messages = _conversationService.BuildMessageHistory(conversationId).ToList();
                messages.Insert(0, LLMMessage.System(systemPrompt));
            }

            // Save final assistant response
            if (response != null && !string.IsNullOrEmpty(response.Content))
            {
                _conversationService.AddMessage(
                    conversationId,
                    "assistant",
                    response.Content,
                    inputTokens: response.InputTokens,
                    outputTokens: response.OutputTokens);
            }

            return response;
        }

        public async IAsyncEnumerable<LLMStreamChunk> ProcessMessageStreamAsync(
            Guid conversationId,
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _conversationService.AddMessage(conversationId, "user", userMessage);

            // Build message history
            var messages = _conversationService.BuildMessageHistory(conversationId).ToList();

            // Add system prompt
            var systemPrompt = BuildSystemPrompt();
            messages.Insert(0, LLMMessage.System(systemPrompt));

            // Get tool definitions
            var tools = _toolContext.Tools.Select(t => t.Definition).ToList();

            int iterations = 0;
            var contentBuilder = new StringBuilder();
            int? totalInputTokens = null;
            int? totalOutputTokens = null;

            // Track whether a tool call has been executed at least once.
            // Before the first tool call we buffer content so that any LLM preamble
            // that appears before the tool_use block is never forwarded to the client.
            // After the first tool call the LLM is producing its actual answer, so
            // content is streamed in real-time from that point on.
            bool firstToolCallComplete = false;

            // Accumulate file references across all tool iterations
            var pendingFiles = new List<JObject>();

            while (iterations < MaxToolIterations)
            {
                iterations++;
                contentBuilder.Clear();
                bool toolCallMade = false;
                bool nonStreamingFallbackUsed = false;

                // Buffer content in the first turn only (before any tool has been called).
                bool bufferThisTurn = !firstToolCallComplete;
                var bufferedChunks = bufferThisTurn ? new List<LLMStreamChunk>() : null;

                await foreach (var chunk in _provider.StreamMessageAsync(messages, tools, cancellationToken))
                {
                    if (chunk.Type == "content_block_delta" && !string.IsNullOrEmpty(chunk.Content))
                    {
                        contentBuilder.Append(chunk.Content);

                        if (bufferThisTurn)
                        {
                            // Hold back — we don't yet know if a tool_use block follows.
                            bufferedChunks!.Add(chunk);
                        }
                        else
                        {
                            // Post-tool-call turn: the LLM is producing its answer — stream live.
                            yield return chunk;
                        }
                    }
                    else if (chunk.Type == "tool_use")
                    {
                        toolCallMade = true;
                        firstToolCallComplete = true;

                        // Discard any buffered pre-tool preamble — it was narration, not an answer.
                        bufferedChunks?.Clear();
                        contentBuilder.Clear();

                        var toolCalls = JArray.Parse(chunk.Content);
                        var (toolResults, fileRefs) = await ExecuteToolsAsync(toolCalls, conversationId, cancellationToken);
                        pendingFiles.AddRange(fileRefs);

                        _conversationService.AddMessage(
                            conversationId,
                            "assistant",
                            null,
                            toolCalls: chunk.Content,
                            inputTokens: chunk.InputTokens,
                            outputTokens: chunk.OutputTokens);

                        _conversationService.AddMessage(
                            conversationId,
                            "tool",
                            null,
                            toolResults: toolResults.ToString());

                        messages = _conversationService.BuildMessageHistory(conversationId).ToList();
                        messages.Insert(0, LLMMessage.System(systemPrompt));

                        yield return new LLMStreamChunk
                        {
                            Type = "tool_execution",
                            Content = "Executing tools...",
                            IsComplete = false
                        };

                        break;
                    }
                    else if (chunk.Type == "message_stop" || chunk.IsComplete)
                    {
                        totalInputTokens = chunk.InputTokens;
                        totalOutputTokens = chunk.OutputTokens;

                        if (contentBuilder.Length > 0)
                        {
                            _conversationService.AddMessage(
                                conversationId,
                                "assistant",
                                contentBuilder.ToString(),
                                inputTokens: totalInputTokens,
                                outputTokens: totalOutputTokens);

                            // Flush any buffered content before the completion signal.
                            if (bufferThisTurn && bufferedChunks!.Count > 0)
                            {
                                foreach (var buffered in bufferedChunks)
                                    yield return buffered;
                            }

                            // Emit file chunks before the completion signal
                            foreach (var fileRef in pendingFiles)
                            {
                                yield return new LLMStreamChunk
                                {
                                    Type = "file",
                                    Content = fileRef["summary"]?.ToString(),
                                    FileId = fileRef["fileId"]?.ToString(),
                                    FileName = fileRef["fileName"]?.ToString(),
                                    FileSize = fileRef["fileSize"]?.Value<long>(),
                                    IsComplete = false
                                };
                            }
                            pendingFiles.Clear();

                            yield return chunk;
                            yield break;
                        }
                    }
                    else if (chunk.Type == "metadata")
                    {
                        totalInputTokens = chunk.InputTokens;
                        totalOutputTokens = chunk.OutputTokens;
                    }
                }

                if (!toolCallMade)
                {
                    /*
                     * Streaming produced no content and no tool calls. Some providers (e.g. Ollama)
                     * only surface tool calls in non-streaming mode. Try once with SendMessageAsync
                     * before giving up.
                     */
                    if (!nonStreamingFallbackUsed && tools.Any())
                    {
                        nonStreamingFallbackUsed = true;

                        var fallback = await _provider.SendMessageAsync(messages, tools, cancellationToken);

                        if (fallback.RequiresToolExecution)
                        {
                            var (toolResults, fileRefs) = await ExecuteToolsAsync(fallback.ToolCalls, conversationId, cancellationToken);
                            pendingFiles.AddRange(fileRefs);

                            _conversationService.AddMessage(
                                conversationId,
                                "assistant",
                                null,
                                toolCalls: fallback.ToolCalls?.ToString(),
                                inputTokens: fallback.InputTokens,
                                outputTokens: fallback.OutputTokens);

                            _conversationService.AddMessage(
                                conversationId,
                                "tool",
                                null,
                                toolResults: toolResults.ToString());

                            messages = _conversationService.BuildMessageHistory(conversationId).ToList();
                            messages.Insert(0, LLMMessage.System(systemPrompt));

                            yield return new LLMStreamChunk
                            {
                                Type = "tool_execution",
                                Content = "Executing tools...",
                                IsComplete = false
                            };

                            continue;
                        }
                        else if (!string.IsNullOrEmpty(fallback.Content))
                        {
                            _conversationService.AddMessage(
                                conversationId,
                                "assistant",
                                fallback.Content,
                                inputTokens: fallback.InputTokens,
                                outputTokens: fallback.OutputTokens);

                            totalInputTokens = fallback.InputTokens;
                            totalOutputTokens = fallback.OutputTokens;

                            yield return new LLMStreamChunk
                            {
                                Type = "content_block_delta",
                                Content = fallback.Content,
                                IsComplete = false
                            };
                        }
                    }

                    break;
                }
            }

            // Emit any remaining file chunks before the final completion signal
            foreach (var fileRef in pendingFiles)
            {
                yield return new LLMStreamChunk
                {
                    Type = "file",
                    Content = fileRef["summary"]?.ToString(),
                    FileId = fileRef["fileId"]?.ToString(),
                    FileName = fileRef["fileName"]?.ToString(),
                    FileSize = fileRef["fileSize"]?.Value<long>(),
                    IsComplete = false
                };
            }

            yield return new LLMStreamChunk
            {
                Type = "message_stop",
                IsComplete = true,
                InputTokens = totalInputTokens,
                OutputTokens = totalOutputTokens
            };
        }

        private string BuildSystemPrompt()
        {
            var promptType = _toolContext.Scope == MCPScope.Admin ? "admin" : "user";
            var customPrompt = _conversationService.GetSystemPrompt(promptType);

            var basePrompt = _toolContext.Scope == MCPScope.Admin
                ? "You are an AI assistant for the Identity management system. You have access to tools that allow you to query and analyze identity data including users, audiences, issuers, roles, claims, logins, and authentication activity. Use these tools to help administrators understand and manage the identity system.\n\nWhen you use a tool, always include the actual data from the tool results in your response. Present tables, lists, counts, and details directly — never say \"the output shows\" without including the data itself. Format data clearly using lists or tables when appropriate.\n\nYou also have an 'export' tool that can generate downloadable CSV or JSON files. Use it when the user asks for a data export, download, or spreadsheet."
                : BuildUserPromptWithContext();

            return !string.IsNullOrEmpty(customPrompt)
                ? $"{basePrompt}\n\nAdditional instructions:\n{customPrompt}"
                : basePrompt;
        }

        private string BuildUserPromptWithContext()
        {
            var sb = new StringBuilder();

            sb.Append($"You are an AI assistant for the Identity user portal. You can help the user view their profile, roles, settings, and session information. You only have access to data belonging to user ID: {_toolContext.UserId}. Never attempt to access data belonging to other users.");

            var contextData = BuildUserContextData();
            if (!string.IsNullOrEmpty(contextData))
            {
                sb.Append("\n\nThe user's current data is provided below. Answer questions from this data when possible. Only use tools when:");
                sb.Append("\n- The user asks for data not shown below (e.g., refresh tokens, older activity beyond the last 10)");
                sb.Append("\n- The user asks for a data export or download");
                sb.Append("\n- The user asks for a quote or message of the day");
                sb.Append("\n- The user explicitly asks to refresh the data");
                sb.Append(contextData);
            }

            sb.Append("\n\nWhen you use a tool, always include the actual data from the tool results in your response. Present your findings directly — never say \"the output shows\" without including the data itself.");
            sb.Append("\n\nYou also have an 'export' tool that can generate downloadable CSV or JSON files of your own data.");

            return sb.ToString();
        }

        private string BuildUserContextData()
        {
            if (_toolContext.Scope != MCPScope.User || !_toolContext.UserId.HasValue)
                return null;

            var uow = _toolContext.UnitOfWork;
            var userId = _toolContext.UserId.Value;
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            try
            {
                var sb = new StringBuilder();

                var user = uow.Users.Get(x => x.Id == userId).FirstOrDefault();
                if (user == null)
                    return null;

                var userJson = JObject.FromObject(user, serializer);
                sb.Append("\n\n--- YOUR PROFILE ---\n");
                sb.Append(SensitiveFieldFilter.Filter(userJson).ToString(Formatting.Indented));

                var roles = uow.Users.GetRolesForUser(userId);
                var rolesJson = JArray.FromObject(roles, serializer);
                sb.Append("\n\n--- YOUR ROLES ---\n");
                sb.Append(SensitiveFieldFilter.Filter(rolesJson).ToString(Formatting.Indented));

                var settings = uow.Settings.Get(x => x.UserId == userId)
                    .OrderBy(x => x.ConfigKey)
                    .ToList();
                if (settings.Count > 0)
                {
                    var settingsJson = JArray.FromObject(settings, serializer);
                    sb.Append("\n\n--- YOUR SETTINGS ---\n");
                    sb.Append(SensitiveFieldFilter.Filter(settingsJson).ToString(Formatting.Indented));
                }

                var activityTotal = uow.AuthActivity.Get(x => x.UserId == userId).Count();
                var activities = uow.AuthActivity.Get(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(10)
                    .ToList();
                if (activities.Count > 0)
                {
                    var activityJson = JArray.FromObject(activities, serializer);
                    sb.Append($"\n\n--- YOUR RECENT ACTIVITY (last 10 of {activityTotal}) ---\n");
                    sb.Append(SensitiveFieldFilter.Filter(activityJson).ToString(Formatting.Indented));
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private async Task<(JArray results, List<JObject> fileRefs)> ExecuteToolsAsync(
            JArray toolCalls, Guid conversationId, CancellationToken cancellationToken)
        {
            var results = new JArray();
            var fileRefs = new List<JObject>();

            foreach (var call in toolCalls)
            {
                var toolId = call["id"]?.ToString();
                var toolName = call["name"]?.ToString();
                var toolInput = call["input"] as JObject ?? new JObject();

                var tool = _toolContext.GetTool(toolName);
                MCPToolResult result;

                if (tool == null)
                {
                    result = MCPToolResult.Fail($"Tool not found: {toolName}");
                }
                else
                {
                    result = await tool.ExecuteAsync(toolInput);
                }

                // Intercept file_export results: save blob to DB, replace with reference
                if (result.Success && result.Data?["type"]?.ToString() == "file_export")
                {
                    var fileRef = SaveFileToDatabase(conversationId, result.Data);
                    fileRefs.Add(fileRef);

                    results.Add(new JObject
                    {
                        ["tool_use_id"] = toolId,
                        ["content"] = fileRef.ToString(Formatting.Indented)
                    });
                }
                else
                {
                    results.Add(new JObject
                    {
                        ["tool_use_id"] = toolId,
                        ["content"] = result.Success
                            ? result.Data?.ToString(Formatting.Indented)
                            : $"Error: {result.Error}"
                    });
                }
            }

            return (results, fileRefs);
        }

        private JObject SaveFileToDatabase(Guid conversationId, JToken fileData)
        {
            var uow = _toolContext.UnitOfWork;
            var bytes = Convert.FromBase64String(fileData["contentBase64"].ToString());
            var fileName = fileData["fileName"].ToString();
            var contentType = fileData["contentType"].ToString();
            var summary = fileData["summary"]?.ToString();

            var file = new tbl_ChatFile
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                FileName = fileName,
                ContentType = contentType,
                FileSize = bytes.Length,
                FileContent = bytes,
                Summary = summary,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
                CreatedUtc = DateTimeOffset.UtcNow
            };

            uow.ChatFiles.Create(file);
            uow.Commit();

            return new JObject
            {
                ["type"] = "file_reference",
                ["fileId"] = file.Id.ToString(),
                ["fileName"] = fileName,
                ["contentType"] = contentType,
                ["fileSize"] = bytes.Length,
                ["summary"] = summary
            };
        }
    }
}
