namespace NaGet.Web;

/// <summary>
///     The code behind class for the <see cref="MainLayout" /> page.
/// </summary>
public class MainLayoutBase : LayoutComponentBase
{
    /// <summary>
    /// Gets or sets the localizer.
    /// </summary>
    [Inject]
    [NotNull]
    protected IStringLocalizer<MainLayout>? Localizer { get; set; }

    /// <summary>
    /// Gets or sets the error boundary.
    /// </summary>
    protected ErrorBoundary? ErrorBoundary { get; set; }

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    protected string SearchQuery { get; set; } = string.Empty;

    /// <inheritdoc cref="ComponentBase" />
    protected override void OnParametersSet()
    {
        this.ErrorBoundary?.Recover();
    }
}
