using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRtask.DAL;
using SignalRtask.Models;
using SignalRtask.ViewModels;

namespace SignalRtask.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public AuthController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        AppUser appUser = null;

        appUser = await _context.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == vm.Email.ToUpper());

        if (appUser is { })
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View();
        }

        appUser = await _userManager.FindByNameAsync(vm.Username);

        if (appUser is { })
        {
            ModelState.AddModelError("Username", "Username already exists");
            return View();
        }

        appUser = new AppUser()
        {
            Email = vm.Email,
            UserName = vm.Username,
        };

        var result = await _userManager.CreateAsync(appUser, vm.Password);

        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
                return View();
            }
        }

        return RedirectToAction("index", "home");

    }


}   
