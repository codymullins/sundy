using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;

namespace Sundy.Test;

public class MetaTests
{
    [Theory]
    [Auto]
    public async Task MetaStore_CanInitialize(IMediator mediator)
    {
        await DatabaseManager.CreateDatabaseFileAsync("Data Source=:memory:");
        await mediator.Send(new InitializeDatabaseCommand());
        await mediator.Send(new CreateCalendarCommand(new Calendar
        {
            Id = "cal1", Name = "Test Calendar", Color = "#FF0000", Type = CalendarType.Local, EnableBlocking = true,
            ReceiveBlocks = true
        }));

        var calendars = await mediator.Send(new GetAllCalendarsQuery());
        Assert.Single(calendars);
    }

    [Theory]
    [Auto]
    public void MetaStore_CanReset(IMediator mediator)
    {
    }
}

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
            var connectionString = "Data Source=:memory:";
            services.AddSingleton(_ => new EventStore(connectionString));
            services.AddSingleton(_ => new DatabaseManager(connectionString));
            services.AddSingleton(_ => new CalendarStore(connectionString));

            // Register Services
            services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();

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

public class ServiceProviderRelay : ISpecimenBuilder
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderRelay(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type)
        {
            var service = _serviceProvider.GetService(type);
            if (service != null)
                return service;
        }

        return new NoSpecimen();
    }
}