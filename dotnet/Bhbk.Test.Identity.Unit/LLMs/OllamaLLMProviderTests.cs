using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.LLM.Providers;
using Bhbk.Lib.Identity.MCP.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Test.Identity.Unit.LLMs
{
    [Collection("LLMTests")]
    public class OllamaLLMProviderTests
    {
        private OllamaLLMProvider CreateProvider(HttpClient httpClient)
        {
            var settings = new LLMProviderSettings
            {
                Ollama = new OllamaSettings
                {
                    BaseUrl = "http://localhost:11434",
                    ModelName = "llama3.2",
                    MaxTokens = 4096,
                    Temperature = 0.7f,
                    TimeoutSeconds = 60
                }
            };
            var options = Options.Create(settings);
            return new OllamaLLMProvider(options, httpClient);
        }

        private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task SendMessageAsync_ReturnsResponse()
        {
            var ollamaResponse = @"{""model"":""llama3.2"",""message"":{""role"":""assistant"",""content"":""Hello! How can I help you today?""},""done"":true,""prompt_eval_count"":10,""eval_count"":15}";

            var httpClient = CreateMockHttpClient(ollamaResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("Hello")
            };

            var response = await provider.SendMessageAsync(messages, null);

            response.Should().NotBeNull();
            response.Content.Should().Be("Hello! How can I help you today?");
            response.InputTokens.Should().Be(10);
            response.OutputTokens.Should().Be(15);
            response.RequiresToolExecution.Should().BeFalse();
        }

        [Fact]
        public async Task SendMessageAsync_WithToolCall_ReturnsToolCall()
        {
            var ollamaResponse = @"{""model"":""llama3.2"",""message"":{""role"":""assistant"",""content"":"""",""tool_calls"":[{""id"":""call_123"",""function"":{""name"":""users"",""arguments"":{""action"":""list"",""take"":10}}}]},""done"":true,""prompt_eval_count"":50,""eval_count"":25}";

            var httpClient = CreateMockHttpClient(ollamaResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("List all users")
            };
            var tools = new List<MCPToolDefinition>
            {
                new MCPToolDefinition
                {
                    Name = "users",
                    Description = "User management tool",
                    Scope = MCPScope.Admin,
                    InputSchema = new JObject()
                }
            };

            var response = await provider.SendMessageAsync(messages, tools);

            response.Should().NotBeNull();
            response.RequiresToolExecution.Should().BeTrue();
            response.ToolCalls.Should().NotBeNull();
            response.ToolCalls.Should().HaveCount(1);
            response.ToolCalls[0]["name"].ToString().Should().Be("users");
        }

        [Fact]
        public async Task StreamMessageAsync_ReturnsChunks()
        {
            var streamResponse = @"{""message"":{""content"":""Hello""},""done"":false}
{""message"":{""content"":"" there""},""done"":false}
{""message"":{""content"":""!""},""done"":false}
{""message"":{""content"":""""},""done"":true,""prompt_eval_count"":10,""eval_count"":3}";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(streamResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("Hello")
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
            var streamResponse = @"{""message"":{""content"":""Hi""},""done"":false}
{""message"":{""content"":""""},""done"":true,""prompt_eval_count"":5,""eval_count"":1}";

            var httpClient = CreateMockHttpClient(streamResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("Hello")
            };

            var chunks = new List<LLMStreamChunk>();
            await foreach (var chunk in provider.StreamMessageAsync(messages, null))
            {
                chunks.Add(chunk);
            }

            var lastChunk = chunks[chunks.Count - 1];
            lastChunk.IsComplete.Should().BeTrue();
            lastChunk.Type.Should().Be("message_stop");
        }

        [Fact]
        public async Task SendMessageAsync_TracksTokenUsage()
        {
            var ollamaResponse = @"{""model"":""llama3.2"",""message"":{""role"":""assistant"",""content"":""Test response""},""done"":true,""prompt_eval_count"":42,""eval_count"":17}";

            var httpClient = CreateMockHttpClient(ollamaResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("Test message")
            };

            var response = await provider.SendMessageAsync(messages, null);

            response.InputTokens.Should().Be(42);
            response.OutputTokens.Should().Be(17);
        }

        [Fact]
        public void ProviderName_ReturnsOllama()
        {
            var httpClient = CreateMockHttpClient("{}");
            var provider = CreateProvider(httpClient);

            provider.ProviderName.Should().Be("Ollama");
        }

        [Fact]
        public async Task SendMessageAsync_WithSystemMessage_IncludesInRequest()
        {
            var ollamaResponse = @"{""model"":""llama3.2"",""message"":{""role"":""assistant"",""content"":""I am a helpful assistant.""},""done"":true,""prompt_eval_count"":20,""eval_count"":10}";

            string capturedBody = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                    capturedBody = req.Content.ReadAsStringAsync().GetAwaiter().GetResult())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ollamaResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.System("You are a helpful assistant."),
                LLMMessage.User("What are you?")
            };

            await provider.SendMessageAsync(messages, null);

            capturedBody.Should().NotBeNull();
            capturedBody.Should().Contain("You are a helpful assistant");
        }

        [Fact]
        public async Task StreamMessageAsync_WithToolCall_ReturnsToolUse()
        {
            var streamResponse = @"{""message"":{""content"":"""",""tool_calls"":[{""id"":""call_1"",""function"":{""name"":""users"",""arguments"":{""action"":""list""}}}]},""done"":true,""prompt_eval_count"":30,""eval_count"":20}";

            var httpClient = CreateMockHttpClient(streamResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("List users")
            };
            var tools = new List<MCPToolDefinition>
            {
                new MCPToolDefinition
                {
                    Name = "users",
                    Description = "User management",
                    Scope = MCPScope.Admin,
                    InputSchema = new JObject()
                }
            };

            var chunks = new List<LLMStreamChunk>();
            await foreach (var chunk in provider.StreamMessageAsync(messages, tools))
            {
                chunks.Add(chunk);
            }

            chunks.Should().Contain(c => c.Type == "tool_use");
            var toolChunk = chunks.Find(c => c.Type == "tool_use");
            toolChunk.IsComplete.Should().BeTrue();
            toolChunk.Content.Should().Contain("users");
        }

        [Fact]
        public async Task SendMessageAsync_HandlesEmptyResponse()
        {
            var ollamaResponse = @"{""model"":""llama3.2"",""message"":{""role"":""assistant"",""content"":""""},""done"":true,""prompt_eval_count"":5,""eval_count"":0}";

            var httpClient = CreateMockHttpClient(ollamaResponse);
            var provider = CreateProvider(httpClient);

            var messages = new List<LLMMessage>
            {
                LLMMessage.User("...")
            };

            var response = await provider.SendMessageAsync(messages, null);

            response.Should().NotBeNull();
            response.Content.Should().BeEmpty();
            response.RequiresToolExecution.Should().BeFalse();
        }
    }
}
