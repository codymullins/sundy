import SwiftUI

struct TimeGutterView: View {
    var body: some View {
        VStack(spacing: 0) {
            ForEach(0..<24, id: \.self) { hour in
                HStack {
                    Text(DateUtils.formatHourLabel(hour))
                        .font(ThemeTypography.body(11))
                        .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                        .frame(maxWidth: .infinity, alignment: .trailing)
                        .padding(.trailing, 6)
                }
                .frame(height: 48)
            }
        }
        .background(ThemeColors.background.swiftUIColor)
    }
}
