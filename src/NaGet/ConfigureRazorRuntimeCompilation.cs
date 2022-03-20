// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureRazorRuntimeCompilation.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   A class that configures the razor runtime compilation options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet;

/// <summary>
/// A class that configures the razor runtime compilation options.
/// </summary>
public class ConfigureRazorRuntimeCompilation : IConfigureOptions<MvcRazorRuntimeCompilationOptions>
{
    /// <summary>
    /// The host environment.
    /// </summary>
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureRazorRuntimeCompilation"/> class.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <exception cref="ArgumentNullException">Thrown if the environment is null.</exception>
    public ConfigureRazorRuntimeCompilation(IHostEnvironment env)
    {
        environment = env ?? throw new ArgumentNullException(nameof(env));
    }

    public void Configure(MvcRazorRuntimeCompilationOptions options)
    {
        var path = Path.Combine(environment.ContentRootPath, "..", "NaGet.Web");

        // Try to enable Razor "hot reload".
        if (!environment.IsDevelopment())
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            return;
        }

        var provider = new PhysicalFileProvider(Path.GetFullPath(path));
        options.FileProviders.Add(provider);
    }
}
