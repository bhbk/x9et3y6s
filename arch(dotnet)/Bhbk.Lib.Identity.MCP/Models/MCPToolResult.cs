using Newtonsoft.Json.Linq;

namespace Bhbk.Lib.Identity.MCP.Models
{
    public class MCPToolResult
    {
        public bool Success { get; set; }
        public JToken Data { get; set; }
        public string Error { get; set; }

        public static MCPToolResult Ok(JToken data)
        {
            return new MCPToolResult
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        public static MCPToolResult Fail(string error)
        {
            return new MCPToolResult
            {
                Success = false,
                Data = null,
                Error = error
            };
        }
    }
}
