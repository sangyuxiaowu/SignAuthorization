using Sang.AspNetCore.SignAuthorization;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var cookieOptions = new CookieAuthorizationOptions
{
    sToken = "you-api-token",
    CookieName = "SignAuthorization",
    Expire = 3600,
    ReuseExpire = true
};

cookieOptions.AllowedUsers.UnionWith(new[] { "root", "admin", "init" });

app.UseCookieAuthorization(opt =>
{
    opt.sToken = cookieOptions.sToken;
    opt.CookieName = cookieOptions.CookieName;
    opt.Expire = cookieOptions.Expire;
    opt.ReuseExpire = cookieOptions.ReuseExpire;
    opt.CookieSeparator = cookieOptions.CookieSeparator;
    opt.AllowedUsers.UnionWith(cookieOptions.AllowedUsers);
});

app.MapGet("/login/{user}", (string user, HttpContext context) =>
{
    var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    var cookieValue = MakeSignAuthorization.MakeCookieValue(
        cookieOptions.sToken,
        user,
        timeStamp,
        cookieOptions.CookieSeparator);

    context.Response.Cookies.Append(
        cookieOptions.CookieName,
        cookieValue,
        new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.Now.AddSeconds(cookieOptions.Expire)
        });

    return Results.Ok(new { success = true, user });
});

app.MapGet("/secure", (HttpContext context) =>
{
    return Results.Ok(new
    {
        user = context.User.Identity?.Name,
        item = context.Items[cookieOptions.UserNameItemKey]
    });
}).WithMetadata(new CookieAuthorizeAttribute("root", "admin"));

app.Run();
