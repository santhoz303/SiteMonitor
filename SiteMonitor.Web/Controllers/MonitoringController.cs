using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteMonitor.Web.Data;
using SiteMonitor.Web.Models;
using SiteMonitor.Web.ViewModels.Monitoring;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Controllers;

[Authorize]
public class MonitoringController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MonitoringController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSiteViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var site = new MonitoredSite
            {
                Name = model.Name,
                Url = model.Url,
                IsActive = true,
                UserId = user.Id
            };

            _context.MonitoredSites.Add(site);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var site = await _context.MonitoredSites
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

        if (site == null)
        {
            return NotFound();
        }

        var viewModel = new EditSiteViewModel
        {
            Id = site.Id,
            Name = site.Name,
            Url = site.Url,
            IsActive = site.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditSiteViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var site = await _context.MonitoredSites
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (site == null)
            {
                return NotFound();
            }

            site.Name = model.Name;
            site.Url = model.Url;
            site.IsActive = model.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Dashboard");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SiteExistsAsync(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var site = await _context.MonitoredSites
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

        if (site == null)
        {
            return NotFound();
        }

        _context.MonitoredSites.Remove(site);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Dashboard");
    }

    private async Task<bool> SiteExistsAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        return await _context.MonitoredSites
            .AnyAsync(s => s.Id == id && s.UserId == user.Id);
    }
}