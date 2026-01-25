import SwiftUI

#if os(iOS)
import UIKit
#elseif os(macOS)
import AppKit
#endif

extension Color {
    func toColorValue() -> ColorValue {
        #if os(iOS)
        let uiColor = UIColor(self)
        var red: CGFloat = 0
        var green: CGFloat = 0
        var blue: CGFloat = 0
        var alpha: CGFloat = 0
        uiColor.getRed(&red, green: &green, blue: &blue, alpha: &alpha)
        return ColorValue(red: Double(red), green: Double(green), blue: Double(blue), alpha: Double(alpha))
        #elseif os(macOS)
        let nsColor = NSColor(self)
        guard let rgb = nsColor.usingColorSpace(.deviceRGB) else {
            return ThemeColors.calendarDefault
        }
        return ColorValue(red: Double(rgb.redComponent), green: Double(rgb.greenComponent), blue: Double(rgb.blueComponent), alpha: Double(rgb.alphaComponent))
        #else
        return ThemeColors.calendarDefault
        #endif
    }
}
