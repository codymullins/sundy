import Foundation

struct Settings: Codable, Equatable {
    var privacyMode: Bool
    var privacyHideEmails: Bool
    var privacyHideEventTitles: Bool
    var collapsePastEvents: Bool
    var dynamicViewEnabled: Bool
    var showStatusBar: Bool
    var demoMode: Bool
    var demoBannerDismissed: Bool
    var telemetryEnabled: Bool
    var remindersEnabled: Bool
    var defaultReminderMinutes: Int
    var notificationPreference: String
    var syncIntervalMinutes: Int
    var autoBackupEnabled: Bool
    var backupIntervalDays: Int
    var retentionCount: Int
    var sidebarOpen: Bool

    static let `default` = Settings(
        privacyMode: false,
        privacyHideEmails: false,
        privacyHideEventTitles: false,
        collapsePastEvents: false,
        dynamicViewEnabled: true,
        showStatusBar: false,
        demoMode: false,
        demoBannerDismissed: false,
        telemetryEnabled: false,
        remindersEnabled: true,
        defaultReminderMinutes: 15,
        notificationPreference: "os_only",
        syncIntervalMinutes: 5,
        autoBackupEnabled: false,
        backupIntervalDays: 1,
        retentionCount: 10,
        sidebarOpen: true
    )
}
