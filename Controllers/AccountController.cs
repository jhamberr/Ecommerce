using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;//request from the service container application user
        private readonly IConfiguration configuration;//use field to read app settings.json. we are requesting this object from a service container Iconfig

        //request identity services from the services container
        //constructore
        /*signInmanager in the logout function is this service that we requested from the service
         contaienr and the constructor of this controller */
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration)//interface-request object of type Iconfig from a service container
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }


        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))//if authentidcated already return to home
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /*this method is accessible only by using http post method so
         * decorate it with http post attribute
         and requires submitted data which is of type registerdto*/
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {



            if (signInManager.IsSignedIn(User))//if authentidcated already return to home
            {
                return RedirectToAction("Index", "Home");
            }
            //if model state not valid then we have validation errors
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }
            /*other wise register user by making object of type application user
            create new account and authenticate the user
            we can set the first 6 attributes using the submitted
            data from registered Dto from the form
            username is uniqure identifier of user so we have to set to email address(no one has same email)
            we didnt set password , it will be encrypted by user manager*/
            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email, //email will be used to authenticate user
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreatedAt = DateTime.Now,//set date to current date.
            };
            //pass in the user that we want to create from above with pass from DTO 
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded) //if succeeded then acct created correctly
            {
                //successful user registration
                await userManager.AddToRoleAsync(user, "client");//add user role
                // sign the new user in
                await signInManager.SignInAsync(user, false);//authenticate user

                return RedirectToAction("Index", "Home");
            }

            //registration failed --> show registration errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(registerDto);
        }

        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");//redirect to index action of home controller
        }

        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))//if authentidcated already return to home
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {


            if (signInManager.IsSignedIn(User))//if authentidcated already return to home
            {
                return RedirectToAction("Index", "Home");
            }

            //check if submitted data is valid
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }
            //sign in the user if valid
            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
                loginDto.RememberMe, false);//pass persistent flag and dont lockout upon too many failed attempts
            //check if user is authenticated correctly
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                /*could add general error to model state but we are defining 
                 * the property in view bag here and display it in the view*/
                ViewBag.ErrorMessage = "Invalid login attempt.";
            }
            return View(loginDto);
        }

        [Authorize]//allows only authenticated users to access the profile action
        public async Task<IActionResult> Profile()
        {
            //read curr user
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)//this method is redundant sinc ethis function only allows authenticated uses but added anyway
            {
                return RedirectToAction("Index", "Home");
            }

            /*create object of type profile dto and fill it user app user
              profile dto is the model we made and defined now fill in information instantiate it also
              using data available in appuser which is stored as a variable 
              by the identity framework and asp.net automatically*/
            var profileDto = new ProfileDto()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",//email from dto cant be null but email from app user can be null but 
                PhoneNumber = appUser.PhoneNumber,
                Address = appUser.Address,
            };
            return View(profileDto);
            // originally forgot to pass object to view  return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto profileDto)
        {
            if (!ModelState.IsValid)//if state is invalid enter for loop
            {
                ViewBag.ErrorMessage = "Please fill all the required fieldds with valid values";
                return View(profileDto);
            }
            //read current authenticated user
            var appUser = await userManager.GetUserAsync(User);//get the curr user
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // update the user profile using the data of profileDto that we received from the form
            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName;
            appUser.UserName = profileDto.Email;
            appUser.Email = profileDto.Email;
            appUser.PhoneNumber = profileDto.PhoneNumber;
            appUser.Address = profileDto.Address;

            var result = await userManager.UpdateAsync(appUser);//then save the data to the database

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated successfully";
            }
            else 
            {
                ViewBag.ErrorMessage = "Unable to update the profile: " + result.Errors.First().Description;//display the first error of the different errors of the result. display the description of the first error
            }

            return View(profileDto);
        }

        [Authorize]
        public IActionResult Password() 
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDto passwordDto)//pass in submitted data of type passwordDto
        {
            if (!ModelState.IsValid) //if not valid return the view
            {
                return View();
            }

            //otherwise read/get the curr user
            var appUser = await userManager.GetUserAsync(User);//get the user property of this controller
            if (appUser == null) 
            {
                return RedirectToAction("Index", "Home");
            }
            //if not null then update the user password using app user since app user has more specific properties to the class it is based off of user defined
            var result = await userManager.ChangePasswordAsync(appUser,
                passwordDto.CurrentPassword, passwordDto.NewPassword);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password updated successfully!";
            }
            else
            {
                ViewBag.ErrorMessage = "Error: " + result.Errors.First().Description;
            }
            return View();
        }
        public IActionResult AccessDenied() 
        {
            return RedirectToAction("Index", "Home");
        }

        //takes you to the forgot password page
        public IActionResult ForgotPassword()//only for non authenticated users
        {
            if (signInManager.IsSignedIn(User))//if user authenticated redirect to home page
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //forgot password action that handles the post request. only accessible using the httppost method
        [HttpPost]
        //validate using required, and emailaddress to make sure it has the format
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)//only for non authenticated users|email must match the named parameter in cshtml as it will be passed in from there||need to match name input field
        {
            if (signInManager.IsSignedIn(User))//if user authenticated redirect to home page
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Email = email;//add email passed in from cshtml to viewbag then back to cshtml to  be displayed?

            if (!ModelState.IsValid)
            {
                /*save error message in email property of viewbag and it displays in cshtml the email error that will be displayed
                 will be equal to the Model state of the email variable that we passed in Dot errors which 
                contains all errors associated with this email. display error message of the first error 
                read error message only if model state of email is not null(["email"]?) otherwise if it is null we will add a default
                error message so we can display invalid email address message * ?? "Invalid Email Address";*/
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }
            //otherwise read the user containiing the passed in email from the database
            var user = await userManager.FindByEmailAsync(email);//this is the email of the user passed in
            //check if we found the user or not
            if (user != null) // if user not null then we have teh user with that email address
            {
                //generate the password reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);//generate token to this user
                /*add token to url. action link allows us to generate an absolute URL for an action method which contains a specific
                 action name, controller name, and url parameters*/
                string resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";//this will be the url that allows user to reset password otherwise send an error

                //send url by email
                string senderName = configuration["BrevoSettings:SenderName"] ?? "";
                string senderEmail = configuration["BrevoSettings:SenderEmail"] ?? "";
                string username = user.FirstName + " " + user.LastName;
                string subject = "Password Reset";
                string message = "Dear " + username + ",\n\n" +
                                 "You can reset your password using the following link:\n\n" +
                                 resetUrl + "\n\n" +
                                 "Best Regards";
               
                EmailSender.SendEmail(senderName, senderEmail, username, email, subject, message);
            }

            ViewBag.SuccessMessage = "Please check your Email account and click on the Password Reset link!";
            return View();
        }

        //reset action with a token from above
        public IActionResult ResetPassword(string? token)
        {
            if (signInManager.IsSignedIn(User))//if authenticated go to home page
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)// if null go to home page
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }
        //reset action that will handle the post request from the reset password page
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string? token, PasswordResetDto passwordResetDto)//need token from url and password dto submitted form data
        {
            if (signInManager.IsSignedIn(User))//if authenticated go to home page
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)// if null go to home page
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid) //if submitted data invalid return the same page/view
            {
                return View(passwordResetDto);
            }
            //otherwise read the user and submit the data that is available in the model passwordresetDto
            var user = await userManager.FindByEmailAsync(passwordResetDto.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Token not valid!";//if no user then add error message to viewbag
                return View(passwordResetDto);//return same data also with the form /view etc
            }
            var result = await userManager.ResetPasswordAsync(user, token, passwordResetDto.Password);
            //if result successful add new property to viewbag reflecting that
            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password reset successfully!";
            }
            else
            {
                //showing how to add general errors to model state instead of viewbag this timer
                foreach (var error in result.Errors) 
                {
                    //general error due to no key and we add every error to model state
                    ModelState.AddModelError("", error.Description);
                }
                
            }
            return View(passwordResetDto);
        }
    }
}
