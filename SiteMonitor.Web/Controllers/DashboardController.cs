using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteMonitor.Web.Data;
using SiteMonitor.Web.Models;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var sites = await _context.MonitoredSites
            .Where(s => s.UserId == user.Id)
            .Include(s => s.StatusHistory
                .OrderByDescending(h => h.CheckedAt)
                .Take(10))
            .ToListAsync();

        return View(sites);
    }
}