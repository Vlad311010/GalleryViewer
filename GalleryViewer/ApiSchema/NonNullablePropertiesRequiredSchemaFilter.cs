using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace GalleryViewer.ApiSchema
{
    internal sealed class NonNullablePropertiesRequiredSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null)
                return;

            foreach (var property in context.Type.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (!IsNonNullable(property))
                    continue;

                var jsonName = GetJsonPropertyName(property);

                if (schema.Properties.ContainsKey(jsonName))
                    schema.Required.Add(jsonName);
            }
        }

        private static bool IsNonNullable(PropertyInfo property)
        {
            // Value types:
            // int, bool, DateTime, etc. are non-nullable unless Nullable<T>.
            if (property.PropertyType.IsValueType)
                return Nullable.GetUnderlyingType(property.PropertyType) is null;

            // Reference types:
            // string vs string?, IEnumerable<T> vs IEnumerable<T>?
            var nullability = new NullabilityInfoContext()
                .Create(property);

            return nullability.ReadState == NullabilityState.NotNull;
        }

        private static string GetJsonPropertyName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();

            return attribute?.Name
                ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
        }
    }
}
