using Crawler;
using Crawler.PdfParser;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using GoogleCalendarDealer.Handlers;
using GoogleCalendarDealer.Logic;
using Microsoft.Extensions.Configuration;

class Program
{
    private static AppSettings _settings = new();

    public static async Task Main(string[] args)
    {
        BuildSettings();
        var auth = new Auth();
        await auth.Authenticate();
        
        if (args.Contains("/add"))
            await AddEventsToCalendar(auth);

        if (args.Contains("/delete"))
            await DeleteEvents(auth);

    }

    #region Operations
    private static async Task DeleteEvents(Auth auth)
    {
        Console.WriteLine("Deleting entire calendar...");
        var calendarId = _settings.CalendarId;
        var request = auth.Service.Events.List(calendarId);
        var events = await request.ExecuteAsync();
        foreach (var e in events.Items)
        {
            if (DateTime.Parse(e.Start.Date).Year != DateTime.Now.Year)
                continue;
            var deleteRequest = auth.Service.Events.Delete(calendarId, e.Id);
            Console.WriteLine($"{e.Summary}");
            await deleteRequest.ExecuteAsync();
        }
        Console.WriteLine("Deleted entire calendar.");
    }

    private static async Task AddEvents(Auth auth, IEnumerable<Event> additions, string calendarId)
    {
        var service = auth.Service;
        foreach (var newEvent in additions)
        {
            UpdateEventColor(newEvent);
            var request = service.Events.Insert(newEvent, calendarId);
            Console.WriteLine($"Adding {newEvent.Summary}...");
            await request.ExecuteAsync();
        }
    }
    
    #endregion

    private static void UpdateEventColor(Event e)
    {
        if (e.Summary.Contains("BWF"))
            e.ColorId = "8";    // grey,graphite
        if (e.Summary.Contains("Torneio de Clube"))
            e.ColorId = "9";    // blueberry
        if (e.Summary.Contains("Jornada Nacional") || e.Summary.Contains("Campeonato Nacional"))
            e.ColorId = "11";   // red, tomato
        if (e.Summary.Contains("Zonal"))
            e.ColorId = "10";   // green,basil

    }
    private static async Task AddEventsToCalendar(Auth auth)
    {
        Console.WriteLine("Adding events to calendar...");
        var calendarId = _settings.CalendarId;
        var request = auth.Service.Events.List(calendarId);
        request.TimeMinDateTimeOffset = DateTimeOffset.Now;

        var fpbEvents = GetEventsFromPdf();
        var flattenFpbEvents = fpbEvents.SelectMany(x => x).ToList();
        var googleEventsToAdd = flattenFpbEvents.Select(fpbEvent => GoogleEventHandler.CreateEvent(fpbEvent));
        
        await AddEvents(auth, googleEventsToAdd, calendarId);
        Console.WriteLine("Events added to calendar.");
    }

    private static List<List<FpbEvent>> GetEventsFromPdf()
    {
        var pdfCrawler = new PdfCrawler();
        var downloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), _settings.DownloadDirectory);
        pdfCrawler.DownloadPdf(downloadDirectory);

        // Combine directory
        var filePath = Path.Combine(downloadDirectory, pdfCrawler.PdfFileName);
        var parser = new Parser();
        var pages = parser.ReadPdfPages(filePath);
        return parser.ParsePdf(pages);
    }



    private static void BuildSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings/appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        
        _settings = config.GetSection("AppSettings").Get<AppSettings>();
    }
}