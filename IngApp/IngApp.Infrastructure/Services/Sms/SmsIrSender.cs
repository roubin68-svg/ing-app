using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace IngApp.Infrastructure.Services.Sms;

public class SmsIrSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SmsIrSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task SendOtpAsync(string phoneNumber, string code)
    {
        var apiKey = _config["SmsIr:ApiKey"];
        var templateId = _config["SmsIr:TemplateId"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("SmsIr ApiKey is missing.");

        if (string.IsNullOrWhiteSpace(templateId))
            throw new Exception("SmsIr TemplateId is missing.");

        var url = "https://api.sms.ir/v1/send/verify";

        var payload = new
        {
            mobile = phoneNumber,
            templateId = int.Parse(templateId),
            parameters = new[]
            {
                new { name = "code", value = code }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", apiKey);
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"SMS.ir Error ({response.StatusCode}): {content}");
        }

        Console.WriteLine($"[SMS.ir] OTP sent to {phoneNumber} (code: {code})");
    }
}
