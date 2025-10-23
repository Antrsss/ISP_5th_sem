namespace WEB_353502_ZGIRSKAYA.UI.Services.Authentication
{
    public interface ITokenAccessor
    {
        Task SetAuthorizationHeaderAsync(HttpClient httpClient,
            bool isClient);
    }
}
