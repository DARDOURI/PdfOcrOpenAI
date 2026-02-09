using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PdfOcrOpenAI.Models;
using PdfOcrOpenAI.Services;
using Serilog;
using System.Net.Http.Headers;

namespace PdfOcrOpenAI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var config = new OcrConfiguration();
        configuration.Bind(config);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(config.Logging.LogLevel))
            .WriteTo.Console()
            .WriteTo.File(config.Logging.LogFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("=== PDF OCR with OpenAI - Starting ===");
            Log.Information("Environment: {Env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

            if (string.IsNullOrWhiteSpace(config.OpenAI.ApiKey) || config.OpenAI.ApiKey == "METTEZ_VOTRE_CLE_ICI")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ ERREUR: Clé API OpenAI non configurée!");
                Console.ResetColor();
                Console.WriteLine("\n📝 Instructions:");
                Console.WriteLine("1. Ouvrez le fichier 'appsettings.json'");
                Console.WriteLine("2. Remplacez 'METTEZ_VOTRE_CLE_ICI' par votre clé API OpenAI");
                Console.WriteLine("3. Sauvegardez le fichier");
                Console.WriteLine("4. Relancez le programme\n");
                Console.WriteLine("💡 Obtenez votre clé sur: https://platform.openai.com/api-keys\n");
                return 1;
            }

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(config);

                    services.AddHttpClient("OpenAI", client =>
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", config.OpenAI.ApiKey);
                        client.Timeout = TimeSpan.FromSeconds(config.OpenAI.TimeoutSeconds);
                    });

                    services.AddSingleton<ILogger>(Log.Logger);
                    services.AddSingleton<IPdfConverter, PdfConverter>();
                    services.AddSingleton<IOpenAIOcrService, OpenAIOcrService>();
                })
                .UseSerilog()
                .Build();

            var ocrService = host.Services.GetRequiredService<IOpenAIOcrService>();

            if (args.Length == 0)
            {
                Console.WriteLine("\n📋 Usage: PdfOcrOpenAI <path-to-pdf>");
                Console.WriteLine("📋 Exemple: PdfOcrOpenAI document.pdf");
                Console.WriteLine("📋 Exemple: dotnet run document.pdf\n");
                return 1;
            }

            var pdfPath = args[0];

            if (!File.Exists(pdfPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Fichier introuvable: {pdfPath}\n");
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine($"\n🚀 Processing: {pdfPath}");
            Console.WriteLine($"📋 Model: {config.OpenAI.Model}");
            Console.WriteLine($"🎯 Detail: {config.OpenAI.ImageDetail}\n");

            var result = await ocrService.ProcessPdfAsync(pdfPath);

            Console.WriteLine("\n" + new string('=', 80));
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ OCR COMPLETED SUCCESSFULLY");
                Console.ResetColor();
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"📄 Pages processed: {result.PageCount}");
                Console.WriteLine($"📝 Characters extracted: {result.Text.Length:N0}");
                Console.WriteLine($"⏱️  Processing time: {result.ProcessingTime.TotalSeconds:F2}s");
                Console.WriteLine($"💰 Estimated cost: ${result.EstimatedCost:F4}");
                Console.WriteLine($"📊 Success rate: {result.Pages.Count(p => p.Success)}/{result.Pages.Count} pages");
                Console.WriteLine(new string('=', 80));

                Console.WriteLine("\n📄 Per-Page Statistics:");
                foreach (var page in result.Pages)
                {
                    var status = page.Success ? "✓" : "✗";
                    Console.WriteLine($"  {status} Page {page.PageNumber}: {page.Text.Length} chars, " +
                                      $"{page.InputTokens + page.OutputTokens} tokens, ${page.Cost:F4}");
                }

                Console.WriteLine($"\n💾 Results saved to: {config.Pdf.OutputDirectory}\n");
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ OCR FAILED");
                Console.ResetColor();
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"Error: {result.ErrorMessage}\n");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Erreur fatale: {ex.Message}\n");
            Console.ResetColor();
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}