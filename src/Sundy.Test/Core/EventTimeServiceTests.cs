using Sundy.Core.System;

namespace Sundy.Test.Core;

public class EventTimeServiceTests
{
    [Fact]
    public void CreateEventTime_WithSystemTimeZone_PreservesLocalTime()
    {
        // Arrange
        var provider = new SystemTimeZoneProvider();
        var service = new EventTimeService(provider);
        var localDate = new DateTime(2024, 6, 15);
        var localTime = new TimeSpan(21, 0, 0); // 9pm

        // Act
        var result = service.CreateEventTime(localDate, localTime);

        // Assert
        Assert.Equal(21, result.Hour);
        Assert.Equal(0, result.Minute);
        Assert.Equal(15, result.Day);
        Assert.Equal(6, result.Month);
    }

    [Fact]
    public void CreateEventTime_WithFixedTimeZone_AppliesCorrectOffset()
    {
        // Arrange - Use Eastern Time (UTC-5 or UTC-4 depending on DST)
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var provider = new FixedTimeZoneProvider(eastern);
        var service = new EventTimeService(provider);
        
        // June 15 is in DST, so Eastern is UTC-4
        var localDate = new DateTime(2024, 6, 15);
        var localTime = new TimeSpan(21, 0, 0); // 9pm

        // Act
        var result = service.CreateEventTime(localDate, localTime);

        // Assert - offset should be -4 hours (DST)
        Assert.Equal(TimeSpan.FromHours(-4), result.Offset);
        Assert.Equal(21, result.Hour); // Local hour preserved
    }

    [Fact]
    public void GetLocalDateTime_ConvertsUtcToLocal()
    {
        // Arrange - Use Eastern Time
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var provider = new FixedTimeZoneProvider(eastern);
        var service = new EventTimeService(provider);
        
        // UTC time: June 16, 2024 at 01:00 UTC = June 15, 2024 at 9pm Eastern (DST)
        var utcTime = new DateTimeOffset(2024, 6, 16, 1, 0, 0, TimeSpan.Zero);

        // Act
        var (date, time) = service.GetLocalDateTime(utcTime);

        // Assert
        Assert.Equal(new DateTime(2024, 6, 15), date);
        Assert.Equal(new TimeSpan(21, 0, 0), time); // 9pm local
    }

    [Fact]
    public void RoundTrip_PreservesLocalTime()
    {
        // Arrange
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var provider = new FixedTimeZoneProvider(eastern);
        var service = new EventTimeService(provider);
        
        var originalDate = new DateTime(2024, 6, 15);
        var originalTime = new TimeSpan(21, 0, 0); // 9pm Friday

        // Act
        var stored = service.CreateEventTime(originalDate, originalTime);
        var (retrievedDate, retrievedTime) = service.GetLocalDateTime(stored);

        // Assert
        Assert.Equal(originalDate, retrievedDate);
        Assert.Equal(originalTime, retrievedTime);
    }

    [Fact]
    public void RoundTrip_DifferentTimeZones_PreservesInstant()
    {
        // Arrange - Create in Eastern, read in Pacific
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var pacific = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        
        var easternService = new EventTimeService(new FixedTimeZoneProvider(eastern));
        var pacificService = new EventTimeService(new FixedTimeZoneProvider(pacific));
        
        // 9pm Eastern on June 15
        var easternDate = new DateTime(2024, 6, 15);
        var easternTime = new TimeSpan(21, 0, 0);

        // Act
        var stored = easternService.CreateEventTime(easternDate, easternTime);
        var (pacificDate, pacificTime) = pacificService.GetLocalDateTime(stored);

        // Assert - 9pm Eastern = 6pm Pacific (3 hour difference)
        Assert.Equal(new DateTime(2024, 6, 15), pacificDate);
        Assert.Equal(new TimeSpan(18, 0, 0), pacificTime); // 6pm Pacific
    }

    [Fact]
    public void GetLocalDate_ReturnsDateOnly()
    {
        // Arrange
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var provider = new FixedTimeZoneProvider(eastern);
        var service = new EventTimeService(provider);
        
        // 2am UTC on June 16 = 10pm Eastern on June 15 (DST)
        var utcTime = new DateTimeOffset(2024, 6, 16, 2, 0, 0, TimeSpan.Zero);

        // Act
        var date = service.GetLocalDate(utcTime);

        // Assert
        Assert.Equal(new DateTime(2024, 6, 15), date);
    }

    [Fact]
    public void GetLocalTime_ReturnsTimeOnly()
    {
        // Arrange
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var provider = new FixedTimeZoneProvider(eastern);
        var service = new EventTimeService(provider);
        
        // 2am UTC on June 16 = 10pm Eastern on June 15 (DST)
        var utcTime = new DateTimeOffset(2024, 6, 16, 2, 0, 0, TimeSpan.Zero);

        // Act
        var time = service.GetLocalTime(utcTime);

        // Assert
        Assert.Equal(new TimeSpan(22, 0, 0), time); // 10pm
    }
}
