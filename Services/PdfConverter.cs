using PDFtoImage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Serilog;

namespace PdfOcrOpenAI.Services;

public interface IPdfConverter
{
    Task<List<string>> ConvertPdfToImagesAsync(string pdfPath, string outputDirectory);
}

public class PdfConverter : IPdfConverter
{
    private readonly ILogger _logger;

    public PdfConverter(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> ConvertPdfToImagesAsync(string pdfPath, string outputDirectory)
    {
        var imagePaths = new List<string>();

        try
        {
            _logger.Information("Converting PDF to images: {PdfPath}", pdfPath);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            await using var pdfStream = File.OpenRead(pdfPath);
            
            var pageCount = PDFtoImage.Conversion.GetPageCount(pdfStream);
            _logger.Information("PDF has {PageCount} pages", pageCount);

            pdfStream.Position = 0;

            await foreach (var (image, pageNumber) in PDFtoImage.Conversion.ToImagesAsync(pdfStream))
            {
                var imagePath = Path.Combine(outputDirectory, $"page_{pageNumber:D4}.png");
                
                await image.SaveAsPngAsync(imagePath, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression
                });

                imagePaths.Add(imagePath);
                _logger.Debug("Saved page {PageNumber} to {ImagePath}", pageNumber, imagePath);
            }

            _logger.Information("Successfully converted {Count} pages to images", imagePaths.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error converting PDF to images");
            throw;
        }

        return imagePaths;
    }
}