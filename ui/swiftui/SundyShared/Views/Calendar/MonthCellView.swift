import SwiftUI

struct MonthCellView: View {
    var day: Date
    var currentDate: Date
    var events: [CalendarEvent]
    var calendarLookup: [UUID: SundyCalendar]
    var isToday: Bool
    var hideEventTitles: Bool
    var collapsePastEvents: Bool
    var isExpanded: Bool
    var onToggleExpanded: () -> Void
    var onDayClick: () -> Void
    var onEventClick: (CalendarEvent) -> Void

    var body: some View {
        GeometryReader { geometry in
            let isCurrentMonth = Calendar.current.isDate(day, equalTo: currentDate, toGranularity: .month)
            let isPastDay = day.startOfDay < Date().startOfDay
            let shouldCollapse = collapsePastEvents && isPastDay && !isExpanded

            VStack(alignment: .leading, spacing: 4) {
                HStack {
                    Spacer()
                    Text("\(Calendar.current.component(.day, from: day))")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(isCurrentMonth ? ThemeColors.textSecondary.swiftUIColor : ThemeColors.textDisabled.swiftUIColor)
                        .padding(6)
                        .background(
                            isToday ? Circle().fill(ThemeColors.accent.swiftUIColor) : Circle().fill(Color.clear)
                        )
                }

                if isToday {
                    MonthCurrentTimeIndicator(height: geometry.size.height)
                }

                if shouldCollapse && !events.isEmpty {
                    Button(action: onToggleExpanded) {
                        Text("\(events.count) event\(events.count == 1 ? "" : "s")")
                            .font(ThemeTypography.body(11))
                            .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                            .padding(.vertical, 4)
                            .padding(.horizontal, 6)
                            .background(
                                RoundedRectangle(cornerRadius: 4)
                                    .fill(ThemeColors.surfaceAlt.swiftUIColor.opacity(0.2))
                            )
                    }
                    .buttonStyle(.plain)
                } else {
                    ForEach(events.prefix(3)) { event in
                        Button {
                            onEventClick(event)
                        } label: {
                            Text(displayTitle(for: event))
                                .font(ThemeTypography.body(12))
                                .foregroundStyle(Color.white)
                                .lineLimit(1)
                                .padding(.vertical, 2)
                                .padding(.horizontal, 6)
                                .frame(maxWidth: .infinity, alignment: .leading)
                                .background(
                                    RoundedRectangle(cornerRadius: 4)
                                        .fill(eventColor(for: event))
                                )
                        }
                        .buttonStyle(.plain)
                        .opacity(isPastEvent(event) ? 0.5 : 1.0)
                    }

                    if events.count > 3 {
                        Text("+\(events.count - 3) more")
                            .font(ThemeTypography.body(11))
                            .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    }

                    if isPastDay && collapsePastEvents && isExpanded && !events.isEmpty {
                        Button(action: onToggleExpanded) {
                            Text("collapse")
                                .font(ThemeTypography.body(10))
                                .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                                .frame(maxWidth: .infinity)
                        }
                        .buttonStyle(.plain)
                    }
                }

                Spacer()
            }
            .padding(8)
            .frame(maxWidth: .infinity, maxHeight: .infinity)
            .background(
                Rectangle().fill(isCurrentMonth ? ThemeColors.background.swiftUIColor : ThemeColors.surface.swiftUIColor)
            )
            .overlay(
                Rectangle().stroke(ThemeColors.surfaceAlt.swiftUIColor, lineWidth: 0.5)
            )
            .onTapGesture {
                onDayClick()
            }
        }
    }

    private func isPastEvent(_ event: CalendarEvent) -> Bool {
        event.endTime < Date()
    }

    private func displayTitle(for event: CalendarEvent) -> String {
        if hideEventTitles {
            return "Private Event"
        }
        return event.title.isEmpty ? "(No title)" : event.title
    }

    private func eventColor(for event: CalendarEvent) -> Color {
        calendarLookup[event.calendarId]?.color.swiftUIColor ?? ThemeColors.calendarDefault.swiftUIColor
    }
}

struct MonthCurrentTimeIndicator: View {
    var height: CGFloat

    var body: some View {
        TimelineView(.everyMinute) { context in
            let now = context.date
            let minutes = Double(Calendar.current.component(.hour, from: now) * 60 + Calendar.current.component(.minute, from: now))
            let percent = minutes / 1440.0
            let top = CGFloat(28) + (height - 28) * CGFloat(percent)

            HStack(spacing: 4) {
                Circle()
                    .fill(ThemeColors.accent.swiftUIColor)
                    .frame(width: 5, height: 5)
                Rectangle()
                    .fill(ThemeColors.accent.swiftUIColor)
                    .frame(height: 1)
            }
            .offset(y: top - 28)
        }
    }
}
