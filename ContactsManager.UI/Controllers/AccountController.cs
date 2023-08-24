using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers;

//[Route("[controller]/[action]")] -stavili smo u Program.cs
[AllowAnonymous] // all action methods of this controller should be accessed without login
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AccountController(
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
        //Check for validation errors
        if (ModelState.IsValid == false) 
        {
            // za prikazivanje error messages one by one u view (return to view)
            ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
            return View(registerDTO);
        }
        //ukoliko nema gresaka, kreiramo usera
        ApplicationUser user = new ApplicationUser()
        { 
            Email = registerDTO.Email,
            PhoneNumber = registerDTO.Phone,
            UserName = registerDTO.Email,
            PersonName = registerDTO.PersonName,
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
        if (result.Succeeded)
        {
            //Check status of radio buttom
            if (registerDTO.UserType == Core.Enums.UserTypeOptions.Admin)
            {
                //Create Admin role
                if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                {
                    ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.Admin.ToString()};
                    await _roleManager.CreateAsync(applicationRole);
                }
                //Add the new user into Admin role
                await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
            }
            else
            {
                //Add the new user into User role
                await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());
            }
            //sign in
            await _signInManager.SignInAsync(user, isPersistent: false); // true->cookie ce biti persistent i kad se kasnije ponovo otvori isti browswer(loged in)


            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }
        // ukoliko ima gresaka prilikom kreiranja usera
        else
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }
            return View(registerDTO);
        }
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
            return View(loginDTO);
        }
        var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false); //ako pokusa 3 pogresna ulaza da li da se zakljuca
        
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl); //same domain
            }
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }

        ModelState.AddModelError("Login", "Inalid email or password");
        return View(loginDTO);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync(); // remove cookie (.AspNetCore.Identity.Application)
        return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }

    public async Task<IActionResult> IsEmailAlreadyRegistred(string email)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(email);

        if (user ==null)
        {
            return Json(true); // valid
        }
        else
        {
            return Json(false); //invalid
        }
    }
}

