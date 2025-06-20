using Crawler.Logic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

var calendarRetriever = new CalendarRetriever();
app.MapGet("/Calendar", () => calendarRetriever.GetCalendar());

app.Run();