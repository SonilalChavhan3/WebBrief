using Markdig;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBrief.Models;
using WebBrief.Services;

namespace WebBrief.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebsiteScraper _scraper;
        private readonly IAIService _aiService;

        public HomeController(
            IWebsiteScraper scraper,
            IAIService aiService)
        {
            _scraper = scraper;
            _aiService = aiService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new SummaryRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SummaryRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Url))
            {
                ModelState.AddModelError(
                    nameof(model.Url),
                    "Please enter a website URL.");

                return View(model);
            }

            try
            {
                var website =
                    await _scraper.FetchWebsiteContentsAsync(model.Url);

                model.Summary = Markdown.ToHtml(
                    await _aiService.SummarizeAsync(
                     website,
                     model.Personality));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Error: {ex.Message}");
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
