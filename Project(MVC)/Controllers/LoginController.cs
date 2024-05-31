using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_MVC_.Models.DB;
using System;
using System.Security.Claims;

namespace Project_MVC_.Controllers
{
    public class LoginController : Controller
    {
        public CrmContext db;

        public LoginController(CrmContext context)
        {
            db = context;
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Registration()
        {
            var logins = "";
            if (db.Users.Count() > 0)
            {
                logins = string.Join('|', db.Users.Select(i => i.Login));
            }

            return View("Registration", logins);
        }
        public async Task<IActionResult> CheckLogin(string login, string password)
        {
            User u = db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);
            if (u == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, u.Login) };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Profile", "User");
            }
        }

        public async Task<IActionResult> CheckRegistration(User u)
        {
            if (db.Users.Count(i => i.Login == u.Login) == 0)
            {
                db.Users.Add(u);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return RedirectToAction("Registration");
        }
    }
}
