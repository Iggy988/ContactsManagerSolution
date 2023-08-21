using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
}
