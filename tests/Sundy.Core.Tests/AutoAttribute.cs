using System.Data;
using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;

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
            services.AddLogging(builder => {  });

            // Add Mediator
            services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

            // Register in-memory SQLite connection for Dapper-based components
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            services.AddSingleton<IDbConnection>(connection);
            services.AddSingleton<DapperDatabaseManager>();

            // Register in-memory stores for testing
            services.AddSingleton<InMemoryEventStore>();
            services.AddSingleton<IEventStore>(sp => sp.GetRequiredService<InMemoryEventStore>());
            services.AddSingleton<InMemoryCalendarStore>();
            services.AddSingleton<ICalendarStore>(sp => sp.GetRequiredService<InMemoryCalendarStore>());

            // Register Outlook services (not connected in tests, but needed for DI)
            services.AddSingleton<MicrosoftGraphAuthService>();
            services.AddSingleton<OutlookCalendarProvider>();

            // Register Services
            services.AddScoped<ICalendarProvider, LocalCalendarProvider>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Initialize database schema for tests that need it
            var dbManager = serviceProvider.GetRequiredService<DapperDatabaseManager>();
            dbManager.InitializeDatabaseAsync().GetAwaiter().GetResult();

            // Register a resolver that gets services from the DI container
            fixture.Customize(new SupportMutableValueTypesCustomization());
            fixture.Customizations.Add(new ServiceProviderRelay(serviceProvider));

            return fixture;
        })
    {
    }
}
