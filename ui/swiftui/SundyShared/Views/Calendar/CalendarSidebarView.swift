import SwiftUI

struct CalendarSidebarView: View {
    @EnvironmentObject private var store: SundyStore
    @Binding var showHiddenCalendars: Bool
    @Binding var renamingCalendar: SundyCalendar?
    @Binding var renameText: String
    @Binding var showRenameCalendarModal: Bool
    var onOpenSettings: () -> Void

    @State private var expandedGroups: Set<String> = []

    private var groupedCalendars: [(key: String, calendars: [SundyCalendar])] {
        let groups = Dictionary(grouping: store.calendars) { $0.accountId ?? "local" }
        return groups
            .map { key, calendars in
                (key: key, calendars: calendars.sorted(by: { $0.name < $1.name }))
            }
            .sorted { lhs, rhs in
                if lhs.key == "local" { return true }
                if rhs.key == "local" { return false }
                return lhs.key < rhs.key
            }
    }

    var body: some View {
        VStack(spacing: 0) {
            Rectangle()
                .fill(Color.clear)
                .frame(height: 28)

            HStack {
                Spacer()
                Button {
                    // Sidebar toggle handled by parent
                } label: {
                    Image(systemName: "sidebar.left")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .opacity(0)
            }
            .padding(.top, 8)
            .padding(.horizontal, 16)

            HStack {
                Text("My Calendars")
                    .font(ThemeTypography.body(16))
                    .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                Spacer()
            }
            .padding(.horizontal, 20)
            .padding(.bottom, 12)
            .padding(.top, 4)
            .overlay(
                Rectangle()
                    .fill(ThemeColors.border.swiftUIColor)
                    .frame(height: 1),
                alignment: .bottom
            )

            ScrollView {
                VStack(alignment: .leading, spacing: 12) {
                    ForEach(groupedCalendars, id: \ .key) { group in
                        CalendarGroupView(
                            groupKey: group.key,
                            calendars: group.calendars,
                            expandedGroups: $expandedGroups,
                            showHiddenCalendars: $showHiddenCalendars,
                            renamingCalendar: $renamingCalendar,
                            renameText: $renameText,
                            showRenameCalendarModal: $showRenameCalendarModal
                        )
                    }

                    if hiddenCalendarCount > 0 {
                        Button {
                            showHiddenCalendars.toggle()
                        } label: {
                            Text(showHiddenCalendars ? "Hide system calendars" : "Show hidden calendars (\(hiddenCalendarCount))")
                                .font(ThemeTypography.body(12))
                                .foregroundStyle(ThemeColors.textLightMuted.swiftUIColor)
                                .frame(maxWidth: .infinity)
                                .padding(.vertical, 8)
                                .background(
                                    RoundedRectangle(cornerRadius: 6)
                                        .strokeBorder(ThemeColors.border.swiftUIColor, style: StrokeStyle(lineWidth: 1, dash: [4]))
                                )
                        }
                        .buttonStyle(.plain)
                    }
                }
                .padding(.horizontal, 16)
                .padding(.vertical, 12)
            }

            Button {
                onOpenSettings()
            } label: {
                HStack(spacing: 8) {
                    Image(systemName: "gearshape")
                    Text("Settings")
                        .font(ThemeTypography.body(14))
                }
                .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
                .frame(maxWidth: .infinity)
                .padding(8)
                .background(
                    RoundedRectangle(cornerRadius: 6)
                        .fill(Color.clear)
                )
            }
            .buttonStyle(.plain)
            .padding(16)
            .background(
                Rectangle()
                    .fill(ThemeColors.border.swiftUIColor)
                    .frame(height: 1),
                alignment: .top
            )
        }
        .background(ThemeColors.surface.swiftUIColor)
        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
        .onAppear {
            if expandedGroups.isEmpty {
                expandedGroups = Set(groupedCalendars.map { $0.key })
            }
        }
    }

    private var hiddenCalendarCount: Int {
        store.calendars.filter { $0.isHidden }.count
    }
}

struct CalendarGroupView: View {
    @EnvironmentObject private var store: SundyStore
    let groupKey: String
    let calendars: [SundyCalendar]
    @Binding var expandedGroups: Set<String>
    @Binding var showHiddenCalendars: Bool
    @Binding var renamingCalendar: SundyCalendar?
    @Binding var renameText: String
    @Binding var showRenameCalendarModal: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Button {
                toggleGroup()
            } label: {
                HStack(spacing: 8) {
                    Text(isExpanded ? "▼" : "►")
                        .font(ThemeTypography.body(10))
                    Text(groupTitle)
                        .font(ThemeTypography.body(12))
                        .foregroundStyle(ThemeColors.textUppercase.swiftUIColor)
                    Text("(\(visibleCalendars.count))")
                        .font(ThemeTypography.body(12))
                        .foregroundStyle(ThemeColors.textDarkMuted.swiftUIColor)
                    Spacer()
                }
                .padding(.vertical, 6)
                .padding(.horizontal, 8)
                .background(
                    RoundedRectangle(cornerRadius: 6)
                        .fill(ThemeColors.surfaceAlt.swiftUIColor.opacity(0.4))
                )
            }
            .buttonStyle(.plain)

            if isExpanded {
                ForEach(filteredCalendars) { calendar in
                    CalendarRowView(calendar: calendar) {
                        renamingCalendar = calendar
                        renameText = calendar.effectiveName
                        showRenameCalendarModal = true
                    }
                }
            }
        }
    }

    private var groupTitle: String {
        if groupKey == "local" {
            return "Local"
        }
        if store.settings.privacyMode && store.settings.privacyHideEmails {
            return maskEmail(store.calendars.first(where: { $0.accountId == groupKey })?.accountEmail ?? groupKey)
        }
        return store.calendars.first(where: { $0.accountId == groupKey })?.accountEmail ?? groupKey
    }

    private var isExpanded: Bool {
        expandedGroups.contains(groupKey)
    }

    private var filteredCalendars: [SundyCalendar] {
        calendars.filter { !($0.isHidden && !showHiddenCalendars) }
    }

    private var visibleCalendars: [SundyCalendar] {
        filteredCalendars
    }

    private func toggleGroup() {
        if isExpanded {
            expandedGroups.remove(groupKey)
        } else {
            expandedGroups.insert(groupKey)
        }
    }

    private func maskEmail(_ email: String) -> String {
        let parts = email.split(separator: "@")
        guard parts.count == 2 else { return "***" }
        let local = parts[0].prefix(1)
        let domainParts = parts[1].split(separator: ".")
        let tld = domainParts.last ?? "***"
        return "\(local)***@***.\(tld)"
    }
}

struct CalendarRowView: View {
    @EnvironmentObject private var store: SundyStore
    let calendar: SundyCalendar
    var onRename: () -> Void

    var body: some View {
        HStack(spacing: 12) {
            Button {
                store.toggleCalendarVisibility(id: calendar.id)
            } label: {
                Image(systemName: calendar.isVisible ? "checkmark.square.fill" : "square")
                    .foregroundStyle(ThemeColors.accent.swiftUIColor)
            }
            .buttonStyle(.plain)

            Circle()
                .fill(calendar.color.swiftUIColor)
                .frame(width: 10, height: 10)

            Text(calendar.effectiveName)
                .font(ThemeTypography.body(14))
                .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                .lineLimit(1)

            Spacer()
        }
        .padding(.vertical, 6)
        .padding(.horizontal, 4)
        .background(
            RoundedRectangle(cornerRadius: 6)
                .fill(ThemeColors.surfaceAlt.swiftUIColor.opacity(calendar.isHidden ? 0.2 : 0.0))
        )
        .contextMenu {
            Button("Rename") {
                onRename()
            }
        }
    }
}
