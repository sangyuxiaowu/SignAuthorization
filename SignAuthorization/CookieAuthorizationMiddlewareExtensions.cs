namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 提供 CookieAuthorizationMiddleware 扩展方法的静态类。
    /// </summary>
    public static class CookieAuthorizationMiddlewareExtensions
    {
        /// <summary>
        /// 使用默认的 CookieAuthorizationOptions 配置 CookieAuthorizationMiddleware。
        /// </summary>
        /// <param name="app">要配置中间件的 WebApplication 实例。</param>
        public static void UseCookieAuthorization(this WebApplication app)
        {
            app.UseMiddleware<CookieAuthorizationMiddleware>(new CookieAuthorizationOptions());
        }

        /// <summary>
        /// 使用自定义 CookieAuthorizationOptions 配置 CookieAuthorizationMiddleware。
        /// </summary>
        /// <param name="app">要配置中间件的 WebApplication 实例。</param>
        /// <param name="configureOptions">用于配置 CookieAuthorizationOptions 的委托。</param>
        public static void UseCookieAuthorization(this WebApplication app, Action<CookieAuthorizationOptions> configureOptions)
        {
            var options = new CookieAuthorizationOptions();
            configureOptions(options);
            options.Validate();
            app.UseMiddleware<CookieAuthorizationMiddleware>(options);
        }
    }
}
