using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EquipmentBorrowing.Contracts;

namespace EquipmentBorrowing.Desktop.Services;

/// <summary>
/// The ONLY way the desktop app reads or writes application data: asynchronous HTTP + JSON against /api/v1.
/// </summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public Uri BaseAddress => _http.BaseAddress!;

    // ---- equipment ----
    public Task<List<EquipmentDto>> GetEquipmentAsync(bool availableOnly, CancellationToken ct = default) =>
        SendAsync<List<EquipmentDto>>(HttpMethod.Get, availableOnly ? "api/v1/equipment?available=true" : "api/v1/equipment", null, false, ct);

    public Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest r, CancellationToken ct = default) =>
        SendAsync<EquipmentDto>(HttpMethod.Post, "api/v1/equipment", r, true, ct);

    public Task<EquipmentDto> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest r, CancellationToken ct = default) =>
        SendAsync<EquipmentDto>(HttpMethod.Put, $"api/v1/equipment/{id}", r, true, ct);

    public Task<List<BorrowingDto>> GetEquipmentBorrowingsAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<List<BorrowingDto>>(HttpMethod.Get, $"api/v1/equipment/{id}/borrowings", null, false, ct);

    // ---- students ----
    public Task<List<StudentDto>> GetStudentsAsync(string? search, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/v1/students" : $"api/v1/students?search={Uri.EscapeDataString(search.Trim())}";
        return SendAsync<List<StudentDto>>(HttpMethod.Get, url, null, false, ct);
    }

    public Task<StudentDto> CreateStudentAsync(CreateStudentRequest r, CancellationToken ct = default) =>
        SendAsync<StudentDto>(HttpMethod.Post, "api/v1/students", r, true, ct);

    public Task<StudentDto> UpdateStudentAsync(Guid id, UpdateStudentRequest r, CancellationToken ct = default) =>
        SendAsync<StudentDto>(HttpMethod.Put, $"api/v1/students/{id}", r, true, ct);

    public Task<StudentDto> SetEligibilityAsync(Guid id, bool isEligible, CancellationToken ct = default) =>
        SendAsync<StudentDto>(HttpMethod.Patch, $"api/v1/students/{id}/eligibility", new SetEligibilityRequest(isEligible), true, ct);

    public Task<List<BorrowingDto>> GetStudentBorrowingsAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<List<BorrowingDto>>(HttpMethod.Get, $"api/v1/students/{id}/borrowings", null, false, ct);

    // ---- borrowings ----
    public Task<List<BorrowingDto>> GetBorrowingsAsync(string? search, string status, string sort, string direction, CancellationToken ct = default)
    {
        var q = new List<string>
        {
            $"status={Uri.EscapeDataString(status)}",
            $"sort={Uri.EscapeDataString(sort)}",
            $"direction={Uri.EscapeDataString(direction)}"
        };
        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search.Trim())}");
        return SendAsync<List<BorrowingDto>>(HttpMethod.Get, "api/v1/borrowings?" + string.Join('&', q), null, false, ct);
    }

    public Task<BorrowingDto> BorrowAsync(CreateBorrowingRequest r, CancellationToken ct = default) =>
        SendAsync<BorrowingDto>(HttpMethod.Post, "api/v1/borrowings", r, true, ct);

    public Task<CheckoutResponse> CheckoutAsync(CheckoutRequest r, CancellationToken ct = default) =>
        SendAsync<CheckoutResponse>(HttpMethod.Post, "api/v1/checkout-requests", r, true, ct);

    public Task<BorrowingDto> ReturnAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<BorrowingDto>(HttpMethod.Post, $"api/v1/borrowings/{id}/return", new ReturnRequest(), true, ct);

    // ---- transport ----
    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body, bool mutation, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body is not null)
                request.Content = JsonContent.Create(body, body.GetType(), null, Json);
            response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
        }
        catch (HttpRequestException ex)
        {
            throw Classify(ex, mutation);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ApiNoResponseException(mutation, "The API did not answer in time.", ex);
        }
        catch (IOException ex)
        {
            throw new ApiNoResponseException(mutation, "The connection to the API was lost.", ex);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var value = await response.Content.ReadFromJsonAsync<T>(Json, ct);
                    if (value is not null) return value;
                }
                catch (JsonException) { /* fall through */ }
                throw new ApiErrorException((int)response.StatusCode, "invalid_response",
                    "The API returned a response this application could not understand.");
            }
            throw await ToErrorAsync(response, ct);
        }
    }

    private static ApiException Classify(HttpRequestException ex, bool mutation)
    {
        var neverDelivered = ex.HttpRequestError is HttpRequestError.ConnectionError
            or HttpRequestError.NameResolutionError or HttpRequestError.SecureConnectionError;
        return neverDelivered
            ? new ApiUnreachableException("Could not connect to the API.", ex)
            : new ApiNoResponseException(mutation, "The connection to the API was lost.", ex);
    }

    private static async Task<ApiException> ToErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var status = (int)response.StatusCode;
        string? text = null;
        try { text = await response.Content.ReadAsStringAsync(ct); } catch { /* ignore */ }

        ErrorResponse? err = null;
        if (!string.IsNullOrWhiteSpace(text))
        {
            try { err = JsonSerializer.Deserialize<ErrorResponse>(text, Json); } catch (JsonException) { }
        }
        return err is { Code.Length: > 0 }
            ? new ApiErrorException(status, err.Code, err.Message ?? "")
            : new ApiErrorException(status, $"http_{status}", $"The API returned HTTP {status} ({(HttpStatusCode)status}).");
    }
}
