using Crawler.Logic;
using Google.Apis.Calendar.v3.Data;
using GoogleCalendarDealer.Handlers;

namespace GoogleCalendarDealer.Logic;

public class CalendarMergeResult
{
    public Dictionary<string, Event> Updates = new();
    public List<Event> Additions = new();
}

/// <summary>
/// Class responsible for merging the current google calendar with the new one.
/// </summary>
public class CalendarMerge
{
    public static CalendarMergeResult Merge(Events googleCalendar, List<FpbEvent> fpbCalendar)
    {
        var fpbEvents = fpbCalendar.ToDictionary(fpb => fpb.EventName);
        var googleEvents = googleCalendar.Items.ToDictionary(googleEvent => googleEvent.Summary);
        
        var updates = GetUpdates(googleCalendar, fpbEvents);
        var additions = GetAdditions(fpbCalendar, googleEvents);

        return new CalendarMergeResult { Updates = updates, Additions = additions };
    }

    private static List<Event> GetAdditions(List<FpbEvent> fpbCalendar, Dictionary<string, Event> googleEvents)
    {
        var additions = new List<Event>();
        foreach (var fpbEvent in fpbCalendar)
        {
            if (!googleEvents.ContainsKey(fpbEvent.EventName))
                additions.Add(GoogleEventHandler.CreateEvent(fpbEvent));
        }

        return additions;
    }

    private static Dictionary<string, Event> GetUpdates(Events googleCalendar, Dictionary<string, FpbEvent> fpbEvents)
    {
        var updates = new Dictionary<string, Event>();
        foreach (var googleEvent in googleCalendar.Items)
        {
            var fpbEvent = fpbEvents[googleEvent.Summary];
            if (!IsEquals(googleEvent, fpbEvent))
                updates.Add(googleEvent.Id, GoogleEventHandler.CreateEvent(fpbEvent));
        }

        return updates;
    }


    private static bool IsEquals(Event googleEvent, FpbEvent fpbEvent)
    {
        if (googleEvent.Start.Date != fpbEvent.StartDate)
            return false;
        if (googleEvent.End.Date != GoogleEventHandler.GetActualEndDate(fpbEvent.EndDate))
            return false;
        return googleEvent.Description == Utils.ArrayAsString(fpbEvent.Local);
    }
}