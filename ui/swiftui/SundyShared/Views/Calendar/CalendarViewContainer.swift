import SwiftUI

struct CalendarViewContainer: View {
    @Binding var currentDate: Date
    var currentView: CalendarViewType
    var calendars: [SundyCalendar]
    var events: [CalendarEvent]
    var hideEventTitles: Bool
    var collapsePastEvents: Bool
    var onDayClick: (Date) -> Void
    var onEventClick: (CalendarEvent) -> Void

    var body: some View {
        switch currentView {
        case .month:
            VerticalMonthPager(currentDate: $currentDate) { date in
                MonthView(
                    currentDate: date,
                    calendars: calendars,
                    events: events,
                    hideEventTitles: hideEventTitles,
                    collapsePastEvents: collapsePastEvents,
                    onDayClick: onDayClick,
                    onEventClick: onEventClick
                )
            }
        case .week:
            HorizontalPager(currentDate: $currentDate, dateIncrement: .week) { date in
                WeekView(
                    currentDate: date,
                    calendars: calendars,
                    events: events,
                    hideEventTitles: hideEventTitles,
                    onDayClick: onDayClick,
                    onEventClick: onEventClick
                )
            }
        case .day:
            HorizontalPager(currentDate: $currentDate, dateIncrement: .day) { date in
                DayView(
                    currentDate: date,
                    calendars: calendars,
                    events: events,
                    hideEventTitles: hideEventTitles,
                    onDayClick: onDayClick,
                    onEventClick: onEventClick
                )
            }
        case .dynamic:
            VerticalMonthPager(currentDate: $currentDate) { date in
                DynamicMonthView(
                    currentDate: date,
                    calendars: calendars,
                    events: events,
                    hideEventTitles: hideEventTitles,
                    collapsePastEvents: collapsePastEvents,
                    onDayClick: onDayClick,
                    onEventClick: onEventClick
                )
            }
        }
    }
}
