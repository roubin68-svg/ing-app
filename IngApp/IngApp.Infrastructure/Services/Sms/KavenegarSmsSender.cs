using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace IngApp.Infrastructure.Services.Sms;

public class KavenegarSmsSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public KavenegarSmsSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task SendMessageAsync(string phoneNumber, string message)
    {
        var apiKey = _config["Kavenegar:ApiKey"];
        var sender = _config["Kavenegar:Sender"];

        var url = $"https://api.kavenegar.com/v1/{apiKey}/sms/send.json";

        var payload = new
        {
            message = message,
            sender = sender,
            receptor = phoneNumber
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Kavenegar SMS Error ({response.StatusCode}): {content}");
        }
    }
}
