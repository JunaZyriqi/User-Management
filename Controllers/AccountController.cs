using ImbUserManagment2.Models;
using ImbUserManagment2.Services;
using ImbUserManagment2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ImbUserManagment2.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailService emailService;
        private readonly FirestoreUserService firestoreUserService;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            FirestoreUserService firestoreUserService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailService = emailService;
            this.firestoreUserService = firestoreUserService;
        }

        private bool IsImbEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                   email.Trim().ToLower().EndsWith("@imb.al");
        }

        private async Task<IActionResult> RedirectUserByRole(Users user)
        {
            if (await userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Admin", "Home");
            }

            if (await userManager.IsInRoleAsync(user, "User"))
            {
                return RedirectToAction("User", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    return await RedirectUserByRole(currentUser);
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!IsImbEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Only @imb.al emails are allowed.");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    return await RedirectUserByRole(user);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action(
                "ExternalLoginCallback",
                "Account"
            );

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                redirectUrl
            );

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                TempData["ErrorMessage"] = "Google login failed. Please try again.";
                return RedirectToAction("Login", "Account");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Google account email was not found.";
                return RedirectToAction("Login", "Account");
            }

            if (!IsImbEmail(email))
            {
                TempData["ErrorMessage"] = "Only @imb.al Google accounts are allowed.";
                return RedirectToAction("Login", "Account");
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true
            );

            if (signInResult.Succeeded)
            {
                var existingUser = await userManager.FindByEmailAsync(email);

                if (existingUser != null)
                {
                    return await RedirectUserByRole(existingUser);
                }

                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new Users
                {
                    FullName = fullName ?? email,
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now,
                    Department = "General",
                    Position = "User"
                };

                var createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    var errorMessage = string.Join(" ", createResult.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = errorMessage;

                    return RedirectToAction("Login", "Account");
                }

                var roleExists = await roleManager.RoleExistsAsync("User");

                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                await userManager.AddToRoleAsync(user, "User");
            }

            var addLoginResult = await userManager.AddLoginAsync(user, info);

            if (!addLoginResult.Succeeded)
            {
                var errorMessage = string.Join(" ", addLoginResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Google account could not be linked."
                    : errorMessage;

                return RedirectToAction("Login", "Account");
            }

            await signInManager.SignInAsync(user, isPersistent: false);

            return await RedirectUserByRole(user);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    return await RedirectUserByRole(currentUser);
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!IsImbEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Only @imb.al emails are allowed.");
                return View(model);
            }

            var existsInFirestore = await firestoreUserService.UserExistsAsync(model.Email);

            if (existsInFirestore)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ky perdorues ekziston ne Firestore dhe nuk mund te regjistrohet perseri."
                );

                return View(model);
            }

            var userExistsLocally = await userManager.FindByEmailAsync(model.Email);

            if (userExistsLocally != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ky email ekziston tashme ne databazen lokale."
                );

                return View(model);
            }

            var user = new Users
            {
                FullName = model.Name,
                UserName = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                Department = "General",
                Position = "User"
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleExist = await roleManager.RoleExistsAsync("User");

                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                await userManager.AddToRoleAsync(user, "User");

                await signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("User", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!IsImbEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Only @imb.al emails are allowed.");
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ChangePassword",
                "Account",
                new { email = model.Email, token = resetToken },
                Request.Scheme
            );

            var subject = "Reset Password";

            var body = $"Please reset your password by clicking here: <a href='{resetLink}'>Reset Password</a>";

            await emailService.SendEmailAsync(model.Email, subject, body);

            return RedirectToAction("EmailSent", "Account");
        }

        [HttpGet]
        public IActionResult EmailSent()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong.");
                return View(model);
            }

            if (!IsImbEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Only @imb.al emails are allowed.");
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}