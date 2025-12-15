using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Serilog;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Services;

namespace Sundy.ViewModels;

public partial class CalendarSettingsViewModel : ObservableObject, IDisposable
{
    private readonly IMediator mediator;
    private readonly OutlookCalendarProvider outlookProvider;
    private readonly MicrosoftGraphAuthService authService;
    private readonly IClipboardService clipboardService;
    private readonly Func<Task>? onClosed;

    public CalendarSettingsViewModel(
        IMediator mediator,
        OutlookCalendarProvider outlookProvider,
        MicrosoftGraphAuthService authService,
        IClipboardService clipboardService,
        Func<Task>? onClosed = null)
    {
        this.mediator = mediator;
        this.outlookProvider = outlookProvider;
        this.authService = authService;
        this.clipboardService = clipboardService;
        this.onClosed = onClosed;

        // Subscribe to device code events
        authService.DeviceCodeReceived += OnDeviceCodeReceived;
    }
    [ObservableProperty] private ObservableCollection<CalendarItemViewModel> _calendars = [];

    [ObservableProperty] private string _selectedSection = "General";

    [ObservableProperty] private bool _isDeleteConfirmationOpen;

    [ObservableProperty] private CalendarItemViewModel? _calendarToDelete;

    [ObservableProperty] private string _newCalendarName = string.Empty;

    [ObservableProperty] private bool _isDeleting;

    [ObservableProperty] private bool _isResetConfirmationOpen;

    [ObservableProperty] private bool _isResettingDatabase;

    // Outlook integration state
    [ObservableProperty] private bool _isOutlookConnecting;

    [ObservableProperty] private bool _isOutlookConnected;

    [ObservableProperty] private string? _outlookUserName;

    [ObservableProperty] private string? _outlookConnectionError;

    // Device code flow properties
    [ObservableProperty] private string? _deviceCodeUrl;

    [ObservableProperty] private string? _deviceCodeUserCode;

    [ObservableProperty] private bool _isDeviceCodeFlowActive;

    public event EventHandler? OutlookConnected;

    [RelayCommand]
    private async Task CreateCalendar()
    {
        if (string.IsNullOrWhiteSpace(NewCalendarName))
            return;

        var calendar = new Calendar
        {
            Id = Guid.NewGuid().ToString(),
            Name = NewCalendarName.Trim(),
            Color = "#4A90E2", // Default blue
            Type = CalendarType.Local,
            EnableBlocking = true,
            ReceiveBlocks = true
        };

        await mediator.Send(new CreateCalendarCommand(calendar)).ConfigureAwait(false);

        Calendars.Add(new CalendarItemViewModel(calendar, RequestDeleteCalendar));
        NewCalendarName = string.Empty;
    }

    [RelayCommand]
    private Task RequestDeleteCalendar(CalendarItemViewModel calendar, CancellationToken ct)
    {
        CalendarToDelete = calendar;
        IsDeleteConfirmationOpen = true;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ConfirmDelete()
    {
        if (CalendarToDelete == null || IsDeleting)
            return;

        IsDeleting = true;
        try
        {
            var calendarId = CalendarToDelete.Id;

            await mediator.Send(new DeleteCalendarCommand(calendarId)).ConfigureAwait(false);

            // Remove from UI collection
            Calendars.Remove(CalendarToDelete);

            IsDeleteConfirmationOpen = false;
            CalendarToDelete = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting calendar {CalendarId}", CalendarToDelete?.Id);
        }
        finally
        {
            IsDeleting = false;
            IsDeleteConfirmationOpen = false;
            CalendarToDelete = null;
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
        CalendarToDelete = null;
    }

    [RelayCommand]
    private async Task Close()
    {
        if (onClosed != null)
        {
            await onClosed();
        }
    }

    [RelayCommand]
    private void ResetDatabase()
    {
        IsResetConfirmationOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmReset()
    {
        if (IsResettingDatabase)
            return;

        IsResettingDatabase = true;
        try
        {
            await mediator.Send(new ResetDatabaseCommand());
            Calendars.Clear();

            IsResetConfirmationOpen = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting database");
        }
        finally
        {
            IsResettingDatabase = false;
        }
    }

    [RelayCommand]
    private void CancelReset()
    {
        IsResetConfirmationOpen = false;
    }

    [RelayCommand]
    private void SetSection(string section)
    {
        SelectedSection = section;
    }

    [RelayCommand]
    private async Task ConnectOutlook()
    {
        if (IsOutlookConnecting)
            return;

        IsOutlookConnecting = true;
        OutlookConnectionError = null;

        // Reset device code state
        IsDeviceCodeFlowActive = false;
        DeviceCodeUrl = null;
        DeviceCodeUserCode = null;

        try
        {
            Log.Information("Starting Outlook connection...");
            var result = await mediator.Send(new ConnectOutlookCommand());

            if (result.Success)
            {
                IsOutlookConnected = true;
                OutlookUserName = result.UserDisplayName;
                Log.Information("Successfully connected to Outlook as {UserName}", result.UserDisplayName);

                // Reload calendars to show the new Outlook calendars
                await LoadCalendarsAsync();

                // Notify that Outlook was connected so the calendar view can refresh
                OutlookConnected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OutlookConnectionError = result.ErrorMessage ?? "Failed to connect to Outlook";
                Log.Warning("Failed to connect to Outlook: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            OutlookConnectionError = ex.Message;
            Log.Error(ex, "Error connecting to Outlook");
        }
        finally
        {
            IsOutlookConnecting = false;
            // Clear device code state after completion
            IsDeviceCodeFlowActive = false;
        }
    }

    [RelayCommand]
    private void DisconnectOutlook()
    {
        outlookProvider.Disconnect();
        IsOutlookConnected = false;
        OutlookUserName = null;
        OutlookConnectionError = null;
        Log.Information("Disconnected from Outlook");
    }

    private void OnDeviceCodeReceived(object? sender, DeviceCodeInfo e)
    {
        // Update UI on the UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            IsDeviceCodeFlowActive = true;
            DeviceCodeUrl = e.VerificationUrl;
            DeviceCodeUserCode = e.UserCode;

            Log.Information("Device code received: URL={Url}, Code={Code}",
                e.VerificationUrl, e.UserCode);
        });
    }

    [RelayCommand]
    private async Task CopyDeviceCodeUrl()
    {
        if (!string.IsNullOrEmpty(DeviceCodeUrl))
        {
            await clipboardService.SetTextAsync(DeviceCodeUrl);
            Log.Information("Copied device code URL to clipboard");
        }
    }

    [RelayCommand]
    private async Task CopyDeviceCode()
    {
        if (!string.IsNullOrEmpty(DeviceCodeUserCode))
        {
            await clipboardService.SetTextAsync(DeviceCodeUserCode);
            Log.Information("Copied device code to clipboard");
        }
    }

    public async Task LoadCalendarsAsync()
    {
        var cals = await mediator.Send(new GetAllCalendarsQuery());
        Calendars = new ObservableCollection<CalendarItemViewModel>(cals.Select(p =>
            new CalendarItemViewModel(p, RequestDeleteCalendar)));

        // Initialize Outlook connection status
        IsOutlookConnected = outlookProvider.IsConnected;
        OutlookUserName = outlookProvider.UserDisplayName;
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        authService.DeviceCodeReceived -= OnDeviceCodeReceived;
    }
}
