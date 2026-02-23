using JobAlertApi.Data;
using JobAlertApi.DTOs;
using JobAlertApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobAlertApi.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/Jobs (PUBLIC)
        // =====================================================
        [HttpGet]
        [AllowAnonymous] // public job listing
        public async Task<IActionResult> GetJobs([FromQuery] JobQueryParams queryParams)
        {
            // Safety defaults
            var pageNumber = queryParams.PageNumber <= 0 ? 1 : queryParams.PageNumber;
            var pageSize = queryParams.PageSize <= 0 || queryParams.PageSize > 50
                ? 10
                : queryParams.PageSize;

            var query = _context.Jobs
                .AsNoTracking()
                .AsQueryable();

            // Keyword search (EF-safe)
            if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
            {
                var keyword = queryParams.Keyword.Trim();

                query = query.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.Company.Contains(keyword));
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(queryParams.Location))
            {
                var location = queryParams.Location.Trim();

                query = query.Where(j =>
                    j.Location.Contains(location));
            }

            var totalCount = await query.CountAsync();

            var jobs = await query
                .OrderByDescending(j => j.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                pageNumber,
                pageSize,
                data = jobs
            });
        }

        // =====================================================
        // ADVANCED SEARCH (LOGIN REQUIRED)
        // =====================================================
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? title,
            string? company,
            string? location,
            int page = 1,
            int pageSize = 5,
            string sortBy = "createdDate",
            string sortOrder = "desc")
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 50 ? 10 : pageSize;

            var query = _context.Jobs
                .AsNoTracking()
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(j => j.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(company))
                query = query.Where(j => j.Company.Contains(company));

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(j => j.Location.Contains(location));

            // Sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder == "asc"
                    ? query.OrderBy(j => j.Title)
                    : query.OrderByDescending(j => j.Title),

                "company" => sortOrder == "asc"
                    ? query.OrderBy(j => j.Company)
                    : query.OrderByDescending(j => j.Company),

                _ => sortOrder == "asc"
                    ? query.OrderBy(j => j.CreatedDate)
                    : query.OrderByDescending(j => j.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                data = jobs
            });
        }

        // =====================================================
        // GET BY ID (PUBLIC)
        // =====================================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound(new { message = "Job not found" });

            return Ok(job);
        }

        // =====================================================
        // CREATE JOB (ADMIN ONLY)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Job job)
        {
            if (job == null)
                return BadRequest(new { message = "Job cannot be null" });

            job.CreatedDate = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = job.Id },
                job);
        }

        // =====================================================
        // UPDATE JOB (ADMIN ONLY)
        // =====================================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Job updatedJob)
        {
            if (updatedJob == null || id != updatedJob.Id)
                return BadRequest(new { message = "Invalid job data" });

            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound(new { message = "Job not found" });

            job.Title = updatedJob.Title;
            job.Company = updatedJob.Company;
            job.Location = updatedJob.Location;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================================
        // DELETE JOB (ADMIN ONLY)
        // =====================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound(new { message = "Job not found" });

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}