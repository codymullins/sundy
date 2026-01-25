import SwiftUI

@main
struct SundySwiftUIMacApp: App {
    var body: some Scene {
        WindowGroup {
            SundyAppRootView()
                .preferredColorScheme(.dark)
                .frame(minWidth: 900, minHeight: 600)
        }
    }
}
