import SwiftUI

struct ModalOverlayView<Content: View>: View {
    @ViewBuilder var content: Content

    var body: some View {
        ZStack {
            Color.black.opacity(0.6)
                .ignoresSafeArea()
            content
                .padding(16)
        }
        .transition(.opacity)
    }
}
