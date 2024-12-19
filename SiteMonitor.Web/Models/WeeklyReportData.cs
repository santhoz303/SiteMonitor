namespace SiteMonitor.Web.Models;

public class WeeklyReportData
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSites { get; set; }
    public double AverageUptime { get; set; }
    public int TotalIncidents { get; set; }
}