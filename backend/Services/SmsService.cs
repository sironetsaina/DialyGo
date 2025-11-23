using System.Text;
using System.Text.Json;

public class SmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SmsService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient();
        _config = config;

        _httpClient.BaseAddress = new Uri(_config["Infobip:BaseUrl"]);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"App {_config["Infobip:ApiKey"]}");
    }

    public async Task<bool> SendSms(string phoneNumber, string message)
    {
        var payload = new
        {
            messages = new[]
            {
                new
                {
                    from = _config["Infobip:Sender"] ?? "DialyGo",
                    destinations = new[] { new { to = phoneNumber } },
                    text = message
                }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/sms/2/text/advanced", content);
        return response.IsSuccessStatusCode;
    }
}
