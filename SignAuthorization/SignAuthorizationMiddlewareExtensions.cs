namespace Sang.AspNetCore.SignAuthorization
{

    /// <summary>
    /// 提供 SignAuthorizationMiddleware 扩展方法的静态类。
    /// </summary>
    public static class SignAuthorizationMiddlewareExtensions
    {
        /// <summary>  
        /// 使用默认的 SignAuthorizationOptions 配置 SignAuthorizationMiddleware。  
        /// </summary>  
        /// <param name="app">要配置中间件的 WebApplication 实例。</param> 
        public static void UseSignAuthorization(this WebApplication app)
        {
            app.UseMiddleware<SignAuthorizationMiddleware>(new SignAuthorizationOptions());
        }

        /// <summary>  
        /// 使用自定义 SignAuthorizationOptions 配置 SignAuthorizationMiddleware。  
        /// </summary>  
        /// <param name="app">要配置中间件的 WebApplication 实例。</param>  
        /// <param name="configureOptions">用于配置 SignAuthorizationOptions 的委托。</param> 
        public static void UseSignAuthorization(this WebApplication app, Action<SignAuthorizationOptions> configureOptions)
        {
            var options = new SignAuthorizationOptions();
            configureOptions(options);
            app.UseMiddleware<SignAuthorizationMiddleware>(options);
        }
    }
}
