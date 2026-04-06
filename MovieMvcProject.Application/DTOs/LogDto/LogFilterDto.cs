namespace MovieMvcProject.Application.DTOs.LogDto
{
    public class LogFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string? LogLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        
        public Dictionary<string, string> ToRouteDictionary()
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(SearchTerm)) dict.Add(nameof(SearchTerm), SearchTerm);
            if (!string.IsNullOrWhiteSpace(LogLevel)) dict.Add(nameof(LogLevel), LogLevel);
            if (StartDate.HasValue) dict.Add(nameof(StartDate), StartDate.Value.ToString("yyyy-MM-dd"));
            if (EndDate.HasValue) dict.Add(nameof(EndDate), EndDate.Value.ToString("yyyy-MM-dd"));
            dict.Add(nameof(PageSize), PageSize.ToString());

            return dict;
        }
    }
}
