import Foundation
import UserNotifications

final class NotificationService {
    private let center = UNUserNotificationCenter.current()

    func requestPermission() async -> Bool {
        do {
            return try await center.requestAuthorization(options: [.alert, .badge, .sound])
        } catch {
            return false
        }
    }

    func authorizationStatus() async -> UNAuthorizationStatus {
        let settings = await center.notificationSettings()
        return settings.authorizationStatus
    }

    func scheduleReminder(for event: CalendarEvent, minutesBefore: Int) {
        let reminderDate = event.startTime.addingTimeInterval(TimeInterval(-minutesBefore * 60))
        guard reminderDate > Date() else { return }

        let content = UNMutableNotificationContent()
        content.title = event.title.isEmpty ? "Reminder" : event.title
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        formatter.dateStyle = .none
        content.body = "Starts at \(formatter.string(from: event.startTime))"
        content.sound = .default

        let triggerDate = Calendar.current.dateComponents(
            [.year, .month, .day, .hour, .minute],
            from: reminderDate
        )
        let trigger = UNCalendarNotificationTrigger(dateMatching: triggerDate, repeats: false)

        let request = UNNotificationRequest(
            identifier: event.id.uuidString,
            content: content,
            trigger: trigger
        )

        center.add(request)
    }

    func sendTestNotification() {
        let content = UNMutableNotificationContent()
        content.title = "Test Notification"
        content.body = "Notifications are working!"
        content.sound = .default

        let trigger = UNTimeIntervalNotificationTrigger(timeInterval: 1, repeats: false)
        let request = UNNotificationRequest(
            identifier: UUID().uuidString,
            content: content,
            trigger: trigger
        )
        center.add(request)
    }

    func removePendingNotification(for eventId: UUID) {
        center.removePendingNotificationRequests(withIdentifiers: [eventId.uuidString])
    }
}
