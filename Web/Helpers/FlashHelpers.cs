using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace System.Web.Mvc
{
    public static class FlashHelpers
    {

        public static void FlashInfo(this Controller controller, string message)
        {
            controller.TempData["info"] = EscapeJS(message);
            controller.ViewBag.FlashMessage = message; //no-js fallback (output in your #flashmessage element)
        }
        public static void FlashWarning(this Controller controller, string message)
        {
            controller.TempData["warning"] = EscapeJS(message);
            controller.ViewBag.FlashMessage = message; //no-js fallback (output in your #flashmessage element)
        }
        public static void FlashError(this Controller controller, string message)
        {
            controller.TempData["error"] = EscapeJS(message);
            controller.ViewBag.FlashMessage = message; //no-js fallback (output in your #flashmessage element)
        }

        public static void FlashValidationSummaryErrors(this Controller controller)
        {
            controller.TempData["errorValidationSummary"] = true;
        }

        public static HtmlString Flash(this HtmlHelper helper)
        {

            var message = "";
            var className = "";
            var useValidationSummary = false;
            if (helper.ViewContext.TempData["info"] != null)
            {
                message = helper.ViewContext.TempData["info"].ToString();
                className = "info";
            }
            else if (helper.ViewContext.TempData["warning"] != null)
            {
                message = helper.ViewContext.TempData["warning"].ToString();
                className = "warning";
            }
            else if (helper.ViewContext.TempData["error"] != null)
            {
                message = helper.ViewContext.TempData["error"].ToString();
                className = "error";
            }
            else if (helper.ViewContext.TempData["errorValidationSummary"] != null)
            {
                useValidationSummary = true;
                className = "error";
            }
            var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(message) || useValidationSummary)
            {
                string messageHtml = null;
                sb.AppendLine("<script>");
                sb.AppendLine("$(document).ready(function() {");
                if (useValidationSummary)
                {
                    sb.AppendFormat("$.flashBase('{0}',$('.validation-summary-errors').html());", className);
                }
                else
                {
                    sb.AppendFormat("$.flashBase('{0}',$('#flashMessageText').html());", className);
                    messageHtml = "<div id=\"flashMessageText\" style=\"display:none;\">" + message + "</div>";
                }
                sb.AppendLine("});");
                sb.AppendLine("</script>");
                if (!string.IsNullOrEmpty(messageHtml))
                {
                    sb.AppendLine(messageHtml);
                }
                
            }
            return new HtmlString(sb.ToString());
        }

        private static string EscapeJS(string value)
        {
            return value.Replace("'", "\\\'");
        }
    }
}
