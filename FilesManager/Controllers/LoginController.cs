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
            user.senha = "sdasd";
            user.usuario = "asdasd";

            if (ModelState.IsValid)
            {
                string LoginStatus = "Sucess";

                if (LoginStatus == "Sucess")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,user.usuario)
                    };
                    ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                    await HttpContext.SignInAsync(principal);

                    return RedirectToAction("index", "Home");
                }
                else
                {
                    TempData["UserLoginFailed"] = "Por favr entre com usuario e senha";
                    return View();
                }
            }
            else
                return View();
        }

    }
}
