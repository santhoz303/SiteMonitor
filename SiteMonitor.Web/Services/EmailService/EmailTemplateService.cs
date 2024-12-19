using SiteMonitor.Web.Models;
using System.Threading.Tasks;

namespace SiteMonitor.Web.Services.EmailService;

public interface IEmailTemplateService
{
    Task<string> GetSiteDownEmailTemplate(string siteName, string siteUrl, int statusCode);
    Task<string> GetSiteUpEmailTemplate(string siteName, string siteUrl);
    Task<string> GetWeeklyReportTemplate(WeeklyReportData reportData);
}

public class EmailTemplateService : IEmailTemplateService
{
    public Task<string> GetSiteDownEmailTemplate(string siteName, string siteUrl, int statusCode)
    {
        var template = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #dc3545;'>⚠️ Site Down Alert</h2>
                    <p>Your monitored site <strong>{siteName}</strong> is currently experiencing issues.</p>
                    <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                        <p><strong>Site URL:</strong> {siteUrl}</p>
                        <p><strong>Status Code:</strong> {statusCode}</p>
                        <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </div>
                    <p>We will notify you when the site is back online.</p>
                </div>
            </body>
            </html>";

        return Task.FromResult(template);
    }

    public Task<string> GetSiteUpEmailTemplate(string siteName, string siteUrl)
    {
        var template = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #28a745;'>✅ Site is Back Online</h2>
                    <p>Good news! Your site <strong>{siteName}</strong> is now back online.</p>
                    <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                        <p><strong>Site URL:</strong> {siteUrl}</p>
                        <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </div>
                </div>
            </body>
            </html>";

        return Task.FromResult(template);
    }

    public Task<string> GetWeeklyReportTemplate(WeeklyReportData reportData)
    {
        var template = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #007bff;'>📊 Weekly Monitoring Report</h2>
                    <div style='background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                        <h3>Summary for {reportData.StartDate:MMM dd} - {reportData.EndDate:MMM dd, yyyy}</h3>
                        <ul style='list-style: none; padding: 0;'>
                            <li style='margin: 10px 0;'>Total Sites Monitored: {reportData.TotalSites}</li>
                            <li style='margin: 10px 0;'>Average Uptime: {reportData.AverageUptime:F2}%</li>
                            <li style='margin: 10px 0;'>Total Incidents: {reportData.TotalIncidents}</li>
                        </ul>
                    </div>
                </div>
            </body>
            </html>";

        return Task.FromResult(template);
    }
}