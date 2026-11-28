namespace Leaflet.Frontend.Components;

public sealed class ContextMenuItem
{
    public string Label { get; init; } = string.Empty;
    public Func<Task>? Action { get; init; }
    public bool Danger { get; init; }
    public bool SeparatorBefore { get; init; }
}
