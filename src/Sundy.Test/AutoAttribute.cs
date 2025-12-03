using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sundy.Core;

namespace Sundy.Test;

/// <summary>
/// AutoData attribute that sets up AutoFixture with the necessary dependencies for Sundy tests.
/// </summary>
public class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute()
        : base(() =>
        {
            var fixture = new Fixture();

            // Create service collection like in the main app
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => { builder.AddSerilog(dispose: false); });

            // Add Mediator
            services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

            // Use in-memory database for tests
            // Keep the connection open for the lifetime of the test to maintain the in-memory database
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
            connection.Open();
            
            services.AddDbContext<SundyDbContext>(options =>
                options.UseSqlite(connection));

            // Register stores and database manager
            services.AddScoped<EventStore>();
            services.AddScoped<DatabaseManager>();
            services.AddScoped<CalendarStore>();

            // Register Services
            services.AddScoped<ICalendarProvider, LocalCalendarProvider>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Register a resolver that gets services from the DI container
            fixture.Customize(new SupportMutableValueTypesCustomization());
            fixture.Customizations.Add(new ServiceProviderRelay(serviceProvider));

            return fixture;
        })
    {
    }
}