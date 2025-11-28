using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarItemViewModel : ObservableObject
{
    private readonly Calendar _calendar;
    private readonly SundyDbContext _db;
    private readonly Func<CalendarItemViewModel, Task>? _onDeleteRequested;
    private CancellationTokenSource? _saveCts;

    public CalendarItemViewModel(Calendar calendar, SundyDbContext db, Func<CalendarItemViewModel, Task>? onDeleteRequested = null)
    {
        _calendar = calendar;
        _db = db;
        _onDeleteRequested = onDeleteRequested;
    }

    public string Id => _calendar.Id;
    public string Name => _calendar.Name;
    public string Color => _calendar.Color;
    
    public bool EnableBlocking
    {
        get => _calendar.EnableBlocking;
        set
        {
            if (_calendar.EnableBlocking != value)
            {
                _calendar.EnableBlocking = value;
                OnPropertyChanged();
                DebouncedSave();
            }
        }
    }
    
    public bool ReceiveBlocks
    {
        get => _calendar.ReceiveBlocks;
        set
        {
            if (_calendar.ReceiveBlocks != value)
            {
                _calendar.ReceiveBlocks = value;
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
                    await _db.SaveChangesAsync(token);
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
        if (_onDeleteRequested != null)
        {
            await _onDeleteRequested(this);
        }
    }
}

