namespace NaGet.Web.Models;

/// <summary>
/// The dependency group model class.
/// </summary>
public class DependencyGroupModel
{
    /// <summary>
    /// Gets or sets the group model name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public IReadOnlyList<DependencyModel> Dependencies { get; set; } = new List<DependencyModel>();
}
