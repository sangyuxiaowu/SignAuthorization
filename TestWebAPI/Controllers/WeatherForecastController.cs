using Microsoft.AspNetCore.Mvc;
using Sang.AspNetCore.SignAuthorization;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [SignAuthorize]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("test")]

        public IActionResult Test() {
            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var sNonce = Guid.NewGuid().ToString();
            // ×ÖµäÅÅÐò
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

            return Redirect($"/weatherforecast?timestamp={unixTimestamp}&nonce={sNonce}&signature={sign}");
        }
    }
}