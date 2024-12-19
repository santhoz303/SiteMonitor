using SiteMonitor.Web.Models;
using System.Collections.Generic;

namespace SiteMonitor.Web.ViewModels.Dashboard;

public class MonitoredSiteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public SiteStatusHistory? LastStatus { get; set; }
    public List<SiteStatusHistory> RecentHistory { get; set; } = new();
    public double UptimePercentage { get; set; }
}