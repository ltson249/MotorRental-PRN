using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MotorRental.Web.Services;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = "";
    public HttpStatusCode Status { get; set; }
}

// Lop boc HttpClient: tu dong gan JWT (luu trong claim "access_token") va doc loi tu API
public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient http;
    private readonly IHttpContextAccessor accessor;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        this.http = http;
        this.accessor = accessor;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url);
    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) => SendAsync<T>(HttpMethod.Post, url, body);
    public Task<ApiResult<T>> PutAsync<T>(string url, object body) => SendAsync<T>(HttpMethod.Put, url, body);
    public Task<ApiResult<T>> DeleteAsync<T>(string url) => SendAsync<T>(HttpMethod.Delete, url);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using var req = new HttpRequestMessage(method, url);
            var token = accessor.HttpContext?.User.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
                req.Content = JsonContent.Create(body, body.GetType(), options: Json);
            else if (method == HttpMethod.Post || method == HttpMethod.Put)
                req.Content = new StringContent("", Encoding.UTF8, "application/json");

            using var res = await http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                T? data = default;
                if (!string.IsNullOrWhiteSpace(text))
                    data = JsonSerializer.Deserialize<T>(text, Json);
                return new ApiResult<T> { Success = true, Data = data, Status = res.StatusCode };
            }

            return new ApiResult<T> { Success = false, Status = res.StatusCode, Message = ParseError(text, res.StatusCode) };
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            return new ApiResult<T>
            {
                Success = false,
                Status = HttpStatusCode.ServiceUnavailable,
                Message = "Không kết nối được Web API. Hãy chắc chắn project MotorRental.Api đang chạy."
            };
        }
    }

    private static string ParseError(string text, HttpStatusCode status)
    {
        if (status == HttpStatusCode.Unauthorized && string.IsNullOrWhiteSpace(text))
            return "Phiên đăng nhập hết hạn, hãy đăng nhập lại.";
        if (status == HttpStatusCode.Forbidden)
            return "Bạn không có quyền thực hiện chức năng này.";

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var m)) return m.GetString() ?? "Có lỗi xảy ra";
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                {
                    var list = new List<string>();
                    foreach (var p in errs.EnumerateObject())
                        foreach (var e in p.Value.EnumerateArray())
                            list.Add(e.GetString() ?? "");
                    if (list.Count > 0) return string.Join(" | ", list);
                }
                if (root.TryGetProperty("title", out var t)) return t.GetString() ?? "Có lỗi xảy ra";
            }
        }
        catch { /* khong phai JSON */ }

        return $"Lỗi {(int)status} từ Web API";
    }
}
