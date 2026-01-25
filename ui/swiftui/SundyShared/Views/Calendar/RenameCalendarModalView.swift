import SwiftUI

struct RenameCalendarModalView: View {
    var calendar: SundyCalendar
    @Binding var renameText: String
    var onCancel: () -> Void
    var onSave: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            Text("Rename Calendar")
                .font(ThemeTypography.body(16))
                .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)

            if calendar.displayName != nil {
                Text("Original: \(calendar.name)")
                    .font(ThemeTypography.body(12))
                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
            }

            HStack(spacing: 12) {
                Circle()
                    .fill(calendar.color.swiftUIColor)
                    .frame(width: 12, height: 12)
                TextField("Calendar name", text: $renameText)
                    .textFieldStyle(.plain)
                    .padding(10)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(ThemeColors.surface.swiftUIColor)
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                            )
                    )
            }

            HStack(spacing: 8) {
                Spacer()
                Button("Cancel", action: onCancel)
                    .font(ThemeTypography.body(14))
                    .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                    .padding(.horizontal, 16)
                    .padding(.vertical, 8)
                    .background(
                        Capsule().fill(ThemeColors.surfaceElevated.swiftUIColor)
                    )
                Button("Save", action: onSave)
                    .font(ThemeTypography.body(14))
                    .foregroundStyle(Color.white)
                    .padding(.horizontal, 16)
                    .padding(.vertical, 8)
                    .background(
                        Capsule().fill(ThemeColors.accent.swiftUIColor)
                    )
            }
        }
        .padding(20)
        .frame(maxWidth: 320)
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
}
