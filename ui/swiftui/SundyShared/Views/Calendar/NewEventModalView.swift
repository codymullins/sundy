import SwiftUI

struct NewEventModalView: View {
    @Binding var draft: EventDraft
    var calendars: [SundyCalendar]
    var isEditMode: Bool
    var onClose: () -> Void
    var onOpenScheduler: () -> Void
    var onSave: () -> Void

    var body: some View {
        VStack(spacing: 0) {
            HStack {
                Text(isEditMode ? "Edit Event" : "New Event")
                    .font(ThemeTypography.body(20))
                    .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)

                Spacer()

                Menu {
                    ForEach(calendars) { calendar in
                        Button {
                            draft.calendarId = calendar.id
                        } label: {
                            HStack {
                                Circle()
                                    .fill(calendar.color.swiftUIColor)
                                    .frame(width: 10, height: 10)
                                Text(calendar.effectiveName)
                            }
                        }
                    }
                } label: {
                    HStack(spacing: 8) {
                        Circle()
                            .fill(selectedCalendarColor)
                            .frame(width: 16, height: 16)
                        Text(selectedCalendarName)
                            .font(ThemeTypography.body(14))
                        Image(systemName: "chevron.down")
                            .font(.system(size: 12))
                    }
                    .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                    .padding(.horizontal, 12)
                    .padding(.vertical, 8)
                    .background(
                        RoundedRectangle(cornerRadius: 6)
                            .fill(ThemeColors.surfaceAlt.swiftUIColor)
                    )
                }
            }
            .padding(20)
            .background(
                Rectangle()
                    .fill(ThemeColors.surface.swiftUIColor)
                    .overlay(
                        Rectangle()
                            .fill(ThemeColors.border.swiftUIColor)
                            .frame(height: 1),
                        alignment: .bottom
                    )
            )

            ScrollView {
                VStack(alignment: .leading, spacing: 16) {
                    VStack(alignment: .leading, spacing: 8) {
                        Text("Title *")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        TextField("Event title", text: $draft.title)
                            .textFieldStyle(.plain)
                            .padding(12)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(ThemeColors.surface.swiftUIColor)
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 8)
                                            .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                                    )
                            )
                            .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                    }

                    VStack(alignment: .leading, spacing: 8) {
                        Text("Date & Time *")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)

                        Button(action: onOpenScheduler) {
                            HStack(spacing: 16) {
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(ThemeColors.accent.swiftUIColor)
                                    .frame(width: 48, height: 48)
                                    .overlay(
                                        Image(systemName: "calendar")
                                            .foregroundStyle(Color.white)
                                    )

                                VStack(alignment: .leading, spacing: 4) {
                                    Text(draft.date, format: .dateTime.weekday().month().day().year())
                                        .font(ThemeTypography.body(15))
                                        .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)

                                    Text(timeSummary)
                                        .font(ThemeTypography.body(14))
                                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)

                                    Text(durationText)
                                        .font(ThemeTypography.body(13))
                                        .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                                }

                                Spacer()

                                Image(systemName: "pencil")
                                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                            }
                            .padding(16)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(ThemeColors.surface.swiftUIColor)
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 8)
                                            .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                                    )
                            )
                        }
                        .buttonStyle(.plain)
                    }

                    Toggle(isOn: $draft.isAllDay) {
                        Text("All day event")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                    }
                    .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                    VStack(alignment: .leading, spacing: 8) {
                        Text("Description")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        TextEditor(text: $draft.details)
                            .frame(minHeight: 100)
                            .padding(10)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(ThemeColors.surface.swiftUIColor)
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 8)
                                            .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                                    )
                            )
                            .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                    }
                }
                .padding(24)
            }

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

                Button(action: onSave) {
                    Text(isEditMode ? "Save" : "Create")
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
                        Rectangle()
                            .fill(ThemeColors.border.swiftUIColor)
                            .frame(height: 1),
                        alignment: .top
                    )
            )
        }
        .frame(maxWidth: 520, maxHeight: 560)
        .background(
            RoundedRectangle(cornerRadius: 12)
                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                )
        )
        .shadow(color: Color.black.opacity(0.4), radius: 30, x: 0, y: 10)
    }

    private var selectedCalendarName: String {
        calendars.first(where: { $0.id == draft.calendarId })?.effectiveName ?? "My Calendar"
    }

    private var selectedCalendarColor: Color {
        calendars.first(where: { $0.id == draft.calendarId })?.color.swiftUIColor ?? ThemeColors.calendarDefault.swiftUIColor
    }

    private var timeSummary: String {
        if draft.isAllDay {
            return "All day"
        }
        return "\(DateUtils.formatTime(draft.startTime)) → \(DateUtils.formatTime(draft.endTime))"
    }

    private var durationText: String {
        if draft.isAllDay {
            return "Duration: All day"
        }
        let minutes = Int(draft.endTime.timeIntervalSince(draft.startTime) / 60)
        if minutes <= 0 { return "Duration: 1 hr" }
        let hours = minutes / 60
        let remainder = minutes % 60
        if remainder == 0 {
            return hours == 1 ? "Duration: 1 hr" : "Duration: \(hours) hrs"
        }
        if hours == 0 {
            return "Duration: \(remainder) min"
        }
        return "Duration: \(hours) hr \(remainder) min"
    }
}
