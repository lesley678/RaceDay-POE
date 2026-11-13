
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AccountController(ApiService api) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (r, e) = await api.SendAsync(
            HttpMethod.Post,
            "api/auth/login",
            vm
        );

        if (e != null || !r.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                e ?? "Invalid email or password."
            );

            return View(vm);
        }

        try
        {
            using var doc = await JsonDocument.ParseAsync(
                await r.Content.ReadAsStreamAsync()
            );

            var root = doc.RootElement;

            if (!root.TryGetProperty("user", out var user))
            {
                ModelState.AddModelError(
                    "",
                    "User information was not returned by the API."
                );

                return View(vm);
            }

            // Get USER_ID (actual API property: useR_ID)
            if (!user.TryGetProperty("useR_ID", out var userId) ||
                !userId.TryGetInt32(out int id))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid or missing USER_ID."
                );

                return View(vm);
            }

            // Get ROLE
            if (!user.TryGetProperty("role", out var role))
            {
                ModelState.AddModelError(
                    "",
                    "User role was not returned by the API."
                );

                return View(vm);
            }

            string userRole = role.GetString() ?? "";

            // Get FIRSTNAME
            string firstName = "";

            if (user.TryGetProperty("firstname", out var name))
            {
                firstName = name.GetString() ?? "";
            }

            // Store user details in session
            HttpContext.Session.SetInt32("UserId", id);

            HttpContext.Session.SetString(
                "UserRole",
                userRole
            );

            HttpContext.Session.SetString(
                "UserName",
                firstName
            );

            // Redirect based on role
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (userRole.Equals(
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/Organiser");
            }

            return Redirect("/Participant");
        }
        catch (JsonException)
        {
            ModelState.AddModelError(
                "",
                "Invalid response received from the server."
            );

            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (ok, msg) = await api.PostAsync(
            "api/auth/register",
            vm
        );

        if (!ok)
        {
            ModelState.AddModelError("", msg);
            return View(vm);
        }

        TempData["Success"] =
            "Registration successful. Please log in.";

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await api.SendAsync(
            HttpMethod.Post,
            "api/auth/logout"
        );

        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}

