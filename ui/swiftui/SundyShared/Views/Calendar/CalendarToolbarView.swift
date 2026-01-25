import SwiftUI

struct CalendarToolbarView: View {
    var currentDate: Date
    var currentView: CalendarViewType
    var isCompact: Bool
    var showDynamicView: Bool
    @Binding var sidebarOpen: Bool
    @Binding var showViewDropdown: Bool
    var onToday: () -> Void
    var onPrev: () -> Void
    var onNext: () -> Void
    var onSelectView: (CalendarViewType) -> Void
    var onNewEvent: () -> Void

    var body: some View {
        HStack {
            HStack(spacing: 8) {
                Button {
                    withAnimation(.easeInOut) {
                        sidebarOpen.toggle()
                    }
                } label: {
                    Image(systemName: "line.3.horizontal")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .frame(width: 36, height: 36)
                .background(
                    Circle().fill(ThemeColors.surfaceAlt.swiftUIColor)
                )

                #if !os(iOS)
                Button(action: onToday) {
                    Text("Today")
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .padding(.horizontal, 16)
                .frame(height: 36)
                .background(
                    Capsule().fill(ThemeColors.surfaceAlt.swiftUIColor)
                )

                Button(action: onPrev) {
                    Image(systemName: "chevron.left")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .frame(width: 36, height: 36)
                .background(
                    Circle().fill(ThemeColors.surfaceAlt.swiftUIColor)
                )

                Button(action: onNext) {
                    Image(systemName: "chevron.right")
                        .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .frame(width: 36, height: 36)
                .background(
                    Circle().fill(ThemeColors.surfaceAlt.swiftUIColor)
                )
                #endif

                Text(DateUtils.headerTitle(for: currentDate, view: currentView))
                    .font(ThemeTypography.body(20))
                    .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                    .lineLimit(1)
                    .padding(.leading, 8)
            }

            Spacer()

            HStack(spacing: 12) {
                if isCompact {
                    Menu {
                        ForEach(availableViews) { view in
                            Button(view.displayName) {
                                onSelectView(view)
                            }
                        }
                    } label: {
                        Image(systemName: iconName(for: currentView))
                            .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
                            .frame(width: 36, height: 36)
                            .background(Circle().fill(ThemeColors.surfaceAlt.swiftUIColor))
                    }
                } else {
                    ViewToggleView(currentView: currentView, availableViews: availableViews, onSelectView: onSelectView)
                }

                #if !os(iOS)
                Button(action: onNewEvent) {
                    HStack(spacing: 8) {
                        Image(systemName: "plus.circle")
                        if !isCompact {
                            Text("New Event")
                                .font(ThemeTypography.body(14))
                        }
                    }
                    .foregroundStyle(ThemeColors.textPrimary.swiftUIColor)
                }
                .buttonStyle(.plain)
                .padding(.horizontal, isCompact ? 10 : 16)
                .frame(height: 36)
                .background(
                    Capsule().fill(ThemeColors.accent.swiftUIColor)
                )
                #endif
            }
        }
        .padding(.horizontal, 20)
        .frame(height: 60)
        .background(ThemeColors.surface.swiftUIColor)
        .overlay(
            Rectangle()
                .fill(ThemeColors.border.swiftUIColor)
                .frame(height: 1),
            alignment: .bottom
        )
    }

    private func iconName(for view: CalendarViewType) -> String {
        switch view {
        case .day: return "calendar"
        case .week: return "calendar.badge.clock"
        case .month: return "calendar"
        case .dynamic: return "calendar.day.timeline.left"
        }
    }

    private var availableViews: [CalendarViewType] {
        showDynamicView ? CalendarViewType.allCases : CalendarViewType.allCases.filter { $0 != .dynamic }
    }
}

struct ViewToggleView: View {
    var currentView: CalendarViewType
    var availableViews: [CalendarViewType]
    var onSelectView: (CalendarViewType) -> Void
    @Namespace private var sliderNamespace

    var body: some View {
        HStack(spacing: 4) {
            ForEach(availableViews) { view in
                Button {
                    onSelectView(view)
                } label: {
                    Text(view.displayName)
                        .font(ThemeTypography.body(14))
                        .foregroundStyle(currentView == view ? Color.white : ThemeColors.textMuted.swiftUIColor)
                        .padding(.vertical, 6)
                        .padding(.horizontal, 12)
                        .background(
                            ZStack {
                                if currentView == view {
                                    RoundedRectangle(cornerRadius: 14)
                                        .fill(
                                            LinearGradient(
                                                colors: [
                                                    ThemeColors.accentGradientStart.swiftUIColor,
                                                    ThemeColors.accentGradientEnd.swiftUIColor
                                                ],
                                                startPoint: .topLeading,
                                                endPoint: .bottomTrailing
                                            )
                                        )
                                        .matchedGeometryEffect(id: "slider", in: sliderNamespace)
                                }
                            }
                        )
                }
                .buttonStyle(.plain)
            }
        }
        .padding(4)
        .background(
            Capsule().fill(ThemeColors.surfaceAlt.swiftUIColor)
        )
    }
}
