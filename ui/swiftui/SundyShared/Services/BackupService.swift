import Foundation

final class BackupService {
    private let fileManager: FileManager
    private let backupsURL: URL

    init(fileManager: FileManager = .default) {
        self.fileManager = fileManager
        let baseURL = fileManager.sundyAppSupportDirectory
        let backups = baseURL.appendingPathComponent("Backups", isDirectory: true)
        fileManager.ensureDirectoryExists(at: backups)
        backupsURL = backups
    }

    func createBackup(data: SundyData) -> BackupInfo? {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyyMMdd-HHmmss"
        let stamp = formatter.string(from: Date())
        let fileName = "sundy-backup-\(stamp).json"
        let fileURL = backupsURL.appendingPathComponent(fileName)

        do {
            let encoder = JSONEncoder()
            encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
            encoder.dateEncodingStrategy = .iso8601
            let encoded = try encoder.encode(data)
            try encoded.write(to: fileURL, options: .atomic)
            let info = BackupInfo(
                id: UUID(),
                createdAt: Date(),
                sizeBytes: Int64(encoded.count),
                fileName: fileName
            )
            return info
        } catch {
            return nil
        }
    }

    func loadBackups() -> [BackupInfo] {
        guard let files = try? fileManager.contentsOfDirectory(
            at: backupsURL,
            includingPropertiesForKeys: [.creationDateKey, .fileSizeKey],
            options: [.skipsHiddenFiles]
        ) else {
            return []
        }

        return files.compactMap { url in
            guard url.pathExtension == "json" else { return nil }
            let values = try? url.resourceValues(forKeys: [.creationDateKey, .fileSizeKey])
            let createdAt = values?.creationDate ?? Date()
            let size = Int64(values?.fileSize ?? 0)
            return BackupInfo(
                id: UUID(),
                createdAt: createdAt,
                sizeBytes: size,
                fileName: url.lastPathComponent
            )
        }
        .sorted(by: { $0.createdAt > $1.createdAt })
    }

    func deleteBackup(fileName: String) {
        let url = backupsURL.appendingPathComponent(fileName)
        try? fileManager.removeItem(at: url)
    }
}
