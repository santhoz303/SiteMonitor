using System.Collections.Generic;

namespace SiteMonitor.Web.ViewModels.Dashboard;

public class DashboardViewModel
{
    public List<MonitoredSiteViewModel> Sites { get; set; } = new();
    public int TotalSites { get; set; }
    public int DownSites { get; set; }
    public double OverallUptime { get; set; }
}