using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.Web.Models;

namespace SchoolJournal.Web.Controllers;

public class HomeController(IStatisticsService statistics) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await statistics.GetAsync(ct));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
