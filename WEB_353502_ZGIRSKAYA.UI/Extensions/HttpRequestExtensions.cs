using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace WEB_353502_ZGIRSKAYA.UI.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers != null)
            {
                string? headerValue = request.Headers["X-Requested-With"].ToString();
                if (string.Equals(headerValue, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (request.Query.TryGetValue("ajax", out StringValues ajaxValue) &&
                string.Equals(ajaxValue.ToString(), "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
