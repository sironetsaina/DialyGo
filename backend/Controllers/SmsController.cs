using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SmsController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;

            // Set base address from config
            _httpClient.BaseAddress = new Uri(_config["Infobip:BaseUrl"]);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"App {_config["Infobip:ApiKey"]}");
        }

        public SmsController(HttpClient client, IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Phone number and message are required.");

            var payload = new
            {
                messages = new[]
                {
                    new
                    {
                        from = "DialyGo",
                        destinations = new[] { new { to = request.PhoneNumber } },
                        text = request.Message
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { success = false, error = result });

            return Ok(new { success = true, message = "SMS sent successfully!", response = result });
        }

        public class SmsRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}
