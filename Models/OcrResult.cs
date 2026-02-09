namespace PdfOcrOpenAI.Models;

public class OcrResult
{
    public bool Success { get; set; }
    public string Text { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public double EstimatedCost { get; set; }
    public string? ErrorMessage { get; set; }
    public List<PageResult> Pages { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
}

public class PageResult
{
    public int PageNumber { get; set; }
    public bool Success { get; set; }
    public string Text { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public double Cost { get; set; }
    public string? ErrorMessage { get; set; }
}