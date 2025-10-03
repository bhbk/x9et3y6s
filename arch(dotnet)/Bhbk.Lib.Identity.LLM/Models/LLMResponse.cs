using Newtonsoft.Json.Linq;

namespace Bhbk.Lib.Identity.LLM.Models
{
    public class LLMResponse
    {
        public string Content { get; set; }
        public JArray ToolCalls { get; set; }
        public bool RequiresToolExecution { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string StopReason { get; set; }
    }
}
