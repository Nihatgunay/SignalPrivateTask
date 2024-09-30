using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalRtask.Models;
using SignalRtask.ViewModels;

namespace SignalRtask.Controllers
{
	public class AuthController : Controller //CHATGPT CODE <<<<---------------------
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IConfiguration _configuration;

		public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
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
				return View(vm);
			}

			var appUser = new AppUser
			{
				Email = vm.Email,
				UserName = vm.Username
			};

			var result = await _userManager.CreateAsync(appUser, vm.Password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View(vm);
			}

			await _signInManager.SignInAsync(appUser, isPersistent: false);
			return RedirectToAction("index", "home");
		}

		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			if (!ModelState.IsValid)
			{
				return View(vm);
			}

			var user = await _userManager.FindByNameAsync(vm.Username);

			if (user == null)
			{
				ModelState.AddModelError("", "Invalid login attempt.");
				return View(vm);
			}

			var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, false);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Invalid login attempt.");
				return View(vm);
			}

			// Generate JWT token
			var token = GenerateJwtToken(user);

			// Store the token in cookies
			Response.Cookies.Append("AuthToken", token, new CookieOptions
			{
				HttpOnly = true,  // Prevent client-side JavaScript from accessing the token for security purposes
				Expires = DateTimeOffset.UtcNow.AddMinutes(60),  // Expiration matches the token expiration
				Secure = true,    // Use this in production to ensure cookie is only sent over HTTPS
				SameSite = SameSiteMode.Strict // Prevent CSRF attacks
			});

			return RedirectToAction("index", "home");
		}

		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			// Optionally remove the token from cookies when logging out
			Response.Cookies.Delete("AuthToken");

			return RedirectToAction("index", "home");
		}

		private string GenerateJwtToken(AppUser user)
		{
			// Get claims for the token
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			};

			// Add roles if needed (optional)
			var roles = _userManager.GetRolesAsync(user).Result;
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			// Get JWT secret key from configuration
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

			// Create signing credentials
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			// Set token expiration (e.g., 1 hour)
			var expires = DateTime.Now.AddMinutes(60);

			// Create the JWT security token
			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:Issuer"],
				audience: _configuration["JWT:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
