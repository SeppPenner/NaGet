namespace NaGet.Web;

/// <summary>
/// The application base class.
/// </summary>
public class AppBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the localizer.
    /// </summary>
    [Inject]
    [NotNull]
    protected IStringLocalizer<App>? Localizer { get; set; }
}
