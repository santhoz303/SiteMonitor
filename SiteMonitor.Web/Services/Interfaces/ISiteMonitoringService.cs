using SiteMonitor.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Services.Interfaces;

public interface ISiteMonitoringService
{
    Task<bool> CheckSiteStatusAsync(string url);
    Task<SiteStatusHistory> MonitorSiteAsync(MonitoredSite site);
    Task MonitorAllSitesAsync();
    Task<IEnumerable<SiteStatusHistory>> GetSiteHistoryAsync(int siteId, DateTime? startDate = null, DateTime? endDate = null);
    Task<double> CalculateUptimePercentageAsync(int siteId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MonitoredSite>> GetDownSitesAsync();
    Task HandleSiteStatusChangeAsync(MonitoredSite site, bool isCurrentlyUp, int statusCode);
    Task<MonitoringSummary> GetMonitoringSummaryAsync(DateTime startDate, DateTime endDate);
}