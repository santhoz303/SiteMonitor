using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteMonitor.Web.Data;
using SiteMonitor.Web.Models;
using SiteMonitor.Web.Services.EmailService;
using SiteMonitor.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Services.BackgroundServices;

public class SiteMonitoringService : ISiteMonitoringService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SiteMonitoringService> _logger;
    private readonly HttpClient _httpClient;

    public SiteMonitoringService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<SiteMonitoringService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> CheckSiteStatusAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking status for URL: {Url}", url);
            return false;
        }
    }

    public async Task<SiteStatusHistory> MonitorSiteAsync(MonitoredSite site)
    {
        try
        {
            var response = await _httpClient.GetAsync(site.Url);
            var isUp = response.StatusCode is >= (System.Net.HttpStatusCode)200 and < (System.Net.HttpStatusCode)400;
            var statusCode = (int)response.StatusCode;

            var history = new SiteStatusHistory
            {
                MonitoredSiteId = site.Id,
                IsUp = isUp,
                StatusCode = statusCode,
                CheckedAt = DateTime.UtcNow
            };

            await HandleSiteStatusChangeAsync(site, isUp, statusCode);

            _context.SiteStatusHistory.Add(history);
            await _context.SaveChangesAsync();

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring site: {SiteName} ({Url})", site.Name, site.Url);

            var history = new SiteStatusHistory
            {
                MonitoredSiteId = site.Id,
                IsUp = false,
                StatusCode = 0,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };

            _context.SiteStatusHistory.Add(history);
            await _context.SaveChangesAsync();

            return history;
        }
    }

    public async Task MonitorAllSitesAsync()
    {
        var sites = await _context.MonitoredSites
            .Include(s => s.User)
            .Where(s => s.IsActive)
            .ToListAsync();

        foreach (var site in sites)
        {
            await MonitorSiteAsync(site);
        }
    }

    public async Task<IEnumerable<SiteStatusHistory>> GetSiteHistoryAsync(
        int siteId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.SiteStatusHistory
            .Where(h => h.MonitoredSiteId == siteId);

        if (startDate.HasValue)
        {
            query = query.Where(h => h.CheckedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.CheckedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(h => h.CheckedAt)
            .ToListAsync();
    }

    public async Task<double> CalculateUptimePercentageAsync(
        int siteId,
        DateTime startDate,
        DateTime endDate)
    {
        var history = await _context.SiteStatusHistory
            .Where(h => h.MonitoredSiteId == siteId)
            .Where(h => h.CheckedAt >= startDate && h.CheckedAt <= endDate)
            .ToListAsync();

        if (!history.Any())
        {
            return 0;
        }

        var totalChecks = history.Count;
        var upChecks = history.Count(h => h.IsUp);

        return (double)upChecks / totalChecks * 100;
    }

    public async Task<IEnumerable<MonitoredSite>> GetDownSitesAsync()
    {
        return await _context.MonitoredSites
            .Include(s => s.StatusHistory
                .OrderByDescending(h => h.CheckedAt)
                .Take(1))
            .Where(s => s.IsActive && s.StatusHistory
                .OrderByDescending(h => h.CheckedAt)
                .FirstOrDefault().IsUp == false)
            .ToListAsync();
    }

    public async Task HandleSiteStatusChangeAsync(MonitoredSite site, bool isCurrentlyUp, int statusCode)
    {
        var previousStatus = await _context.SiteStatusHistory
            .Where(h => h.MonitoredSiteId == site.Id)
            .OrderByDescending(h => h.CheckedAt)
            .FirstOrDefaultAsync();

        if (previousStatus != null && previousStatus.IsUp != isCurrentlyUp && site.User?.Email != null)
        {
            if (!isCurrentlyUp)
            {
                await _emailService.SendSiteDownNotificationAsync(
                    site.User.Email,
                    site.Name,
                    site.Url,
                    statusCode);
            }
            else
            {
                await _emailService.SendSiteUpNotificationAsync(
                    site.User.Email,
                    site.Name,
                    site.Url);
            }
        }
    }

    public async Task<MonitoringSummary> GetMonitoringSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var sites = await _context.MonitoredSites
            .Include(s => s.StatusHistory
                .Where(h => h.CheckedAt >= startDate && h.CheckedAt <= endDate))
            .ToListAsync();

        var summary = new MonitoringSummary
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalSites = sites.Count,
            ActiveSites = sites.Count(s => s.IsActive),
            TotalChecks = sites.Sum(s => s.StatusHistory.Count),
            DowntimeIncidents = sites.Sum(s =>
                CalculateDowntimeIncidents(s.StatusHistory))
        };

        if (summary.TotalChecks > 0)
        {
            var totalUpChecks = sites.Sum(s =>
                s.StatusHistory.Count(h => h.IsUp));
            summary.OverallUptime = (double)totalUpChecks / summary.TotalChecks * 100;
        }

        return summary;
    }

    private int CalculateDowntimeIncidents(IEnumerable<SiteStatusHistory> history)
    {
        var incidents = 0;
        var wasUp = true;

        foreach (var status in history.OrderBy(h => h.CheckedAt))
        {
            if (wasUp && !status.IsUp)
            {
                incidents++;
            }
            wasUp = status.IsUp;
        }

        return incidents;
    }
}
