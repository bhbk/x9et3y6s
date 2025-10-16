using Newtonsoft.Json.Linq;

namespace Bhbk.Lib.Identity.MCP.Models
{
    public class MCPToolDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MCPScope Scope { get; set; }
        public JObject InputSchema { get; set; }
    }
}
