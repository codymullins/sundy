import SwiftUI

enum ThemeColors {
    static let background = ColorValue.fromRGB(26, 26, 26)
    static let surface = ColorValue.fromRGB(30, 30, 30)
    static let surfaceAlt = ColorValue.fromRGB(42, 42, 42)
    static let surfaceElevated = ColorValue.fromRGB(58, 58, 58)
    static let border = ColorValue.fromRGB(51, 51, 51)
    static let accent = ColorValue.fromRGB(124, 58, 237)
    static let accentHover = ColorValue.fromRGB(109, 40, 217)
    static let accentGradientStart = ColorValue.fromRGB(124, 58, 237)
    static let accentGradientEnd = ColorValue.fromRGB(147, 51, 234)
    static let calendarDefault = ColorValue.fromRGB(66, 133, 244)

    static let textPrimary = ColorValue.fromRGB(246, 246, 246)
    static let textSecondary = ColorValue.fromRGB(224, 224, 224)
    static let textMuted = ColorValue.fromRGB(160, 160, 160)
    static let textSubtle = ColorValue.fromRGB(128, 128, 128)
    static let textDisabled = ColorValue.fromRGB(80, 80, 80)
    static let textHint = ColorValue.fromRGB(96, 96, 96)
    static let textMeta = ColorValue.fromRGB(144, 144, 144)
    static let textNote = ColorValue.fromRGB(112, 112, 112)
    static let textDarkMuted = ColorValue.fromRGB(102, 102, 102)
    static let textLightMuted = ColorValue.fromRGB(136, 136, 136)
    static let textUppercase = ColorValue.fromRGB(153, 153, 153)

    static let success = ColorValue.fromRGB(16, 185, 129)
    static let successAlt = ColorValue.fromRGB(81, 207, 102)
    static let error = ColorValue.fromRGB(239, 68, 68)
    static let errorAlt = ColorValue.fromRGB(255, 107, 107)
    static let danger = ColorValue.fromRGB(220, 38, 38)
    static let dangerHover = ColorValue.fromRGB(185, 28, 28)
    static let warning = ColorValue.fromRGB(245, 158, 11)
    static let info = ColorValue.fromRGB(23, 162, 184)
    static let infoAlt = ColorValue.fromRGB(116, 192, 252)
    static let blueAccent = ColorValue.fromRGB(0, 120, 212)

    static let toastErrorBackground = ColorValue.fromRGB(58, 31, 31)
    static let toastSuccessBackground = ColorValue.fromRGB(31, 58, 31)
    static let toastInfoBackground = ColorValue.fromRGB(31, 42, 58)
}

enum ThemeTypography {
    static func title(_ size: CGFloat) -> Font {
        Font.custom("New York", size: size)
    }

    static func body(_ size: CGFloat) -> Font {
        Font.custom("Avenir Next", size: size)
    }
}

extension ColorValue {
    var swiftUIColor: Color {
        color
    }
}
