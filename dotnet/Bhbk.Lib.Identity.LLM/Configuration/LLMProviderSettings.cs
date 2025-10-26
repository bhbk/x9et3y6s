using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bhbk.Lib.Identity.LLM.Configuration
{
    public class LLMProviderSettings
    {
        public List<string> Failover { get; set; } = new List<string>();
        public AWSBedrockSettings AWSBedrock { get; set; } = new AWSBedrockSettings();
        public OllamaSettings Ollama { get; set; } = new OllamaSettings();
        public AzureOpenAISettings AzureOpenAI { get; set; } = new AzureOpenAISettings();
        public VertexAISettings VertexAI { get; set; } = new VertexAISettings();

        public static LLMProviderSettings FromProviderConfigs(
            IEnumerable<(string Name, bool Enabled, int FailoverOrder, IDictionary<string, string> Settings)> providers)
        {
            var result = new LLMProviderSettings();
            var ordered = providers.OrderBy(p => p.FailoverOrder).ToList();

            result.Failover = ordered
                .Where(p => p.Enabled)
                .Select(p => p.Name)
                .ToList();

            foreach (var (name, enabled, _, settings) in ordered)
            {
                switch (name)
                {
                    case "Ollama":
                        result.Ollama = OllamaSettings.FromDictionary(settings, enabled);
                        break;
                    case "AWSBedrock":
                        result.AWSBedrock = AWSBedrockSettings.FromDictionary(settings, enabled);
                        break;
                    case "AzureOpenAI":
                        result.AzureOpenAI = AzureOpenAISettings.FromDictionary(settings, enabled);
                        break;
                    case "GoogleVertexAI":
                        result.VertexAI = VertexAISettings.FromDictionary(settings, enabled);
                        break;
                }
            }

            return result;
        }
    }

    public class AWSBedrockSettings
    {
        public bool Enabled { get; set; } = false;
        public string Region { get; set; } = "us-east-1";
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        public string ModelName { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";
        public int MaxTokens { get; set; } = 32000;

        public static AWSBedrockSettings FromDictionary(IDictionary<string, string> settings, bool enabled)
        {
            var result = new AWSBedrockSettings { Enabled = enabled };

            if (settings.TryGetValue("Region", out var region))
                result.Region = region;
            if (settings.TryGetValue("AccessKeyId", out var accessKeyId))
                result.AccessKeyId = accessKeyId;
            if (settings.TryGetValue("SecretAccessKey", out var secretAccessKey))
                result.SecretAccessKey = secretAccessKey;
            if (settings.TryGetValue("SessionToken", out var sessionToken))
                result.SessionToken = sessionToken;
            if (settings.TryGetValue("ModelName", out var model))
                result.ModelName = model;
            if (settings.TryGetValue("MaxTokens", out var maxTokens) && int.TryParse(maxTokens, out var mt))
                result.MaxTokens = mt;

            return result;
        }
    }

    public class OllamaSettings
    {
        public bool Enabled { get; set; } = false;
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string ApiKey { get; set; }
        public string ModelName { get; set; } = "llama3.2";
        public int MaxTokens { get; set; } = 32000;
        public float Temperature { get; set; } = 0.7f;
        public int TimeoutSeconds { get; set; } = 120;

        public static OllamaSettings FromDictionary(IDictionary<string, string> settings, bool enabled)
        {
            var result = new OllamaSettings { Enabled = enabled };

            if (settings.TryGetValue("BaseUrl", out var baseUrl))
                result.BaseUrl = baseUrl;
            if (settings.TryGetValue("ApiKey", out var apiKey))
                result.ApiKey = apiKey;
            if (settings.TryGetValue("ModelName", out var model))
                result.ModelName = model;
            if (settings.TryGetValue("MaxTokens", out var maxTokens) && int.TryParse(maxTokens, out var mt))
                result.MaxTokens = mt;
            if (settings.TryGetValue("Temperature", out var temp) && float.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out var t))
                result.Temperature = t;
            if (settings.TryGetValue("TimeoutSeconds", out var timeout) && int.TryParse(timeout, out var ts))
                result.TimeoutSeconds = ts;

            return result;
        }
    }

    public class AzureOpenAISettings
    {
        public bool Enabled { get; set; } = false;
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string DeploymentName { get; set; }
        public int MaxTokens { get; set; } = 32000;
        public float Temperature { get; set; } = 0.7f;

        public static AzureOpenAISettings FromDictionary(IDictionary<string, string> settings, bool enabled)
        {
            var result = new AzureOpenAISettings { Enabled = enabled };

            if (settings.TryGetValue("Endpoint", out var endpoint))
                result.Endpoint = endpoint;
            if (settings.TryGetValue("ApiKey", out var apiKey))
                result.ApiKey = apiKey;
            if (settings.TryGetValue("DeploymentName", out var deployment))
                result.DeploymentName = deployment;
            if (settings.TryGetValue("MaxTokens", out var maxTokens) && int.TryParse(maxTokens, out var mt))
                result.MaxTokens = mt;
            if (settings.TryGetValue("Temperature", out var temp) && float.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out var t))
                result.Temperature = t;

            return result;
        }
    }

    public class VertexAISettings
    {
        public bool Enabled { get; set; } = false;
        public string ProjectId { get; set; }
        public string Location { get; set; } = "us-central1";
        public string ApiKey { get; set; }
        public string ModelName { get; set; } = "gemini-2.0-flash";
        public int MaxOutputTokens { get; set; } = 8192;
        public float Temperature { get; set; } = 0.7f;

        public static VertexAISettings FromDictionary(IDictionary<string, string> settings, bool enabled)
        {
            var result = new VertexAISettings { Enabled = enabled };

            if (settings.TryGetValue("ProjectId", out var projectId))
                result.ProjectId = projectId;
            if (settings.TryGetValue("Location", out var location))
                result.Location = location;
            if (settings.TryGetValue("ApiKey", out var apiKey))
                result.ApiKey = apiKey;
            if (settings.TryGetValue("ModelName", out var model))
                result.ModelName = model;
            if (settings.TryGetValue("MaxOutputTokens", out var maxTokens) && int.TryParse(maxTokens, out var mt))
                result.MaxOutputTokens = mt;
            if (settings.TryGetValue("Temperature", out var temp) && float.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out var t))
                result.Temperature = t;

            return result;
        }
    }
}
