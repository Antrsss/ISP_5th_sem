using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System.Text.Encodings.Web;

namespace WEB_353502_ZGIRSKAYA.UI.TagHelpers
{
    [HtmlTargetElement("pager")]
    public class PagerTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        [HtmlAttributeName("current-page")]
        public int CurrentPage { get; set; } = 1;

        [HtmlAttributeName("total-pages")]
        public int TotalPages { get; set; }

        [HtmlAttributeName("category")]
        public string? Category { get; set; }

        [HtmlAttributeName("admin")]
        public bool Admin { get; set; } = false;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        public PagerTagHelper(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (TotalPages <= 1)
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "nav";
            output.Attributes.SetAttribute("aria-label", "Page navigation");
            output.AddClass("mt-4", HtmlEncoder.Default);

            var ulTag = new TagBuilder("ul");
            ulTag.AddCssClass("pagination justify-content-center");

            // Previous button
            ulTag.InnerHtml.AppendHtml(CreatePageItem("Previous", CurrentPage - 1, CurrentPage == 1, "bi-chevron-left"));

            // Page numbers
            for (int i = 1; i <= TotalPages; i++)
            {
                ulTag.InnerHtml.AppendHtml(CreatePageItem(i.ToString(), i, false, null, i == CurrentPage));
            }

            // Next button
            ulTag.InnerHtml.AppendHtml(CreatePageItem("Next", CurrentPage + 1, CurrentPage == TotalPages, "bi-chevron-right"));

            output.Content.SetHtmlContent(ulTag);
        }

        private TagBuilder CreatePageItem(string text, int targetPage, bool disabled, string? iconClass, bool active = false)
        {
            var liTag = new TagBuilder("li");
            liTag.AddCssClass("page-item");

            if (active)
                liTag.AddCssClass("active");
            if (disabled)
                liTag.AddCssClass("disabled");

            var aTag = new TagBuilder("a");
            aTag.AddCssClass("page-link");

            if (!disabled && targetPage >= 1 && targetPage <= TotalPages)
            {
                var href = GeneratePageUrl(targetPage);
                aTag.Attributes.Add("href", href);
            }
            else
            {
                aTag.Attributes.Add("href", "#");
            }

            if (text == "Previous" || text == "Next")
            {
                aTag.Attributes.Add("aria-label", text);

                var spanTag = new TagBuilder("span");
                spanTag.AddCssClass("bi");
                spanTag.AddCssClass(iconClass!);
                spanTag.Attributes.Add("aria-hidden", "true");

                aTag.InnerHtml.AppendHtml(spanTag);
            }
            else
            {
                aTag.InnerHtml.Append(text);
            }

            liTag.InnerHtml.AppendHtml(aTag);
            return liTag;
        }

        private string GeneratePageUrl(int pageNumber)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return "#";

            var routeValues = new RouteValueDictionary
            {
                ["pageNo"] = pageNumber
            };

            if (!string.IsNullOrEmpty(Category))
            {
                routeValues["category"] = Category;
            }

            // Для Razor Pages всегда используем GetPathByPage
            var pagePath = ViewContext.RouteData.Values["page"]?.ToString();
            return _linkGenerator.GetPathByPage(
                httpContext: httpContext,
                page: pagePath,
                values: routeValues
            ) ?? "#";
        }
    }
}