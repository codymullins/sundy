import SwiftUI

struct SchedulerModalView: View {
    @Binding var draft: EventDraft
    var onClose: () -> Void

    @State private var selectedDate: Date = Date()
    @State private var tempStart: Date = Date()
    @State private var tempEnd: Date = Date().addingTimeInterval(3600)

    private let pixelsPerHour: CGFloat = 48
    private let stepMinutes = 15

    var body: some View {
        VStack(spacing: 0) {
            HStack(spacing: 8) {
                Image(systemName: "clock")
                Text(monthTitle)
                    .font(ThemeTypography.body(16))
                    .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                Spacer()
            }
            .padding(16)
            .background(
                Rectangle()
                    .fill(ThemeColors.surface.swiftUIColor)
                    .overlay(
                        Rectangle().fill(ThemeColors.border.swiftUIColor).frame(height: 1),
                        alignment: .bottom
                    )
            )

            HStack(spacing: 12) {
                Button(action: previousWeek) {
                    Image(systemName: "chevron.left")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        .frame(width: 32, height: 32)
                        .background(
                            RoundedRectangle(cornerRadius: 6)
                                .fill(Color.clear)
                        )
                }
                .buttonStyle(.plain)

                HStack(spacing: 8) {
                    ForEach(DateUtils.weekDays(for: selectedDate), id: \.self) { day in
                        Button {
                            selectedDate = day
                        } label: {
                            VStack(spacing: 4) {
                                Text(dayAbbrev(day))
                                    .font(ThemeTypography.body(12))
                                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                                Text("\(Calendar.current.component(.day, from: day))")
                                    .font(ThemeTypography.body(14))
                                    .foregroundStyle(isSelected(day) ? Color.white : ThemeColors.textSecondary.swiftUIColor)
                                    .frame(width: 32, height: 32)
                                    .background(
                                        Circle()
                                            .fill(isSelected(day) ? ThemeColors.accent.swiftUIColor : Color.clear)
                                    )
                            }
                        }
                        .buttonStyle(.plain)
                    }
                }

                Button(action: nextWeek) {
                    Image(systemName: "chevron.right")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        .frame(width: 32, height: 32)
                }
                .buttonStyle(.plain)
            }
            .padding(.horizontal, 16)
            .padding(.vertical, 12)
            .background(
                Rectangle()
                    .fill(ThemeColors.surface.swiftUIColor)
                    .overlay(
                        Rectangle().fill(ThemeColors.border.swiftUIColor).frame(height: 1),
                        alignment: .bottom
                    )
            )

            ZStack(alignment: .topLeading) {
                ScrollView {
                    VStack(spacing: 0) {
                        ForEach(0..<24, id: \.self) { hour in
                            HStack(spacing: 12) {
                                Text(DateUtils.formatHourLabel(hour))
                                    .font(ThemeTypography.body(12))
                                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                                    .frame(width: 60, alignment: .trailing)
                                Rectangle()
                                    .fill(ThemeColors.surfaceAlt.swiftUIColor)
                                    .frame(height: 1)
                            }
                            .frame(height: pixelsPerHour)
                            .contentShape(Rectangle())
                            .onTapGesture {
                                setStartHour(hour)
                            }
                        }
                    }
                    .padding(.horizontal, 20)
                }

                TimeRangeIndicatorView(
                    startTime: tempStart,
                    endTime: tempEnd,
                    pixelsPerHour: pixelsPerHour,
                    onMove: adjustRange,
                    onResizeStart: adjustStart,
                    onResizeEnd: adjustEnd
                )
                .padding(.horizontal, 20)
                .padding(.top, 0)
            }
            .frame(maxHeight: 400)

            HStack(spacing: 12) {
                Button(action: onClose) {
                    Text("Cancel")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        .padding(.horizontal, 24)
                        .padding(.vertical, 10)
                        .background(
                            Capsule().fill(ThemeColors.surfaceElevated.swiftUIColor)
                        )
                }
                .buttonStyle(.plain)

                Button(action: confirmSchedule) {
                    Text("Confirm")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(Color.white)
                        .padding(.horizontal, 24)
                        .padding(.vertical, 10)
                        .background(
                            Capsule().fill(ThemeColors.accent.swiftUIColor)
                        )
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .frame(maxWidth: .infinity, alignment: .trailing)
            .background(
                Rectangle()
                    .fill(ThemeColors.surface.swiftUIColor)
                    .overlay(
                        Rectangle().fill(ThemeColors.border.swiftUIColor).frame(height: 1),
                        alignment: .top
                    )
            )
        }
        .frame(maxWidth: 620)
        .background(
            RoundedRectangle(cornerRadius: 12)
                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                )
        )
        .shadow(color: Color.black.opacity(0.4), radius: 30, x: 0, y: 10)
        .onAppear {
            selectedDate = draft.date
            tempStart = draft.startTime
            tempEnd = draft.endTime
        }
    }

    private var monthTitle: String {
        let formatter = DateFormatter()
        formatter.dateFormat = "MMMM"
        return formatter.string(from: selectedDate)
    }

    private func dayAbbrev(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "EEE"
        return String(formatter.string(from: date).prefix(1))
    }

    private func isSelected(_ date: Date) -> Bool {
        Calendar.current.isDate(date, inSameDayAs: selectedDate)
    }

    private func previousWeek() {
        selectedDate = Calendar.current.date(byAdding: .day, value: -7, to: selectedDate) ?? selectedDate
    }

    private func nextWeek() {
        selectedDate = Calendar.current.date(byAdding: .day, value: 7, to: selectedDate) ?? selectedDate
    }

    private func setStartHour(_ hour: Int) {
        tempStart = setTime(for: selectedDate, hour: hour, minute: 0)
        tempEnd = setTime(for: selectedDate, hour: (hour + 1) % 24, minute: 0)
    }

    private func confirmSchedule() {
        draft.date = selectedDate
        draft.startTime = tempStart
        draft.endTime = tempEnd
        onClose()
    }

    private func adjustRange(deltaMinutes: Int) {
        tempStart = addMinutes(tempStart, deltaMinutes)
        tempEnd = addMinutes(tempEnd, deltaMinutes)
    }

    private func adjustStart(deltaMinutes: Int) {
        let newStart = addMinutes(tempStart, deltaMinutes)
        if newStart < tempEnd {
            tempStart = newStart
        }
    }

    private func adjustEnd(deltaMinutes: Int) {
        let newEnd = addMinutes(tempEnd, deltaMinutes)
        if newEnd > tempStart {
            tempEnd = newEnd
        }
    }

    private func setTime(for date: Date, hour: Int, minute: Int) -> Date {
        var components = Calendar.current.dateComponents([.year, .month, .day], from: date)
        components.hour = hour
        components.minute = minute
        return Calendar.current.date(from: components) ?? date
    }

    private func addMinutes(_ date: Date, _ minutes: Int) -> Date {
        Calendar.current.date(byAdding: .minute, value: minutes, to: date) ?? date
    }
}

private struct TimeRangeIndicatorView: View {
    var startTime: Date
    var endTime: Date
    var pixelsPerHour: CGFloat
    var onMove: (Int) -> Void
    var onResizeStart: (Int) -> Void
    var onResizeEnd: (Int) -> Void
    @State private var lastMoveDelta: Int = 0
    @State private var lastStartDelta: Int = 0
    @State private var lastEndDelta: Int = 0

    var body: some View {
        GeometryReader { geometry in
            let startMinutes = minutesFromStartOfDay(startTime)
            let endMinutes = max(minutesFromStartOfDay(endTime), startMinutes + 15)
            let top = CGFloat(startMinutes) / 60.0 * pixelsPerHour
            let height = CGFloat(endMinutes - startMinutes) / 60.0 * pixelsPerHour

            RoundedRectangle(cornerRadius: 8)
                .fill(ThemeColors.accent.swiftUIColor)
                .frame(height: height)
                .overlay(
                    VStack(alignment: .leading, spacing: 2) {
                        Text(timeRangeText())
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(Color.white)
                        Text(durationText())
                            .font(ThemeTypography.body(12))
                            .foregroundStyle(Color.white.opacity(0.7))
                    }
                    .padding(.horizontal, 16)
                    .padding(.vertical, 8),
                    alignment: .topLeading
                )
                .overlay(
                    Circle()
                        .stroke(Color.white, lineWidth: 2)
                        .background(Circle().fill(ThemeColors.surface.swiftUIColor))
                        .frame(width: 12, height: 12)
                        .offset(y: -6)
                        .gesture(
                            DragGesture()
                                .onChanged { value in
                                    let delta = minutesFromDrag(value.translation.height)
                                    onResizeStart(delta - lastStartDelta)
                                    lastStartDelta = delta
                                }
                                .onEnded { _ in
                                    lastStartDelta = 0
                                }
                        ),
                    alignment: .top
                )
                .overlay(
                    Circle()
                        .stroke(Color.white, lineWidth: 2)
                        .background(Circle().fill(ThemeColors.surface.swiftUIColor))
                        .frame(width: 12, height: 12)
                        .offset(y: height - 6)
                        .gesture(
                            DragGesture()
                                .onChanged { value in
                                    let delta = minutesFromDrag(value.translation.height)
                                    onResizeEnd(delta - lastEndDelta)
                                    lastEndDelta = delta
                                }
                                .onEnded { _ in
                                    lastEndDelta = 0
                                }
                        ),
                    alignment: .top
                )
                .offset(y: top)
                .gesture(
                    DragGesture()
                        .onChanged { value in
                            let delta = minutesFromDrag(value.translation.height)
                            onMove(delta - lastMoveDelta)
                            lastMoveDelta = delta
                        }
                        .onEnded { _ in
                            lastMoveDelta = 0
                        }
                )
        }
    }

    private func minutesFromStartOfDay(_ date: Date) -> Int {
        let hour = Calendar.current.component(.hour, from: date)
        let minute = Calendar.current.component(.minute, from: date)
        return hour * 60 + minute
    }

    private func minutesFromDrag(_ translation: CGFloat) -> Int {
        let minutes = Int((translation / 12).rounded()) * 15
        return minutes
    }

    private func timeRangeText() -> String {
        "\(DateUtils.formatTime(startTime)) → \(DateUtils.formatTime(endTime))"
    }

    private func durationText() -> String {
        let minutes = Int(endTime.timeIntervalSince(startTime) / 60)
        if minutes <= 0 { return "1 hr" }
        let hours = minutes / 60
        let remainder = minutes % 60
        if remainder == 0 {
            return hours == 1 ? "1 hr" : "\(hours) hrs"
        }
        if hours == 0 {
            return "\(remainder) min"
        }
        return "\(hours) hr \(remainder) min"
    }
}
