namespace Misete.Models
{
    /// <summary>
    /// Record representing the result of a search.
    /// </summary>
    public record ImageAnalysisModel
    {
        public string? DirectoryName { get; set; }
        public string? TagName { get; set; }
        public string? TagDescription { get; set; }
        public int? TagType { get; set; }
    }
}