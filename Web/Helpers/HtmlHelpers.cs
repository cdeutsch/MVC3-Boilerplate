using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace System.Web.Mvc {
    public static class HtmlHelpers {
        const string cssDir = "~/content/";
        const string imageDir = "~/content/images/";
        const string scriptDir="~/scripts/";

        public static HtmlString DatePickerEnable(this HtmlHelper helper) {
            StringBuilder sb=new StringBuilder();
            sb.AppendLine(@"<script type=""text/javascript"">$(document).ready(function() {$("".date-selector"").datepicker();});</script>" + Environment.NewLine);
            return new HtmlString(sb.ToString());
        }

        public static HtmlString Friendly(this HtmlHelper helper)
        {
            if (helper.ViewContext.HttpContext.Request.Cookies["friendly"] != null) {
                return new HtmlString(helper.ViewContext.HttpContext.Request.Cookies["friendly"].Value);
            } else {
                return new HtmlString("");
            }
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
            if (!fileName.EndsWith(".css"))
                fileName += ".css";
            return new HtmlString(UrlHelper.GenerateContentUrl(helper.AttributeEncode(VirtualCSSPath(fileName)), helper.ViewContext.HttpContext));
        }
        public static string VirtualCSSPath(string fileName)
        {
            return (cssDir + fileName);
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
    }
}
