using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using JevApplicationTeste.Api.Models;
using Microsoft.Extensions.Options;

namespace JevApplicationTeste.Api.Services;

public sealed class JevOptions
{
    public bool UseMock { get; set; } = true;
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "jev-latest";
    public double MinConfidence { get; set; } = 0.8;
}

public sealed class JevIntegrationException(string message) : Exception(message);

public sealed class JevClient(HttpClient http, IOptions<JevOptions> options)
{
    public async Task<JevResponse> EvaluateAsync(JevRequest payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/systemone");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
        request.Content = JsonContent.Create(payload);
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new JevIntegrationException($"TypeSafe retornou HTTP {(int)response.StatusCode}.");
        try
        {
            return await response.Content.ReadFromJsonAsync<JevResponse>(cancellationToken: ct)
                ?? throw new JevIntegrationException("TypeSafe retornou resposta vazia.");
        }
        catch (JsonException)
        {
            throw new JevIntegrationException("TypeSafe retornou JSON incompatível.");
        }
    }
}
