using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.MCP.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.LLM.Abstractions
{
    /*
     * Implementors must serialize all LLMMessage fields: Role, Content, ToolCalls, and ToolResults.
     * Each provider translates these into its own native API format.
     */
    public interface ILLMProvider
    {
        string ProviderName { get; }

        Task<LLMResponse> SendMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<LLMStreamChunk> StreamMessageAsync(
            IEnumerable<LLMMessage> messages,
            IEnumerable<MCPToolDefinition> tools = null,
            CancellationToken cancellationToken = default);
    }
}
