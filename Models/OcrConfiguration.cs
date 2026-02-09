namespace PdfOcrOpenAI.Models;

public class OcrConfiguration
{
    public OpenAISettings OpenAI { get; set; } = new();
    public PdfSettings Pdf { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public ProcessingSettings Processing { get; set; } = new();
}

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.1;
    public string ImageDetail { get; set; } = "high";
    public string ApiUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
}

public class PdfSettings
{
    public int DpiResolution { get; set; } = 300;
    public string ImageFormat { get; set; } = "png";
    public string TempDirectory { get; set; } = "./temp";
    public string OutputDirectory { get; set; } = "./output";
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public string LogFilePath { get; set; } = "./logs/ocr-{Date}.log";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
}

public class ProcessingSettings
{
    public int MaxConcurrentPages { get; set; } = 3;
    public bool CleanupTempFiles { get; set; } = true;
    public bool SaveIntermediateResults { get; set; } = false;
}