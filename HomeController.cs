using Microsoft.AspNetCore.Mvc;using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class HomeController(ApiService api):Controller{public async Task<IActionResult> Index(){ViewBag.Events=await api.GetAsync<List<Dictionary<string,object>>>("api/events")??[];return View();}public IActionResult AccessDenied()=>View();public IActionResult Error()=>View();}
