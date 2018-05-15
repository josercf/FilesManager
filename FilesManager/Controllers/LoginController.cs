using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FilesManager.Models
{
    public class LoginController : Controller
    {
        private readonly IUserService userService;

        public LoginController(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            await HttpContext.SignOutAsync();
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(nameof(Login));

                var _user = await userService.Authenticate(model.User, model.Password);
                if (_user == null)
                {
                    ViewBag.Message = "Usuário ou senha inválidos";
                    return View(nameof(Index));
                }

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, _user.Name)};
                ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "password");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("index", "User");
            }
            catch
            {
                TempData["fail"] = "Por favor digite o usuario e senha!";
                return RedirectToAction("index", "Login");
            }
        }
    }
}
