using Newtonsoft.Json.Linq;

namespace Bhbk.Lib.Identity.LLM.Models
{
    public class LLMMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public JArray ToolCalls { get; set; }
        public JArray ToolResults { get; set; }

        public static LLMMessage User(string content)
        {
            return new LLMMessage { Role = "user", Content = content };
        }

        public static LLMMessage Assistant(string content)
        {
            return new LLMMessage { Role = "assistant", Content = content };
        }

        public static LLMMessage System(string content)
        {
            return new LLMMessage { Role = "system", Content = content };
        }
    }
}
