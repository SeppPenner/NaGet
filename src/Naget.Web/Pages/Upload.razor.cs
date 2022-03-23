namespace NaGet.Web.Pages;

/// <inheritdoc cref="ComponentBase"/>
/// <summary>
/// The code behind class for the <see cref="Upload"/> page.
/// </summary>
public class UploadBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the url generator.
    /// </summary>
    [Inject]
    [NotNull]
    protected IUrlGenerator? UrlGenerator { get; set; }

    /// <summary>
    /// Gets or sets the localizer.
    /// </summary>
    [Inject]
    [NotNull]
    protected IStringLocalizer<Upload>? Localizer { get; set; }

    /// <summary>
    /// Gets or sets the tabs.
    /// </summary>
    protected List<TabModel> Tabs { get; set; } = new();

    /// <summary>
    /// Gets or sets the active tab.
    /// </summary>
    protected TabModel ActiveTab { get; set; } = new();

    /// <summary>
    /// Gets or sets the text shown 
    /// </summary>
    protected string YouCanPushPackagesText => string.Format(this.Localizer["YouCanPushPackages"], this.UrlGenerator.GetServiceIndexUrl());

    /// <inheritdoc cref="ComponentBase"/>
    protected override Task OnInitializedAsync()
    {
        var firstTab = this.Tabs.FirstOrDefault();

        if (firstTab is not null)
        {
            this.ActiveTab = this.Tabs[0];
        }

        this.Tabs = new List<TabModel>
        {
            new TabModel
            {
                Name = ".NET CLI",
                Index = 0,
                CssClass = "active",
                Script = new List<string> { $"dotnet nuget push -s {this.UrlGenerator.GetServiceIndexUrl()} package.nupkg" },
                Documentation = "https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push"
            },
            new TabModel
            {
                Name = "NuGet",
                Index = 1,
                CssClass = string.Empty,
                Script = new List<string> { $"nuget push -Source {this.UrlGenerator.GetServiceIndexUrl()} package.nupkg" },
                Documentation = "https://docs.microsoft.com/en-us/nuget/tools/cli-ref-push"
            },
            new TabModel
            {
                Name = "Paket",
                Index = 2,
                CssClass = string.Empty,
                // Todo: Get correct url for index here (From HTTPContext?)
                Script = new List<string> { $"paket push --url /index package.nupkg" },
                Documentation = "https://fsprojects.github.io/Paket/paket-push.html"
            },
            new TabModel
            {
                Name = "PowerShellGet",
                Index = 3,
                CssClass = string.Empty,
                Script = new List<string>
                {
                    $"Register-PSRepository -Name \"NaGet\" -SourceLocation \"{this.UrlGenerator.GetServiceIndexUrl()}\" -PublishLocation \"{this.UrlGenerator.GetPackagePublishResourceUrl()}\" -InstallationPolicy \"Trusted\"",
                    "Publish-Module -Name PS-Module -Repository NaGet"
                },
                Documentation = "https://docs.microsoft.com/en-us/powershell/module/powershellget/publish-module"
            }
        };

        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Handles the click on a new tab.
    /// </summary>
    /// <param name="tab">The tab model.</param>
    protected void OnClickTab(TabModel tab)
    {
        this.ActiveTab = this.Tabs.ElementAt(tab.Index);

        // Set active class to active tab only.
        this.Tabs.ForEach(tab => tab.CssClass = string.Empty);
        this.ActiveTab.CssClass = "active";
    }
}
