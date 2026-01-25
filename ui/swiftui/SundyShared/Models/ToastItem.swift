import Foundation

enum ToastType: String, Codable {
    case info
    case success
    case error
}

struct ToastItem: Identifiable, Codable, Equatable {
    var id: UUID
    var message: String
    var type: ToastType
    var createdAt: Date
}
