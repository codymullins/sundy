import Foundation
import SwiftUI

struct ColorValue: Codable, Equatable, Hashable {
    var red: Double
    var green: Double
    var blue: Double
    var alpha: Double

    var color: Color {
        Color(.sRGB, red: red, green: green, blue: blue, opacity: alpha)
    }

    static func fromRGB(_ red: Int, _ green: Int, _ blue: Int, alpha: Double = 1.0) -> ColorValue {
        ColorValue(
            red: Double(red) / 255.0,
            green: Double(green) / 255.0,
            blue: Double(blue) / 255.0,
            alpha: alpha
        )
    }
}
