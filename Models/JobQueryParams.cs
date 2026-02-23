namespace JobAlertApi.Models
{
    public class JobQueryParams
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }

        private int pageSize = 10;
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > 50 ? 50 : value;
        }
    }
}