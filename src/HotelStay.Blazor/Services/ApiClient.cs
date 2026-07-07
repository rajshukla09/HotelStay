using System.Net.Http.Json;
using System.Text.Json;

namespace HotelStay.Blazor.Services;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        return await ToApiResultAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        return await ToApiResultAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<ApiResult<TResponse>> ToApiResultAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
            return ApiResult<TResponse>.Success(data, statusCode);
        }

        var errorMessage = await ReadErrorMessageAsync(response, cancellationToken);
        return ApiResult<TResponse>.Failure(errorMessage, statusCode);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return response.ReasonPhrase ?? "The API request failed.";
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return response.ReasonPhrase ?? "The API request failed.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryGetString(root, "message", out var message))
            {
                return message;
            }

            if (TryGetString(root, "detail", out var detail))
            {
                return detail;
            }

            if (TryGetValidationErrors(root, out var validationErrors))
            {
                return validationErrors;
            }

            if (TryGetString(root, "title", out var title))
            {
                return title;
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return response.ReasonPhrase ?? content;
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetValidationErrors(JsonElement root, out string message)
    {
        message = string.Empty;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var messages = new List<string>();
        foreach (var error in errors.EnumerateObject())
        {
            if (error.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in error.Value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                {
                    messages.Add($"{error.Name}: {item.GetString()}");
                }
            }
        }

        message = string.Join(" ", messages);
        return messages.Count > 0;
    }
}
