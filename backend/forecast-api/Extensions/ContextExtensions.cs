using System.Net;

namespace forecast_api.Extensions
{
    public static class ContextExtensions
    {
        public static string? GetIpAddress(this HttpContext context)
        {
            var ipAddress = context.GetServerVariable("HTTP_X_FORWARDED_FOR");

            if(ipAddress == null)
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if(remoteIpAddress != null && !IPAddress.IsLoopback(remoteIpAddress))
                {
                    ipAddress = remoteIpAddress.ToString();
                }
            }

            return ipAddress;
        }
    }
}