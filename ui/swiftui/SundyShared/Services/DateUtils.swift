import Foundation

enum DateUtils {
    static func weekStart(for date: Date) -> Date {
        let calendar = Calendar.current
        let weekday = calendar.component(.weekday, from: date)
        let daysFromStart = weekday - calendar.firstWeekday
        return calendar.date(byAdding: .day, value: -daysFromStart, to: date.startOfDay) ?? date
    }

    static func weekDays(for date: Date) -> [Date] {
        let start = weekStart(for: date)
        return (0..<7).compactMap { Calendar.current.date(byAdding: .day, value: $0, to: start) }
    }

    static func monthWeeks(for date: Date) -> [[Date]] {
        let calendar = Calendar.current
        let firstOfMonth = calendar.date(from: calendar.dateComponents([.year, .month], from: date)) ?? date
        let start = weekStart(for: firstOfMonth)
        var weeks: [[Date]] = []
        var current = start
        for _ in 0..<6 {
            let week = (0..<7).compactMap { dayOffset in
                calendar.date(byAdding: .day, value: dayOffset, to: current)
            }
            weeks.append(week)
            current = calendar.date(byAdding: .day, value: 7, to: current) ?? current
        }
        return weeks
    }

    static func headerTitle(for date: Date, view: CalendarViewType) -> String {
        let formatter = DateFormatter()
        formatter.locale = Locale.current
        switch view {
        case .day:
            formatter.dateFormat = "MMMM d, yyyy"
            return formatter.string(from: date)
        case .week:
            return weekRangeTitle(for: date)
        case .month, .dynamic:
            formatter.dateFormat = "MMMM yyyy"
            return formatter.string(from: date)
        }
    }

    static func weekRangeTitle(for date: Date) -> String {
        let days = weekDays(for: date)
        guard let start = days.first, let end = days.last else { return "" }
        let formatter = DateFormatter()
        formatter.locale = Locale.current

        if Calendar.current.isDate(start, equalTo: end, toGranularity: .month) {
            formatter.dateFormat = "MMM d"
            let startText = formatter.string(from: start)
            formatter.dateFormat = "d, yyyy"
            let endText = formatter.string(from: end)
            return "\(startText) - \(endText)"
        } else if Calendar.current.isDate(start, equalTo: end, toGranularity: .year) {
            formatter.dateFormat = "MMM d"
            let startText = formatter.string(from: start)
            formatter.dateFormat = "MMM d, yyyy"
            let endText = formatter.string(from: end)
            return "\(startText) - \(endText)"
        } else {
            formatter.dateFormat = "MMM d, yyyy"
            let startText = formatter.string(from: start)
            let endText = formatter.string(from: end)
            return "\(startText) - \(endText)"
        }
    }

    static func formatTime(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        formatter.dateStyle = .none
        return formatter.string(from: date)
    }

    static func formatHourLabel(_ hour: Int) -> String {
        if hour == 0 { return "12 AM" }
        if hour == 12 { return "12 PM" }
        if hour < 12 { return "\(hour) AM" }
        return "\(hour - 12) PM"
    }
}
