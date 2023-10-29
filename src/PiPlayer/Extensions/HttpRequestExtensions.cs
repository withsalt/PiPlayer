using System.Text;
using Microsoft.AspNetCore.Http;

namespace PiPlayer.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetHostUri(this HttpRequest request)
        {
            return new StringBuilder()
             .Append(request.Scheme)
             .Append("://")
             .Append(request.Host)
             .Append(request.PathBase)
             .ToString();
        }

    }
}
