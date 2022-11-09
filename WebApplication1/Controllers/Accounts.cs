using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.Accounts;

namespace WebApplication1.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(SignInManager<CognitoUser> signinManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signinManager;
            _userManager = userManager;
            _pool = pool;
        }
        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if ( ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if ( user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                    return View(model);
                }

                //Dictionary<string, string> validationData = new Dictionary<string, string>(StringComparer.Ordinal)
                //{
                //    {CognitoAttribute.Email.AttributeName, model.Email },
                //    {CognitoAttribute.Name.AttributeName, model.Email }
                //};

                //var createdUser = await ((CognitoUserManager<CognitoUser>)_userManager).CreateAsync(user, model.Password, validationData);

                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);
                user.Attributes.Add(CognitoAttribute.Email.AttributeName, model.Email);
                var createdUser = await _userManager.CreateAsync(user, model.Password);

                if ( createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }
            return View();
        }

        public async Task<IActionResult> Confirm(ConfirmModel model)
        {            
            return View(model);
        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> Confirm_Post(ConfirmModel model)
        {
            if ( ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if ( user == null)
                {
                    ModelState.AddModelError("Not Found", "a user with this email address was not found");
                    return View(model);
                }

                var result = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(user, model.Code, true);
                //var result = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(user, model.Code, true);

                if ( result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach( var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }

            }

            return View(model);
            
        }

        [HttpGet]
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<ActionResult> Login_Post(LoginModel model)
        {
            if ( ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if ( result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login Error", "Email and password do not work");
                }
            }
            return View("Login", model);
        }
    }
}
