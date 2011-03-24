using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.Infrastructure.FormsAuthenticationService;

namespace Web.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public IFormsAuthenticationService FormsAuthService { get; set; }

        SiteDB _db;

        public HomeController(IFormsAuthenticationService FormsAuthService)
        {
            _db = new SiteDB();
            this.FormsAuthService = FormsAuthService;
        }

        public ActionResult Index()
        {
            var model = 1;

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }
    }

}
