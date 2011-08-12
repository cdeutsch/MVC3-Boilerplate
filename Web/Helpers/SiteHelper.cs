using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.IO;
using System.Configuration;
using System.Web.Hosting;

namespace Web
{

    public static class SiteHelper
    {
        public static string ApplicationName
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationName"];
            }
        }

        /// <summary>
        /// Returns the default formatted page title
        /// </summary>
        /// <returns></returns>
        public static string GetFullPageTitle()
        {
            return ApplicationName;
        }
        /// <summary>
        /// Returns the formatted page title
        /// </summary>
        /// <returns></returns>
        public static string GetFullPageTitle(string PageTitle)
        {
            return ApplicationName + " - " + PageTitle;
        }

        public static string UploadsVirtualPath
        {
            get
            {
                return "~/App_Data/uploads/";
            }
        }

        public static string UploadsServerPath
        {
            get
            {
                return HostingEnvironment.MapPath(UploadsVirtualPath);
            }
        }

        /// <summary>
        /// This method returns a fully qualified absolute server Url which includes
        /// the protocol, server, port in addition to the server relative Url.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
        /// <param name="forceHttps">if true forces the url to use https</param>
        /// <returns></returns>
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (string.IsNullOrEmpty(serverUrl))
                return serverUrl;

            // *** Is it already an absolute Url?
            if (IsAbsolutePath(serverUrl))
            {
                if (!forceHttps)
                    return serverUrl;
                else
                    return ForceUriToHttps(new Uri(serverUrl)).ToString();
            }

            string newServerUrl = ResolveUrl(serverUrl);
            Uri result = new Uri(HttpContext.Current.Request.Url, newServerUrl);

            if (!forceHttps)
                return result.ToString();
            else
                return ForceUriToHttps(result).ToString();
        }

        /// <summary>
        /// This method returns a fully qualified absolute server Url which includes
        /// the protocol, server, port in addition to the server relative Url.
        /// 
        /// It work like Page.ResolveUrl, but adds these to the beginning.
        /// This method is useful for generating Urls for AJAX methods
        /// </summary>
        /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
        /// <returns></returns>
        public static string ResolveServerUrl(string serverUrl)
        {
            return ResolveServerUrl(serverUrl, false);
        }

        /// <summary>
        /// Returns a site relative HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="originalUrl">Any Url including those starting with ~</param>
        /// <returns>relative url</returns>
        public static string ResolveUrl(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
                return originalUrl;

            // *** Absolute path - just return
            if (IsAbsolutePath(originalUrl))
                return originalUrl;

            // *** We don't start with the '~' -> we don't process the Url
            if (!originalUrl.StartsWith("~"))
                return originalUrl;

            // *** Fix up path for ~ root app dir directory
            // VirtualPathUtility blows up if there is a 
            // query string, so we have to account for this.
            int queryStringStartIndex = originalUrl.IndexOf('?');
            if (queryStringStartIndex != -1)
            {
                string queryString = originalUrl.Substring(queryStringStartIndex);
                string baseUrl = originalUrl.Substring(0, queryStringStartIndex);

                return string.Concat(
                    VirtualPathUtility.ToAbsolute(baseUrl),
                    queryString);
            }
            else
            {
                return VirtualPathUtility.ToAbsolute(originalUrl);
            }

        }

        /// <summary>
        /// Forces the Uri to use https
        /// </summary>
        private static Uri ForceUriToHttps(Uri uri)
        {
            // ** Re-write Url using builder.
            UriBuilder builder = new UriBuilder(uri);
            builder.Scheme = Uri.UriSchemeHttps;
            builder.Port = 443;

            return builder.Uri;
        }

        private static bool IsAbsolutePath(string originalUrl)
        {
            // *** Absolute path - just return
            int IndexOfSlashes = originalUrl.IndexOf("://");
            int IndexOfQuestionMarks = originalUrl.IndexOf("?");

            if (IndexOfSlashes > -1 &&
                 (IndexOfQuestionMarks < 0 ||
                  (IndexOfQuestionMarks > -1 && IndexOfQuestionMarks > IndexOfSlashes)
                  )
                )
                return true;

            return false;
        }
    }
}