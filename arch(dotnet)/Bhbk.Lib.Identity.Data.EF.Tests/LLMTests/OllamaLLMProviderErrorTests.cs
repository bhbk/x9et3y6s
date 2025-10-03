using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.LLM.Providers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.LLMTests
{
    [Collection("LLMTests")]
    public class OllamaLLMProviderErrorTests
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

        private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
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

        private HttpClient CreateTimeoutHttpClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException(
                    "The request was canceled due to the configured HttpClient.Timeout."));

            return new HttpClient(handlerMock.Object);
        }

        private List<LLMMessage> SimpleMessages()
        {
            return new List<LLMMessage> { LLMMessage.User("Hello") };
        }

        // ── SendMessageAsync error tests ──────────────────────────────

        [Fact]
        public async Task SendMessageAsync_Unauthorized_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""authentication required""}",
                HttpStatusCode.Unauthorized);
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task SendMessageAsync_NotFound_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""model 'nonexistent' not found""}",
                HttpStatusCode.NotFound);
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task SendMessageAsync_BadRequest_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""invalid options: num_predict must be > 0""}",
                HttpStatusCode.BadRequest);
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task SendMessageAsync_InternalServerError_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""internal server error""}",
                HttpStatusCode.InternalServerError);
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task SendMessageAsync_ServiceUnavailable_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                "Service Unavailable",
                HttpStatusCode.ServiceUnavailable);
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task SendMessageAsync_Timeout_ThrowsTaskCanceledException()
        {
            var httpClient = CreateTimeoutHttpClient();
            var provider = CreateProvider(httpClient);

            var act = () => provider.SendMessageAsync(SimpleMessages(), null);

            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        // ── StreamMessageAsync error tests ────────────────────────────

        [Fact]
        public async Task StreamMessageAsync_Unauthorized_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""authentication required""}",
                HttpStatusCode.Unauthorized);
            var provider = CreateProvider(httpClient);

            var act = async () =>
            {
                await foreach (var chunk in provider.StreamMessageAsync(SimpleMessages(), null))
                {
                    // Should not reach here
                }
            };

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task StreamMessageAsync_NotFound_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""model 'nonexistent' not found""}",
                HttpStatusCode.NotFound);
            var provider = CreateProvider(httpClient);

            var act = async () =>
            {
                await foreach (var chunk in provider.StreamMessageAsync(SimpleMessages(), null))
                {
                    // Should not reach here
                }
            };

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task StreamMessageAsync_BadRequest_ThrowsHttpRequestException()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""invalid options""}",
                HttpStatusCode.BadRequest);
            var provider = CreateProvider(httpClient);

            var act = async () =>
            {
                await foreach (var chunk in provider.StreamMessageAsync(SimpleMessages(), null))
                {
                    // Should not reach here
                }
            };

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task StreamMessageAsync_Timeout_ThrowsTaskCanceledException()
        {
            var httpClient = CreateTimeoutHttpClient();
            var provider = CreateProvider(httpClient);

            var act = async () =>
            {
                await foreach (var chunk in provider.StreamMessageAsync(SimpleMessages(), null))
                {
                    // Should not reach here
                }
            };

            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        // ── Verify exception does NOT contain response body details ───

        [Fact]
        public async Task SendMessageAsync_ErrorResponse_ExceptionDoesNotLeakResponseBody()
        {
            var httpClient = CreateMockHttpClient(
                @"{""error"":""secret internal details here""}",
                HttpStatusCode.Forbidden);
            var provider = CreateProvider(httpClient);

            try
            {
                await provider.SendMessageAsync(SimpleMessages(), null);
                Assert.True(false, "Expected HttpRequestException");
            }
            catch (HttpRequestException ex)
            {
                // EnsureSuccessStatusCode includes status code but not body
                ex.Message.Should().NotContain("secret internal details");
            }
        }
    }
}
