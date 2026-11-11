using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MsAuthAdmin.Models;

namespace MsAuthAdmin.Services;

public sealed class AuthApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<ApiCall<IssueTokenResponse>> IssueTokenAsync(Guid uidUser, string apiKey, int idService, CancellationToken cancellationToken = default)
    {
        return PostAsync<IssueTokenResponse>(
            "api/Permissions/IssueToken",
            new { uidUser, apiKey, idService },
            token: null,
            cancellationToken);
    }

    public async Task<ApiCall<T>> GetAsync<T>(string path, string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendAsync<T>(request, cancellationToken);
    }

    public async Task<ApiCall<T>> PostAsync<T>(string path, object body, string? token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: Json)
        };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await SendAsync<T>(request, cancellationToken);
    }

    private async Task<ApiCall<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_http.BaseAddress is null)
        {
            return ApiCall<T>.Fail(0, "Falta configurar AuthApi:BaseUrl con la dirección del microservicio.");
        }

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ApiCall<T>.Fail((int)response.StatusCode, await ReadErrorAsync(response));
            }

            if (response.Content.Headers.ContentLength == 0)
            {
                return ApiCall<T>.Success(default);
            }

            var data = await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken);
            return ApiCall<T>.Success(data);
        }
        catch (JsonException)
        {
            return ApiCall<T>.Fail(StatusCodes.Status502BadGateway, "La API devolvió una respuesta que no se pudo leer.");
        }
        catch (HttpRequestException exception)
        {
            return ApiCall<T>.Fail(0, "No se pudo conectar con el microservicio. " + exception.Message);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiCall<T>.Fail(0, "El microservicio no respondió a tiempo.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"La API respondió {(int)response.StatusCode}.";
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<ApiErrorBody>(body, Json);
            if (!string.IsNullOrWhiteSpace(parsed?.Message))
            {
                return parsed.Message;
            }

            if (parsed?.Errors is { Count: > 0 })
            {
                return string.Join(" ", parsed.Errors.SelectMany(pair => pair.Value));
            }

            if (!string.IsNullOrWhiteSpace(parsed?.Title))
            {
                return parsed.Title;
            }
        }
        catch (JsonException)
        {
        }

        return body.Length > 300 ? body[..300] : body;
    }

    private sealed class ApiErrorBody
    {
        public string? Message { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
