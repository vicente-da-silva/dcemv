/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using DCEMV.DemoServer.Models.AccountViewModels;
using DCEMV.DemoServer.Models.ManageViewModels;
using DCEMV.DemoServer.Persistence.Credentials;
using DCEMV.FormattingUtils;
using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Components;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Controllers.Credentials
{
    [Authorize]
    //[SecurityHeaders]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly AccountService _account;
        private readonly IAccountsRepository _accountsRepository;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IIdentityServerInteractionService interaction,
            IHttpContextAccessor httpContext,
            IClientStore clientStore,
            IAccountsRepository accountsRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<ProfileController>();
            _interaction = interaction;
            _clientStore = clientStore;
            _accountsRepository = accountsRepository;

            _account = new AccountService(interaction, httpContext, clientStore);
        }

        #region methods required by web ui
        [HttpGet]
        [Route("profile/confirmemail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
       
        [HttpGet]
        [Route("profile/resetpassword")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [Route("profile/resetpassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ProfileController.ResetPasswordConfirmation), "Profile");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ProfileController.ResetPasswordConfirmation), "Profile");
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [Route("profile/resetpasswordconfirmation")]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        ////
        //// GET: /Account/Login
        //[AllowAnonymous]
        //[Route("profile/login")]
        //[HttpGet]
        //public async Task<IActionResult> Login(string returnUrl)
        //{
        //    var vm = await _account.BuildLoginViewModelAsync(returnUrl);

        //    if (vm.IsExternalLoginOnly)
        //    {
        //        // only one option for logging in
        //        return ExternalLogin(vm.ExternalProviders.First().AuthenticationScheme, returnUrl);
        //    }

        //    return View(vm);
        //}

        ////
        //// POST: /Account/Login
        //[HttpPost]
        //[Route("profile/login")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginInputModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Require the user to have a confirmed email before they can log on.
        //        var user = await _userManager.FindByEmailAsync(model.Email);
        //        if (user != null)
        //        {
        //            if (!await _userManager.IsEmailConfirmedAsync(user))
        //            {
        //                ModelState.AddModelError(string.Empty,
        //                              "You must have a confirmed email to log in.");
        //                return View(await _account.BuildLoginViewModelAsync(model));
        //            }
        //        }

        //        // This doesn't count login failures towards account lockout
        //        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberLogin, lockoutOnFailure: false);
        //        if (result.Succeeded)
        //        {
        //            _logger.LogInformation(1, "User logged in.");
        //            return RedirectToLocal(model.ReturnUrl);
        //        }
        //        if (result.RequiresTwoFactor)
        //        {
        //            return RedirectToAction(nameof(SendCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberLogin });
        //        }
        //        if (result.IsLockedOut)
        //        {
        //            _logger.LogWarning(2, "User account locked out.");
        //            return View("Lockout");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //            return View(await _account.BuildLoginViewModelAsync(model));
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(await _account.BuildLoginViewModelAsync(model));
        //}

        ////
        //// POST: /Account/ExternalLogin
        //[HttpPost]
        //[Route("profile/externallogin")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public IActionResult ExternalLogin(string provider, string returnUrl = null)
        //{
        //    // Request a redirect to the external login provider.
        //    var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        //    return Challenge(properties, provider);
        //}

        ////
        //// GET: /Account/SendCode
        //[HttpGet]
        //[Route("profile/sendcode")]
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        //{
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ///// <summary>
        ///// Show logout page
        ///// </summary>
        //[AllowAnonymous]
        //[Route("profile/logout")]
        //[HttpGet]
        //public async Task<IActionResult> Logout(string logoutId)
        //{
        //    var vm = await _account.BuildLogoutViewModelAsync(logoutId);

        //    if (vm.ShowLogoutPrompt == false)
        //    {
        //        // no need to show prompt
        //        return await Logout(vm);
        //    }

        //    return View(vm);
        //}

        ///// <summary>
        ///// Handle logout page postback
        ///// </summary>
        //[HttpPost]
        //[Route("profile/logout")]
        //[ValidateAntiForgeryToken]
        //[AllowAnonymous]
        //public async Task<IActionResult> Logout(LogoutViewModel model)
        //{
        //    var vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);
        //    if (vm.TriggerExternalSignout)
        //    {
        //        string url = Url.Action("Logout", new { logoutId = vm.LogoutId });
        //        try
        //        {
        //            // hack: try/catch to handle social providers that throw
        //            await HttpContext.Authentication.SignOutAsync(vm.ExternalAuthenticationScheme,
        //                new AuthenticationProperties { RedirectUri = url });
        //        }
        //        catch (NotSupportedException) // this is for the external providers that don't have signout
        //        {
        //        }
        //        catch (InvalidOperationException) // this is for Windows/Negotiate
        //        {
        //        }
        //    }

        //    // delete authentication cookie
        //    await _signInManager.SignOutAsync();

        //    return View("LoggedOut", vm);
        //}

        ////
        //// GET: /Account/Register
        //[HttpGet]
        //[Route("profile/register")]
        //[AllowAnonymous]
        //public IActionResult Register(string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    return View();
        //}

        ////
        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("profile/register")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
        //            // Send an email with this link
        //            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
        //            await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
        //                $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
        //            //await _signInManager.SignInAsync(user, isPersistent: false);
        //            _logger.LogInformation(3, "User created a new account with password.");
        //            return RedirectToLocal(returnUrl);
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        ////
        //// GET: /Account/ExternalLoginCallback
        //[HttpGet]
        //[Route("profile/externallogincallback")]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        //{
        //    if (remoteError != null)
        //    {
        //        ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
        //        return View(nameof(Login));
        //    }
        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return RedirectToAction(nameof(Login));
        //    }

        //    // Sign in the user with this external login provider if the user already has a login.
        //    var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        //    if (result.Succeeded)
        //    {
        //        _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
        //        return RedirectToLocal(returnUrl);
        //    }
        //    if (result.RequiresTwoFactor)
        //    {
        //        return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        return View("Lockout");
        //    }
        //    else
        //    {
        //        // If the user does not have an account, then ask the user to create an account.
        //        ViewData["ReturnUrl"] = returnUrl;
        //        ViewData["LoginProvider"] = info.LoginProvider;
        //        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
        //    }
        //}

        ////
        //// POST: /Account/ExternalLoginConfirmation
        //[HttpPost]
        //[Route("profile/externalloginconfirmation")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await _signInManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await _userManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await _userManager.AddLoginAsync(user, info);
        //            if (result.Succeeded)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false);
        //                _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewData["ReturnUrl"] = returnUrl;
        //    return View(model);
        //}

        //
        // GET: /Account/ConfirmEmail
        // //
        // // GET: /Account/ForgotPassword
        // [HttpGet]
        // [Route("profile/forgotpassword")]
        // [AllowAnonymous]
        // public IActionResult ForgotPassword()
        // {
        //     return View();
        // }


        // //
        // // POST: /Account/ForgotPassword
        //[HttpPost]
        //[Route("profile/forgotpassword")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        // public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = await _userManager.FindByNameAsync(model.Email);
        //         if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //         {
        //             // Don't reveal that the user does not exist or is not confirmed
        //             return View("ForgotPasswordConfirmation");
        //         }

        //         // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
        //         // Send an email with this link
        //         var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //         var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
        //         await _emailSender.SendEmailAsync(model.Email, "Reset Password",
        //            $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
        //         return View("ForgotPasswordConfirmation");
        //     }

        //     // If we got this far, something failed, redisplay form
        //     return View(model);
        // }

        // //
        // // GET: /Account/ForgotPasswordConfirmation
        // [HttpGet]
        // [Route("profile/forgotpasswordconfirmation")]
        // [AllowAnonymous]
        // public IActionResult ForgotPasswordConfirmation()
        // {
        //     return View();
        // }

        //
        // GET: /Account/ResetPassword
        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[Route("profile/sendcode")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }

        //    // Generate the token and send it
        //    var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
        //    if (string.IsNullOrWhiteSpace(code))
        //    {
        //        return View("Error");
        //    }

        //    var message = "Your security code is: " + code;
        //    if (model.SelectedProvider == "Email")
        //    {
        //        await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
        //    }
        //    else if (model.SelectedProvider == "Phone")
        //    {
        //        await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
        //    }

        //    return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        ////
        //// GET: /Account/VerifyCode
        //[HttpGet]
        //[Route("profile/verifycode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[Route("profile/verifycode")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes.
        //    // If a user enters incorrect codes for a specified amount of time then the user account
        //    // will be locked out for a specified amount of time.
        //    var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(model.ReturnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        _logger.LogWarning(7, "User account locked out.");
        //        return View("Lockout");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid code.");
        //        return View(model);
        //    }
        //}

        #endregion

        #region methods required by app
        [HttpGet]
        [Route("profile/getprofiledetails")]
        public async Task<Profile> GetProfileDetails()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            if (user != null)
            {
                Profile model = new Profile
                {
                    HasPassword = await _userManager.HasPasswordAsync(user),
                    PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                    TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                    //Logins = await _userManager.GetLoginsAsync(user),
                    BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
                };

                return model;
            }
            else
                throw new ValidationException("Invalid User");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("profile/register")]
        public async Task<CallBackUrl> Register(string email, string password)
        {
            if (!Validate.EmailValidation(email))
                throw new ValidationException("Invalid inputs");

            if (!Validate.PasswordValidation(password))
                throw new ValidationException("Invalid inputs");

            ApplicationUser user = new ApplicationUser { UserName = email, Email = email };
            IdentityResult result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                try
                {
                    _accountsRepository.AddAccount(GuidBuilder.Create().ToString(), user.Id);
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    string callbackUrl = Url.Action("ConfirmEmail", "Profile", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(email, "Please confirm your email address",
                        $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a> or by entering this code in the app: " + code);

                    return new CallBackUrl() { Url = callbackUrl };
                }
                catch (Exception ex)
                {
                    await _userManager.DeleteAsync(user);
                    throw new ValidationException("Could not create user:" + ex.Message);
                }
            }
            else
                throw new ValidationException("Could not create user");
        }

        [HttpPost]
        [Route("profile/addphonenumber")]
        public async Task AddPhoneNumber(string phoneNumber)
        {
            if (!Validate.PhoneNumberValidation(phoneNumber))
                throw new ValidationException("Invalid inputs");

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
                await _smsSender.SendSmsAsync(phoneNumber, "Your Payloola OTP is: " + code);
            }
            else
                throw new ValidationException("Invalid User");
        }

        [HttpPost]
        [Route("profile/verifyphonenumber")]
        public async Task VerifyPhoneNumber(string phoneNumber, string code)
        {
            if (!Validate.PhoneNumberValidation(phoneNumber))
                throw new ValidationException("Invalid inputs");

            if (code == null)
                throw new ValidationException("Invalid inputs");

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }
                else
                    throw new ValidationException("Could verify phone number, invalid otp");
            }
            else
                throw new ValidationException("Invalid User");
        }

        [HttpPost]
        [Route("profile/removephonenumber")]
        public async Task RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }
                else
                    throw new ValidationException("Could not remove phone number");
            }
            else
                throw new ValidationException("Invalid User");
        }

        [HttpPost]
        [Route("profile/changepassword")]
        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            if (!Validate.PasswordValidation(oldPassword))
                throw new ValidationException("Invalid inputs");

            if (!Validate.PasswordValidation(newPassword))
                throw new ValidationException("Invalid inputs");

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                }
                else
                    throw new ValidationException("Could not change password");
            }
            else
                throw new ValidationException("Invalid User");
        }

        [HttpPost]
        [Route("profile/forgotpassword")]
        [AllowAnonymous]
        public async Task ForgotPassword(string email)
        {
            if (!Validate.EmailValidation(email))
                throw new ValidationException("Invalid inputs");

            var user = await _userManager.FindByNameAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                throw new ValidationException("Could not send email");
            }
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Profile", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a> or by entering this code in the app: " + code);
        }

        [HttpPost]
        [Route("profile/resetpassword2")]
        [AllowAnonymous]
        public async Task ResetPassword(string email, string code, string password)
        {
            if (!Validate.EmailValidation(email))
                throw new ValidationException("Invalid inputs");

            if (!Validate.PasswordValidation(password))
                throw new ValidationException("Invalid inputs");

            if (code == null)
                throw new ValidationException("Invalid inputs");

            var user = await _userManager.FindByNameAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                throw new ValidationException("Could not reset password");
            }
            var result = await _userManager.ResetPasswordAsync(user, code, password);
            if (!result.Succeeded)
            {
                throw new ValidationException("Could not reset password");
            }
        }

        [HttpPost]
        [Route("profile/confirmemail")]
        [AllowAnonymous]
        public async Task ConfirmEmail2(string userId, string code)
        {
            if (!Validate.EmailValidation(userId))
                throw new ValidationException("Invalid inputs");

            if (code == null)
                throw new ValidationException("Invalid inputs");

            var user = await _userManager.FindByNameAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (!result.Succeeded)
                    throw new ValidationException("Could not confirm email");
            }
            else
                throw new ValidationException("Invalid User");
        }
        [HttpGet]
        [Route("profile/resendconfirmemail")]
        [AllowAnonymous]
        public async Task ResendConfirmEmail(string userId)
        {
            var user = await _userManager.FindByEmailAsync(userId);
            if (user != null)
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string callbackUrl = Url.Action("ConfirmEmail", "Profile", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(userId, "Please confirm your email address",
                    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a> or by entering this code in the app: " + code);
            }
            else
                throw new ValidationException("Invalid User");
        }

        //[HttpPost]
        //[Route("credentials/setpassword")]
        //public async Task SetPassword(string newPassword)
        //{
        //    if (newPassword == null)
        //    {
        //        throw new ValidationException("Invalid inputs");
        //    }
        //    var user = await GetCurrentUserAsync();
        //    if (user != null)
        //    {
        //        var result = await _userManager.AddPasswordAsync(user, newPassword);
        //        if (result.Succeeded)
        //        {
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //        }
        //        else
        //            throw new ValidationException("Could not set password");
        //    }
        //    else
        //        throw new ValidationException("Invalid User");
        //}


        //[HttpPost]
        //[Route("credentials/enabletwofactorauthentication")]
        //public async Task EnableTwoFactorAuthentication()
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (user != null)
        //    {
        //        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        //        if (result.Succeeded)
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //        else
        //            throw new ValidationException("Could not enable two-factor authentication");
        //    }
        //    else
        //        throw new ValidationException("Invalid User");
        //}

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        //[HttpPost]
        //[Route("credentials/disabletwofactorauthentication")]
        //public async Task DisableTwoFactorAuthentication()
        //{
        //    var user = await GetCurrentUserAsync();
        //    if (user != null)
        //    {
        //        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        //        if (result.Succeeded)
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //        else
        //            throw new ValidationException("Could not disable two-factor authentication");
        //    }
        //    else
        //        throw new ValidationException("Invalid User");
        //}
        //[HttpPost]
        //[Route("credentials/sendcode")]
        //[AllowAnonymous]
        //public async Task SendCode(string provider)
        //{
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        throw new ValidationException("Could not get user");
        //    }

        //    // Generate the token and send it
        //    var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
        //    if (string.IsNullOrWhiteSpace(code))
        //    {
        //        throw new ValidationException("Could not get token");
        //    }

        //    var message = "Your security code is: " + code;
        //    if (provider == "Email")
        //    {
        //        await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
        //    }
        //    else if (provider == "Phone")
        //    {
        //        await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
        //    }
        //}

        //[HttpPost]
        //[Route("credentials/verifycode")]
        //[AllowAnonymous]
        //public async Task VerifyCode(string provider,string code,bool rememberMe, bool rememberBrowser )
        //{
        //    // The following code protects for brute force attacks against the two factor codes.
        //    // If a user enters incorrect codes for a specified amount of time then the user account
        //    // will be locked out for a specified amount of time.
        //    var result = await _signInManager.TwoFactorSignInAsync(provider, code, rememberMe, rememberBrowser);
        //    if (!result.Succeeded)
        //    {
        //        throw new ValidationException("Could not validate code");
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        throw new ValidationException("User locked out");
        //    }
        //    else
        //    {
        //        throw new ValidationException("Could not validate code");
        //    }
        //}
        #endregion

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            //ClaimsIdentity newIdentity = ((ClaimsIdentity)User.Identity);
            //newIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, User.FindFirst("sub").Value));
            //newIdentity.AddClaim(new Claim(ClaimTypes.Name, User.FindFirst("sub").Value));
            //return _userManager.GetUserAsync(new ClaimsPrincipal(newIdentity));

            return _userManager.FindByIdAsync(User.FindFirst("sub").Value);
        }
    }
}
