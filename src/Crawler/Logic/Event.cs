namespace Crawler.Logic;


public class Event
{
    public List<string> Local { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string EventName { get; set; }

    private string _localHeader = "Local";
    private string _eventName = "Evento";
    private string _days = "Dias";
    private string _month = "Mês";


    public Event(List<string> cells, List<string> headers)
    {
        AddLocal(cells, headers);
        AddEventName(cells, headers);
        AddStartAndEndDate(cells, headers);
    }

    private void AddStartAndEndDate(List<string> cells, List<string> headers)
    {
        var monthPos = -1;
        var dayPos = -1;
        for (ushort i = 0; i < headers.Count; i++)
        {
            if (headers[i] == _month)
                monthPos = i;
            else if (headers[i] == _days)
                dayPos = i;
        }

        // No month or day specified.
        if (monthPos == -1 || dayPos == -1)
        {
            StartDate = DateTime.MinValue.ToString("dd/MM/yyyy");
            EndDate = DateTime.MinValue.ToString("dd/MM/yyyy");
            return;
        }

        // format: mes/year, example Ago/25
        var monthYear = cells[monthPos].Split('/');
        var month = Utils.MonthNameToNum[monthYear[0]];
        var year = "20" + monthYear[1];
        
        // Example when there are two days: "19 e 20".
        // Example when there are more than two days: "19 a 21"
        char splitChar = ' ';
        if (cells[dayPos].Contains('a'))
            splitChar = 'a';
        else if (cells[dayPos].Contains('e'))
            splitChar = 'e';
        
        var days = cells[dayPos].Split(splitChar);
        var startDay = days[0].Trim().PadLeft(2, '0');
        var endDay = days.Count() > 1 ? days[1].Trim().PadLeft(2, '0') : startDay;

        var startMonth = month;
        if (int.Parse(startDay) > int.Parse(endDay))
            startMonth = (int.Parse(startMonth) -1).ToString().PadLeft(2, '0');
        
        StartDate = DateTime.ParseExact($"{startDay}-{startMonth}-{year}", "dd-MM-yyyy", null).ToString("dd-MM-yyyy");
        EndDate = DateTime.ParseExact($"{endDay}-{month}-{year}", "dd-MM-yyyy", null).ToString("dd-MM-yyyy");
    }
    
    private void AddEventName(List<string> cells, List<string> headers)
    {
        var pos = headers.FindIndex(i => i == _eventName);
        EventName = cells[pos].Replace("&amp;", "&");
    }

    private void AddLocal(List<string> cells, List<string> headers)
    {
        var pos = headers.FindIndex(i => i == _localHeader);
        Local = cells[pos].Split(';').Select(local => local.Trim()).ToList();
    }
}