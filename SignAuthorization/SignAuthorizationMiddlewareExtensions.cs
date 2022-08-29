namespace Sang.AspNetCore.SignAuthorization
{
    public static class SignAuthorizationMiddlewareExtensions
    {
        public static void UseSignAuthorization(this WebApplication app)
        {
            app.UseMiddleware<SignAuthorizationMiddleware>(new SignAuthorizationOptions());
        }

        public static void UseSignAuthorization(this WebApplication app, Action<SignAuthorizationOptions> configureOptions)
        {
            var options = new SignAuthorizationOptions();
            configureOptions(options);
            app.UseMiddleware<SignAuthorizationMiddleware>(options);
        }
    }
}
