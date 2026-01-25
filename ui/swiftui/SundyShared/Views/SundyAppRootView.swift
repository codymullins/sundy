import SwiftUI

struct SundyAppRootView: View {
    @StateObject private var store = SundyStore()

    var body: some View {
        ZStack(alignment: .bottom) {
            CalendarShellView()
                .environmentObject(store)

            ToastContainerView()
                .environmentObject(store)
        }
        .background(ThemeColors.background.swiftUIColor)
    }
}
