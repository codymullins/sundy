import Foundation

struct BackupInfo: Identifiable, Codable, Equatable {
    var id: UUID
    var createdAt: Date
    var sizeBytes: Int64
    var fileName: String
}
