using Microsoft.JSInterop;

namespace Leaflet.Frontend.Services;

public sealed class DialogService(IJSRuntime js)
{
    public ValueTask<bool> ConfirmAsync(string message) =>
        js.InvokeAsync<bool>("confirm", message);
}
