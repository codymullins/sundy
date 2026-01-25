import Foundation

enum CalendarViewType: String, CaseIterable, Identifiable {
    case day
    case week
    case month
    case dynamic

    var id: String { rawValue }

    var displayName: String {
        switch self {
        case .day: return "Day"
        case .week: return "Week"
        case .month: return "Month"
        case .dynamic: return "Dynamic"
        }
    }
}
