using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace FreeCourse.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SigninInput model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var response = await _identityService.SignIn(model);

            if (!response.IsSuccessful)
            {
                response.Errors.ForEach(error => {
                    ModelState.AddModelError(string.Empty, error);
                });
                return View();
            }

            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
