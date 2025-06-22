using Crawler;
using Crawler.PdfParser;

var builder = WebApplication.CreateBuilder(args);
var downloadDirectory = $"{Directory.GetCurrentDirectory()}/Download/";
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

var pdfCrawler = new PdfCrawler();
pdfCrawler.DownloadPdf(downloadDirectory);

// Combine directory
var filePath = Path.Combine(downloadDirectory, pdfCrawler.PdfFileName);
var parser = new Parser();
var pages = parser.ReadPdfPages(filePath);
var content = parser.ParsePdf(pages);

app.MapGet("/Calendar", () => content);

app.Run();