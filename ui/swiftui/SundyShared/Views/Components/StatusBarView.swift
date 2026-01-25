import SwiftUI

struct StatusBarView: View {
    @EnvironmentObject private var store: SundyStore
    @State private var isMinimized = true

    var body: some View {
        VStack(spacing: 0) {
            if isMinimized {
                HStack {
                    Text(summaryText)
                        .font(ThemeTypography.body(12))
                        .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                    Spacer()
                    Button {
                        isMinimized = false
                    } label: {
                        Image(systemName: "chevron.up")
                            .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    }
                    .buttonStyle(.plain)
                }
                .padding(.horizontal, 16)
                .frame(height: 36)
                .background(ThemeColors.surface.swiftUIColor)
            } else {
                HStack {
                    Text("Activity Log")
                        .font(ThemeTypography.body(11))
                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    Spacer()
                    Button("Clear") {
                        store.clearLogs()
                    }
                    .font(ThemeTypography.body(11))
                    .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    .padding(.horizontal, 8)
                    .padding(.vertical, 4)
                    .background(
                        RoundedRectangle(cornerRadius: 4)
                            .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                    )
                    Button {
                        isMinimized = true
                    } label: {
                        Image(systemName: "chevron.down")
                            .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                    }
                    .buttonStyle(.plain)
                }
                .padding(.horizontal, 16)
                .frame(height: 36)
                .background(ThemeColors.surface.swiftUIColor)

                ScrollView {
                    VStack(alignment: .leading, spacing: 4) {
                        if store.logs.isEmpty {
                            Text("No sync activity yet")
                                .font(ThemeTypography.body(12))
                                .foregroundStyle(ThemeColors.textDisabled.swiftUIColor)
                                .padding(.vertical, 16)
                                .frame(maxWidth: .infinity, alignment: .center)
                        } else {
                            ForEach(store.logs.prefix(50)) { entry in
                                HStack(alignment: .firstTextBaseline, spacing: 12) {
                                    Text(timeText(entry.timestamp))
                                        .font(ThemeTypography.body(11))
                                        .foregroundStyle(ThemeColors.textDisabled.swiftUIColor)
                                    if let calendarName = entry.calendarName {
                                        Text("[\(calendarName)]")
                                            .font(ThemeTypography.body(11))
                                            .foregroundStyle(ThemeColors.accent.swiftUIColor)
                                    }
                                    Text(entry.message)
                                        .font(ThemeTypography.body(12))
                                        .foregroundStyle(color(for: entry.level))
                                    Spacer()
                                }
                                .padding(.horizontal, 16)
                                .padding(.vertical, 2)
                            }
                        }
                    }
                }
                .frame(height: 114)
                .background(ThemeColors.background.swiftUIColor)
            }
        }
        .frame(maxWidth: .infinity)
        .background(ThemeColors.background.swiftUIColor)
        .overlay(
            Rectangle()
                .fill(ThemeColors.border.swiftUIColor)
                .frame(height: 1),
            alignment: .top
        )
    }

    private var summaryText: String {
        guard let entry = store.logs.first else { return "No recent activity" }
        let timeSince = Date().timeIntervalSince(entry.timestamp)
        if timeSince < 60 { return "Last activity: just now" }
        if timeSince < 3600 { return "Last activity: \(Int(timeSince / 60))m ago" }
        if timeSince < 86400 { return "Last activity: \(Int(timeSince / 3600))h ago" }
        let formatter = DateFormatter()
        formatter.dateFormat = "MMM d, h:mm a"
        return "Last activity: \(formatter.string(from: entry.timestamp))"
    }

    private func timeText(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "HH:mm:ss"
        return formatter.string(from: date)
    }

    private func color(for level: LogLevel) -> Color {
        switch level {
        case .success:
            return ThemeColors.success.swiftUIColor
        case .warning:
            return ThemeColors.warning.swiftUIColor
        case .error:
            return ThemeColors.error.swiftUIColor
        case .info:
            return ThemeColors.textMeta.swiftUIColor
        }
    }
}
