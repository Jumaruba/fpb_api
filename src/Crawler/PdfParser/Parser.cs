using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Crawler.PdfParser;

public class Parser
{
    private readonly LineDiscard _lineDiscard = new();

    /// <summary>
    /// String that indicates a new month.
    /// If this string is found, the next 6 lines should be discarded.
    /// </summary>
    private const string MonthStartString = "Dia Evento Localidade";

    /// <summary>
    /// Reads the content of a pdf.
    /// </summary>
    /// <returns>The content of a pdf. Each page is a position in the list.</returns>
    public List<string> ReadPdfPages(string filePath)
    {
        var pdfReader = new PdfReader(filePath);
        var pages = new List<string>();

        for (int i = 0; i < pdfReader.NumberOfPages; i++)
        {
            var locationTextExtractionStrategy = new SimpleTextExtractionStrategy();

            var textFromPage = PdfTextExtractor.GetTextFromPage(pdfReader, i + 1, locationTextExtractionStrategy);
            pages.Add(textFromPage);
        }

        return pages;
    }
    
    /// <summary>
    /// Parses all the events of a from a pdf, given the pdf pages content.
    /// </summary>
    /// <param name="pages">The content of the pdf separated by pages.</param>
    /// <returns>A list of events for each month. The first index of the list, for instance, contains the events
    /// for January.</returns>
    public List<List<FpbEvent>> ParsePdf(List<string> pages)
    {
        // Structure containing the lines of a month.
        var monthEventLines = new List<string>();
        var result = new List<List<FpbEvent>>();
        
        var currentMonth = -1;
        // The number of the first n events to be ignored in a month,
        // because they were already computed in the previous month.
        var eventsToJump = 0;
        
        foreach (var page in pages)
        {
            var pageLines = page.Split("\n");
            foreach (var line in pageLines)
            {
                // The month -1 is not computed, because when calling ParseMonth, monthEventLines is empty.
                if (IsNewMonth(line))
                {
                    (var monthEvents, eventsToJump) = ParseEventsInMonth(monthEventLines, currentMonth, eventsToJump);
                    result.Add(monthEvents);
                    currentMonth++;
                    monthEventLines.Clear();
                }
                
                if (_lineDiscard.ShouldDiscard(line))
                    continue;

                monthEventLines.Add(line);
            }
        }

        return result;
    }

    #region ParseMonth

    /// <summary>
    /// 
    /// </summary>
    /// <param name="events"></param>
    /// <param name="currentMonth"></param>
    /// <param name="numEventsToIgnore"></param>
    /// <returns></returns>
    private (List<FpbEvent>, int) ParseEventsInMonth(List<string> events, int currentMonth, int numEventsToIgnore)
    {
        var result = new List<FpbEvent>();
        var eventsToJumpNext = 0;

        var pos = IgnoreEvents(numEventsToIgnore, events);
        
        while (pos < events.Count)
        {
            var line = events[pos];
            FpbEvent.ParseEventName(line, out var eventName);
            
            // Extract the date.
            var daysAsString = FpbEvent.ParseDaysAsString(line);
            var (startDate, endDate) = FpbEvent.GetStartDateAndEndDate(daysAsString, currentMonth + 1, 2025);
            if (startDate.Month != endDate.Month)
                eventsToJumpNext++;

            pos++;

            var description = GetDescription(events, ref pos);

            var e = new FpbEvent
            {
                EventName = eventName,
                Description = description,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };
            result.Add(e);
            
        }

        return (result, eventsToJumpNext);
    }

    private string GetDescription(List<string> events, ref int pos)
    {
        var description = new StringBuilder();
        var isNewEvent = true;
        if (pos < events.Count)
            isNewEvent = FpbEvent.ParseEventName(events[pos],  out _);
        while (pos < events.Count && !isNewEvent)
        {
            // If it is not a date, should go to the description without any formatation.
            if (!GetSubscriptionDates(events[pos], ref description))
                description.AppendLine(events[pos].Trim());
            pos++;
            if (pos < events.Count)
                isNewEvent = FpbEvent.ParseEventName(events[pos], out _);
        }
        return description.ToString();
    }

    private bool GetSubscriptionDates(string line, ref StringBuilder sb)
    {
        List<string> dateHeads = ["Start subscription", "End subscription", "Withdrawal limit without fine"];
        var matches = Regex.IsMatch(line, @"^(\d{1,2}\/\d{1,2}\/\d{2,4}\s?){1,3}$");
        // There are no dates specified.
        if (!matches)
            return false;

        var dates = line.Split(" ");
        for (var i = 0; i < dates.Length; i++)
        {
            sb.Append($"{dateHeads[i]}: {dates[i]}\n");
        }

        return true;
    }
    
    /// <summary>
    /// Function that ignores the first N events in a list and returns the position in the array after ignoring these events.
    /// </summary>
    /// <param name="numEventsToIgnore">The number of events to ignore/jump.</param>
    /// <param name="events">A list of strings representing the events. An event can be represented by many indexes, not just one.</param>
    /// <returns></returns>
    private int IgnoreEvents(int numEventsToIgnore, List<string> events)
    {
        var pos = 0;
        while (numEventsToIgnore > 0 && pos < events.Count)
        {
            pos++;
            var line = events[pos];
            var isNewEvent = FpbEvent.ParseEventName(line, out _);
            if (isNewEvent)
                numEventsToIgnore--;
        }

        return pos;
    }
    
    #endregion


    #region Utils
    /// <summary>
    /// Identifies if this line indicates the beggining of a new month.
    /// </summary>
    /// <param name="line">The line to be analyzed</param>
    /// <returns>True if the line indicates the start of a new month. False otherwise.</returns>
    private bool IsNewMonth(string line)
    {
        return line.Contains(MonthStartString);
    }
    #endregion
}