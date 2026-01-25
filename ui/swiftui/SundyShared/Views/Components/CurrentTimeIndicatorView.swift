import SwiftUI

struct CurrentTimeIndicatorLine: View {
    var pixelsPerHour: CGFloat = 48
    var showLabel: Bool = true
    var labelWidth: CGFloat = 56

    var body: some View {
        TimelineView(.everyMinute) { context in
            let now = context.date
            let minutes = Double(Calendar.current.component(.hour, from: now) * 60 + Calendar.current.component(.minute, from: now))
            let top = CGFloat(minutes / 60.0) * pixelsPerHour

            HStack(spacing: 4) {
                if showLabel {
                    Text(DateUtils.formatTime(now).uppercased())
                        .font(ThemeTypography.body(10))
                        .foregroundStyle(ThemeColors.accent.swiftUIColor)
                        .frame(width: labelWidth, alignment: .trailing)
                }

                Circle()
                    .fill(ThemeColors.accent.swiftUIColor)
                    .frame(width: 8, height: 8)

                Rectangle()
                    .fill(ThemeColors.accent.swiftUIColor)
                    .frame(height: 2)
            }
            .frame(maxWidth: .infinity, alignment: .leading)
            .offset(y: top)
        }
    }
}
