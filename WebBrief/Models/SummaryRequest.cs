namespace WebBrief.Models
{
    public class SummaryRequest
    {
        public string Url { get; set; } = string.Empty;

        public string Personality { get; set; } = "Friendly";

        public string? Summary { get; set; }
    }
}
