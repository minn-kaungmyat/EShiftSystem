using EShiftSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Services
{
    public interface IJobNumberGenerator
    {
        Task<string> GenerateJobNumberAsync();
    }

    public class JobNumberGenerator : IJobNumberGenerator
    {
        private readonly ApplicationDbContext _context;

        public JobNumberGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateJobNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            // Get the count of jobs created this month
            var monthlyJobCount = await _context.Jobs
                .Where(j => j.CreatedAt.Year == currentYear && j.CreatedAt.Month == currentMonth)
                .CountAsync();

            // Generate job number in format: JOB-YYYY-MM-XXXX
            var jobNumber = $"JOB-{currentYear}-{currentMonth:D2}-{(monthlyJobCount + 1):D4}";

            // Ensure uniqueness (in case of concurrent requests)
            while (await _context.Jobs.AnyAsync(j => j.JobNumber == jobNumber))
            {
                monthlyJobCount++;
                jobNumber = $"JOB-{currentYear}-{currentMonth:D2}-{(monthlyJobCount + 1):D4}";
            }

            return jobNumber;
        }
    }
} 