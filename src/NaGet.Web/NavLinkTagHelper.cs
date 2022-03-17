using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NaGet.Web
{
    [HtmlTargetElement(Attributes = "nav-link")]
    public class NavLinkTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor? accessor;

        public NavLinkTagHelper(IHttpContextAccessor accessor)
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        [HtmlAttributeName("asp-page")]
        public string Page { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsActiveLink())
            {
                output.Attributes.SetAttribute("class", "active");
            }
        }

        private bool IsActiveLink()
        {
            var endpoint = accessor.HttpContext?.GetEndpoint();
            var pageDescriptor = endpoint?.Metadata.GetMetadata<PageActionDescriptor>();

            if (pageDescriptor is null) return false;
            if (pageDescriptor.AreaName is not null) return false;
            if (pageDescriptor.ViewEnginePath != Page) return false;

            return true;
        }
    }
}
