using SiteMonitor.Web.Models;

namespace SiteMonitor.Web.ViewModels.Admin;

public class AdminSiteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public SiteStatusHistory? LastStatus { get; set; }
    public double UptimePercentage { get; set; }
    public int TotalChecks { get; set; }
    public int DowntimeIncidents { get; set; }
}