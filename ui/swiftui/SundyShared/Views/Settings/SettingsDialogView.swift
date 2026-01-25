import SwiftUI
import UserNotifications

struct SettingsDialogView: View {
    @EnvironmentObject private var store: SundyStore
    var onClose: () -> Void

    @State private var activeTab: SettingsTab = .general
    @State private var showNewCalendarForm = false
    @State private var newCalendarName = ""
    @State private var newCalendarColor: Color = ThemeColors.calendarDefault.swiftUIColor
    @State private var showResetConfirmation = false
    @State private var editingCalendar: SundyCalendar?
    @State private var editCalendarName = ""

    @State private var notificationStatus: UNAuthorizationStatus = .notDetermined
    @State private var isRequestingPermission = false
    @State private var isSendingTestNotification = false

    var body: some View {
        VStack(spacing: 0) {
            HStack {
                Text("Settings")
                    .font(ThemeTypography.body(18))
                    .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                Spacer()
                Button(action: onClose) {
                    Image(systemName: "xmark")
                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                        .frame(width: 28, height: 28)
                        .background(
                            RoundedRectangle(cornerRadius: 6)
                                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                        )
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .background(
                Rectangle()
                    .fill(ThemeColors.surface.swiftUIColor)
                    .overlay(
                        Rectangle().fill(ThemeColors.border.swiftUIColor).frame(height: 1),
                        alignment: .bottom
                    )
            )

            HStack(spacing: 0) {
                SettingsSidebarView(activeTab: $activeTab)
                settingsContent
            }
        }
        .frame(maxWidth: 760, maxHeight: 540)
        .background(
            RoundedRectangle(cornerRadius: 12)
                .fill(ThemeColors.surfaceAlt.swiftUIColor)
                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                )
        )
        .shadow(color: Color.black.opacity(0.4), radius: 40, x: 0, y: 16)
        .onAppear {
            store.refreshBackups()
            Task { await loadNotificationStatus() }
        }
    }

    @ViewBuilder
    private var settingsContent: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 24) {
                switch activeTab {
                case .general:
                    generalTab
                case .integrations:
                    integrationsTab
                case .sync:
                    syncTab
                case .backup:
                    backupTab
                case .privacy:
                    privacyTab
                case .notifications:
                    notificationsTab
                case .advanced:
                    advancedTab
                }
            }
            .padding(24)
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .background(ThemeColors.background.swiftUIColor)
    }

    private var generalTab: some View {
        VStack(alignment: .leading, spacing: 24) {
            SettingsSection(title: "Calendars") {
                VStack(spacing: 8) {
                    ForEach(store.calendars) { calendar in
                        HStack {
                            HStack(spacing: 12) {
                                Circle()
                                    .fill(calendar.color.swiftUIColor)
                                    .frame(width: 12, height: 12)
                                Text(calendar.effectiveName)
                                    .font(ThemeTypography.body(14))
                                    .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                            }
                            Spacer()
                            HStack(spacing: 6) {
                                Button {
                                    editingCalendar = calendar
                                    editCalendarName = calendar.effectiveName
                                } label: {
                                    Image(systemName: "pencil")
                                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                                }
                                .buttonStyle(.plain)

                                Button {
                                    store.deleteCalendar(id: calendar.id)
                                    store.showToast(message: "Calendar deleted.", type: .info)
                                } label: {
                                    Image(systemName: "trash")
                                        .foregroundStyle(ThemeColors.error.swiftUIColor)
                                }
                                .buttonStyle(.plain)
                            }
                        }
                        .padding(12)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(ThemeColors.surface.swiftUIColor)
                        )
                    }
                }

                if let calendar = editingCalendar {
                    VStack(alignment: .leading, spacing: 12) {
                        Text("Rename Calendar")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)

                        HStack(spacing: 12) {
                            Circle()
                                .fill(calendar.color.swiftUIColor)
                                .frame(width: 12, height: 12)
                            TextField("Calendar name", text: $editCalendarName)
                                .textFieldStyle(.plain)
                                .padding(10)
                                .background(
                                    RoundedRectangle(cornerRadius: 8)
                                        .fill(ThemeColors.surface.swiftUIColor)
                                        .overlay(
                                            RoundedRectangle(cornerRadius: 8)
                                                .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                                        )
                                )
                        }

                        HStack(spacing: 8) {
                            Button("Cancel") {
                                editingCalendar = nil
                                editCalendarName = ""
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.surfaceElevated.swiftUIColor))

                            Button("Save") {
                                if let target = editingCalendar {
                                    store.renameCalendar(id: target.id, newName: editCalendarName)
                                    store.showToast(message: "Calendar renamed.", type: .success)
                                }
                                editingCalendar = nil
                                editCalendarName = ""
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(Color.white)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.accent.swiftUIColor))
                        }
                    }
                    .padding(16)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(ThemeColors.surface.swiftUIColor)
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                            )
                    )
                }

                if showNewCalendarForm {
                    VStack(spacing: 12) {
                        HStack(spacing: 12) {
                            ColorPicker("", selection: $newCalendarColor)
                                .labelsHidden()
                                .frame(width: 40, height: 40)
                            TextField("Calendar name", text: $newCalendarName)
                                .textFieldStyle(.plain)
                                .padding(10)
                                .background(
                                    RoundedRectangle(cornerRadius: 8)
                                        .fill(ThemeColors.surface.swiftUIColor)
                                        .overlay(
                                            RoundedRectangle(cornerRadius: 8)
                                                .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                                        )
                                )
                        }

                        HStack(spacing: 8) {
                            Button("Cancel") {
                                showNewCalendarForm = false
                                newCalendarName = ""
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.surfaceElevated.swiftUIColor))

                            Button("Create") {
                                guard !newCalendarName.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else { return }
                                store.addCalendar(name: newCalendarName, color: newCalendarColor.toColorValue())
                                store.showToast(message: "Calendar created.", type: .success)
                                showNewCalendarForm = false
                                newCalendarName = ""
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(Color.white)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.accent.swiftUIColor))
                        }
                    }
                    .padding(16)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(ThemeColors.surface.swiftUIColor)
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                            )
                    )
                } else {
                    Button {
                        showNewCalendarForm = true
                        newCalendarName = ""
                    } label: {
                        HStack(spacing: 8) {
                            Image(systemName: "plus")
                            Text("Add Calendar")
                        }
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                        .frame(maxWidth: .infinity)
                        .padding(12)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(ThemeColors.border.swiftUIColor, style: StrokeStyle(lineWidth: 1, dash: [4]))
                        )
                    }
                    .buttonStyle(.plain)
                }
            }

            SettingsSection(title: "Data") {
                if showResetConfirmation {
                    VStack(alignment: .leading, spacing: 12) {
                        Text("Are you sure you want to reset the database? This will delete all calendars and events.")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        HStack(spacing: 8) {
                            Button("Cancel") {
                                showResetConfirmation = false
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.surfaceElevated.swiftUIColor))

                            Button("Reset") {
                                store.resetData()
                                store.showToast(message: "Database reset.", type: .info)
                                showResetConfirmation = false
                            }
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(Color.white)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 8)
                            .background(Capsule().fill(ThemeColors.danger.swiftUIColor))
                        }
                    }
                } else {
                    Button {
                        showResetConfirmation = true
                    } label: {
                        HStack(spacing: 8) {
                            Image(systemName: "arrow.counterclockwise")
                            Text("Reset Database")
                        }
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                        .frame(maxWidth: .infinity)
                        .padding(12)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(ThemeColors.surface.swiftUIColor)
                        )
                    }
                    .buttonStyle(.plain)
                }
            }
        }
    }

    private var integrationsTab: some View {
        SettingsSection(title: "Microsoft Accounts") {
            Text("Outlook integration is not available in the SwiftUI build yet.")
                .font(ThemeTypography.body(13))
                .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
        }
    }

    private var syncTab: some View {
        SettingsSection(title: "Remote Sync") {
            Text("Remote sync is not available in the SwiftUI build yet.")
                .font(ThemeTypography.body(13))
                .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
        }
    }

    private var backupTab: some View {
        VStack(alignment: .leading, spacing: 16) {
            SettingsSection(title: "Local Backups") {
                Button {
                    store.createBackup()
                } label: {
                    HStack(spacing: 8) {
                        Image(systemName: "tray.and.arrow.down")
                        Text("Create Backup")
                    }
                    .font(ThemeTypography.body(14))
                    .foregroundStyle(Color.white)
                    .padding(.horizontal, 16)
                    .padding(.vertical, 10)
                    .background(Capsule().fill(ThemeColors.accent.swiftUIColor))
                }
                .buttonStyle(.plain)

                if !store.backups.isEmpty {
                    VStack(alignment: .leading, spacing: 8) {
                        Text("Recent Backups")
                            .font(ThemeTypography.body(13))
                            .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                        ForEach(store.backups.prefix(5)) { backup in
                            HStack {
                                VStack(alignment: .leading, spacing: 2) {
                                    Text(backup.createdAt, format: .dateTime.month().day().year().hour().minute())
                                        .font(ThemeTypography.body(14))
                                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                                    Text(formatSize(backup.sizeBytes))
                                        .font(ThemeTypography.body(12))
                                        .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                                }
                                Spacer()
                                Button {
                                    store.deleteBackup(id: backup.id)
                                } label: {
                                    Image(systemName: "trash")
                                        .foregroundStyle(ThemeColors.error.swiftUIColor)
                                }
                                .buttonStyle(.plain)
                            }
                            .padding(12)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(ThemeColors.surface.swiftUIColor)
                            )
                        }
                    }
                }
            }

            SettingsSection(title: "Automatic Backups") {
                Toggle("Enable automatic backups", isOn: Binding(
                    get: { store.settings.autoBackupEnabled },
                    set: { newValue in
                        store.updateSettings { $0.autoBackupEnabled = newValue }
                    }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                if store.settings.autoBackupEnabled {
                    HStack {
                        Text("Backup Frequency")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        Spacer()
                        Picker("", selection: Binding(
                            get: { store.settings.backupIntervalDays },
                            set: { newValue in store.updateSettings { $0.backupIntervalDays = newValue } }
                        )) {
                            Text("Daily").tag(1)
                            Text("Weekly").tag(7)
                            Text("Monthly").tag(30)
                        }
                        .pickerStyle(.menu)
                    }

                    HStack {
                        Text("Keep Last")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        Spacer()
                        Picker("", selection: Binding(
                            get: { store.settings.retentionCount },
                            set: { newValue in store.updateSettings { $0.retentionCount = newValue } }
                        )) {
                            Text("5").tag(5)
                            Text("10").tag(10)
                            Text("20").tag(20)
                            Text("50").tag(50)
                        }
                        .pickerStyle(.menu)
                    }
                }
            }
        }
    }

    private var privacyTab: some View {
        VStack(alignment: .leading, spacing: 16) {
            SettingsSection(title: "Telemetry") {
                Text("Sundy uses telemetry to improve stability. No personal calendar data is ever sent.")
                    .font(ThemeTypography.body(13))
                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
                Toggle("Enable telemetry", isOn: Binding(
                    get: { store.settings.telemetryEnabled },
                    set: { newValue in
                        store.updateSettings { $0.telemetryEnabled = newValue }
                    }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))
            }

            SettingsSection(title: "Display Privacy") {
                Toggle("Privacy Mode", isOn: Binding(
                    get: { store.settings.privacyMode },
                    set: { newValue in
                        store.updateSettings { $0.privacyMode = newValue }
                    }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                if store.settings.privacyMode {
                    Toggle("Hide email addresses", isOn: Binding(
                        get: { store.settings.privacyHideEmails },
                        set: { newValue in store.updateSettings { $0.privacyHideEmails = newValue } }
                    ))
                    .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                    Toggle("Hide event titles", isOn: Binding(
                        get: { store.settings.privacyHideEventTitles },
                        set: { newValue in store.updateSettings { $0.privacyHideEventTitles = newValue } }
                    ))
                    .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))
                }
            }
        }
    }

    private var notificationsTab: some View {
        SettingsSection(title: "Event Reminders") {
            if notificationStatus != .authorized {
                Button {
                    requestNotificationPermission()
                } label: {
                    Text(isRequestingPermission ? "Requesting..." : "Enable Notifications")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(Color.white)
                        .padding(.horizontal, 16)
                        .padding(.vertical, 10)
                        .background(Capsule().fill(ThemeColors.accent.swiftUIColor))
                }
                .buttonStyle(.plain)
            } else {
                Toggle("Enable event reminders", isOn: Binding(
                    get: { store.settings.remindersEnabled },
                    set: { newValue in store.updateSettings { $0.remindersEnabled = newValue } }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                if store.settings.remindersEnabled {
                    HStack {
                        Text("Default Reminder Time")
                            .font(ThemeTypography.body(14))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                        Spacer()
                        Picker("", selection: Binding(
                            get: { store.settings.defaultReminderMinutes },
                            set: { newValue in store.updateSettings { $0.defaultReminderMinutes = newValue } }
                        )) {
                            Text("At start").tag(0)
                            Text("5 minutes before").tag(5)
                            Text("10 minutes before").tag(10)
                            Text("15 minutes before").tag(15)
                            Text("30 minutes before").tag(30)
                            Text("1 hour before").tag(60)
                        }
                        .pickerStyle(.menu)
                    }
                }

                Button {
                    sendTestNotification()
                } label: {
                    Text(isSendingTestNotification ? "Sending..." : "Send Test Notification")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(Color.white)
                        .padding(.horizontal, 16)
                        .padding(.vertical, 10)
                        .background(Capsule().fill(ThemeColors.accent.swiftUIColor))
                }
                .buttonStyle(.plain)
            }
        }
    }

    private var advancedTab: some View {
        VStack(alignment: .leading, spacing: 16) {
            SettingsSection(title: "Outlook Sync") {
                Text("Outlook sync is not available in the SwiftUI build yet.")
                    .font(ThemeTypography.body(13))
                    .foregroundStyle(ThemeColors.textSubtle.swiftUIColor)
            }

            SettingsSection(title: "Developer Options") {
                Toggle("Collapse past events in month view", isOn: Binding(
                    get: { store.settings.collapsePastEvents },
                    set: { newValue in store.updateSettings { $0.collapsePastEvents = newValue } }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))

                Toggle("Enable dynamic month view", isOn: Binding(
                    get: { store.settings.dynamicViewEnabled },
                    set: { newValue in store.updateSettings { $0.dynamicViewEnabled = newValue } }
                ))
                .toggleStyle(SwitchToggleStyle(tint: ThemeColors.accent.swiftUIColor))
            }
        }
    }

    private func formatSize(_ bytes: Int64) -> String {
        if bytes < 1024 { return "\(bytes) B" }
        if bytes < 1024 * 1024 { return String(format: "%.1f KB", Double(bytes) / 1024.0) }
        return String(format: "%.1f MB", Double(bytes) / (1024.0 * 1024.0))
    }

    private func loadNotificationStatus() async {
        notificationStatus = await store.notificationAuthorizationStatus()
    }

    private func requestNotificationPermission() {
        isRequestingPermission = true
        Task {
            let granted = await store.requestNotificationPermission()
            notificationStatus = granted ? .authorized : .denied
            isRequestingPermission = false
        }
    }

    private func sendTestNotification() {
        isSendingTestNotification = true
        store.sendTestNotification()
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
            isSendingTestNotification = false
        }
    }
}

enum SettingsTab: String, CaseIterable, Identifiable {
    case general
    case integrations
    case sync
    case backup
    case privacy
    case notifications
    case advanced

    var id: String { rawValue }

    var title: String {
        switch self {
        case .general: return "General"
        case .integrations: return "Integrations"
        case .sync: return "Sync"
        case .backup: return "Backup"
        case .privacy: return "Privacy"
        case .notifications: return "Notifications"
        case .advanced: return "Advanced"
        }
    }

    var icon: String {
        switch self {
        case .general: return "gearshape"
        case .integrations: return "square.on.square"
        case .sync: return "arrow.triangle.2.circlepath"
        case .backup: return "tray.and.arrow.down"
        case .privacy: return "lock"
        case .notifications: return "bell"
        case .advanced: return "wrench"
        }
    }
}

private struct SettingsSidebarView: View {
    @Binding var activeTab: SettingsTab

    var body: some View {
        VStack(spacing: 0) {
            ForEach(SettingsTab.allCases) { tab in
                Button {
                    activeTab = tab
                } label: {
                    HStack(spacing: 10) {
                        Image(systemName: tab.icon)
                            .frame(width: 20)
                        Text(tab.title)
                            .font(ThemeTypography.body(14))
                        Spacer()
                    }
                    .foregroundStyle(activeTab == tab ? Color.white : ThemeColors.textMuted.swiftUIColor)
                    .padding(.vertical, 12)
                    .padding(.horizontal, 16)
                    .background(activeTab == tab ? ThemeColors.surfaceAlt.swiftUIColor : Color.clear)
                    .overlay(
                        Rectangle()
                            .fill(activeTab == tab ? ThemeColors.accent.swiftUIColor : Color.clear)
                            .frame(width: 3),
                        alignment: .leading
                    )
                }
                .buttonStyle(.plain)
            }
            Spacer()
        }
        .frame(width: 180)
        .background(ThemeColors.surface.swiftUIColor)
    }
}

private struct SettingsSection<Content: View>: View {
    var title: String
    @ViewBuilder var content: Content

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            Text(title.uppercased())
                .font(ThemeTypography.body(12))
                .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
            content
        }
    }
}
