using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Providers
{
    public class FailoverLLMProvider : ILLMProvider
    {
        private readonly IReadOnlyList<ILLMProvider> _providers;
        private readonly ILogger<FailoverLLMProvider> _logger;

        public string ProviderName =>
            $"Failover({string.Join(", ", _providers.Select(p => p.ProviderName))})";

        public FailoverLLMProvider(IReadOnlyList<ILLMProvider> providers, ILogger<FailoverLLMProvider> logger)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (providers.Count == 0)
                throw new ArgumentException("At least one provider is required", nameof(providers));
        }

        public async Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default)
        {
            Exception lastException = null;

            foreach (var provider in _providers)
            {
                try
                {
                    _logger.LogDebug("Trying provider {Provider} for SendMessageAsync", provider.ProviderName);
                    return await provider.SendMessageAsync(messages, tools, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex,
                        "Provider {Provider} failed in SendMessageAsync, falling through to next",
                        provider.ProviderName);
                }
            }

            throw new InvalidOperationException(
                "All providers in the failover chain failed", lastException);
        }

        public async IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Exception lastException = null;

            foreach (var provider in _providers)
            {
                IAsyncEnumerator<LLMStreamChunk> enumerator = null;
                LLMStreamChunk firstChunk = null;
                bool firstChunkSucceeded = false;

                try
                {
                    _logger.LogDebug("Trying provider {Provider} for StreamMessageAsync", provider.ProviderName);

                    enumerator = provider.StreamMessageAsync(messages, tools, cancellationToken)
                        .GetAsyncEnumerator(cancellationToken);

                    // Try to get the first chunk — if this throws, try next provider
                    if (!await enumerator.MoveNextAsync())
                    {
                        await enumerator.DisposeAsync();
                        yield break;
                    }

                    firstChunk = enumerator.Current;
                    firstChunkSucceeded = true;
                }
                catch (OperationCanceledException)
                {
                    if (enumerator != null) await enumerator.DisposeAsync();
                    throw;
                }
                catch (Exception ex) when (!firstChunkSucceeded)
                {
                    lastException = ex;
                    _logger.LogWarning(ex,
                        "Provider {Provider} stream failed before first chunk, falling through to next",
                        provider.ProviderName);
                    if (enumerator != null) await enumerator.DisposeAsync();
                    continue;
                }

                // Yield the first chunk outside the try-catch (C# prohibits yield return in try-catch)
                yield return firstChunk;

                // Stream remaining from this provider (no failover mid-stream)
                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        yield return enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
                yield break;
            }

            throw new InvalidOperationException(
                "All providers in the failover chain failed for streaming", lastException);
        }
    }
}
