namespace SiteMonitor.Web.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalSites { get; set; }
    public int TotalChecks { get; set; }
    public int DownSites { get; set; }
    public double AverageUptime { get; set; }
    public Dictionary<string, int> SiteStatusDistribution { get; set; } = new();
}