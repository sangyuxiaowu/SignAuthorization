using System.Collections;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace Sang.AspNetCore.SignAuthorization
{
    public class SignAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SignAuthorizationOptions _options;
        public SignAuthorizationMiddleware(RequestDelegate next, SignAuthorizationOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var endpoint = context.GetEndpoint();

            // 检查授权情况
            if (endpoint != null && endpoint.Metadata.Any(x => x is SignAuthorizeAttribute))
            {
                var sTimeStamp = context.Request.Query[_options.nTimeStamp];
                var sNonce = context.Request.Query[_options.nNonce];
                var sSign = context.Request.Query[_options.nSign];

                // 检查验签参数
                if (sTimeStamp.Count > 0 && sSign.Count > 0 && sNonce.Count > 0)
                {
                    // 检查时间
                    var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if (unixTimestamp - Convert.ToDouble(sTimeStamp[0]) <= _options.Expire)
                    {
                        // 字典排序
                        ArrayList AL = new ArrayList();
                        AL.Add(_options.sToken);
                        AL.Add(sTimeStamp[0]);
                        AL.Add(sNonce[0]);
                        // 签名包含路径
                        if (_options.WithPath) AL.Add(context.Request.Path.Value);
                        AL.Sort(StringComparer.Ordinal);

                        // 计算 SHA1
                        var raw = string.Join("", AL.ToArray());
                        using SHA1 sha1 = SHA1.Create();
                        byte[] encry = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
                        string sign = string.Join("", encry.Select(b => string.Format("{0:x2}", b)).ToArray()).ToLower();

                        // 验签通过
                        if (sSign[0] == sign)
                        {
                            await _next(context);
                            return;
                        }

                    }

                }

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(_options.UnauthorizedBack));
                return;
            }

            await _next(context);
        }
    }
}