using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Mvc.Html;
using Web.Common;
using Web.Infrastructure.FormsAuthenticationService;

namespace System.Web.Mvc
{
    public static class HtmlHelpers
    {
        const string cssDir = "~/content/";
        const string imageDir = "~/content/images/";
        const string scriptDir = "~/scripts/";

        public static HtmlString DatePickerEnable(this HtmlHelper helper)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<script type=""text/javascript"">$(document).ready(function() {$("".date-selector"").datepicker();});</script>" + Environment.NewLine);
            return new HtmlString(sb.ToString());
        }

        public static HtmlString Friendly(this HtmlHelper helper)
        {
            return new HtmlString(CacheHelper.GetUserFriendlyName(new FormsAuthenticationService()));
        }

        public static HtmlString Script(this HtmlHelper helper, string fileName)
        {
            var jsPath = string.Format(@"<script src=""{0}"" ></script>" + Environment.NewLine, ScriptPath(helper, fileName));
            return new HtmlString(jsPath);
        }
        public static HtmlString ScriptPath(this HtmlHelper helper, string fileName)
        {
            if (!fileName.EndsWith(".js") && !fileName.EndsWith(".swf") && !fileName.EndsWith(".xap") && !fileName.EndsWith(".xaml"))
                fileName += ".js";
            return new HtmlString(UrlHelper.GenerateContentUrl(helper.AttributeEncode(VirtualScriptPath(fileName)), helper.ViewContext.HttpContext));
        }
        public static string VirtualScriptPath(string fileName)
        {
            return (scriptDir + fileName);
        }
        public static HtmlString CSS(this HtmlHelper helper, string fileName)
        {
            return CSS(helper, fileName, "screen");
        }
        public static HtmlString CSS(this HtmlHelper helper, string fileName, string media)
        {
            var jsPath = string.Format(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}""  media=""" + media + @""" />" + Environment.NewLine, CSSPath(helper, fileName, media));
            return new HtmlString(jsPath);
        }
        public static HtmlString CSSPath(this HtmlHelper helper, string fileName)
        {
            return CSSPath(helper, fileName, "screen");
        }
        public static HtmlString CSSPath(this HtmlHelper helper, string fileName, string media)
        {
            if (!fileName.EndsWith(".css") && !fileName.Contains("?"))
                fileName += ".css";
            return new HtmlString(UrlHelper.GenerateContentUrl(helper.AttributeEncode(VirtualCSSPath(fileName)), helper.ViewContext.HttpContext));
        }
        public static string VirtualCSSPath(string fileName)
        {
            return (cssDir + fileName);
        }
        public static HtmlString ImagePath(this HtmlHelper helper, string fileName)
        {
            return new HtmlString(UrlHelper.GenerateContentUrl(VirtualImagePath(fileName), helper.ViewContext.HttpContext));
        }
        public static HtmlString Image(this HtmlHelper helper, string fileName)
        {
            return Image(helper, fileName, "");
        }
        public static HtmlString Image(this HtmlHelper helper, string fileName, string attributes)
        {
            fileName = string.Format("{0}", UrlHelper.GenerateContentUrl(VirtualImagePath(fileName), helper.ViewContext.HttpContext));
            return new HtmlString(string.Format(@"<img src=""{0}"" {1} />", helper.AttributeEncode(fileName), helper.AttributeEncode(attributes)));
        }
        public static string VirtualImagePath(string fileName)
        {
            return (imageDir + fileName);
        }


        /// <summary>
        /// Returns a form with a delete button, plus a hidden anchor we can display via Ajax to make a delete link that does a post.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="linkText"></param>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static HtmlString DeleteLink(this HtmlHelper helper, string linkText, string actionName, string controllerName, object routeValues)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            string url = urlHelper.Action(actionName, controllerName, routeValues);

            string format = @"<form method=""post"" action=""{0}"" class=""deleteLink""><input type=""submit"" value=""{1}"" />{2}</form>";

            string form = string.Format(format, helper.AttributeEncode(url), helper.AttributeEncode(linkText), helper.AntiForgeryToken());
            return new HtmlString(form + helper.ActionLink(linkText, actionName, controllerName, routeValues, new { @class = "deleteLink" }).ToString());
        }

        public static HtmlString SelectedAttributeIfTrue(this HtmlHelper helper, bool Condition)
        {
            if (Condition)
            {
                return new HtmlString(" selected=\"selected\"");
            }
            else
            {
                return new HtmlString("");
            }
        }
        public static HtmlString DisplayNoneStyleAttributeIfTrue(this HtmlHelper helper, bool Condition)
        {
            if (Condition)
            {
                return new HtmlString(" style=\"display:none;\"");
            }
            else
            {
                return new HtmlString("");
            }
        }

    }
}
