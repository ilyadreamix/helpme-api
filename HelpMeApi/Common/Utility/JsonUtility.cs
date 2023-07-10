using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HelpMeApi.Common.Utility;

public static class JsonSerializerUtility
{
    public static string SerializeObject(this object obj)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        return JsonConvert.SerializeObject(obj, settings);
    }
    
    public static byte[] SerializeObjectToByteArray(this object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        return Encoding.UTF8.GetBytes(json);
    }

    public static T? DeserializeObject<T>(this string json)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        return JsonConvert.DeserializeObject<T>(json, settings);
    }
}
