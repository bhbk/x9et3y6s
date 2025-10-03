using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class JobTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "jobs",
            Description = "Query scheduled jobs and their settings. Supports listing all jobs, getting by ID, searching by name, viewing job settings (secret values are masked), and counting total records.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': {
                        'type': 'string',
                        'enum': ['count', 'list', 'get', 'search', 'settings'],
                        'description': 'The action to perform. settings returns config key-value pairs for a job (secret values are masked).'
                    },
                    'id': {
                        'type': 'string',
                        'description': 'Job ID (GUID) for get/settings actions'
                    },
                    'query': {
                        'type': 'string',
                        'description': 'Search query for job name'
                    },
                    'skip': {
                        'type': 'integer',
                        'description': 'Number of records to skip (default: 0)'
                    },
                    'take': {
                        'type': 'integer',
                        'description': 'Number of records to take (default: 50, max: 100)'
                    }
                },
                'required': ['action']
            }")
        };

        public JobTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 50, 100);

            try
            {
                switch (action)
                {
                    case "count":
                        var total = _uow.Jobs.Get(x => true).Count();
                        return Task.FromResult(MCPToolResult.Ok(new JObject { ["total"] = total }));

                    case "list":
                        return Task.FromResult(ListJobs(skip, take));

                    case "get":
                        var id = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var jobId))
                            return Task.FromResult(MCPToolResult.Fail("Valid job ID is required"));
                        return Task.FromResult(GetJob(jobId));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchJobs(query, skip, take));

                    case "settings":
                        var settingsId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(settingsId) || !Guid.TryParse(settingsId, out var settingsJobId))
                            return Task.FromResult(MCPToolResult.Fail("Valid job ID is required"));
                        return Task.FromResult(GetSettings(settingsJobId));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListJobs(int skip, int take)
        {
            var jobs = _uow.Jobs.Get(x => true)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.Jobs.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["jobs"] = JArray.FromObject(jobs, _serializer)
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetJob(Guid id)
        {
            var job = _uow.Jobs.Get(x => x.Id == id).FirstOrDefault();
            if (job == null)
                return MCPToolResult.Fail($"Job not found: {id}");

            var result = JObject.FromObject(job, _serializer);
            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchJobs(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();
            var jobs = _uow.Jobs.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery))
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = jobs.Count,
                ["jobs"] = JArray.FromObject(jobs, _serializer)
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetSettings(Guid jobId)
        {
            var job = _uow.Jobs.Get(x => x.Id == jobId).FirstOrDefault();
            if (job == null)
                return MCPToolResult.Fail($"Job not found: {jobId}");

            var settings = _uow.JobSettings.Get(x => x.JobId == jobId)
                .OrderBy(x => x.ConfigKey)
                .ToList();

            var settingsArray = new JArray();
            foreach (var s in settings)
            {
                settingsArray.Add(new JObject
                {
                    ["id"] = s.Id.ToString(),
                    ["configKey"] = s.ConfigKey,
                    ["configValue"] = s.IsSecret ? "********" : s.ConfigValue,
                    ["isSecret"] = s.IsSecret,
                });
            }

            var result = new JObject
            {
                ["jobId"] = jobId.ToString(),
                ["jobName"] = job.Name,
                ["settings"] = settingsArray
            };

            return MCPToolResult.Ok(result);
        }
    }
}
