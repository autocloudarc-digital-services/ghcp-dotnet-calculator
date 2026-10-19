#nullable enable

namespace calculator.web.Services;

/// <summary>Maintains the selected appearance within one Blazor circuit.</summary>
public sealed class ThemeService
{
    /// <summary>Gets whether the dark theme is selected.</summary>
    public bool IsDark { get; private set; }

    /// <summary>Gets the CSS class for the selected appearance.</summary>
    public string CssClass => IsDark ? "theme-dark" : "theme-light";

    /// <summary>Switches between light and dark appearance.</summary>
    public void Toggle() => IsDark = !IsDark;
}
