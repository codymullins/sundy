import SwiftUI

struct CalendarShellView: View {
    @EnvironmentObject private var store: SundyStore
    @State private var currentDate = Date()
    @State private var currentView: CalendarViewType = .month
    @State private var sidebarOpen = false
    @State private var showHiddenCalendars = false
    @State private var showSettingsDialog = false
    @State private var showNewEventModal = false
    @State private var showSchedulerModal = false
    @State private var showViewDropdown = false
    @State private var showRenameCalendarModal = false
    @State private var renamingCalendar: SundyCalendar?
    @State private var renameText = ""
    @State private var editingEventId: UUID?
    @State private var eventDraft = EventDraft()

    var body: some View {
        GeometryReader { geometry in
            let isCompact = geometry.size.width < 640
            ZStack(alignment: .leading) {
                HStack(spacing: 0) {
                    // Show sidebar in HStack only on non-compact (desktop) layouts
                    if !isCompact && sidebarOpen {
                        CalendarSidebarView(
                            showHiddenCalendars: $showHiddenCalendars,
                            renamingCalendar: $renamingCalendar,
                            renameText: $renameText,
                            showRenameCalendarModal: $showRenameCalendarModal,
                            onOpenSettings: { showSettingsDialog = true }
                        )
                        .frame(width: 280)
                        .transition(.move(edge: .leading))
                    }

                    CalendarMainView(
                        currentDate: $currentDate,
                        currentView: $currentView,
                        isCompact: isCompact,
                        showDynamicView: store.settings.dynamicViewEnabled,
                        sidebarOpen: $sidebarOpen,
                        showViewDropdown: $showViewDropdown,
                        onNewEvent: openNewEvent,
                        onEventClick: openEditEvent
                    )
                }

                // Compact mode: overlay dims only the main content, then sidebar on top
                if isCompact && sidebarOpen {
                    Color.black.opacity(0.5)
                        .ignoresSafeArea()
                        .onTapGesture {
                            withAnimation(.easeInOut) {
                                sidebarOpen = false
                            }
                        }

                    CalendarSidebarView(
                        showHiddenCalendars: $showHiddenCalendars,
                        renamingCalendar: $renamingCalendar,
                        renameText: $renameText,
                        showRenameCalendarModal: $showRenameCalendarModal,
                        onOpenSettings: { showSettingsDialog = true }
                    )
                    .frame(width: 280)
                    .transition(.move(edge: .leading))
                }

                if showNewEventModal {
                    ModalOverlayView {
                        NewEventModalView(
                            draft: $eventDraft,
                            calendars: store.calendars,
                            isEditMode: editingEventId != nil,
                            onClose: closeNewEventModal,
                            onOpenScheduler: { showSchedulerModal = true },
                            onSave: saveEvent
                        )
                    }
                }

                if showSchedulerModal {
                    ModalOverlayView {
                        SchedulerModalView(
                            draft: $eventDraft,
                            onClose: { showSchedulerModal = false }
                        )
                    }
                }

                if showSettingsDialog {
                    ModalOverlayView {
                        SettingsDialogView(onClose: { showSettingsDialog = false })
                            .environmentObject(store)
                    }
                }

                if store.settings.demoMode && !store.settings.demoBannerDismissed {
                    DemoModeBannerView {
                        store.updateSettings { $0.demoBannerDismissed = true }
                    }
                }

                if showRenameCalendarModal, let calendar = renamingCalendar {
                    ModalOverlayView {
                        RenameCalendarModalView(
                            calendar: calendar,
                            renameText: $renameText,
                            onCancel: { showRenameCalendarModal = false },
                            onSave: {
                                store.renameCalendar(id: calendar.id, newName: renameText)
                                showRenameCalendarModal = false
                            }
                        )
                    }
                }
            }
            .onAppear {
                sidebarOpen = store.settings.sidebarOpen
                if !store.settings.dynamicViewEnabled && currentView == .dynamic {
                    currentView = .month
                }
            }
            .onChange(of: sidebarOpen) { newValue in
                store.updateSettings { $0.sidebarOpen = newValue }
            }
            .onChange(of: store.settings.dynamicViewEnabled) { enabled in
                if !enabled && currentView == .dynamic {
                    currentView = .month
                }
            }
        }
    }

    private func openNewEvent(date: Date? = nil) {
        editingEventId = nil
        let defaults = defaultEventTimes()
        eventDraft = EventDraft(
            title: "",
            date: date ?? Date(),
            startTime: defaults.start,
            endTime: defaults.end,
            details: "",
            isAllDay: false,
            calendarId: store.calendars.first?.id ?? SundyCalendar.defaultCalendar.id
        )
        showNewEventModal = true
    }

    private func openEditEvent(_ event: CalendarEvent) {
        editingEventId = event.id
        eventDraft = EventDraft(
            title: event.title,
            date: event.startTime.startOfDay,
            startTime: event.startTime,
            endTime: event.endTime,
            details: event.details,
            isAllDay: event.isAllDay,
            calendarId: event.calendarId
        )
        showNewEventModal = true
    }

    private func closeNewEventModal() {
        showNewEventModal = false
        editingEventId = nil
    }

    private func saveEvent() {
        guard !eventDraft.title.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            store.showToast(message: "Event title is required.", type: .error)
            return
        }

        if let editingId = editingEventId {
            store.updateEvent(id: editingId, from: eventDraft)
            store.showToast(message: "Event updated.", type: .success)
        } else {
            store.createEvent(from: eventDraft)
            store.showToast(message: "Event created.", type: .success)
        }
        closeNewEventModal()
    }

    private func defaultEventTimes() -> (start: Date, end: Date) {
        let now = Date()
        let calendar = Calendar.current
        let minute = calendar.component(.minute, from: now)
        var roundedMinutes = ((minute / 15) + 1) * 15
        var start = calendar.date(bySettingHour: calendar.component(.hour, from: now), minute: 0, second: 0, of: now) ?? now
        if roundedMinutes >= 60 {
            start = calendar.date(byAdding: .hour, value: 1, to: start) ?? start
            roundedMinutes = 0
        }
        start = calendar.date(bySettingHour: calendar.component(.hour, from: start), minute: roundedMinutes, second: 0, of: start) ?? start
        let end = calendar.date(byAdding: .hour, value: 1, to: start) ?? start.addingTimeInterval(3600)
        return (start, end)
    }
}

struct CalendarMainView: View {
    @EnvironmentObject private var store: SundyStore
    @Binding var currentDate: Date
    @Binding var currentView: CalendarViewType
    var isCompact: Bool
    var showDynamicView: Bool
    @Binding var sidebarOpen: Bool
    @Binding var showViewDropdown: Bool
    var onNewEvent: (Date?) -> Void
    var onEventClick: (CalendarEvent) -> Void

    var body: some View {
        ZStack(alignment: .bottomTrailing) {
            VStack(spacing: 0) {
                CalendarToolbarView(
                    currentDate: currentDate,
                    currentView: currentView,
                    isCompact: isCompact,
                    showDynamicView: showDynamicView,
                    sidebarOpen: $sidebarOpen,
                    showViewDropdown: $showViewDropdown,
                    onToday: goToToday,
                    onPrev: navigatePrevious,
                    onNext: navigateNext,
                    onSelectView: { view in
                        currentView = view
                    },
                    onNewEvent: { onNewEvent(nil) }
                )

                CalendarViewContainer(
                    currentDate: $currentDate,
                    currentView: currentView,
                    calendars: store.calendars,
                    events: store.events,
                    hideEventTitles: store.settings.privacyMode && store.settings.privacyHideEventTitles,
                    collapsePastEvents: store.settings.collapsePastEvents,
                    onDayClick: { date in
                        onNewEvent(date)
                    },
                    onEventClick: onEventClick
                )
            }

            #if os(iOS)
            VStack(spacing: 12) {
                if !DateUtils.isTodayVisible(for: currentDate, view: currentView) {
                    FloatingTodayButton(action: goToToday)
                }
                FloatingNewEventButton(action: { onNewEvent(nil) })
            }
            .padding(.trailing, 20)
            .padding(.bottom, 20)
            #endif
        }
        .background(ThemeColors.background.swiftUIColor)
    }

    private func goToToday() {
        currentDate = Date()
    }

    private func navigatePrevious() {
        switch currentView {
        case .day:
            currentDate = Calendar.current.date(byAdding: .day, value: -1, to: currentDate) ?? currentDate
        case .week:
            currentDate = Calendar.current.date(byAdding: .day, value: -7, to: currentDate) ?? currentDate
        case .month, .dynamic:
            currentDate = Calendar.current.date(byAdding: .month, value: -1, to: currentDate) ?? currentDate
        }
    }

    private func navigateNext() {
        switch currentView {
        case .day:
            currentDate = Calendar.current.date(byAdding: .day, value: 1, to: currentDate) ?? currentDate
        case .week:
            currentDate = Calendar.current.date(byAdding: .day, value: 7, to: currentDate) ?? currentDate
        case .month, .dynamic:
            currentDate = Calendar.current.date(byAdding: .month, value: 1, to: currentDate) ?? currentDate
        }
    }
}

private struct FloatingNewEventButton: View {
    var action: () -> Void

    var body: some View {
        Button(action: action) {
            Image(systemName: "plus")
                .font(.system(size: 20, weight: .semibold))
                .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                .frame(width: 56, height: 56)
                .background(
                    Circle().fill(
                        LinearGradient(
                            colors: [
                                ThemeColors.accentGradientStart.swiftUIColor,
                                ThemeColors.accentGradientEnd.swiftUIColor
                            ],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        )
                    )
                )
        }
        .buttonStyle(.plain)
        .shadow(color: ThemeColors.surfaceElevated.swiftUIColor.opacity(0.6), radius: 12, x: 0, y: 6)
        .accessibilityLabel("New Event")
    }
}

private struct FloatingTodayButton: View {
    var action: () -> Void

    var body: some View {
        Button(action: action) {
            Image(systemName: "calendar.badge.clock")
                .font(.system(size: 18, weight: .semibold))
                .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                .frame(width: 48, height: 48)
                .background(
                    Circle().fill(ThemeColors.surfaceElevated.swiftUIColor)
                )
        }
        .buttonStyle(.plain)
        .shadow(color: ThemeColors.surfaceElevated.swiftUIColor.opacity(0.4), radius: 8, x: 0, y: 4)
        .accessibilityLabel("Go to Today")
    }
}
