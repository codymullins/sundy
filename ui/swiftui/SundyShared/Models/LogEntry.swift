import Foundation

enum LogLevel: String, Codable {
    case info
    case success
    case warning
    case error
}

struct LogEntry: Identifiable, Codable, Equatable {
    var id: UUID
    var timestamp: Date
    var calendarName: String?
    var message: String
    var level: LogLevel
}
