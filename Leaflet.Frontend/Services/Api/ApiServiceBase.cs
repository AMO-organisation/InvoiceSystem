using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leaflet.Frontend.Services.Api;

public abstract class ApiServiceBase
{
    protected readonly HttpClient Http;
    protected readonly JsonSerializerOptions Json;

    protected ApiServiceBase(HttpClient http, JsonSerializerOptions json)
    {
        Http = http;
        Json = json;
    }
}
