using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarItemViewModel(
    Calendar calendar,
    SundyDbContext db,
    Func<CalendarItemViewModel, Task>? onDeleteRequested = null)
    : ObservableObject
{
    private CancellationTokenSource? _saveCts;

    public string Id => calendar.Id;
    public string Name => calendar.Name;
    public string Color => calendar.Color;
    
    public bool EnableBlocking
    {
        get => calendar.EnableBlocking;
        set
        {
            if (calendar.EnableBlocking != value)
            {
                calendar.EnableBlocking = value;
                OnPropertyChanged();
                DebouncedSave();
            }
        }
    }
    
    public bool ReceiveBlocks
    {
        get => calendar.ReceiveBlocks;
        set
        {
            if (calendar.ReceiveBlocks != value)
            {
                calendar.ReceiveBlocks = value;
                OnPropertyChanged();
                DebouncedSave();
            }
        }
    }

    private void DebouncedSave()
    {
        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var token = _saveCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    await db.SaveChangesAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving calendar changes: {ex.Message}");
            }
        }, token);
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (onDeleteRequested != null)
        {
            await onDeleteRequested(this);
        }
    }
}

