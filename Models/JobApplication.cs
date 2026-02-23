namespace JobAlertApi.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Job? Job { get; set; }
    }
}