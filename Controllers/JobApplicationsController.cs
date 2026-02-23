using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JobAlertApi.Data;
using JobAlertApi.Models;
using JobAlertApi.DTOs;

namespace JobAlertApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect all endpoints
    public class JobApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobApplicationsController(AppDbContext context)
        {
            _context = context;
        }

        // Apply to a job
        // POST: api/JobApplications/apply/1
        [HttpPost("apply/{jobId}")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid token" });

            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!jobExists)
                return NotFound(new { message = "Job not found" });

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.UserEmail == userEmail);

            if (alreadyApplied)
                return BadRequest(new { message = "Already applied to this job" });

            var application = new JobApplication
            {
                JobId = jobId,
                UserEmail = userEmail,
                AppliedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Applied successfully" });
        }

        // Get logged-in user's applications with pagination
        // GET: api/JobApplications/my?pageNumber=1&pageSize=5
        [HttpGet("my")]
        public async Task<IActionResult> GetMyApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid token" });

            var query = _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.UserEmail == userEmail);

            var totalCount = await query.CountAsync();

            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new JobApplicationDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    UserEmail = a.UserEmail,
                    AppliedAt = a.AppliedAt,
                    Job = a.Job != null ? new JobDto
                    {
                        Id = a.Job.Id,
                        Title = a.Job.Title,
                        Company = a.Job.Company,
                        Location = a.Job.Location,
                        CreatedDate = a.Job.CreatedDate
                    } : null
                })
                .ToListAsync();

            return Ok(new
            {
                metadata = new
                {
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                },
                data = applications
            });
        }
    }
}