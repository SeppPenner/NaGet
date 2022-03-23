namespace NaGet.Web.Models;

/// <summary>
/// The tab model class.
/// </summary>
public class TabModel
{
    /// <summary>
    /// Gets or sets the tab name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the CSS class.
    /// </summary>
    public string CssClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the script lines.
    /// </summary>
    public List<string> Script { get; set; } = new();

    /// <summary>
    /// Gets or sets the documentation.
    /// </summary>
    public string Documentation { get; set; } = string.Empty;
}
