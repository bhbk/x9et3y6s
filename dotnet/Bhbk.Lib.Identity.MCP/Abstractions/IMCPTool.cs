using Bhbk.Lib.Identity.MCP.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Abstractions
{
    public interface IMCPTool
    {
        MCPToolDefinition Definition { get; }
        Task<MCPToolResult> ExecuteAsync(JObject parameters);
    }
}
