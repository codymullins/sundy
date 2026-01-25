import SwiftUI

struct DemoModeBannerView: View {
    var onDismiss: () -> Void

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: "info.circle")
                .foregroundStyle(Color.white.opacity(0.9))
            Text("Demo Mode: Telemetry is enabled by default. Go to Settings > Privacy to disable it.")
                .font(ThemeTypography.body(14))
                .foregroundStyle(Color.white)
            Spacer()
            Button(action: onDismiss) {
                Image(systemName: "xmark")
                    .foregroundStyle(Color.white)
                    .frame(width: 28, height: 28)
                    .background(
                        RoundedRectangle(cornerRadius: 6)
                            .fill(Color.white.opacity(0.2))
                    )
            }
            .buttonStyle(.plain)
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 10)
        .background(
            LinearGradient(
                colors: [
                    ThemeColors.accentGradientStart.swiftUIColor,
                    ThemeColors.accentGradientEnd.swiftUIColor
                ],
                startPoint: .topLeading,
                endPoint: .bottomTrailing
            )
        )
        .frame(maxWidth: .infinity)
        .transition(.move(edge: .bottom).combined(with: .opacity))
        .frame(maxHeight: .infinity, alignment: .bottom)
    }
}
