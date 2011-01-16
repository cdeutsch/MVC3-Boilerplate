using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Web.Models;
using Web.Infrastructure.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Web.Controllers
{
    public class SessionController : Controller
    {

        IAuthenticationService _authService;
        SiteDB _db;
        UserActivity _log;
        public SessionController(IAuthenticationService authService)
        {
            _db = new SiteDB();
            _authService = authService;
            _log = new UserActivity(_db);
        }

        //
        // GET: /Session/Create
        //Kind of like "Login"
        public ActionResult Create()
        {

            if (Request.QueryString["ReturnUrl"] != null)
            {
                Session["ReturnUrl"] = Request.QueryString["ReturnUrl"];
            }
            
            return View(CreateForm.NewCreateForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(FormCollection collection)
        {
            var login = collection["usernew.username"];
            var email = collection["usernew.email"];
            var password = collection["password"];
            var confirm = collection["confirm"];
            if (!String.IsNullOrEmpty(login) & !String.IsNullOrEmpty(password) & !String.IsNullOrEmpty(confirm))
            {
                try
                {
                    var registered = _authService.RegisterUser(login, password, confirm, email, "", "");
                    if (registered)
                    {
                        //this.FlashInfo("Thank you for signing up!");

                        //get userid.
                        User user = UserRepository.GetUser(_db, login);
                        if (user != null)
                        {
                            _log.LogIt(user.UserId, "Registered");

                            this.FlashInfo("Thank you for signing up!");

                            return AuthAndRedirect(login, user.UserId.ToString());
                        }

                    }
                    else
                    {
                        this.FlashWarning("There was a problem with your registration");
                    }
                }
                catch (Exception x)
                {
                    //the auth service should return a usable exception message
                    this.FlashError(x.Message);
                }
            }
            else
            {
                this.FlashError("Invalid user name or password");
            }
            return RedirectToAction("create");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateForm createForm)
        {
            if (!String.IsNullOrEmpty(createForm.UserLogin.LoginName) & !String.IsNullOrEmpty(createForm.UserLogin.Password))
            {
                if (_authService.IsValidLogin(createForm.UserLogin.LoginName, createForm.UserLogin.Password))
                {
                    //get userid.
                    User user = UserRepository.GetUser(_db, createForm.UserLogin.LoginName);
                    if (user != null)
                    {
                        _log.LogIt(user.UserId, "Registered");

                        return AuthAndRedirect(user.Username, user.UserId.ToString());
                    }
                }
            }
            this.FlashWarning("Invalid login");

            return View(createForm);

        }

        ActionResult AuthAndRedirect(string friendly, string userKey)
        {
            Response.Cookies["friendly"].Value = friendly;
            Response.Cookies["friendly"].Expires = DateTime.Now.AddDays(30);
            Response.Cookies["friendly"].HttpOnly = true;

            FormsAuthentication.SetAuthCookie(userKey, true);
            if (Session["ReturnUrl"] != null)
            {
                return Redirect(Session["ReturnUrl"].ToString());
            }
            else
            {
                return Redirect("/");
            }
        }
        
        //Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete()
        {
            Response.Cookies["friendly"].Value = null;
            _log.LogIt(Convert.ToInt64(User.Identity.Name), "Logged out");
            FormsAuthentication.SignOut();
            return RedirectToAction("index", "home");
        }

    }

    public class CreateForm
    {
        public static CreateForm NewCreateForm()
        {
            CreateForm cf = new CreateForm();
            cf.UserLogin = new LoginUser();
            cf.UserNew = new User();
            return cf;
        }

        public CreateForm()
        {

        }

        public CreateForm(LoginUser UserLogin, User UserNew)
        {
            this.UserLogin = UserLogin;
            this.UserNew = UserNew;
        }

        public User UserNew  { get; set; }
        public LoginUser UserLogin { get; set; }
    }

    public class LoginUser
    {
        [Required(ErrorMessage = "Username is required.")]
        public string LoginName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
