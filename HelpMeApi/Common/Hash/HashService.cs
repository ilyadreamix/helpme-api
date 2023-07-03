using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HelpMeApi.Common.Hash;

public class HashService
{
    private readonly HashSettings _settings;

    public HashService(IOptions<HashSettings> settings)
    {
        _settings = settings.Value;
    }

    public string ComputePinCodeHash(string pinCode) => ToSha256(pinCode, _settings.PinCodeHashKey);
    
    private static string ToSha256(string input, string key)
    {
        var hash = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var digest = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(digest);
    }
}
