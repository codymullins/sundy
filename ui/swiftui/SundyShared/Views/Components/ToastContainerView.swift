import SwiftUI

struct ToastContainerView: View {
    @EnvironmentObject private var store: SundyStore

    var body: some View {
        VStack(spacing: 8) {
            ForEach(store.toasts) { toast in
                ToastView(toast: toast)
                    .environmentObject(store)
            }
        }
        .padding(.bottom, 16)
        .padding(.horizontal, 16)
        .frame(maxWidth: 500)
        .frame(maxWidth: .infinity)
        .frame(maxHeight: .infinity, alignment: .bottom)
    }
}

private struct ToastView: View {
    @EnvironmentObject private var store: SundyStore
    let toast: ToastItem

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: iconName)
                .foregroundStyle(iconColor)
            Text(toast.message)
                .font(ThemeTypography.body(14))
                .foregroundStyle(ThemeColors.textSecondary.swiftUIColor)
            Spacer()
            Button {
                store.dismissToast(id: toast.id)
            } label: {
                Image(systemName: "xmark")
                    .foregroundStyle(ThemeColors.textMuted.swiftUIColor)
            }
            .buttonStyle(.plain)
        }
        .padding(12)
        .background(
            RoundedRectangle(cornerRadius: 8)
                .fill(backgroundColor)
                .overlay(
                    RoundedRectangle(cornerRadius: 8)
                        .stroke(ThemeColors.border.swiftUIColor, lineWidth: 1)
                )
        )
        .onAppear {
            DispatchQueue.main.asyncAfter(deadline: .now() + 4) {
                store.dismissToast(id: toast.id)
            }
        }
        .allowsHitTesting(true)
    }

    private var backgroundColor: Color {
        switch toast.type {
        case .error:
            return ThemeColors.toastErrorBackground.swiftUIColor
        case .success:
            return ThemeColors.toastSuccessBackground.swiftUIColor
        case .info:
            return ThemeColors.toastInfoBackground.swiftUIColor
        }
    }

    private var iconName: String {
        switch toast.type {
        case .error:
            return "xmark.circle"
        case .success:
            return "checkmark.circle"
        case .info:
            return "info.circle"
        }
    }

    private var iconColor: Color {
        switch toast.type {
        case .error:
            return ThemeColors.errorAlt.swiftUIColor
        case .success:
            return ThemeColors.successAlt.swiftUIColor
        case .info:
            return ThemeColors.infoAlt.swiftUIColor
        }
    }
}
