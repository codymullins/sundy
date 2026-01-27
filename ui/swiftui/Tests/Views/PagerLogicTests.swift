import XCTest
@testable import Sundy

final class PagerLogicTests: XCTestCase {

    private var logic: PagerLogic!

    override func setUp() {
        logic = PagerLogic()
    }

    override func tearDown() {
        logic = nil
    }

    // MARK: - resistedOffset

    func testResistedOffsetWithinLimitsReturnsValueUnchanged() {
        let result = logic.resistedOffset(50, limit: 100)
        XCTAssertEqual(result, 50, accuracy: 0.001)
    }

    func testResistedOffsetAtUpperLimitReturnsLimit() {
        let result = logic.resistedOffset(100, limit: 100)
        XCTAssertEqual(result, 100, accuracy: 0.001)
    }

    func testResistedOffsetBeyondUpperLimitAppliesDamping() {
        // value=150, limit=100 -> clamped=100, overflow=50 -> 100 + 50*0.2 = 110
        let result = logic.resistedOffset(150, limit: 100)
        XCTAssertEqual(result, 110, accuracy: 0.001)
    }

    func testResistedOffsetBeyondLowerLimitAppliesDamping() {
        // value=-150, limit=100 -> clamped=-100, overflow=-50 -> -100 + (-50)*0.2 = -110
        let result = logic.resistedOffset(-150, limit: 100)
        XCTAssertEqual(result, -110, accuracy: 0.001)
    }

    func testResistedOffsetAtNegativeLimitReturnsLimit() {
        let result = logic.resistedOffset(-100, limit: 100)
        XCTAssertEqual(result, -100, accuracy: 0.001)
    }

    func testResistedOffsetZeroReturnsZero() {
        let result = logic.resistedOffset(0, limit: 100)
        XCTAssertEqual(result, 0, accuracy: 0.001)
    }

    // MARK: - determineAxis

    func testDetermineAxisReturnsNilBelowThreshold() {
        // Movement smaller than threshold should return nil
        let result = logic.determineAxis(dx: 3, dy: 3)
        XCTAssertNil(result)
    }

    func testDetermineAxisReturnsHorizontalWhenWider() {
        // abs(dx) > abs(dy) should return horizontal
        let result = logic.determineAxis(dx: 20, dy: 5)
        XCTAssertEqual(result, .horizontal)
    }

    func testDetermineAxisReturnsVerticalWhenTaller() {
        // abs(dy) > abs(dx) should return vertical
        let result = logic.determineAxis(dx: 5, dy: 20)
        XCTAssertEqual(result, .vertical)
    }

    func testDetermineAxisAtExactThresholdPicksAxis() {
        // At threshold with dx > dy, should pick horizontal
        let result = logic.determineAxis(dx: 10, dy: 5)
        XCTAssertEqual(result, .horizontal)
    }

    func testDetermineAxisWithNegativeValuesWorks() {
        // Should work with negative values
        let result = logic.determineAxis(dx: -20, dy: 5)
        XCTAssertEqual(result, .horizontal)
    }

    func testDetermineAxisWithEqualAbsoluteValuesReturnsHorizontal() {
        // When equal, prefer horizontal (for swipe navigation)
        let result = logic.determineAxis(dx: 10, dy: 10)
        XCTAssertEqual(result, .horizontal)
    }

    // MARK: - shouldNavigate

    func testShouldNavigateForwardOnNegativeTranslation() {
        // Swiping left (negative translation) should navigate forward
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: -60, predicted: -100, threshold: threshold)
        XCTAssertEqual(result, .forward)
    }

    func testShouldNavigateBackwardOnPositiveTranslation() {
        // Swiping right (positive translation) should navigate backward
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: 60, predicted: 100, threshold: threshold)
        XCTAssertEqual(result, .backward)
    }

    func testShouldNotNavigateBelowThreshold() {
        // Small drag (below threshold) with slow velocity should not navigate
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: 20, predicted: 30, threshold: threshold)
        XCTAssertEqual(result, .none)
    }

    func testShouldNavigateOnVelocityForward() {
        // Fast swipe (high predicted) should navigate even with small translation
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: -20, predicted: -100, threshold: threshold)
        XCTAssertEqual(result, .forward)
    }

    func testShouldNavigateOnVelocityBackward() {
        // Fast swipe backward should navigate
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: 20, predicted: 100, threshold: threshold)
        XCTAssertEqual(result, .backward)
    }

    func testShouldNotNavigateAtExactThreshold() {
        // At exact threshold boundary, should not navigate (requires exceeding)
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: 50, predicted: 50, threshold: threshold)
        XCTAssertEqual(result, .none)
    }

    func testShouldNavigateJustPastThreshold() {
        // Just past threshold should navigate
        let threshold: CGFloat = 50
        let result = logic.shouldNavigate(translation: 51, predicted: 51, threshold: threshold)
        XCTAssertEqual(result, .backward)
    }
}
