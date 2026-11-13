
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Services;

public sealed class ApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public ApiService(
        IHttpClientFactory factory,
        IHttpContextAccessor accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    private HttpClient Client => _factory.CreateClient("RaceDayApi");

    public async Task<(HttpResponseMessage Response, string? Error)> SendAsync(
        HttpMethod method,
        string path,
        object? body = null,
        IFormFile? file = null)
    {
        var http = Client;

        using var req = new HttpRequestMessage(method, path);

        var ctx = _accessor.HttpContext;

        if (ctx == null)
        {
            return (
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                "HTTP context is unavailable."
            );
        }

        var cookie = ctx.Session.GetString("ApiSessionCookie");

        if (!string.IsNullOrWhiteSpace(cookie))
        {
            req.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        if (file != null)
        {
            var form = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            var part = new StreamContent(stream);

            part.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            form.Add(part, "file", file.FileName);
            req.Content = form;
        }
        else if (body != null)
        {
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json"
            );
        }

        try
        {
            var response = await http.SendAsync(req);

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var sessionCookie = values.FirstOrDefault(v =>
                    v.StartsWith(
                        ".AspNetCore.Session=",
                        StringComparison.OrdinalIgnoreCase
                    ));

                if (sessionCookie != null)
                {
                    ctx.Session.SetString(
                        "ApiSessionCookie",
                        sessionCookie.Split(';')[0]
                    );
                }
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ctx.Session.Remove("ApiSessionCookie");
                ctx.Session.Remove("UserId");
                ctx.Session.Remove("UserRole");
                ctx.Session.Remove("UserName");
            }

            return (response, null);
        }
        catch (HttpRequestException)
        {
            return (
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                "Cannot reach the RaceDay API. Check ApiSettings:BaseUrl."
            );
        }
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var (r, e) = await SendAsync(HttpMethod.Get, path);

        if (e != null || !r.IsSuccessStatusCode)
            return default;

        return await r.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<(bool Ok, string Message)> PostAsync(
        string path, object body)
    {
        var (r, e) = await SendAsync(HttpMethod.Post, path, body);
        return await Result(r, e);
    }

    public async Task<(bool Ok, string Message)> PutAsync(
        string path, object body)
    {
        var (r, e) = await SendAsync(HttpMethod.Put, path, body);
        return await Result(r, e);
    }

    public async Task<(bool Ok, string Message)> DeleteAsync(string path)
    {
        var (r, e) = await SendAsync(HttpMethod.Delete, path);
        return await Result(r, e);
    }

    public async Task<(bool Ok, string Message)> UploadAsync(
        string path, IFormFile file)
    {
        var (r, e) = await SendAsync(
            HttpMethod.Post, path, file: file
        );

        return await Result(r, e);
    }

    private static async Task<(bool, string)> Result(
        HttpResponseMessage r, string? e)
    {
        if (e != null)
            return (false, e);

        var text = await r.Content.ReadAsStringAsync();

        if (r.IsSuccessStatusCode)
            return (true, "Request completed successfully.");

        try
        {
            using var d = JsonDocument.Parse(text);

            if (d.RootElement.TryGetProperty("message", out var m))
                return (false, m.GetString() ?? "The request failed.");
        }
        catch
        {
        }

        return (
            false,
            $"The API returned {(int)r.StatusCode}. Check the submitted information."
        );
    }
}
