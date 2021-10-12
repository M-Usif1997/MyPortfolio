using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Owner> userManager;
        private readonly SignInManager<Owner> signInManager;
        private readonly ILogger<AccountController> logger;
        private readonly IMapper mapper;
        private readonly IMailingService mailingService;

        public AccountController(UserManager<Owner> userManager, SignInManager<Owner> signInManager, ILogger<AccountController> logger, IMapper mapper, IMailingService mailingService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.mapper = mapper;
            this.mailingService = mailingService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()   //When want to navigate But Still Logout /home/index/{UserId} 
        {
            LoginViewModel model = new LoginViewModel
            {
                ExternalLogins =
               (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
            [ValidateAntiForgeryToken]
      
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");    //after SignIn in Google Redirect to Method Found In My Application

            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);  //Redirect User To SignPage
        }

        [AllowAnonymous]
        public async Task<IActionResult>
        ExternalLoginCallback( string remoteError = null)
        {

            
            LoginViewModel loginViewModel = new LoginViewModel
            {
           
                ExternalLogins =
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }
            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Error loading external login information.");

                return View("Login", loginViewModel);
            }
          
            // Get the email claim from external login provider (Google, Facebook etc)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
          

            Owner user = null;
            if (email != null)
            { 
                user = await userManager.FindByEmailAsync(email);
               
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
             
            if (signInResult.Succeeded)
            {
                return RedirectToAction("index", "home", new { username = user.UserName });
            }
            else
            {

                if (email != null)
                {
                    if (user == null)
                    {
                        return RedirectToAction("SocialLogin");


                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                     await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("index", "home", new { username = user.UserName });
                }

                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on m.yosef.1997@gmail.com";

                return View("Error");
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var _MyUser = await userManager.FindByEmailAsync(model.Email);

                if (_MyUser != null && !_MyUser.EmailConfirmed &&
                    (await userManager.CheckPasswordAsync(_MyUser, model.Password)))   //VI => Without CheckPasswordAsync , he can know What Is True (Password or Email) From Error Message
                {

                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }

                else if (_MyUser == null || (!await userManager.CheckPasswordAsync(_MyUser, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    goto View;

                }
                    var result = await signInManager.PasswordSignInAsync(_MyUser.UserName, model.Password, model.RememberMe, true);



                    if (result.Succeeded)
                    {
                        return RedirectToAction("index", "home", new { username = _MyUser.UserName });

                    }



                if (result.IsLockedOut)
                {
                    return View("AccountLocked");
                }
            }
            View:
            return View(model);
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult SocialLogin()
        {    
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task< IActionResult> SocialLogin(SocialLoginViewModel Model)
        {
            if (ModelState.IsValid)
            {

                //make this as service
                var info = await signInManager.GetExternalLoginInfoAsync();


                Model.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                Model.FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                Model.LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);




                var user = mapper.Map<Owner>(Model);

                user.EmailConfirmed = true;
                try
                {
                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorTitle = "Login Fail";
                    ViewBag.ErrorMessage = ex.Message.ToString();
                    return View("Error");

                }
                        
                return RedirectToAction("index", "home", new { username = user.UserName });
            }

            return View(Model);

        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel Model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to IdentityUser
                var user = mapper.Map<Owner>(Model);

                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, Model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController if it is User
                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    //build the email confirmation link
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);
                    
                    try
                    {

                       string mailText =  mailingService.GetMailBody(Model.UserName, Model.Email, confirmationLink,"E-mailConfirmationView");
                   
                        await mailingService.SendEmailAsync(Model.Email, "Welcome to Our Application", mailText);
                       
                    }

                    catch(Exception ex)
                    {
                        ViewBag.ErrorTitle = "Registration Fail";
                        ViewBag.ErrorMessage = ex.Message.ToString();
                        return View("Error");
                    }


                    //Not Allowed Him SignIn Before he Confirm His Email
                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
                    return View("Error");


                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                

            }

            return View(Model);
        }



        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");

            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {

                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");

        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                // If the user is found AND Email is confirmed
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {

                    // Generate the reset password token
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);

                    // Build the password reset link
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                        new { email = model.Email, token = token }, Request.Scheme);
                 

                    //Send Mail to reset 

                    try
                    {

                        string mailText = mailingService.GetMailBody(user.UserName,model.Email, passwordResetLink,"ForgetPasswordView");

                        await mailingService.SendEmailAsync(model.Email, "Welcome to Our Application", mailText);

                    }

                    catch (Exception ex)
                    {
                        ViewBag.ErrorTitle = "Error Happen";
                        ViewBag.ErrorMessage = ex.Message.ToString();
                        return View("Error");
                    }



                    // Send the user to Forgot Password Confirmation view
                    return View("ForgotPasswordConfirmation");
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");

            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        // Upon successful password reset and if the account is lockedout, set
                        // the account lockout end date to current UTC date time, so the user
                        // can login with the new password
                        if (await userManager.IsLockedOutAsync(user))
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }

                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                       
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }


       

        //To Check Email Unique on Client Side
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // ASP.NET Core MVC uses jQuery remote() method which in turn issues an AJAX call to invoke the server side method. 
                // The jQuery remote() method expects a JSON response,
                // this is the reason we are returning JSON response from the server-side method
                return Json(true);
            }
            else
            {
                return Json($"Email : {email} is already in use.");
            }
        }


        //To Check Username Unique on Client Side
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUsernameInUse(string username)
        {
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                // ASP.NET Core MVC uses jQuery remote() method which in turn issues an AJAX call to invoke the server side method. 
                // The jQuery remote() method expects a JSON response,
                // this is the reason we are returning JSON response from the server-side method
                return Json(true);
            }
            else
            {
                return Json($"UserName : {username} is already in use.");
            }
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
