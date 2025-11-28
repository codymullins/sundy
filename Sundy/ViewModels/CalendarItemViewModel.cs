using System;
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
                _db.SaveChangesAsync();
                OnPropertyChanged();
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
                _db.SaveChangesAsync();
                OnPropertyChanged();
            }
        }
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

