using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HotelStay.Blazor.Models;

namespace HotelStay.Blazor.Services;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient httpClient;

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            return await ToApiResultAsync<TResponse>(response, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ApiResult<TResponse>.Failure("The request was canceled.", 0);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<TResponse>.Failure($"Unable to reach the HotelStay API. {ex.Message}", 0);
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Failure($"Unexpected error while calling the HotelStay API. {ex.Message}", 0);
        }
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
            return await ToApiResultAsync<TResponse>(response, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ApiResult<TResponse>.Failure("The request was canceled.", 0);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<TResponse>.Failure($"Unable to reach the HotelStay API. {ex.Message}", 0);
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Failure($"Unexpected error while calling the HotelStay API. {ex.Message}", 0);
        }
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
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Please correct the highlighted validation errors.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.UnprocessableEntity => "The reservation could not be completed with the supplied document details.",
            _ => response.ReasonPhrase ?? "The API request failed."
        };

        if (response.Content.Headers.ContentLength == 0)
        {
            return fallback;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryGetString(root, "message", out var message) ||
                TryGetString(root, "detail", out message) ||
                TryGetString(root, "title", out message))
            {
                return message;
            }

            if (TryGetProblemDetailsErrors(root, out var validationErrors))
            {
                return validationErrors;
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return fallback;
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

    private static bool TryGetProblemDetailsErrors(JsonElement root, out string message)
    {
        message = string.Empty;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("errors", out var errors))
        {
            return false;
        }

        var messages = new List<string>();
        if (errors.ValueKind == JsonValueKind.Object)
        {
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
        }
        else if (errors.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in errors.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                {
                    messages.Add(item.GetString()!);
                }
            }
        }

        message = string.Join(" ", messages);
        return messages.Count > 0;
    }
}
