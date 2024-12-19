using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SiteMonitor.Web.Models;

public class MonitoredSite
{
    public int Id { get; set; }

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public ICollection<SiteStatusHistory> StatusHistory { get; set; } = new List<SiteStatusHistory>();
}