using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MimoHitoma2.ViewModels;
using MimoHitoma2.Models;

namespace MimoHitoma2.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel login)
        {
            try
            {
                if (ValidLoginInput(login.account, login.password))
                {
                    UserIdentityModel user = new UserIdentityModel();
                    user.account = login.account.ToLower();
                    user.password = login.password.ToLower();
                    if (user.IsAuthenticatedUser(user.account, user.password))
                    {
                        System.Web.Security.FormsAuthentication.RedirectFromLoginPage(user.account, true);
                        // 驗證成功後導向首頁
                        return RedirectToAction("Search", "Post");
                    }
                    // 驗證失敗時，回到登入頁
                    ViewData["ErrorMessage"] = "帳號或密碼錯誤";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Search", "Post"));
            }
            return View();

        }

        private bool ValidLoginInput(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ViewData["ErrorMessage"] = "帳號不可為空白";
                return false;
            }
            if (string.IsNullOrEmpty(password))
            {
                ViewData["ErrorMessage"] = "密碼不可為空白";
                return false;
            }
            return true;
        }

        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account", null);
        }
    }
}