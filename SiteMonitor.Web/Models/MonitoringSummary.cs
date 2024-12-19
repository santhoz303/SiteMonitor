namespace SiteMonitor.Web.Models;

public class MonitoringSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSites { get; set; }
    public int ActiveSites { get; set; }
    public int TotalChecks { get; set; }
    public int DowntimeIncidents { get; set; }
    public double OverallUptime { get; set; }
}