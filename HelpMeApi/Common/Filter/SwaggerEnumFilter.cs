using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpMeApi.Common.Filter;

public class SwaggerEnumFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }
        
        var enumType = context.Type;
        var description = string.Empty;

        schema.Type = "int";
        schema.Format = null;
        schema.Enum.Clear();

        foreach (var enumValue in System.Enum.GetValues(enumType))
        {
            var name = enumValue.ToString();
            var value = (int)System.Enum.Parse(enumType, name!);
            schema.Enum.Add(new OpenApiInteger(value));
            description += $"{name} ({value}), ";
        }

        schema.Description = description[..^2];
    }
}
