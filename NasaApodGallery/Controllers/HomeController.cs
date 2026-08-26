using Microsoft.AspNetCore.Mvc;
using NasaApodGallery.Models;
using NasaApodGallery.Services;

namespace NasaApodGallery.Controllers;
public class HomeController : Controller
{
    private readonly NasaApodService _nasaApodService;

    public HomeController(NasaApodService nasaApodService)
    {
        _nasaApodService = nasaApodService;
    }

    public async Task<IActionResult> Index()
    {

        var apods = await _nasaApodService.GetApodRangesAsync("2026-08-20", "2026-08-25");

        return View(apods);
    }
    
}
