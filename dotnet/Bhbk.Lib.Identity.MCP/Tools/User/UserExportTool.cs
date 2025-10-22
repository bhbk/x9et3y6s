using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.User
{
    public class UserExportTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private readonly Guid _userId;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "export",
            Description = "Export your data as a downloadable CSV or JSON file.",
            Scope = MCPScope.User,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'entity': { 'type': 'string', 'enum': ['profile', 'sessions', 'activity'], 'description': 'The type of your data to export.' },
                    'format': { 'type': 'string', 'enum': ['csv', 'json'], 'description': 'Output file format (default: csv).' }
                },
                'required': ['entity']
            }")
        };

        public UserExportTool(IUnitOfWork uow, Guid userId)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userId = userId;
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var entity = parameters["entity"]?.ToString()?.ToLower();
            var format = parameters["format"]?.ToString()?.ToLower() ?? "csv";

            if (string.IsNullOrEmpty(entity))
                return Task.FromResult(MCPToolResult.Fail("Entity type is required"));

            if (format != "csv" && format != "json")
                return Task.FromResult(MCPToolResult.Fail("Format must be csv or json"));

            try
            {
                var data = QueryEntity(entity);
                if (data == null)
                    return Task.FromResult(MCPToolResult.Fail($"Unknown entity: {entity}"));

                var filtered = SensitiveFieldFilter.Filter(data);
                var items = filtered as JArray ?? new JArray();
                var bytes = format == "csv" ? ToCsvBytes(items) : ToJsonBytes(items);
                var ext = format == "csv" ? "csv" : "json";
                var contentType = format == "csv" ? "text/csv" : "application/json";
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

                var result = new JObject
                {
                    ["type"] = "file_export",
                    ["fileName"] = $"my_{entity}_{timestamp}.{ext}",
                    ["contentType"] = contentType,
                    ["contentBase64"] = Convert.ToBase64String(bytes),
                    ["summary"] = $"Exported {items.Count} {entity} records as {format.ToUpper()}"
                };

                return Task.FromResult(MCPToolResult.Ok(result));
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Export error: {ex.Message}"));
            }
        }

        private JArray QueryEntity(string entity)
        {
            var serializer = JsonSerializer.Create(_jsonSettings);

            switch (entity)
            {
                case "profile":
                {
                    var user = _uow.Users.Get(x => x.Id == _userId).FirstOrDefault();
                    if (user == null)
                        return new JArray();
                    return new JArray(JObject.FromObject(user, serializer));
                }

                case "sessions":
                {
                    var refreshes = _uow.Refreshes.Get(x => x.UserId == _userId)
                        .OrderByDescending(x => x.Issued)
                        .ToList();
                    return JArray.FromObject(refreshes, serializer);
                }

                case "activity":
                {
                    var activity = _uow.UserAuthActivities.Get(x => x.UserId == _userId)
                        .OrderByDescending(x => x.Created)
                        .Take(500)
                        .ToList();
                    return JArray.FromObject(activity, serializer);
                }

                default:
                    return null;
            }
        }

        private byte[] ToCsvBytes(JArray items)
        {
            if (items.Count == 0)
                return Encoding.UTF8.GetBytes("No data");

            var headers = new List<string>();
            foreach (var prop in ((JObject)items[0]).Properties())
                headers.Add(prop.Name);

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (JObject item in items)
            {
                var values = headers.Select(h => EscapeCsv(item[h]?.ToString() ?? ""));
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] ToJsonBytes(JArray items)
        {
            return Encoding.UTF8.GetBytes(items.ToString(Formatting.Indented));
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
