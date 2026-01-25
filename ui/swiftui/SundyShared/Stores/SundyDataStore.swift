import Foundation

struct SundyData: Codable, Equatable {
    var calendars: [SundyCalendar]
    var events: [CalendarEvent]
    var settings: Settings
    var logs: [LogEntry]
    var backups: [BackupInfo]

    static let empty = SundyData(
        calendars: [SundyCalendar.defaultCalendar],
        events: [],
        settings: .default,
        logs: [],
        backups: []
    )
}

final class SundyDataStore {
    private let fileURL: URL

    init(fileManager: FileManager = .default) {
        let baseURL = fileManager.sundyAppSupportDirectory
        fileManager.ensureDirectoryExists(at: baseURL)
        fileURL = baseURL.appendingPathComponent("sundy-data.json")
    }

    func load() -> SundyData {
        guard FileManager.default.fileExists(atPath: fileURL.path) else {
            return .empty
        }

        do {
            let data = try Data(contentsOf: fileURL)
            let decoder = JSONDecoder()
            decoder.dateDecodingStrategy = .iso8601
            return try decoder.decode(SundyData.self, from: data)
        } catch {
            return .empty
        }
    }

    func save(_ data: SundyData) {
        do {
            let encoder = JSONEncoder()
            encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
            encoder.dateEncodingStrategy = .iso8601
            let encoded = try encoder.encode(data)
            try encoded.write(to: fileURL, options: .atomic)
        } catch {
            // Best-effort persistence; ignore failures.
        }
    }
}

extension FileManager {
    var sundyAppSupportDirectory: URL {
        let base = urls(for: .applicationSupportDirectory, in: .userDomainMask).first ?? temporaryDirectory
        return base.appendingPathComponent("Sundy", isDirectory: true)
    }

    func ensureDirectoryExists(at url: URL) {
        if !fileExists(atPath: url.path) {
            try? createDirectory(at: url, withIntermediateDirectories: true)
        }
    }
}
