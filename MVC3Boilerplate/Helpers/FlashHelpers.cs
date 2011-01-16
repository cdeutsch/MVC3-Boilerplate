using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace System.Web.Mvc {
    public static class FlashHelpers {

        public static void FlashInfo(this Controller controller,string message) {
            controller.TempData["info"] = message;
        }
        public static void FlashWarning(this Controller controller, string message) {
            controller.TempData["warning"] = message;
        }
        public static void FlashError(this Controller controller, string message) {
            controller.TempData["error"] = message;
        }

        public static HtmlString Flash(this HtmlHelper helper) {

            var message = "";
            var className = "";
            if (helper.ViewContext.TempData["info"] != null) {
                message =helper.ViewContext.TempData["info"].ToString();
                className = "info";
            } else if (helper.ViewContext.TempData["warning"] != null) {
                message = helper.ViewContext.TempData["warning"].ToString();
                className = "warning";
            } else if (helper.ViewContext.TempData["error"] != null) {
                message = helper.ViewContext.TempData["error"].ToString();
                className = "error";
            }
            var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(message)) {
                sb.AppendLine("<script>");
                sb.AppendLine("$(document).ready(function() {");
                sb.AppendFormat("$.flashBase('{0}','{1}');", className, message);
                sb.AppendLine("});");
                sb.AppendLine("</script>");
            }
            return new HtmlString(sb.ToString());
        }

    }
}
