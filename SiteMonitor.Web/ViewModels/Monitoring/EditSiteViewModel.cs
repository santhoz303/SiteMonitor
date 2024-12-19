using System.ComponentModel.DataAnnotations;

namespace SiteMonitor.Web.ViewModels.Monitoring;

public class EditSiteViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    [StringLength(2000)]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
