using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SiteMonitor.Web.Models;

namespace SiteMonitor.Web.Services.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _templateService;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger,
        IEmailTemplateService templateService)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _templateService = templateService;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendSiteDownNotificationAsync(string to, string siteName, string siteUrl, int statusCode)
    {
        var subject = $"Alert: {siteName} is Down!";
        var htmlBody = await _templateService.GetSiteDownEmailTemplate(siteName, siteUrl, statusCode);
        await SendEmailAsync(to, subject, htmlBody);
    }

    public async Task SendSiteUpNotificationAsync(string to, string siteName, string siteUrl)
    {
        var subject = $"Good News: {siteName} is Back Online";
        var htmlBody = await _templateService.GetSiteUpEmailTemplate(siteName, siteUrl);
        await SendEmailAsync(to, subject, htmlBody);
    }

    public async Task SendWeeklyReportAsync(string to, WeeklyReportData reportData)
    {
        var subject = $"Weekly Monitoring Report - {DateTime.Now:MMMM dd, yyyy}";
        var htmlBody = await _templateService.GetWeeklyReportTemplate(reportData);
        await SendEmailAsync(to, subject, htmlBody);
    }
}
