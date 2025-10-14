using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Lib.Identity.MCP.Services
{
    public class MCPSchemaService
    {
        private readonly IUnitOfWork _uow;
        private static readonly Dictionary<string, JObject> _schemaCache = new Dictionary<string, JObject>();

        public MCPSchemaService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public JObject GetEntitySchema(string entityName)
        {
            if (_schemaCache.TryGetValue(entityName, out var cached))
                return cached;

            var schema = BuildEntitySchema(entityName);
            if (schema != null)
                _schemaCache[entityName] = schema;

            return schema;
        }

        public JArray GetAllEntitySchemas()
        {
            var schemas = new JArray();

            var entityTypes = new[]
            {
                "User", "Audience", "Issuer", "Role", "Claim", "LoginProvider",
                "AuthActivity", "Setting", "State", "Quote", "Url",
                "ChatConversation", "ChatMessage", "ChatPrompt",
                "LLMProvider", "LLMProviderSetting", "Job", "JobSetting",
                "EmailQueue", "TextQueue", "ChatFile"
            };

            foreach (var entityType in entityTypes)
            {
                var schema = GetEntitySchema(entityType);
                if (schema != null)
                    schemas.Add(schema);
            }

            return schemas;
        }

        private JObject BuildEntitySchema(string entityName)
        {
            var modelType = GetModelType(entityName);
            if (modelType == null)
                return null;

            var schema = new JObject
            {
                ["name"] = entityName,
                ["tableName"] = $"tbl_{entityName}",
                ["properties"] = BuildPropertiesSchema(modelType)
            };

            return schema;
        }

        private Type GetModelType(string entityName)
        {
            var assembly = typeof(IUnitOfWork).Assembly;
            var typeName = $"Bhbk.Lib.Identity.Data.EF.Models.tbl_{entityName}";
            return assembly.GetType(typeName);
        }

        private JArray BuildPropertiesSchema(Type modelType)
        {
            var properties = new JArray();

            foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (SensitiveFieldFilter.IsSensitive(prop.Name))
                    continue;

                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    continue;

                if (prop.PropertyType.Namespace?.StartsWith("Bhbk.Lib.Identity") == true &&
                    !prop.PropertyType.IsEnum)
                    continue;

                var propSchema = new JObject
                {
                    ["name"] = prop.Name,
                    ["type"] = GetJsonType(prop.PropertyType),
                    ["nullable"] = IsNullable(prop.PropertyType)
                };

                properties.Add(propSchema);
            }

            return properties;
        }

        private string GetJsonType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType == typeof(string))
                return "string";
            if (underlyingType == typeof(Guid))
                return "string (uuid)";
            if (underlyingType == typeof(bool))
                return "boolean";
            if (underlyingType == typeof(int) || underlyingType == typeof(long))
                return "integer";
            if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
                return "number";
            if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
                return "string (datetime)";

            return "string";
        }

        private bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
