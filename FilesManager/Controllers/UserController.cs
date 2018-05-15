using System.Threading.Tasks;
using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Models;
using Microsoft.AspNetCore.Mvc;

namespace FilesManager.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await userService.Get();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(User model)
        {
            if (ModelState.IsValid)
            {
                userService.Insert(model);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}