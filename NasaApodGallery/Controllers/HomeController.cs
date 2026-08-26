using Microsoft.AspNetCore.Mvc;
using NasaApodGallery.Models;
using NasaApodGallery.Services;

namespace NasaApodGallery.Controllers;
public class HomeController : Controller
{
    private readonly NasaApodService _nasaApodService;
    private readonly ApodRepository _apodRepository;

    public HomeController(NasaApodService nasaApodService, ApodRepository apodRepository)
    {
        _nasaApodService = nasaApodService;
        _apodRepository = apodRepository;
    }

    public IActionResult Index()
    {
        // temporary: still returns empty for now, Step 6 will make this read from DB
        return View(new List<ApodModel>());
    }

    public async Task<IActionResult> Fetch()
    {

        var apods = await _nasaApodService.GetApodRangeAsync("2026-08-20", "2026-08-25");

        foreach (var apod in apods)
        {
            _apodRepository.SaveApod(apod);
        }

        return RedirectToAction("Index");
    }
    
}
