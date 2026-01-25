import Foundation
import UserNotifications

@MainActor
final class SundyStore: ObservableObject {
    @Published private(set) var calendars: [SundyCalendar]
    @Published private(set) var events: [CalendarEvent]
    @Published var settings: Settings
    @Published private(set) var logs: [LogEntry]
    @Published private(set) var backups: [BackupInfo]
    @Published private(set) var toasts: [ToastItem]

    private let dataStore: SundyDataStore
    private let backupService: BackupService
    private let notificationService: NotificationService

    init(
        dataStore: SundyDataStore = SundyDataStore(),
        backupService: BackupService = BackupService(),
        notificationService: NotificationService = NotificationService()
    ) {
        self.dataStore = dataStore
        self.backupService = backupService
        self.notificationService = notificationService
        let data = dataStore.load()
        calendars = data.calendars
        events = data.events
        settings = data.settings
        logs = data.logs
        backups = data.backups
        toasts = []
    }

    func persist() {
        let data = SundyData(
            calendars: calendars,
            events: events,
            settings: settings,
            logs: logs,
            backups: backups
        )
        dataStore.save(data)
    }

    func addCalendar(name: String, color: ColorValue) {
        let calendar = SundyCalendar(
            id: UUID(),
            name: name,
            displayName: nil,
            color: color,
            isVisible: true,
            isHidden: false,
            accountId: nil,
            accountEmail: nil
        )
        calendars.append(calendar)
        addLog(message: "Calendar created: \(calendar.name)", level: .success)
        persist()
    }

    func renameCalendar(id: UUID, newName: String) {
        guard let index = calendars.firstIndex(where: { $0.id == id }) else { return }
        var calendar = calendars[index]
        calendar.displayName = newName
        calendars[index] = calendar
        addLog(message: "Calendar renamed to \(newName)", level: .info)
        persist()
    }

    func deleteCalendar(id: UUID) {
        calendars.removeAll { $0.id == id }
        events.removeAll { $0.calendarId == id }
        addLog(message: "Calendar deleted", level: .warning)
        persist()
    }

    func toggleCalendarVisibility(id: UUID) {
        guard let index = calendars.firstIndex(where: { $0.id == id }) else { return }
        calendars[index].isVisible.toggle()
        persist()
    }

    func setCalendarHidden(id: UUID, hidden: Bool) {
        guard let index = calendars.firstIndex(where: { $0.id == id }) else { return }
        calendars[index].isHidden = hidden
        persist()
    }

    func createEvent(from draft: EventDraft) {
        let event = CalendarEvent(
            id: UUID(),
            calendarId: draft.calendarId,
            title: draft.title.trimmingCharacters(in: .whitespacesAndNewlines),
            startTime: draft.startDateTime,
            endTime: draft.endDateTime,
            details: draft.details,
            isAllDay: draft.isAllDay
        )
        events.append(event)
        addLog(message: "Event created: \(event.title)", level: .success)
        scheduleNotificationsIfNeeded(for: event)
        persist()
    }

    func updateEvent(id: UUID, from draft: EventDraft) {
        guard let index = events.firstIndex(where: { $0.id == id }) else { return }
        let updated = CalendarEvent(
            id: id,
            calendarId: draft.calendarId,
            title: draft.title.trimmingCharacters(in: .whitespacesAndNewlines),
            startTime: draft.startDateTime,
            endTime: draft.endDateTime,
            details: draft.details,
            isAllDay: draft.isAllDay
        )
        events[index] = updated
        addLog(message: "Event updated: \(updated.title)", level: .info)
        notificationService.removePendingNotification(for: id)
        scheduleNotificationsIfNeeded(for: updated)
        persist()
    }

    func deleteEvent(id: UUID) {
        events.removeAll { $0.id == id }
        notificationService.removePendingNotification(for: id)
        addLog(message: "Event deleted", level: .warning)
        persist()
    }

    func refreshBackups() {
        backups = backupService.loadBackups()
        persist()
    }

    func createBackup() {
        let data = SundyData(
            calendars: calendars,
            events: events,
            settings: settings,
            logs: logs,
            backups: backups
        )
        if let info = backupService.createBackup(data: data) {
            backups.insert(info, at: 0)
            addLog(message: "Backup created", level: .success)
            persist()
        }
    }

    func deleteBackup(id: UUID) {
        guard let backup = backups.first(where: { $0.id == id }) else { return }
        backupService.deleteBackup(fileName: backup.fileName)
        backups.removeAll { $0.id == id }
        addLog(message: "Backup deleted", level: .warning)
        persist()
    }

    func resetData() {
        calendars = [SundyCalendar.defaultCalendar]
        events = []
        logs = []
        backups = []
        settings = .default
        persist()
    }

    func clearLogs() {
        logs.removeAll()
        persist()
    }

    func updateSettings(_ update: (inout Settings) -> Void) {
        update(&settings)
        persist()
    }

    func addLog(message: String, level: LogLevel, calendarName: String? = nil) {
        let entry = LogEntry(
            id: UUID(),
            timestamp: Date(),
            calendarName: calendarName,
            message: message,
            level: level
        )
        logs.insert(entry, at: 0)
        if logs.count > 200 {
            logs.removeLast()
        }
        persist()
    }

    func showToast(message: String, type: ToastType) {
        let toast = ToastItem(id: UUID(), message: message, type: type, createdAt: Date())
        toasts.append(toast)
    }

    func dismissToast(id: UUID) {
        toasts.removeAll { $0.id == id }
    }

    func notificationAuthorizationStatus() async -> UNAuthorizationStatus {
        await notificationService.authorizationStatus()
    }

    func requestNotificationPermission() async -> Bool {
        await notificationService.requestPermission()
    }

    func sendTestNotification() {
        notificationService.sendTestNotification()
    }

    private func scheduleNotificationsIfNeeded(for event: CalendarEvent) {
        guard settings.remindersEnabled else { return }
        let minutesBefore = settings.defaultReminderMinutes
        notificationService.scheduleReminder(for: event, minutesBefore: minutesBefore)
    }
}

struct EventDraft: Equatable {
    var title: String = ""
    var date: Date = Date()
    var startTime: Date = Date()
    var endTime: Date = Date().addingTimeInterval(3600)
    var details: String = ""
    var isAllDay: Bool = false
    var calendarId: UUID = SundyCalendar.defaultCalendar.id

    var startDateTime: Date {
        isAllDay ? date.startOfDay : Date.combine(date: date, time: startTime)
    }

    var endDateTime: Date {
        if isAllDay {
            return date.startOfDay.addingTimeInterval(24 * 3600)
        }
        var combined = Date.combine(date: date, time: endTime)
        if combined <= startDateTime {
            combined = combined.addingTimeInterval(24 * 3600)
        }
        return combined
    }
}

extension Date {
    static func combine(date: Date, time: Date) -> Date {
        let calendar = Calendar.current
        let dateComponents = calendar.dateComponents([.year, .month, .day], from: date)
        let timeComponents = calendar.dateComponents([.hour, .minute], from: time)
        var merged = DateComponents()
        merged.year = dateComponents.year
        merged.month = dateComponents.month
        merged.day = dateComponents.day
        merged.hour = timeComponents.hour
        merged.minute = timeComponents.minute
        return calendar.date(from: merged) ?? date
    }

    var startOfDay: Date {
        Calendar.current.startOfDay(for: self)
    }
}
