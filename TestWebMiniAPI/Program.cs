using Sang.AspNetCore.SignAuthorization;
using System.Collections;
using System.Reflection.Metadata;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSignAuthorization(opt => {
    opt.sToken = "you-api-token";
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/test", () =>
{
    var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    var sNonce = Guid.NewGuid().ToString();
    // ×ÖµäÅÅĞò
    ArrayList AL = new ArrayList();
    AL.Add("you-api-token");
    AL.Add(unixTimestamp.ToString());
    AL.Add(sNonce);
    AL.Sort(StringComparer.Ordinal);

    // ¼ÆËã SHA1
    var raw = string.Join("", AL.ToArray());
    using System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
    byte[] encry = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
    string sign = string.Join("", encry.Select(b => string.Format("{0:x2}", b)).ToArray()).ToLower();

    return Results.Redirect($"/weatherforecast?timestamp={unixTimestamp}&nonce={sNonce}&signature={sign}");
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).WithMetadata(new SignAuthorizeAttribute());

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}