using CommunityToolkit.Mvvm.ComponentModel;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarItemViewModel(Calendar calendar, SundyDbContext db) : ObservableObject
{

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
                db.SaveChangesAsync();
                OnPropertyChanged();
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
                db.SaveChangesAsync();
                OnPropertyChanged();
            }
        }
    }
}