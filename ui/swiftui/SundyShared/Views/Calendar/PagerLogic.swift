import Foundation

struct PagerLogic {
    let swipeThresholdRatio: CGFloat = 0.2
    let axisLockThreshold: CGFloat = 6
    let dampingFactor: CGFloat = 0.2

    enum Axis: Equatable {
        case horizontal
        case vertical
    }

    enum NavigationDirection: Equatable {
        case forward
        case backward
        case none
    }

    /// Applies elastic resistance when dragging beyond limits.
    /// - Parameters:
    ///   - value: The current drag offset
    ///   - limit: The maximum allowed offset (positive value, applied symmetrically)
    /// - Returns: The offset with resistance applied beyond the limit
    func resistedOffset(_ value: CGFloat, limit: CGFloat) -> CGFloat {
        let clamped = max(-limit, min(limit, value))
        let overflow = value - clamped
        return clamped + overflow * dampingFactor
    }

    /// Determines the axis of a drag gesture based on the initial movement.
    /// - Parameters:
    ///   - dx: Horizontal translation
    ///   - dy: Vertical translation
    /// - Returns: The dominant axis, or nil if below the lock threshold
    func determineAxis(dx: CGFloat, dy: CGFloat) -> Axis? {
        let absDx = abs(dx)
        let absDy = abs(dy)

        // Below threshold - can't determine axis yet
        guard absDx >= axisLockThreshold || absDy >= axisLockThreshold else {
            return nil
        }

        // Prefer horizontal when equal (common for horizontal pagers)
        return absDx >= absDy ? .horizontal : .vertical
    }

    /// Determines if the user intended to navigate based on drag translation and velocity.
    /// - Parameters:
    ///   - translation: The actual drag translation
    ///   - predicted: The predicted end translation (based on velocity)
    ///   - threshold: The threshold to exceed for navigation
    /// - Returns: The navigation direction, or .none if no navigation should occur
    func shouldNavigate(translation: CGFloat, predicted: CGFloat, threshold: CGFloat) -> NavigationDirection {
        // Forward = swipe left (negative translation)
        if predicted < -threshold || translation < -threshold {
            return .forward
        }
        // Backward = swipe right (positive translation)
        if predicted > threshold || translation > threshold {
            return .backward
        }
        return .none
    }
}
