namespace JobAlertApi.Models
{
    public class SavedJob
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Job? Job { get; set; }
    }
}