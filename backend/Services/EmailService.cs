using System.Net;
using System.Net.Mail;
using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.Services;

public interface IEmailService
{
    Task SendLeaveDecisionEmailAsync(LeaveRequest leave, string decidedByUsername);
    Task SendLeaveSubmittedEmailAsync(LeaveRequest leave);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendLeaveDecisionEmailAsync(LeaveRequest leave, string decidedByUsername)
    {
        if (leave.Employee is null)
        {
            _logger.LogWarning("Skipped leave decision email: employee not loaded for leave {LeaveId}.", leave.Id);
            return;
        }

        var statusText = leave.Status.ToString(); // "Approved" or "Rejected"
        var subject = $"Your {leave.LeaveType} leave request has been {statusText.ToLower()}";

        var body =
            $"Hi {leave.Employee.FirstName},\n\n" +
            $"Your leave request has been {statusText.ToLower()}.\n\n" +
            $"Leave type:     {leave.LeaveType}\n" +
            $"Start date:     {leave.StartDate:MMMM d, yyyy}\n" +
            $"End date:       {leave.EndDate:MMMM d, yyyy}\n" +
            $"Duration:       {leave.DurationInDays} day(s)\n" +
            $"Reason:         {leave.Reason ?? "—"}\n" +
            $"{statusText} by:      {decidedByUsername}\n" +
            $"Decision date:  {leave.ApprovedDate:MMMM d, yyyy}\n\n" +
            "This is an automated message from the Employee Leave Management system. " +
            "Please do not reply to this email.";

        await SendAsync(leave.Employee.Email, subject, body, $"leave decision (leave {leave.Id})");
    }

    public async Task SendLeaveSubmittedEmailAsync(LeaveRequest leave)
    {
        if (leave.Employee is null)
        {
            _logger.LogWarning("Skipped leave submission email: employee not loaded for leave {LeaveId}.", leave.Id);
            return;
        }

        var manager = leave.Employee.Manager;

        if (manager is null)
        {
            // Happens when a Manager applies for their own leave — those requests
            // route to Admin for approval, but Admin accounts have no email address
            // on file in this system (they're not linked to an Employee record), so
            // there's nowhere to send a submission notification. The decision email
            // still goes out fine once an Admin acts on it.
            _logger.LogInformation(
                "Skipped leave submission email for leave {LeaveId}: {Employee} has no manager on file.",
                leave.Id, leave.Employee.Email);
            return;
        }

        var subject = $"New leave request from {leave.Employee.FirstName} {leave.Employee.LastName} awaiting your approval";

        var body =
            $"Hi {manager.FirstName},\n\n" +
            $"{leave.Employee.FirstName} {leave.Employee.LastName} has submitted a new leave request that needs your review.\n\n" +
            $"Leave type:     {leave.LeaveType}\n" +
            $"Start date:     {leave.StartDate:MMMM d, yyyy}\n" +
            $"End date:       {leave.EndDate:MMMM d, yyyy}\n" +
            $"Duration:       {leave.DurationInDays} day(s)\n" +
            $"Reason:         {leave.Reason ?? "—"}\n" +
            $"Submitted on:   {leave.AppliedDate:MMMM d, yyyy}\n\n" +
            "Log in to the Employee Leave Management system to approve or reject this request.\n\n" +
            "This is an automated message. Please do not reply to this email.";

        await SendAsync(manager.Email, subject, body, $"leave submission (leave {leave.Id})");
    }

    private async Task SendAsync(string toEmail, string subject, string body, string context)
    {
        // Email is a side effect of an already-committed database change, not part
        // of the transaction itself. If SMTP is unreachable or misconfigured, log
        // it and move on — never let a failed send turn a successful API call into
        // an error for the caller.
        try
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var fromEmail = smtp["FromEmail"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogWarning("Skipped {Context} email to {Email}: Smtp is not configured in appsettings.json.", context, toEmail);
                return;
            }

            var port = int.Parse(smtp["Port"] ?? "587");
            var username = smtp["Username"];
            var password = smtp["Password"];
            var fromName = smtp["FromName"] ?? "Employee Leave Management";
            var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = string.IsNullOrWhiteSpace(username) ? null : new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Sent {Context} email to {Email}.", context, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Context} email to {Email}.", context, toEmail);
        }
    }
}
