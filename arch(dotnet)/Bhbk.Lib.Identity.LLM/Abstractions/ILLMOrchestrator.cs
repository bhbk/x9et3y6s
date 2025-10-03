using Bhbk.Lib.Identity.LLM.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Abstractions
{
    public interface ILLMOrchestrator
    {
        Task<LLMResponse> ProcessMessageAsync(
            Guid conversationId,
            string userMessage,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<LLMStreamChunk> ProcessMessageStreamAsync(
            Guid conversationId,
            string userMessage,
            CancellationToken cancellationToken = default);
    }
}
