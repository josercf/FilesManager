using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using FilesManager.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FilesManager.Models
{
    public class LoginController : Controller
    {

        public async Task<IActionResult> Index()
        {
            await HttpContext.SignOutAsync();
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new ArgumentException("");

                string LoginStatus = "Sucess";

                if (LoginStatus != "Sucess")
                    throw new ArgumentException("");


                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.usuario) };
                ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("index", "Home");
            }
            catch
            {
                TempData["fail"] = "Por favor digite o usuario e senha!";
                return RedirectToAction("index", "Login");
            }

        }


        public async Task<IActionResult> Fail()
        {
            return RedirectToAction("index", "Login");
        }

    }
}
