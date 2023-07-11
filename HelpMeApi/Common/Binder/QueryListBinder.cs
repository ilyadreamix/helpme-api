using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HelpMeApi.Common.Binder;

[AttributeUsage(AttributeTargets.Parameter)]
public class QueryListAttribute <T> : Attribute, IModelNameProvider, IBinderTypeProviderMetadata
{
    public QueryListAttribute()
    {
        // ...
    }
    
    public QueryListAttribute(string? name)
    {
        Name = name;
    }

    public string? Name { get; }
    public Type BinderType => typeof(QueryListBinder<T>);
    public BindingSource BindingSource => BindingSource.Query;
}

public class QueryListBinder<T> : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
            
        var values = valueProviderResult.FirstValue;
        var list = new List<T>();

        if (!string.IsNullOrEmpty(values))
        {
            var items = values.Split(',');

            foreach (var item in items)
            {
                if (typeof(T) == typeof(Guid) && Guid.TryParse(item, out var guidValue))
                {
                    list.Add((T)(object)guidValue);
                }
                else if (typeof(T) == typeof(string))
                {
                    list.Add((T)(object)item);
                }
                else
                {
                    bindingContext.ModelState.AddModelError(modelName, string.Empty); // Validator is suppressed
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }
            }
        }

        bindingContext.Result = ModelBindingResult.Success(list);
        return Task.CompletedTask;
    }
}
