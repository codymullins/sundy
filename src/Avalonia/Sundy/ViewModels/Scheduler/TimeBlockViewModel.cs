using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sundy.ViewModels.Scheduler;

public partial class TimeBlockViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(TimeRangeText))]
    private TimeOnly _startTime = new(9, 0);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(TimeRangeText))]
    private TimeOnly _endTime = new(10, 0);

    [ObservableProperty]
    private TimeSpan _snapInterval = TimeSpan.FromMinutes(15);

    [ObservableProperty]
    private TimeOnly _minimumTime = new(0, 0);

    [ObservableProperty]
    private TimeOnly _maximumTime = new(23, 59);

    [ObservableProperty]
    private TimeSpan _minimumDuration = TimeSpan.FromMinutes(15);

    public TimeSpan Duration => EndTime - StartTime;

    public string DurationText
    {
        get
        {
            var duration = Duration;
            if (duration.TotalHours >= 1)
            {
                var hours = (int)duration.TotalHours;
                var minutes = duration.Minutes;
                return minutes > 0 ? $"{hours} hr {minutes} min" : $"{hours} hr";
            }
            return $"{(int)duration.TotalMinutes} min";
        }
    }

    public string TimeRangeText => $"{StartTime:h:mm tt} → {EndTime:h:mm tt}";

    public void MoveToTime(TimeOnly newStartTime)
    {
        var duration = Duration;
        var snappedStart = SnapTime(newStartTime);
        var newEnd = snappedStart.Add(duration);

        // Clamp to bounds
        if (snappedStart < MinimumTime)
        {
            snappedStart = MinimumTime;
            newEnd = snappedStart.Add(duration);
        }

        if (newEnd > MaximumTime)
        {
            newEnd = MaximumTime;
            snappedStart = newEnd.Add(-duration);
        }

        StartTime = snappedStart;
        EndTime = newEnd;
    }

    public void SetStartTime(TimeOnly newStartTime)
    {
        var snappedStart = SnapTime(newStartTime);
        var minStart = EndTime.Add(-MaximumTime.ToTimeSpan()).Add(MinimumTime.ToTimeSpan());
        
        // Ensure minimum duration
        var maxStart = EndTime.Add(-MinimumDuration);
        if (snappedStart > maxStart) snappedStart = maxStart;
        if (snappedStart < MinimumTime) snappedStart = MinimumTime;

        StartTime = snappedStart;
    }

    public void SetEndTime(TimeOnly newEndTime)
    {
        var snappedEnd = SnapTime(newEndTime);
        
        // Ensure minimum duration
        var minEnd = StartTime.Add(MinimumDuration);
        if (snappedEnd < minEnd) snappedEnd = minEnd;
        if (snappedEnd > MaximumTime) snappedEnd = MaximumTime;

        EndTime = snappedEnd;
    }

    private TimeOnly SnapTime(TimeOnly time)
    {
        var totalMinutes = time.Hour * 60 + time.Minute;
        var intervalMinutes = (int)SnapInterval.TotalMinutes;
        var snappedMinutes = (int)Math.Round((double)totalMinutes / intervalMinutes) * intervalMinutes;
        snappedMinutes = Math.Clamp(snappedMinutes, 0, 24 * 60 - 1);
        return new TimeOnly(snappedMinutes / 60, snappedMinutes % 60);
    }
}
