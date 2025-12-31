using System.Net.Http.Json;
using System.Text.Json;

namespace Sundy.Core.Sync;

/// <summary>
/// Client-side sync service implementation.
/// Handles device registration, operation upload/download, and conflict resolution.
/// </summary>
public class SyncService : ISyncService
{
    private readonly HttpClient _httpClient;
    private readonly ISyncMetadataStore _metadataStore;
    private readonly IUploadQueueStore _uploadQueueStore;
    private readonly ICalendarStore _calendarStore;
    private readonly IEventStore _eventStore;

    private string? _deviceId;
    private string? _deviceToken;
    private string? _serverUrl;
    private bool _isSyncEnabled;
    private bool _isOnline;
    private int _pendingUploadCount;
    private long _lastServerVersion;
    private DateTimeOffset? _lastSyncAt;

    public string? DeviceId => _deviceId;
    public bool IsSyncEnabled => _isSyncEnabled;
    public bool IsOnline => _isOnline;
    public int PendingUploadCount => _pendingUploadCount;
    public long LastServerVersion => _lastServerVersion;
    public DateTimeOffset? LastSyncAt => _lastSyncAt;

    public event EventHandler<SyncStatusChangedEventArgs>? StatusChanged;

    public SyncService(
        HttpClient httpClient,
        ISyncMetadataStore metadataStore,
        IUploadQueueStore uploadQueueStore,
        ICalendarStore calendarStore,
        IEventStore eventStore)
    {
        _httpClient = httpClient;
        _metadataStore = metadataStore;
        _uploadQueueStore = uploadQueueStore;
        _calendarStore = calendarStore;
        _eventStore = eventStore;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        _deviceId = await _metadataStore.GetDeviceIdAsync(ct).ConfigureAwait(false);
        _deviceToken = await _metadataStore.GetDeviceTokenAsync(ct).ConfigureAwait(false);
        _serverUrl = await _metadataStore.GetServerUrlAsync(ct).ConfigureAwait(false);
        _isSyncEnabled = await _metadataStore.IsSyncEnabledAsync(ct).ConfigureAwait(false);
        _lastServerVersion = await _metadataStore.GetLastServerVersionAsync(ct).ConfigureAwait(false);
        _lastSyncAt = await _metadataStore.GetLastSyncAtAsync(ct).ConfigureAwait(false);
        _pendingUploadCount = await _uploadQueueStore.GetPendingCountAsync(ct).ConfigureAwait(false);

        // Check if server is reachable
        if (_isSyncEnabled && !string.IsNullOrEmpty(_serverUrl))
        {
            _isOnline = await CheckServerStatusAsync(ct).ConfigureAwait(false);
        }
    }

    public async Task<bool> RegisterDeviceAsync(string serverUrl, string? deviceName = null, CancellationToken ct = default)
    {
        try
        {
            _serverUrl = serverUrl.TrimEnd('/');
            _httpClient.BaseAddress = new Uri(_serverUrl);

            var request = new { DeviceName = deviceName, Platform = "blazor-wasm" };
            var response = await _httpClient.PostAsJsonAsync("/api/sync/register", request, ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct).ConfigureAwait(false);
            if (result == null)
                return false;

            _deviceId = result.DeviceId;
            _deviceToken = result.Token;

            // Save to metadata store
            await _metadataStore.SetServerUrlAsync(_serverUrl, ct).ConfigureAwait(false);
            await _metadataStore.SetDeviceIdAsync(_deviceId, ct).ConfigureAwait(false);
            await _metadataStore.SetDeviceTokenAsync(_deviceToken, ct).ConfigureAwait(false);
            await _metadataStore.SetSyncEnabledAsync(true, ct).ConfigureAwait(false);

            _isSyncEnabled = true;
            _isOnline = true;

            RaiseStatusChanged("Device registered successfully");
            return true;
        }
        catch (Exception ex)
        {
            RaiseStatusChanged($"Registration failed: {ex.Message}");
            return false;
        }
    }

    public async Task<SyncResult> SyncAsync(CancellationToken ct = default)
    {
        if (!_isSyncEnabled || string.IsNullOrEmpty(_serverUrl) || string.IsNullOrEmpty(_deviceToken))
        {
            return new SyncResult(false, 0, 0, _lastServerVersion, "Sync not configured");
        }

        try
        {
            RaiseStatusChanged("Syncing...", isSyncing: true);

            // 1. Download operations from server
            var downloaded = await DownloadOperationsAsync(ct).ConfigureAwait(false);

            // 2. Upload pending operations to server
            var uploaded = await UploadOperationsAsync(ct).ConfigureAwait(false);

            // 3. Update last sync timestamp
            _lastSyncAt = DateTimeOffset.UtcNow;
            await _metadataStore.SetLastSyncAtAsync(_lastSyncAt.Value, ct).ConfigureAwait(false);

            _isOnline = true;
            _pendingUploadCount = await _uploadQueueStore.GetPendingCountAsync(ct).ConfigureAwait(false);

            RaiseStatusChanged($"Sync complete: {downloaded} downloaded, {uploaded} uploaded");

            return new SyncResult(true, downloaded, uploaded, _lastServerVersion, null);
        }
        catch (HttpRequestException ex)
        {
            _isOnline = false;
            RaiseStatusChanged($"Sync failed: {ex.Message}");
            return new SyncResult(false, 0, 0, _lastServerVersion, ex.Message);
        }
        catch (Exception ex)
        {
            RaiseStatusChanged($"Sync error: {ex.Message}");
            return new SyncResult(false, 0, 0, _lastServerVersion, ex.Message);
        }
    }

    public async Task DisableSyncAsync(CancellationToken ct = default)
    {
        await _metadataStore.ClearAllAsync(ct).ConfigureAwait(false);
        await _uploadQueueStore.ClearAsync(ct).ConfigureAwait(false);

        _deviceId = null;
        _deviceToken = null;
        _serverUrl = null;
        _isSyncEnabled = false;
        _isOnline = false;
        _pendingUploadCount = 0;
        _lastServerVersion = 0;
        _lastSyncAt = null;

        RaiseStatusChanged("Sync disabled");
    }

    private async Task<int> DownloadOperationsAsync(CancellationToken ct)
    {
        var totalDownloaded = 0;
        var hasMore = true;

        while (hasMore)
        {
            var url = $"/api/sync/download?sinceVersion={_lastServerVersion}&limit=100";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _deviceToken);

            var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var downloadResponse = await response.Content.ReadFromJsonAsync<DownloadResponse>(cancellationToken: ct).ConfigureAwait(false);
            if (downloadResponse == null)
                break;

            foreach (var op in downloadResponse.Operations)
            {
                await ApplyOperationLocallyAsync(op, ct).ConfigureAwait(false);
                totalDownloaded++;
            }

            _lastServerVersion = downloadResponse.ServerVersion;
            await _metadataStore.SetLastServerVersionAsync(_lastServerVersion, ct).ConfigureAwait(false);

            hasMore = downloadResponse.HasMore;
        }

        return totalDownloaded;
    }

    private async Task<int> UploadOperationsAsync(CancellationToken ct)
    {
        var pending = await _uploadQueueStore.GetPendingAsync(100, ct).ConfigureAwait(false);
        if (pending.Count == 0)
            return 0;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/sync/upload");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _deviceToken);
        request.Content = JsonContent.Create(new { Operations = pending });

        var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // Remove uploaded operations from queue
        foreach (var op in pending)
        {
            await _uploadQueueStore.RemoveAsync(op.Id, ct).ConfigureAwait(false);
        }

        return pending.Count;
    }

    private async Task ApplyOperationLocallyAsync(Operation op, CancellationToken ct)
    {
        switch (op.EntityType)
        {
            case EntityType.Calendar:
                await ApplyCalendarOperationAsync(op, ct).ConfigureAwait(false);
                break;
            case EntityType.Event:
                await ApplyEventOperationAsync(op, ct).ConfigureAwait(false);
                break;
            // Settings would be handled similarly
        }
    }

    private async Task ApplyCalendarOperationAsync(Operation op, CancellationToken ct)
    {
        if (op.OpType == OperationType.Delete)
        {
            await _calendarStore.DeleteCalendarAsync(op.EntityId, ct).ConfigureAwait(false);
        }
        else if (op.Payload != null)
        {
            var calendar = JsonSerializer.Deserialize<Calendar>(op.Payload);
            if (calendar != null)
            {
                calendar.Version = op.ServerVersion ?? 0;
                calendar.UpdatedAt = op.Timestamp;

                // Check if exists for upsert logic
                var existing = await _calendarStore.GetCalendarLookupAsync(ct).ConfigureAwait(false);
                if (existing.ContainsKey(calendar.Id))
                {
                    // Update existing - for now we use the simple approach
                    await _calendarStore.DeleteCalendarAsync(calendar.Id, ct).ConfigureAwait(false);
                }
                await _calendarStore.AddAsync(calendar, ct).ConfigureAwait(false);
            }
        }
    }

    private async Task ApplyEventOperationAsync(Operation op, CancellationToken ct)
    {
        if (op.OpType == OperationType.Delete)
        {
            await _eventStore.DeleteEventAsync(op.EntityId, ct).ConfigureAwait(false);
        }
        else if (op.Payload != null)
        {
            var evt = JsonSerializer.Deserialize<CalendarEvent>(op.Payload);
            if (evt != null)
            {
                evt.Version = op.ServerVersion ?? 0;
                evt.UpdatedAt = op.Timestamp;

                var existing = await _eventStore.GetEventByIdAsync(op.EntityId, ct).ConfigureAwait(false);
                if (existing != null)
                {
                    await _eventStore.UpdateEventAsync(evt, ct).ConfigureAwait(false);
                }
                else
                {
                    await _eventStore.CreateEventAsync(evt, ct).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task<bool> CheckServerStatusAsync(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(_serverUrl))
                return false;

            _httpClient.BaseAddress = new Uri(_serverUrl);
            var response = await _httpClient.GetAsync("/api/sync/status", ct).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private void RaiseStatusChanged(string? message = null, bool isSyncing = false)
    {
        StatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
        {
            IsOnline = _isOnline,
            IsSyncing = isSyncing,
            PendingUploadCount = _pendingUploadCount,
            Message = message
        });
    }

    // Response DTOs
    private record RegisterResponse(string DeviceId, string Token);
    private record DownloadResponse(List<Operation> Operations, long ServerVersion, bool HasMore);
}
