﻿namespace Kraken.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Kraken.Models;
    using Kraken.Security;
    using Kraken.Services;
    using Kraken.ViewModels;
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Data.Entity;
    using Newtonsoft.Json;
    using Octopus.Client.Model;
    [Authorize]
    public class DefaultController : Controller
    {
        public DefaultController(IOctopusAuthenticationProxy octopusAuthenticationProxy, ApplicationDbContext dbContext)
        {
            if (octopusAuthenticationProxy == null) throw new ArgumentNullException(nameof(octopusAuthenticationProxy));
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            OctopusAuthenticationProxy = octopusAuthenticationProxy;
            DbContext = dbContext;
        }

        public IOctopusAuthenticationProxy OctopusAuthenticationProxy { get; }
        public ApplicationDbContext DbContext { get; set; }

        public IActionResult App(string view)
        {
            ViewData["View"] = "views/" + view;
            ViewData["Params"] = SerializeQuery(Request.Query);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                UserResource userResource;
                if (OctopusAuthenticationProxy.Login(model.UserName, model.Password, out userResource))
                {
                    var appUser = await ResolveApplicationUserAsync(userResource);
                    var octopusApiKey = appUser.OctopusApiKey;

                    if (String.IsNullOrEmpty(octopusApiKey) || !OctopusAuthenticationProxy.ValidateApiKey(appUser.UserName, octopusApiKey))
                    {
                        octopusApiKey = OctopusAuthenticationProxy.CreateApiKey();

                        await SetApplicationUserOctopusApiKey(appUser.UserName, octopusApiKey);
                    }

                    await HttpContext.Authentication.SignInAsync("Cookies", ClaimsPrincipalHelpers.CreatePrincipal(appUser.UserName, octopusApiKey));

                    if (String.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("App");
                    }
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }

        private async Task<ApplicationUser> ResolveApplicationUserAsync(UserResource userResource)
        {
            var user = await DbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == userResource.Username);

            if (user == null)
            {
                user = new ApplicationUser { UserName = userResource.Username, DisplayName = userResource.DisplayName };
                DbContext.ApplicationUsers.Add(user);
                await DbContext.SaveChangesAsync();
            }
            else if (user.DisplayName != userResource.DisplayName)
            {
                user.DisplayName = userResource.DisplayName;
                await DbContext.SaveChangesAsync();
            }

            return user;
        }

        private async Task SetApplicationUserOctopusApiKey(string userName, string octopusApiKey)
        {
            var user = await DbContext.ApplicationUsers.FirstAsync(u => u.UserName == userName);

            user.OctopusApiKey = octopusApiKey;

            await DbContext.SaveChangesAsync();
        }

        private static string SerializeQuery(IReadableStringCollection query)
        {
            var dictionary = query.ToDictionary(item => item.Key, item => item.Value.Count > 1 ? (object)item.Value : item.Value[0]);

            var settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            return JsonConvert.SerializeObject(dictionary, settings).Replace("\"", "'");
        }
    }
}
