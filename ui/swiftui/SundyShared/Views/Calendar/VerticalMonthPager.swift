import SwiftUI

private enum MonthKeyFormatter {
    static let shared: DateFormatter = {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM"
        formatter.locale = Locale(identifier: "en_US_POSIX")
        return formatter
    }()
}

struct VerticalMonthPager<Content: View>: View {
    @Binding var currentDate: Date
    let content: (Date) -> Content

    @State private var dragOffset: CGFloat = 0
    @State private var isTransitioning = false
    @State private var dragAxis: DragAxis?

    private let transitionAnimation = Animation.easeOut(duration: 0.25)
    private let transitionDuration: TimeInterval = 0.25
    private let swipeThresholdRatio: CGFloat = 0.2
    private let axisLockThreshold: CGFloat = 6
    private let dragMinimumDistance: CGFloat = 1

    private enum DragAxis {
        case vertical
        case horizontal
    }

    var body: some View {
        GeometryReader { proxy in
            let height = max(proxy.size.height, 1)
            let width = max(proxy.size.width, 1)
            ZStack {
                content(previousDate)
                    .id("prev-\(monthKey(for: previousDate))")
                    .frame(width: width, height: height)
                    .offset(y: dragOffset - height)
                content(currentDate)
                    .id("current-\(monthKey(for: currentDate))")
                    .frame(width: width, height: height)
                    .offset(y: dragOffset)
                content(nextDate)
                    .id("next-\(monthKey(for: nextDate))")
                    .frame(width: width, height: height)
                    .offset(y: dragOffset + height)
            }
            .clipped()
            .contentShape(Rectangle())
            .gesture(
                DragGesture(minimumDistance: dragMinimumDistance, coordinateSpace: .local)
                    .onChanged { value in
                        handleDragChanged(value, height: height)
                    }
                    .onEnded { value in
                        handleDragEnded(value, height: height)
                    }
            )
        }
    }

    private var previousDate: Date {
        shiftMonth(by: -1)
    }

    private var nextDate: Date {
        shiftMonth(by: 1)
    }

    private func shiftMonth(by value: Int) -> Date {
        Calendar.current.date(byAdding: .month, value: value, to: currentDate) ?? currentDate
    }

    private func monthKey(for date: Date) -> String {
        MonthKeyFormatter.shared.string(from: date)
    }

    private func handleDragChanged(_ value: DragGesture.Value, height: CGFloat) {
        guard !isTransitioning else { return }
        let translation = value.translation

        if dragAxis == nil {
            if abs(translation.height) < axisLockThreshold && abs(translation.width) < axisLockThreshold {
                return
            }
            dragAxis = abs(translation.height) >= abs(translation.width) ? .vertical : .horizontal
        }

        guard dragAxis == .vertical else { return }
        dragOffset = resistedOffset(translation.height, limit: height)
    }

    private func handleDragEnded(_ value: DragGesture.Value, height: CGFloat) {
        defer { dragAxis = nil }
        guard !isTransitioning else { return }
        guard dragAxis == .vertical else {
            if dragOffset != 0 {
                withAnimation(transitionAnimation) {
                    dragOffset = 0
                }
            }
            return
        }

        let threshold = height * swipeThresholdRatio
        let translation = value.translation.height
        let predicted = value.predictedEndTranslation.height

        if predicted < -threshold || translation < -threshold {
            transition(to: shiftMonth(by: 1), finalOffset: -height)
        } else if predicted > threshold || translation > threshold {
            transition(to: shiftMonth(by: -1), finalOffset: height)
        } else {
            withAnimation(transitionAnimation) {
                dragOffset = 0
            }
        }
    }

    private func transition(to date: Date, finalOffset: CGFloat) {
        isTransitioning = true
        withAnimation(transitionAnimation) {
            dragOffset = finalOffset
        }
        DispatchQueue.main.asyncAfter(deadline: .now() + transitionDuration) {
            let transaction = Transaction(animation: nil)
            withTransaction(transaction) {
                currentDate = date
                dragOffset = 0
            }
            isTransitioning = false
        }
    }

    private func resistedOffset(_ value: CGFloat, limit: CGFloat) -> CGFloat {
        let clamped = max(-limit, min(limit, value))
        let overflow = value - clamped
        return clamped + overflow * 0.2
    }

}
