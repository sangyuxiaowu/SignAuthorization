using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 中间件，用于处理基于Cookie的授权验证。
    /// </summary>
    public class CookieAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CookieAuthorizationOptions _options;
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化 CookieAuthorizationMiddleware 的新实例。
        /// </summary>
        /// <param name="next">表示要执行的下一个中间件的委托。</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
        /// <param name="options">Cookie授权选项，包含用于验证的配置信息。</param>
        /// <exception cref="ArgumentNullException">当 'next' 参数为 null 时引发。</exception>
        public CookieAuthorizationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, CookieAuthorizationOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options;
            _logger = loggerFactory.CreateLogger<CookieAuthorizationMiddleware>();
        }

        /// <summary>
        /// 处理Cookie授权验证的核心方法。
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
            var authorizeAttribute = endpoint?.Metadata.GetMetadata<CookieAuthorizeAttribute>();
            if (authorizeAttribute == null)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Cookies.TryGetValue(_options.CookieName, out var cookieValue))
            {
                await Deny(context, "Cookie is missing.");
                return;
            }

            var parts = cookieValue.Split(new[] { _options.CookieSeparator }, StringSplitOptions.None);
            if (parts.Length != 3)
            {
                await Deny(context, "Cookie format error.");
                return;
            }

            var userName = parts[0];
            var timeStampText = parts[1];
            var sign = parts[2];

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(timeStampText) || string.IsNullOrWhiteSpace(sign))
            {
                await Deny(context, "Cookie data is invalid.");
                return;
            }

            if (!long.TryParse(timeStampText, out var timeStamp))
            {
                await Deny(context, "Cookie timestamp is invalid.");
                return;
            }

            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (unixTimestamp - timeStamp > _options.Expire)
            {
                await Deny(context, "Cookie expired.");
                return;
            }

            var expectedSign = MakeSignAuthorization.MakeCookieSign(_options.sToken, userName, timeStampText);
            if (!string.Equals(sign, expectedSign, StringComparison.OrdinalIgnoreCase))
            {
                await Deny(context, "Cookie signature error.");
                return;
            }

            if (authorizeAttribute.Users.Length > 0)
            {
                if (!authorizeAttribute.Users.Contains(userName, StringComparer.OrdinalIgnoreCase))
                {
                    await Deny(context, "User not allowed.");
                    return;
                }
            }
            else if (_options.AllowedUsers.Count > 0 && !_options.AllowedUsers.Contains(userName))
            {
                await Deny(context, "User not allowed.");
                return;
            }

            var identity = new ClaimsIdentity(new[] { new Claim(_options.UserNameClaimType, userName) }, "SignAuthorizationCookie");
            context.User = new ClaimsPrincipal(identity);
            context.Items[_options.UserNameItemKey] = userName;

            if (_options.ReuseExpire)
            {
                var refreshedTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                var refreshedCookie = MakeSignAuthorization.MakeCookieValue(_options.sToken, userName, refreshedTimeStamp, _options.CookieSeparator);
                var cookieOptions = CloneCookieOptions(_options.CookieOptions);
                cookieOptions.Expires = DateTimeOffset.Now.AddSeconds(_options.Expire);
                context.Response.Cookies.Append(_options.CookieName, refreshedCookie, cookieOptions);
            }

            await _next(context);
        }

        private CookieOptions CloneCookieOptions(CookieOptions options)
        {
            return new CookieOptions
            {
                Domain = options.Domain,
                Path = options.Path,
                Expires = options.Expires,
                HttpOnly = options.HttpOnly,
                Secure = options.Secure,
                SameSite = options.SameSite,
                MaxAge = options.MaxAge,
                IsEssential = options.IsEssential
            };
        }

        private async Task Deny(HttpContext context, string reason)
        {
            _logger.LogWarning("CookieAuthorization: {Reason}", reason);
            context.Response.StatusCode = _options.UnauthorizedStatusCode;
            context.Response.ContentType = "application/json";
            if (!string.IsNullOrEmpty(_options.UnauthorizedBackJson))
            {
                await context.Response.WriteAsync(_options.UnauthorizedBackJson);
            }
            else
            {
                await context.Response.WriteAsync(JsonSerializer.Serialize(_options.UnauthorizedBack));
            }
        }
    }
}
