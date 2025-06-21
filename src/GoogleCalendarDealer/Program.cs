using Crawler.Logic;
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
        var calendarRetriever = new CalendarRetriever(_settings.Website);

        var auth = new Auth();
        await auth.Authenticate();

        var calendarId = _settings.CalendarId;
        var request = auth.Service.Events.List(calendarId);
        request.TimeMinDateTimeOffset = DateTimeOffset.Now;
        var googleEvents = await request.ExecuteAsync();
        var fpbEvents = calendarRetriever.GetCalendar();

        var calendarMergeResult = CalendarMerge.Merge(googleEvents, fpbEvents);
        await ApplyUpdates(auth, calendarMergeResult.Updates, calendarId);
        await ApplyAdditions(auth, calendarMergeResult.Additions, calendarId);
    }

    private static async Task ApplyUpdates(Auth auth, Dictionary<string, Event> updates, string calendarId)
    {
        var service = auth.Service;
        foreach (var updatedEvent in updates)
        {
            var request = service.Events.Update(updatedEvent.Value, calendarId, updatedEvent.Key);
            Console.WriteLine($"Updating {updatedEvent.Value.Summary}...");
            await request.ExecuteAsync();
        }
    }

    private static async Task ApplyAdditions(Auth auth, List<Event> additions, string calendarId)
    {
        var service = auth.Service;
        foreach (var newEvent in additions)
        {
            var request = service.Events.Insert(newEvent, calendarId);
            Console.WriteLine($"Adding {newEvent.Summary}...");
            await request.ExecuteAsync();
        }
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