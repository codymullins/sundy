import SwiftUI

struct MonthView: View {
    var currentDate: Date
    var calendars: [SundyCalendar]
    var events: [CalendarEvent]
    var hideEventTitles: Bool
    var collapsePastEvents: Bool
    var onDayClick: (Date) -> Void
    var onEventClick: (CalendarEvent) -> Void

    @State private var expandedPastDays: Set<String> = []

    private let dayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]

    var body: some View {
        let calendarLookup = Dictionary(uniqueKeysWithValues: calendars.map { ($0.id, $0) })
        VStack(spacing: 0) {
            HStack(spacing: 0) {
                ForEach(dayNames, id: \.self) { day in
                    VStack {
                        Text(String(day.prefix(1)))
                            .font(ThemeTypography.body(13))
                            .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    }
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 12)
                }
            }
            .background(ThemeColors.surface.swiftUIColor)
            .overlay(
                Rectangle()
                    .fill(ThemeColors.border.swiftUIColor)
                    .frame(height: 1),
                alignment: .bottom
            )

            VStack(spacing: 0) {
                ForEach(DateUtils.monthWeeks(for: currentDate), id: \ .first!) { week in
                    HStack(spacing: 0) {
                        ForEach(week, id: \.self) { day in
                            MonthCellView(
                                day: day,
                                currentDate: currentDate,
                                events: eventsForDay(day),
                                calendarLookup: calendarLookup,
                                isToday: Calendar.current.isDateInToday(day),
                                hideEventTitles: hideEventTitles,
                                collapsePastEvents: collapsePastEvents,
                                isExpanded: expandedPastDays.contains(dayKey(day)),
                                onToggleExpanded: { toggleExpanded(day) },
                                onDayClick: { onDayClick(day) },
                                onEventClick: onEventClick
                            )
                        }
                    }
                    .frame(maxHeight: .infinity)
                }
            }
        }
        .background(ThemeColors.background.swiftUIColor)
    }

    private func eventsForDay(_ day: Date) -> [CalendarEvent] {
        let visibleIds = Set(calendars.filter { $0.isVisible }.map { $0.id })
        return events
            .filter { event in
                Calendar.current.isDate(event.startTime, inSameDayAs: day) && visibleIds.contains(event.calendarId)
            }
            .sorted(by: { $0.startTime < $1.startTime })
    }

    private func dayKey(_ day: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM-dd"
        return formatter.string(from: day)
    }

    private func toggleExpanded(_ day: Date) {
        let key = dayKey(day)
        if expandedPastDays.contains(key) {
            expandedPastDays.remove(key)
        } else {
            expandedPastDays.insert(key)
        }
    }
}
