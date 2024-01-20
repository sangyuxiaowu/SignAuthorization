using System.Collections;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 中间件，用于处理基于签名的授权验证。
    /// </summary>
    public class SignAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SignAuthorizationOptions _options;

        /// <summary>
        /// 初始化 SignAuthorizationMiddleware 的新实例。
        /// </summary>
        /// <param name="next">表示要执行的下一个中间件的委托。</param>
        /// <param name="options">签名授权选项，包含用于验证的配置信息。</param>
        /// <exception cref="ArgumentNullException">当 'next' 参数为 null 时引发。</exception>
        public SignAuthorizationMiddleware(RequestDelegate next, SignAuthorizationOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options;
        }

        /// <summary>  
        /// 处理签名授权验证的核心方法。  
        /// </summary>  
        /// <param name="context">当前的 HTTP 上下文。</param>  
        /// <exception cref="ArgumentNullException">当 'context' 参数为 null 时引发。</exception>  
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
                        var parameterList = new ArrayList();
                        parameterList.Add(_options.sToken);
                        parameterList.Add(sTimeStamp[0]);
                        parameterList.Add(sNonce[0]);
                        // 签名包含路径
                        if (_options.WithPath) parameterList.Add(context.Request.Path.Value);
                        parameterList.Sort(StringComparer.Ordinal);

                        // 添加额外参数
                        if (!string.IsNullOrEmpty(_options.nExtra))
                        {
                            var sExtra = context.Request.Query[_options.nExtra];
                            if (sExtra.Count > 0)
                            {
                                parameterList.Add(sExtra[0]);
                            }
                        }

                        // 计算 SHA1
                        var raw = string.Join("", parameterList.ToArray());
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