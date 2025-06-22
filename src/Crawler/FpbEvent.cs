using System.Text.RegularExpressions;

namespace Crawler;

public class FpbEvent
{
    public string Description { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string EventName { get; set; }


    /// <summary>
    /// Extract the days the event happens from the first line of the event.
    /// </summary>
    /// <param name="line">The first line of the event.</param>
    /// <returns>The days that event happens. If there are no days specified it returns null.</returns>
    /// <example>
    /// Output examples:
    /// <list type="bullet">
    ///     <item>"22 a 24"</item>
    ///     <item>"1"</item>
    ///     <item>"22 e 23"</item>
    ///     <item>"17"</item>
    ///     <item>null</item>
    /// </list>
    /// </example>
    public static string? ParseDaysAsString(string line)
    {
        var matchManyDays = Regex.Match(line, @"^\d{1,2}\s*(e|a)\s*\d{1,2}\b");
        if (matchManyDays.Success)
            return matchManyDays.Value.Trim();

        var matchOneDay = Regex.Match(line, @"^\d{1,2}\s\b");
        if (matchOneDay.Success)
            return matchOneDay.Value.Trim();

        return null;
    }

    public static (DateTime, DateTime) GetStartDateAndEndDate(string? days, int month, int year)
    {
        if (string.IsNullOrEmpty(days))
            return (DateTime.MinValue, DateTime.MinValue);

        char splitChar = ' ';
        if (days.Contains('a'))
            splitChar = 'a';
        else if (days.Contains('e'))
            splitChar = 'e';

        var splitedDays = days.Split(splitChar);
        var startDay = splitedDays[0].Trim().PadLeft(2, '0');
        var endDay = splitedDays.Count() > 1 ? splitedDays[1].Trim().PadLeft(2, '0') : startDay;

        var startMonth = month.ToString().PadLeft(2, '0');
        var endMonth = month.ToString().PadLeft(2, '0');
        if (int.Parse(startDay) > int.Parse(endDay))
            endMonth = (month + 1).ToString().PadLeft(2, '0');

        var startDate = DateTime.ParseExact($"{startDay}-{startMonth}-{year}", "dd-MM-yyyy", null);
        var endDate = DateTime.ParseExact($"{endDay}-{endMonth}-{year}", "dd-MM-yyyy", null);

        return (startDate, endDate);
    }

    /// <summary>
    /// Tries to get the name of the event.
    /// This is a best effort function.
    /// There isn't exactly where the name starts and where it ends.
    /// </summary>
    /// <param name="line">The line to extract the event from.</param>
    /// <param name="eventName">The name of the events to be retrieved. If the name of the event can't be extract it returns null.</param>
    /// <returns>True if the name of the event was extracted, false otherwise.</returns>
    public static bool ParseEventName(string line, out string? eventName)
    {
        var days = ParseDaysAsString(line);
        if (days == null)
        {
            eventName = null;
            return false;
        }

        // Every event should start with the date of start and the date of the end.
        var titleStart = days.Length;


        // Try to find out where the title ends.
        // CAR is usually a location. We are attempting to exclude the location from the title.
        var titleEnd = line?.IndexOf("CAR Badminton, Caldas da Rainha");
        if (titleEnd == null || titleEnd == -1)
        {
            // We will include the location in the title. We will try to exclude deadlines.
            var match = Regex.Match(line, @"\b\d{1,2}\/\d{1,2}\/\d{2,4}\b");
            // There are no deadlines. The end of the string is the end of the title.
            if (!match.Success)
                titleEnd = line.Length;
            else
                titleEnd = line.IndexOf(match.Value);
        }

        eventName = line.Substring(titleStart, titleEnd.Value - titleStart).Trim();
        return true;
    }

    /// <inheritdoc /> 
    public override string ToString()
    {
        return $"{{ Name: {EventName},\n StartDate: {StartDate},\n EndDate: {EndDate},\n Description: {Description} }}";
    }
}