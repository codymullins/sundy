import SwiftUI

struct HorizontalPager<Content: View>: View {
    @Binding var currentDate: Date
    let dateIncrement: DateIncrement
    let content: (Date) -> Content

    @State private var dragOffset: CGFloat = 0
    @State private var isTransitioning = false
    @State private var dragAxis: PagerLogic.Axis?

    private let logic = PagerLogic()
    private let transitionAnimation = Animation.easeOut(duration: 0.25)
    private let transitionDuration: TimeInterval = 0.25
    private let dragMinimumDistance: CGFloat = 1

    enum DateIncrement {
        case day
        case week

        var calendarComponent: Calendar.Component {
            switch self {
            case .day: return .day
            case .week: return .weekOfYear
            }
        }

        var value: Int {
            switch self {
            case .day: return 1
            case .week: return 1
            }
        }
    }

    var body: some View {
        GeometryReader { proxy in
            let width = max(proxy.size.width, 1)
            let height = max(proxy.size.height, 1)
            ZStack {
                content(previousDate)
                    .id("prev-\(dateKey(for: previousDate))")
                    .frame(width: width, height: height)
                    .offset(x: dragOffset - width)
                content(currentDate)
                    .id("current-\(dateKey(for: currentDate))")
                    .frame(width: width, height: height)
                    .offset(x: dragOffset)
                content(nextDate)
                    .id("next-\(dateKey(for: nextDate))")
                    .frame(width: width, height: height)
                    .offset(x: dragOffset + width)
            }
            .clipped()
            .contentShape(Rectangle())
            .simultaneousGesture(
                DragGesture(minimumDistance: dragMinimumDistance, coordinateSpace: .local)
                    .onChanged { value in
                        handleDragChanged(value, width: width)
                    }
                    .onEnded { value in
                        handleDragEnded(value, width: width)
                    }
            )
        }
    }

    private var previousDate: Date {
        shiftDate(by: -1)
    }

    private var nextDate: Date {
        shiftDate(by: 1)
    }

    private func shiftDate(by multiplier: Int) -> Date {
        let component = dateIncrement.calendarComponent
        let value = dateIncrement.value * multiplier
        return Calendar.current.date(byAdding: component, value: value, to: currentDate) ?? currentDate
    }

    private func dateKey(for date: Date) -> String {
        let formatter = ISO8601DateFormatter()
        formatter.formatOptions = [.withFullDate]
        return formatter.string(from: date)
    }

    private func handleDragChanged(_ value: DragGesture.Value, width: CGFloat) {
        guard !isTransitioning else { return }
        let translation = value.translation

        if dragAxis == nil {
            dragAxis = logic.determineAxis(dx: translation.width, dy: translation.height)
            if dragAxis == nil { return }
        }

        guard dragAxis == .horizontal else { return }
        dragOffset = logic.resistedOffset(translation.width, limit: width)
    }

    private func handleDragEnded(_ value: DragGesture.Value, width: CGFloat) {
        defer { dragAxis = nil }
        guard !isTransitioning else { return }
        guard dragAxis == .horizontal else {
            if dragOffset != 0 {
                withAnimation(transitionAnimation) {
                    dragOffset = 0
                }
            }
            return
        }

        let threshold = width * logic.swipeThresholdRatio
        let translation = value.translation.width
        let predicted = value.predictedEndTranslation.width

        let direction = logic.shouldNavigate(translation: translation, predicted: predicted, threshold: threshold)

        switch direction {
        case .forward:
            transition(to: shiftDate(by: 1), finalOffset: -width)
        case .backward:
            transition(to: shiftDate(by: -1), finalOffset: width)
        case .none:
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
}
