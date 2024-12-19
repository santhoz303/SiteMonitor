using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SiteMonitor.Web.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<MonitoredSite> MonitoredSites { get; set; } = new List<MonitoredSite>();
}