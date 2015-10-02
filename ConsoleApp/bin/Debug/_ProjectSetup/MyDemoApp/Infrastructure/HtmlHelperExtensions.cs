using MyDemoApp.Core;
using System;
using System.Text;
using System.Web.Mvc;

namespace MyDemoApp.Infrastructure
{
    public static class HtmlHelperExtensions
    {
        //Your Extension method
        public static MvcHtmlString Pager<TSource>(this HtmlHelper html, IPagedList<TSource> model, Func<int, string> toUrl, int pageLinksPerPage = 10)
        {
            StringBuilder sbHtml = new StringBuilder();

            var minValue = model.PageNumber - (pageLinksPerPage / 2);
            var maxValue = model.PageNumber + ((pageLinksPerPage / 2) - 1);
            var isInRange = true;

            if (model.PageCount <= pageLinksPerPage)
            {
                minValue = 1;
                maxValue = model.PageCount;
                isInRange = false;
            }
            else
            {
                var correction = 0;

                correction = minValue < 1 ? -(minValue) + 1 : 0;
                minValue = minValue < 1 ? 1 : minValue;
                maxValue = maxValue + correction;

                maxValue = maxValue <= model.PageCount ? maxValue : model.PageCount;
                minValue = (maxValue + 1) - pageLinksPerPage;
            }

            if (model.PageCount > 1)
            {
                sbHtml.Append("<ul class=\"pagination\">");

                if (isInRange)
                {
                    sbHtml.AppendFormat("<li class=\"pagination-page\"><a href=\"{0}\">&#171;</a></li>", toUrl(1));
                }

                if (model.HasPreviousPage)
                {
                    sbHtml.AppendFormat("<li class=\"pagination-page\"><a href=\"{0}\">&#139;</a></li>", toUrl(model.PageNumber - 1));
                }

                for (int x = minValue; x <= maxValue; x++)
                {
                    if (x != model.PageNumber)
                    {
                        sbHtml.AppendFormat("<li class=\"pagination-page\"><a href=\"{0}\">{1}</a></li>", toUrl(x), x);
                    }
                    else
                    {
                        sbHtml.AppendFormat("<li class=\"pagination-page active\"><a href=\"{0}\">{1}</a></li>", toUrl(x), x);
                    }
                }

                if (model.HasNextPage)
                {
                    sbHtml.AppendFormat("<li class=\"pagination-page\"><a href=\"{0}\">&#155;</a></li>", toUrl(model.PageNumber + 1));
                }

                if (isInRange)
                {
                    sbHtml.AppendFormat("<li class=\"pagination-page\"><a href=\"{0}\">&#187;</a></li>", toUrl(model.PageCount));
                }

                sbHtml.Append("</ul>");
            }

            return new MvcHtmlString(sbHtml.ToString());
        }
    }
}