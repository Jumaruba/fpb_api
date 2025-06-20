using Crawler.Logic;

namespace TestCrawler;

public class Tests
{
    private CalendarRetriever _calendarRetriever;
    [SetUp]
    public void Setup()
    {
        _calendarRetriever = new CalendarRetriever();
    }

    [Test]
    public void GetCalendar()
    {
        
        var page = _calendarRetriever.GetCalendar();
        foreach (var line in page)
        {

        }
    }
}