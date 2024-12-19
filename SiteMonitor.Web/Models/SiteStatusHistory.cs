using System;
using System.ComponentModel.DataAnnotations;

namespace SiteMonitor.Web.Models;

public class SiteStatusHistory
{
    public int Id { get; set; }

    [Required]
    public int MonitoredSiteId { get; set; }
    public MonitoredSite? MonitoredSite { get; set; }

    public bool IsUp { get; set; }
    public int StatusCode { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}
