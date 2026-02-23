using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JobAlertApi.Data;
using JobAlertApi.Models;

namespace JobAlertApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavedJobsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SavedJobsController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 Save a job
        // POST: api/SavedJobs/5
        [HttpPost("{jobId}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid token" });

            // check job exists
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);
            if (!jobExists) return NotFound(new { message = "Job not found" });

            // prevent duplicate
            var alreadySaved = await _context.SavedJobs
                .AnyAsync(s => s.JobId == jobId && s.UserEmail == userEmail);
            if (alreadySaved)
                return BadRequest(new { message = "Already saved this job" });

            var savedJob = new SavedJob
            {
                JobId = jobId,
                UserEmail = userEmail,
                SavedAt = DateTime.UtcNow
            };

            _context.SavedJobs.Add(savedJob);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Job saved successfully" });
        }

        // 📋 Get saved jobs (paginated)
        // GET: api/SavedJobs
        [HttpGet]
        public async Task<IActionResult> GetSavedJobs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid token" });

            var query = _context.SavedJobs
                .Include(s => s.Job)
                .Where(s => s.UserEmail == userEmail)
                .OrderByDescending(s => s.SavedAt);

            var totalCount = await query.CountAsync();

            var savedJobs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                totalCount,
                totalPages,
                pageNumber,
                pageSize,
                data = savedJobs
            });
        }
    }
}