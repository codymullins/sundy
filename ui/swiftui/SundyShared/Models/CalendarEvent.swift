import Foundation

struct CalendarEvent: Identifiable, Codable, Equatable {
    var id: UUID
    var calendarId: UUID
    var title: String
    var startTime: Date
    var endTime: Date
    var details: String
    var isAllDay: Bool

    var duration: TimeInterval {
        endTime.timeIntervalSince(startTime)
    }
}
