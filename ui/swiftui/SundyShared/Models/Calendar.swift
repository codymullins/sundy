import Foundation

struct SundyCalendar: Identifiable, Codable, Equatable {
    var id: UUID
    var name: String
    var displayName: String?
    var color: ColorValue
    var isVisible: Bool
    var isHidden: Bool
    var accountId: String?
    var accountEmail: String?

    var effectiveName: String {
        displayName ?? name
    }

    var displayNameForGroup: String {
        accountEmail ?? accountId ?? "Local"
    }
}

extension SundyCalendar {
    static let defaultId = UUID(uuidString: "00000000-0000-0000-0000-000000000001")!

    static let defaultCalendar = SundyCalendar(
        id: defaultId,
        name: "My Calendar",
        displayName: nil,
        color: ColorValue.fromRGB(66, 133, 244),
        isVisible: true,
        isHidden: false,
        accountId: nil,
        accountEmail: nil
    )
}
