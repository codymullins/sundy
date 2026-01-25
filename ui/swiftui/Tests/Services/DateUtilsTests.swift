import XCTest
@testable import Sundy

final class DateUtilsTests: XCTestCase {
    private var calendar: Calendar {
        var cal = Calendar(identifier: .gregorian)
        cal.firstWeekday = 1 // Sunday
        return cal
    }

    private func date(year: Int, month: Int, day: Int) -> Date {
        var components = DateComponents()
        components.year = year
        components.month = month
        components.day = day
        components.hour = 12
        return calendar.date(from: components)!
    }

    // MARK: - weekStart(for:)

    func testWeekStartReturnsStartOfWeek() {
        // Wednesday, January 15, 2025
        let wednesday = date(year: 2025, month: 1, day: 15)
        let weekStart = DateUtils.weekStart(for: wednesday)

        let components = calendar.dateComponents([.year, .month, .day], from: weekStart)
        XCTAssertEqual(components.year, 2025)
        XCTAssertEqual(components.month, 1)
        XCTAssertEqual(components.day, 12) // Sunday
    }

    func testWeekStartOnSundayReturnsSameDay() {
        // Sunday, January 12, 2025
        let sunday = date(year: 2025, month: 1, day: 12)
        let weekStart = DateUtils.weekStart(for: sunday)

        let components = calendar.dateComponents([.year, .month, .day], from: weekStart)
        XCTAssertEqual(components.day, 12)
    }

    func testWeekStartOnSaturdayReturnsCorrectSunday() {
        // Saturday, January 18, 2025
        let saturday = date(year: 2025, month: 1, day: 18)
        let weekStart = DateUtils.weekStart(for: saturday)

        let components = calendar.dateComponents([.year, .month, .day], from: weekStart)
        XCTAssertEqual(components.day, 12) // Previous Sunday
    }

    // MARK: - weekDays(for:)

    func testWeekDaysReturnsSevenDays() {
        let date = date(year: 2025, month: 1, day: 15)
        let days = DateUtils.weekDays(for: date)

        XCTAssertEqual(days.count, 7)
    }

    func testWeekDaysStartsWithSunday() {
        let date = date(year: 2025, month: 1, day: 15)
        let days = DateUtils.weekDays(for: date)

        let firstDayComponents = calendar.dateComponents([.weekday], from: days[0])
        XCTAssertEqual(firstDayComponents.weekday, 1) // Sunday
    }

    func testWeekDaysEndsWithSaturday() {
        let date = date(year: 2025, month: 1, day: 15)
        let days = DateUtils.weekDays(for: date)

        let lastDayComponents = calendar.dateComponents([.weekday], from: days[6])
        XCTAssertEqual(lastDayComponents.weekday, 7) // Saturday
    }

    func testWeekDaysAreConsecutive() {
        let date = date(year: 2025, month: 1, day: 15)
        let days = DateUtils.weekDays(for: date)

        for i in 0..<6 {
            let diff = calendar.dateComponents([.day], from: days[i], to: days[i + 1])
            XCTAssertEqual(diff.day, 1, "Days should be consecutive")
        }
    }

    // MARK: - formatHourLabel(_:)

    func testFormatHourLabelMidnight() {
        XCTAssertEqual(DateUtils.formatHourLabel(0), "12 AM")
    }

    func testFormatHourLabelNoon() {
        XCTAssertEqual(DateUtils.formatHourLabel(12), "12 PM")
    }

    func testFormatHourLabelMorning() {
        XCTAssertEqual(DateUtils.formatHourLabel(9), "9 AM")
    }

    func testFormatHourLabelAfternoon() {
        XCTAssertEqual(DateUtils.formatHourLabel(14), "2 PM")
    }

    func testFormatHourLabelEvening() {
        XCTAssertEqual(DateUtils.formatHourLabel(23), "11 PM")
    }

    // MARK: - monthWeeks(for:)

    func testMonthWeeksReturnsSixWeeks() {
        let date = date(year: 2025, month: 1, day: 15)
        let weeks = DateUtils.monthWeeks(for: date)

        XCTAssertEqual(weeks.count, 6)
    }

    func testMonthWeeksEachWeekHasSevenDays() {
        let date = date(year: 2025, month: 1, day: 15)
        let weeks = DateUtils.monthWeeks(for: date)

        for week in weeks {
            XCTAssertEqual(week.count, 7, "Each week should have 7 days")
        }
    }

    // MARK: - headerTitle(for:view:)

    func testHeaderTitleDayView() {
        let date = date(year: 2025, month: 1, day: 15)
        let title = DateUtils.headerTitle(for: date, view: .day)

        XCTAssertTrue(title.contains("January"))
        XCTAssertTrue(title.contains("15"))
        XCTAssertTrue(title.contains("2025"))
    }

    func testHeaderTitleMonthView() {
        let date = date(year: 2025, month: 1, day: 15)
        let title = DateUtils.headerTitle(for: date, view: .month)

        XCTAssertTrue(title.contains("January"))
        XCTAssertTrue(title.contains("2025"))
        XCTAssertFalse(title.contains("15"))
    }
}
