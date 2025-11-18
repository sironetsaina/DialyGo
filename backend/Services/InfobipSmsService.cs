using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace backend.Services
{
    public class InfobipSmsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _sender;

        public InfobipSmsService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Infobip:ApiKey"];
            _baseUrl = config["Infobip:BaseUrl"];
            _sender = config["Infobip:Sender"];
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            try
            {
                var payload = new
                {
                    messages = new[]
                    {
                        new
                        {
                            from = _sender,
                            destinations = new[]
                            {
                                new { to = to }
                            },
                            text = message
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("App", _apiKey);

                var response = await _httpClient.PostAsync($"{_baseUrl}/sms/2/text/advanced", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ SMS sent successfully to {to}");
                    return true;
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ SMS failed: {error}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 SMS Error: {ex.Message}");
                return false;
            }
        }
    }
}
