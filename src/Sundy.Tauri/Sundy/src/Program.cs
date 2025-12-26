using System.Data;
using Mediator;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sundy;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Commands;
using Sundy.Core.Meta;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register in-memory SQLite connection (must stay open for lifetime of app)
var connectionString = "Data Source=SundyInMemory;Mode=Memory;Cache=Shared";
var connection = new SqliteConnection(connectionString);
connection.Open();
builder.Services.AddSingleton<IDbConnection>(connection);

// Register database manager and Dapper-based stores
builder.Services.AddSingleton<DapperDatabaseManager>();
builder.Services.AddSingleton<IEventStore, DapperEventStore>();
builder.Services.AddSingleton<ICalendarStore, DapperCalendarStore>();

// Register Outlook dependencies (required by GetEventsInRangeQueryHandler)
builder.Services.AddSingleton<OutlookGraphOptions>(_ => new OutlookGraphOptions());
builder.Services.AddSingleton<ILogger<MicrosoftGraphAuthService>>(
    _ => NullLogger<MicrosoftGraphAuthService>.Instance);
builder.Services.AddSingleton<MicrosoftGraphAuthService>();
builder.Services.AddSingleton<OutlookCalendarProvider>();

// Register Mediator
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Singleton;
});

var host = builder.Build();

// Initialize database schema
var mediator = host.Services.GetRequiredService<IMediator>();
await mediator.Send(new InitializeDatabaseCommand());

// Seed initial data
await SeedDataAsync(host.Services);

await host.RunAsync();

static async Task SeedDataAsync(IServiceProvider services)
{
    var mediator = services.GetRequiredService<IMediator>();
    var calendarStore = services.GetRequiredService<ICalendarStore>();

    // Check if data already exists
    var existingCalendars = await calendarStore.GetAllAsync(CancellationToken.None);
    if (existingCalendars.Count > 0) return;

    // Seed calendars using commands
    var myCalendar = new Calendar
    {
        Id = Guid.NewGuid().ToString(),
        Name = "My Calendar",
        Color = "#4285f4",
        Type = CalendarType.Local
    };
    await mediator.Send(new CreateCalendarCommand(myCalendar));

    var workCalendar = new Calendar
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Work",
        Color = "#7c3aed",
        Type = CalendarType.Local
    };
    await mediator.Send(new CreateCalendarCommand(workCalendar));

    // Seed sample events using commands
    var now = DateTimeOffset.Now;
    var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);

    await mediator.Send(new CreateEventCommand(new CalendarEvent
    {
        CalendarId = myCalendar.Id,
        Title = "Team Meeting",
        StartTime = today.AddHours(10),
        EndTime = today.AddHours(11),
        Description = "Weekly team sync"
    }));

    await mediator.Send(new CreateEventCommand(new CalendarEvent
    {
        CalendarId = workCalendar.Id,
        Title = "Project Review",
        StartTime = today.AddHours(14),
        EndTime = today.AddHours(15).AddMinutes(30),
        Description = "Q4 project status review"
    }));
}
