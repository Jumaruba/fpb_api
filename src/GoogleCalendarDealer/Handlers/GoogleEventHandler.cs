using System.Globalization;
using Crawler;
using Google.Apis.Calendar.v3.Data;

namespace GoogleCalendarDealer.Handlers
{
    public class GoogleEventHandler
    {
        /// <summary>
        /// The end date in google calendar is exclusive.
        /// Thus, an event that starts day 1 and ends day 2 (inclusively), needs to end on day 3 in google calendar.
        /// </summary>
        /// <param name="inclusiveEndDate">The inclusive end date.</param>
        /// <returns>The <paramref name="inclusiveEndDate"/> + 1.</returns>
        public static string GetActualEndDate(string inclusiveEndDate)
        {
            return DateTime.ParseExact(inclusiveEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .AddDays(1)
                .ToString("yyyy-MM-dd");
        }
        
        public static Event CreateEvent(FpbEvent e)
        {
            return new Event
            {
                Summary = e.EventName,
                Description = e.Description,
                End = new EventDateTime { Date = GetActualEndDate(e.EndDate)},
                Start = new EventDateTime { Date= e.StartDate },
            };
        }
        
        
    }
}
