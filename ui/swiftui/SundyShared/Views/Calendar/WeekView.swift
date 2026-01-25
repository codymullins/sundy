import SwiftUI

struct WeekView: View {
    var currentDate: Date
    var calendars: [SundyCalendar]
    var events: [CalendarEvent]
    var hideEventTitles: Bool
    var onDayClick: (Date) -> Void
    var onEventClick: (CalendarEvent) -> Void

    private let pixelsPerHour: CGFloat = 48

    var body: some View {
        let weekDays = DateUtils.weekDays(for: currentDate)
        let calendarLookup = Dictionary(uniqueKeysWithValues: calendars.map { ($0.id, $0) })

        VStack(spacing: 0) {
            WeekHeaderView(weekDays: weekDays)

            ScrollView {
                HStack(spacing: 0) {
                    TimeGutterView()
                        .frame(width: 60)

                    ZStack(alignment: .topLeading) {
                        HStack(spacing: 0) {
                            ForEach(weekDays, id: \.self) { day in
                                WeekDayColumnView(
                                    day: day,
                                    events: eventsForDay(day),
                                    calendarLookup: calendarLookup,
                                    hideEventTitles: hideEventTitles,
                                    onDayClick: { onDayClick(day) },
                                    onEventClick: onEventClick
                                )
                            }
                        }

                        if weekDays.contains(where: { Calendar.current.isDateInToday($0) }) {
                            CurrentTimeIndicatorLine(pixelsPerHour: pixelsPerHour, showLabel: true, labelWidth: 56)
                                .padding(.leading, -60)
                        }
                    }
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
}

private struct WeekHeaderView: View {
    var weekDays: [Date]

    var body: some View {
        HStack(spacing: 0) {
            Rectangle()
                .fill(Color.clear)
                .frame(width: 60, height: 48)

            ForEach(weekDays, id: \.self) { day in
                VStack(spacing: 4) {
                    Text(dayAbbrev(day).uppercased())
                        .font(ThemeTypography.body(11))
                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    Text("\(Calendar.current.component(.day, from: day))")
                        .font(ThemeTypography.body(24))
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        .padding(6)
                        .background(
                            Calendar.current.isDateInToday(day) ? Circle().fill(ThemeColors.accent.swiftUIColor) : Circle().fill(Color.clear)
                        )
                }
                .frame(maxWidth: .infinity)
                .padding(.vertical, 8)
                .background(Calendar.current.isDateInToday(day) ? ThemeColors.accent.swiftUIColor.opacity(0.1) : Color.clear)
                .overlay(
                    Rectangle()
                        .fill(ThemeColors.surfaceAlt.swiftUIColor)
                        .frame(width: 1),
                    alignment: .leading
                )
            }
        }
        .background(ThemeColors.surface.swiftUIColor)
        .overlay(
            Rectangle()
                .fill(ThemeColors.border.swiftUIColor)
                .frame(height: 1),
            alignment: .bottom
        )
    }

    private func dayAbbrev(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "EEE"
        return formatter.string(from: date)
    }
}

private struct WeekDayColumnView: View {
    var day: Date
    var events: [CalendarEvent]
    var calendarLookup: [UUID: SundyCalendar]
    var hideEventTitles: Bool
    var onDayClick: () -> Void
    var onEventClick: (CalendarEvent) -> Void

    private let pixelsPerHour: CGFloat = 48

    var body: some View {
        ZStack(alignment: .topLeading) {
            VStack(spacing: 0) {
                ForEach(0..<24, id: \.self) { _ in
                    Rectangle()
                        .fill(Color.clear)
                        .frame(height: pixelsPerHour)
                        .overlay(
                            Rectangle()
                                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                                .frame(height: 1),
                            alignment: .bottom
                        )
                }
            }

            ForEach(events) { event in
                Button {
                    onEventClick(event)
                } label: {
                    VStack(alignment: .leading, spacing: 2) {
                        Text(displayTitle(for: event))
                            .font(ThemeTypography.body(12))
                            .foregroundStyle(Color.white)
                            .lineLimit(1)
                        Text(eventTimeText(event))
                            .font(ThemeTypography.body(10))
                            .foregroundStyle(Color.white.opacity(0.8))
                            .lineLimit(1)
                    }
                    .padding(6)
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .background(
                        RoundedRectangle(cornerRadius: 4)
                            .fill(eventColor(for: event))
                    )
                }
                .buttonStyle(.plain)
                .frame(height: eventHeight(event))
                .offset(y: eventOffset(event))
                .opacity(isPastEvent(event) ? 0.5 : 1.0)
            }
        }
        .frame(maxWidth: .infinity)
        .background(Calendar.current.isDateInToday(day) ? ThemeColors.accent.swiftUIColor.opacity(0.05) : Color.clear)
        .overlay(
            Rectangle()
                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                .frame(width: 1),
            alignment: .leading
        )
        .contentShape(Rectangle())
        .onTapGesture {
            onDayClick()
        }
    }

    private func eventOffset(_ event: CalendarEvent) -> CGFloat {
        let start = event.startTime
        let hour = CGFloat(Calendar.current.component(.hour, from: start))
        let minute = CGFloat(Calendar.current.component(.minute, from: start))
        return hour * pixelsPerHour + (minute / 60.0) * pixelsPerHour
    }

    private func eventHeight(_ event: CalendarEvent) -> CGFloat {
        let duration = event.endTime.timeIntervalSince(event.startTime)
        let minutes = max(duration / 60.0, 30)
        return CGFloat(minutes) * 0.8
    }

    private func eventColor(for event: CalendarEvent) -> Color {
        calendarLookup[event.calendarId]?.color.swiftUIColor ?? ThemeColors.calendarDefault.swiftUIColor
    }

    private func displayTitle(for event: CalendarEvent) -> String {
        if hideEventTitles { return "Private Event" }
        return event.title.isEmpty ? "(No title)" : event.title
    }

    private func eventTimeText(_ event: CalendarEvent) -> String {
        "\(DateUtils.formatTime(event.startTime)) - \(DateUtils.formatTime(event.endTime))"
    }

    private func isPastEvent(_ event: CalendarEvent) -> Bool {
        event.endTime < Date()
    }
}
