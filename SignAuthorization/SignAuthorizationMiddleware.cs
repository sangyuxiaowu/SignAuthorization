using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;

namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 中间件，用于处理基于签名的授权验证。
    /// </summary>
    public class SignAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SignAuthorizationOptions _options;
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化 SignAuthorizationMiddleware 的新实例。
        /// </summary>
        /// <param name="next">表示要执行的下一个中间件的委托。</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
        /// <param name="options">签名授权选项，包含用于验证的配置信息。</param>
        /// <exception cref="ArgumentNullException">当 'next' 参数为 null 时引发。</exception>
        public SignAuthorizationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, SignAuthorizationOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options;
            _logger = loggerFactory.CreateLogger<SignAuthorizationMiddleware>();
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

                var sTimeStamp = GetHeaderValue(context, _options.nTimeStamp);
                var sNonce = GetHeaderValue(context, _options.nNonce);
                var sSign = GetHeaderValue(context, _options.nSign);

                // 检查验签参数
                if (sTimeStamp.Count > 0 && sSign.Count > 0 && sNonce.Count > 0)
                {
                    // 检查时间
                    var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if (unixTimestamp - Convert.ToDouble(sTimeStamp[0]) <= _options.Expire)
                    {
                        
                        // 签名包含路径
                        var sPath = _options.WithPath ? context.Request.Path.Value : "";
                        // 处理额外参数
                        var sExtra = "";
                        if (!string.IsNullOrEmpty(_options.nExtra))
                        {
                            var sExtraList = GetHeaderValue(context, _options.nExtra);
                            if (sExtraList.Count > 0)
                            {
                                sExtra = sExtraList[0];
                            }
                            else
                            {
                                // 要求进行额外参数参与验签，但是没有额外参数，直接赋值，让验签失败
                                sExtra = "Err.";
                                // 额外参数为空时，签名错误
                                _logger.LogWarning("SignAuthorization: Extra parameter is empty.");
                            }
                        }
                        var sign = MakeSignAuthorization.MakeSign(_options.sToken, sTimeStamp[0], sNonce[0], sExtra, sPath);

                        // 验签通过
                        if (sSign[0] == sign)
                        {
                            await _next(context);
                            return;
                        }else
                        {
                            // 签名错误
                            _logger.LogWarning("SignAuthorization: Signature error.");
                        }

                    }else
                    {
                        // 时间戳过期
                        _logger.LogWarning("SignAuthorization: Timestamp expired.");
                    }

                }else
                {
                    // 时间戳、随机数、签名参数不全
                    _logger.LogWarning("SignAuthorization: Invalid request.");
                }

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(_options.UnauthorizedBack));
                return;
            }

            await _next(context);
        }


        private StringValues GetHeaderValue(HttpContext context, string key)
        {
            if (_options.UseHeader)
            {
                return context.Request.Headers.TryGetValue(key, out var headerValue) ? headerValue : StringValues.Empty;
            }
            else
            {
                return context.Request.Query.TryGetValue(key, out var queryValue) ? queryValue : StringValues.Empty;
            }
        }
    }
}