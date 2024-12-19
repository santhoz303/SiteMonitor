using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteMonitor.Web.Data;
using SiteMonitor.Web.Models;
using SiteMonitor.Web.ViewModels;
using SiteMonitor.Web.ViewModels.Admin;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalSites = await _context.MonitoredSites.CountAsync(),
            TotalChecks = await _context.SiteStatusHistory.CountAsync(),
            DownSites = await _context.MonitoredSites
                .CountAsync(s => s.StatusHistory
                    .OrderByDescending(h => h.CheckedAt)
                    .FirstOrDefault().IsUp == false)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users
            .Include(u => u.MonitoredSites)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                MonitoredSitesCount = u.MonitoredSites.Count,
                IsAdmin = _userManager.IsInRoleAsync(u, "Admin").Result
            })
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Sites()
    {
        var sites = await _context.MonitoredSites
            .Include(s => s.User)
            .Include(s => s.StatusHistory
                .OrderByDescending(h => h.CheckedAt)
                .Take(1))
            .Select(s => new AdminSiteViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Url = s.Url,
                UserEmail = s.User.Email,
                IsActive = s.IsActive,
                LastStatus = s.StatusHistory.FirstOrDefault()
            })
            .ToListAsync();

        return View(sites);
    }

    public async Task<IActionResult> SiteDetails(int id)
    {
        var site = await _context.MonitoredSites
            .Include(s => s.User)
            .Include(s => s.StatusHistory.OrderByDescending(h => h.CheckedAt))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (site == null)
        {
            return NotFound();
        }

        return View(site);
    }
}