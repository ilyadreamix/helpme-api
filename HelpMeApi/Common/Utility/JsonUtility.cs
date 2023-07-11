using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace HelpMeApi.Common.Utility;

public static class JsonSerializerUtility
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public static string SerializeObject(this object obj) =>
        JsonSerializer.Serialize(obj, Options);

    public static byte[] SerializeObjectToByteArray(this object obj)
    {
        var json = JsonSerializer.Serialize(obj, Options);
        return Encoding.UTF8.GetBytes(json);
    }

    public static T? DeserializeObject<T>(this string json) =>
        JsonSerializer.Deserialize<T>(json, Options);
}
