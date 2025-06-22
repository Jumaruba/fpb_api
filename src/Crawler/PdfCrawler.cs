using System.Net;
using HtmlAgilityPack;

namespace Crawler;

/// <summary>
/// Gets the calendar from the fpb website.
/// </summary>
public class PdfCrawler
{
    private readonly string _url;
    private readonly HtmlWeb _web;
    private readonly WebClient _webClient;
    public string PdfFileName;
    public string Year;

    /// <summary>
    /// Class responsible for retrieving and parsing the badminton calendar for the current year.
    /// </summary>
    /// <param name="url">The where from where the calendar should be downloaded.</param>
    public PdfCrawler(string url = "https://fpbadminton.pt/calendario-epoca/")
    {
        _url = url;
        _web = new HtmlWeb();
        _webClient = new();
    }

    /// <summary>
    /// Download the pdf with the calendar from the federation website.
    /// </summary>
    /// <param name="downloadDirectory">The directory to download the file.</param>
    public void DownloadPdf(string downloadDirectory)
    {
        if (!Directory.Exists(downloadDirectory))
            Directory.CreateDirectory(downloadDirectory);
        
        var html = _web.Load(_url);
        var calendarFile =
            html.DocumentNode.SelectSingleNode(
                "//div[@class='bigslam-single-article']//div[@class='wp-block-file'][2]//a[1]");

        if (calendarFile == null)
        {
            Console.WriteLine("No calendar file found");
            return;
        }

        var fileUrl = calendarFile.Attributes["href"].Value;
        PdfFileName = fileUrl.Split('/').Last();
        if (IsFileDownloaded(fileUrl, downloadDirectory))
            DownloadFile(fileUrl, PdfFileName, downloadDirectory);

        Year = PdfFileName.Substring(5, 8);
    }

    /// <summary>
    /// Verifies if the file is already downloaded.
    /// </summary>
    /// <param name="filename">The name of the file to download.</param>
    /// <param name="downloadDirectory">The directory where the download is saved.</param>
    /// <returns>True if the file is not downloaded it. False otherwise.</returns>
    private bool IsFileDownloaded(string filename, string downloadDirectory)
    {
        var files = Directory.GetFiles(downloadDirectory, "*.pdf", SearchOption.TopDirectoryOnly);
        return !files.Contains(filename);
    }

    /// <summary>
    /// Downloads the file from a given url to the download directory.
    /// </summary>
    /// <param name="url">The url from where the file should be downloaded.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="downloadDirectory">The download directory to where we should save the file.</param>
    private void DownloadFile(string url, string fileName, string downloadDirectory)
    {
        var filePath = Path.Combine(downloadDirectory, fileName);
        _webClient.DownloadFile(url, filePath);
    }
}