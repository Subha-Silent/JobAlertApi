namespace JobAlertApi.DTOs
{
    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }

        public JobDto? Job { get; set; } = new();
    }
}