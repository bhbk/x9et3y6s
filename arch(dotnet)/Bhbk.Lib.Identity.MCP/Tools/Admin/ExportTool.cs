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

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class ExportTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "export",
            Description = "Export identity data as a downloadable CSV or JSON file.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'entity': { 'type': 'string', 'enum': ['users', 'audiences', 'issuers', 'roles', 'claims', 'logins', 'activity', 'email_queue', 'text_queue'], 'description': 'The type of identity data to export.' },
                    'format': { 'type': 'string', 'enum': ['csv', 'json'], 'description': 'Output file format (default: csv).' },
                    'query': { 'type': 'string', 'description': 'Optional search filter to narrow the exported data.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to export (default: 1000, max: 10000).' }
                },
                'required': ['entity']
            }")
        };

        public ExportTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var entity = parameters["entity"]?.ToString()?.ToLower();
            var format = parameters["format"]?.ToString()?.ToLower() ?? "csv";
            var query = parameters["query"]?.ToString();
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 100, 1000);

            if (string.IsNullOrEmpty(entity))
                return Task.FromResult(MCPToolResult.Fail("Entity type is required"));

            if (format != "csv" && format != "json")
                return Task.FromResult(MCPToolResult.Fail("Format must be csv or json"));

            try
            {
                var data = QueryEntity(entity, query, skip, take);
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
                    ["fileName"] = $"{entity}_{timestamp}.{ext}",
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

        private JArray QueryEntity(string entity, string query, int skip, int take)
        {
            var serializer = JsonSerializer.Create(_jsonSettings);

            switch (entity)
            {
                case "users":
                {
                    var q = _uow.Users.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.UserName.ToLower().Contains(lq) || x.EmailAddress.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.UserName).Skip(skip).Take(take).ToList(), serializer);
                }

                case "audiences":
                {
                    var q = _uow.Audiences.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.Name.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.Name).Skip(skip).Take(take).ToList(), serializer);
                }

                case "issuers":
                {
                    var q = _uow.Issuers.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.Name.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.Name).Skip(skip).Take(take).ToList(), serializer);
                }

                case "roles":
                {
                    var q = _uow.Roles.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.Name.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.Name).Skip(skip).Take(take).ToList(), serializer);
                }

                case "claims":
                {
                    var q = _uow.Claims.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.Type.ToLower().Contains(lq) || x.Value.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.Type).Skip(skip).Take(take).ToList(), serializer);
                }

                case "logins":
                {
                    var q = _uow.LoginProviders.Get(x => true);
                    if (!string.IsNullOrEmpty(query))
                    {
                        var lq = query.ToLower();
                        q = q.Where(x => x.Name.ToLower().Contains(lq));
                    }
                    return JArray.FromObject(q.OrderBy(x => x.Name).Skip(skip).Take(take).ToList(), serializer);
                }

                case "activity":
                {
                    var q = _uow.AuthActivity.Get(x => true);
                    return JArray.FromObject(q.OrderByDescending(x => x.CreatedUtc).Skip(skip).Take(take).ToList(), serializer);
                }

                case "email_queue":
                {
                    var q = _uow.EmailQueue.Get(x => true);
                    return JArray.FromObject(q.OrderByDescending(x => x.CreatedUtc).Skip(skip).Take(take).ToList(), serializer);
                }

                case "text_queue":
                {
                    var q = _uow.TextQueue.Get(x => true);
                    return JArray.FromObject(q.OrderByDescending(x => x.CreatedUtc).Skip(skip).Take(take).ToList(), serializer);
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
