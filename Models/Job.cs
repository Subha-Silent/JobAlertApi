namespace JobAlertApi.Models
{
    public class Job
    {
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }
}
