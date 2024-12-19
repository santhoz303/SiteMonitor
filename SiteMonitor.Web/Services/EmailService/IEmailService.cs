using SiteMonitor.Web.Models;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Services.EmailService;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendSiteDownNotificationAsync(string to, string siteName, string siteUrl, int statusCode);
    Task SendSiteUpNotificationAsync(string to, string siteName, string siteUrl);
    Task SendWeeklyReportAsync(string to, WeeklyReportData reportData);
}
