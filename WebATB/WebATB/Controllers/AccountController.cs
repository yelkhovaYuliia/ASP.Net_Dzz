using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Security.Claims;
using WebATB.Data.Entities.Identity;
using WebATB.Interfaces;
using WebATB.Models.Account;
using Microsoft.AspNetCore.Authorization;
using WebATB.Data.Entities.Idenity;

namespace WebATB.Controllers
{
    public class AccountController(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, RoleManager<RoleEntity> roleManager, IImageService service, IEmailService emailService, IMapper mapper) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login(string? error)
        {
            if (error != null)
                ModelState.AddModelError("", error);
            return View();
        }

        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            // Read the external identity from the external cookie
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Fallback: read the external cookie directly when info is null
                var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (!authResult.Succeeded || authResult.Principal is null)
                    return RedirectToAction(nameof(Login));


                var providerKey = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(providerKey))
                    return RedirectToAction(nameof(Login));

                info = new ExternalLoginInfo(
                    authResult.Principal,
                    GoogleDefaults.AuthenticationScheme,
                    providerKey,
                    "Google");
            }

            // Try to sign in with the external login

            var extSignIn = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            // Succeeded якщо користувач попереднього разу заходив через Google(у моєму випадку)
            if (extSignIn.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction("Index", "Main");
            }

            // No local user linked to this external login – create or link one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                // Email is required to create a local account
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                // Pass the error via route values so Login(string? error) can display it
                return RedirectToAction(nameof(Login), new { error = "Google аккаунт не надає електронної пошти." });
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                //Create a local user without password
                user = new UserEntity
                {
                    UserName = email,
                    Email = email,
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                    LastName = info.Principal.FindFirstValue(ClaimTypes.Surname)
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    List<string>? errors = new();
                    foreach (var e in createResult.Errors)
                        errors.Add(e.Description);
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return RedirectToAction("Login", new { error = string.Join('\n', errors) });
                }

                // Ensure default "User" role exists and assign it
                const string roleName = "User";
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new RoleEntity { Name = roleName, NormalizedName = roleName.ToUpperInvariant() });
                await userManager.AddToRoleAsync(user, roleName);
            }

            // Link the external login to the user (idempotent)
            var addLoginResult = await userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                List<string>? errors = new();
                foreach (var e in addLoginResult.Errors)
                    errors.Add(e.Description);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction("Login", new { error = string.Join('\n', errors) });
            }

            // Sign in with the application cookie
            await signInManager.SignInAsync(user, isPersistent: false);

            // Clear external cookie to avoid stale state
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return RedirectToAction("Index", "Main");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = null;
                var user = await userManager.FindByEmailAsync(model.UsernameOrEmail);
                if (user != null)
                {
                    result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    //result = await signInManager.PasswordSignInAsync(user,model.Password, model.RememberMe, false);
                }

                if (result != null && result.Succeeded)
                {
                    var externalLogins = await userManager.GetLoginsAsync(user);
                    foreach (var login in externalLogins)
                    {
                        await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                    }
                    await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    return RedirectToAction("Index", "Main");
                }
                else if (result != null && result.IsLockedOut)
                    ModelState.AddModelError("", "Користувач заблокований");
                else
                    ModelState.AddModelError("", "Неправильний логін та/або пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var imageStr = model.Image is not null ? await service.SaveImageAsync(model.Image, "AvatarDir") : null;
                //UserEntity user = new UserEntity
                //{
                //    Email = model.Email,
                //    NormalizedEmail = model.Email,
                //    FirstName = model.FirstName,
                //    LastName = model.LastName,
                //    Image = imageStr,
                //    UserName = model.UserName
                //};
                var user = mapper.Map<UserEntity>(model);

                user.Image = imageStr;
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Ensure default role "User" exists
                    const string roleName = "User";
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var createRoleResult = await roleManager.CreateAsync(new RoleEntity
                        {
                            Name = roleName,
                            NormalizedName = roleName.ToUpperInvariant()
                        });
                        if (!createRoleResult.Succeeded)
                        {
                            foreach (var error in createRoleResult.Errors)
                                ModelState.AddModelError(string.Empty, error.Description);
                            return View(model);
                        }
                    }

                    // Add user to default role
                    var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return View(model);
                    }

                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Main");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Main");
        }

        public IActionResult AccessDenied() => View();

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // Запит посилання для скидання паролю (жоден код не зберігається на стороні сервера)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача з таким email не існує!");
                return View(model);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { email = model.Email, token = tokenEncoded },
                protocol: Request.Scheme);

            var htmlbody = System.IO.File.ReadAllText("Templates/PasswordResetTemplate.html");
            htmlbody = htmlbody.Replace("{callbackUrl}", callbackUrl);

            var sent = await emailService.SendEmailAsync(
                model.Email,
                "Скидання паролю",
                htmlbody);

            if (!sent)
            {
                ModelState.AddModelError(string.Empty, "Лист не було відправлено! Спробуйте пізніше.");
                return View(model);
            }

            ViewBag.Message = $"Ми надіслали інструкції зі скидання паролю на {model.Email}";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest();

            // Тримати токен закодованим, декодувати лише в POST
            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Користувача з таким email не існує!");
                return View(model);
            }

            // Декодувати токен тут
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Недійсний або пошкоджений токен.");
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
                return RedirectToAction("Login", "Account");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
    }
}