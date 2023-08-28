using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using shortid;
using shortid.Configuration;
using API.Entities;
using System.Text.Json;
using System.Globalization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlShortner : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly IConfiguration _configuration;
        public UrlShortner(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _configuration = configuration;
            _database = redis.GetDatabase();
        }

        [HttpPost("CreateShortId")]
        public async Task<string> GetShortId([FromBody] GivenUrls redirectUrl)
        {
            if(redirectUrl.RedirectUrl.Length == 0) return "Please enter url";
            string shortId = ShortId.Generate();
            Url url = new Url(redirectUrl.RedirectUrl);
            await _database.StringSetAsync(shortId, JsonSerializer.Serialize(url), TimeSpan.FromDays(90));
            return JsonSerializer.Serialize(_configuration.GetSection("AppSettings")["laucnhUrl"] + "api/UrlShortner/" + shortId);
        }

        [HttpGet("GetDetails/{shortId}")]
        public async Task<Url> GetUrlDetails(string shortId)
        {
            var url = await _database.StringGetAsync(shortId);
            return url.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Url>(url);
        }
        
        [HttpGet("{shortId}")]
        public async  Task<RedirectResult> RedirectToDestination(string shortId)
        {
            var url = await _database.StringGetAsync(shortId);
            Url redirectUrl = JsonSerializer.Deserialize<Url>(url);
            redirectUrl.Ckicks++;
            DateTime localDate = DateTime.Now;
            redirectUrl.VisitHistory.Add(localDate.ToString("dddd, dd MMMM yyyy hh:mm tt"));
            await _database.StringSetAsync(shortId, JsonSerializer.Serialize(redirectUrl), TimeSpan.FromDays(90));
            return Redirect(redirectUrl.RedirectUrl);
        }
    }
}

