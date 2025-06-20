namespace Crawler.Logic;
using HtmlAgilityPack;

/// <summary>
/// Gets the calendar from the fpb website.
/// </summary>
public class CalendarRetriever
{
    private readonly string _page;
    private readonly HtmlWeb _web;

    public CalendarRetriever(string page = "https://fpbadminton.pt/")
    {
        _page = page;
        _web = new HtmlWeb();
    }

    public List<Event> GetCalendar()
    {
        var html = _web.Load(_page);
        var tables = html.DocumentNode.SelectNodes("//table");
        
        if (!IsHtmlValid(tables))
            throw new Exception("Invalid html.");
        
        var headers = GetHeaders(tables[0]);
        if (headers == null || headers.Count == 0)
            throw new NullReferenceException("No headers found.");
        
        var data = GetData(tables[1], headers);

        return data;
    }

    /// <summary>
    /// Get the headers of a table given the header nodes of a HTML table
    /// </summary>
    /// <param name="table">The HTML table with the headers.</param>
    /// <returns>The headers of the table in a list.</returns>
    private List<string>? GetHeaders(HtmlNode table)
    {
        var headerNodes = table.SelectNodes(".//thead/tr/th");
        return headerNodes?.Select(n => n.InnerText).ToList();
    }

    #region Data
    private List<Event> GetData(HtmlNode table, List<string> headers)
    {
        var rows = table.SelectNodes(".//tbody/tr");
        if (rows == null)
            return [];
        
        var result = new List<Event>();
        foreach (var data in rows)
        {
            var cells = data.SelectNodes(".//td")?.Select(n => n.InnerText).ToList();
            result.Add(new Event(cells, headers));
        }

        return result;
    }
    
    #endregion

    /// <summary>
    /// To the html be valid, it needs to contain two tables: one with the headers
    /// and another with the data.
    /// </summary>
    /// <returns>True if the table is valid. False otherwise.</returns>
    private bool IsHtmlValid(HtmlNodeCollection? tables)
    {
        if (tables == null)
            return false;

        if (tables.Count < 2)
            return false;

        return true;
    }
}