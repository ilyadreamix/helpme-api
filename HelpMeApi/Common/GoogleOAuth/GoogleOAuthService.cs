using HelpMeApi.Common.GoogleOAuth.Model;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HelpMeApi.Common.GoogleOAuth;

public class GoogleOAuthService
{
    private readonly GoogleOAuthSettings _settings;
    private readonly HttpClient _httpClient;

    public GoogleOAuthService(IOptions<GoogleOAuthSettings> settings, HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    public async Task<(bool, GoogleOAuthIdTokenModel?)> VerifyIdToken(string idToken)
    {
        var query = new Dictionary<string, string>
        {
            {"id_token", idToken}
        };
        var url = QueryHelpers.AddQueryString(Constant.GoogleOAuthIdTokenEndpoint, query!);

        try
        {
            var response = await _httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<GoogleOAuthIdTokenModel>();

            return new ValueTuple<bool, GoogleOAuthIdTokenModel?>(
                result!.Audience == _settings.WebClientId,
                result);
        }
        catch
        {
            return new ValueTuple<bool, GoogleOAuthIdTokenModel?>(false, null);
        }
    }
}
