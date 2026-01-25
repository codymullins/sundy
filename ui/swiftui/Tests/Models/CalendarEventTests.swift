import XCTest
@testable import Sundy

final class CalendarEventTests: XCTestCase {

    // MARK: - Initialization

    func testEventInitializationWithRequiredFields() {
        let id = UUID()
        let calendarId = UUID()
        let startTime = Date()
        let endTime = startTime.addingTimeInterval(3600)

        let event = CalendarEvent(
            id: id,
            calendarId: calendarId,
            title: "Test Event",
            startTime: startTime,
            endTime: endTime,
            details: "Some details",
            isAllDay: false
        )

        XCTAssertEqual(event.id, id)
        XCTAssertEqual(event.calendarId, calendarId)
        XCTAssertEqual(event.title, "Test Event")
        XCTAssertEqual(event.startTime, startTime)
        XCTAssertEqual(event.endTime, endTime)
        XCTAssertEqual(event.details, "Some details")
        XCTAssertFalse(event.isAllDay)
    }

    func testAllDayEventInitialization() {
        let event = CalendarEvent(
            id: UUID(),
            calendarId: UUID(),
            title: "All Day Event",
            startTime: Date(),
            endTime: Date().addingTimeInterval(86400),
            details: "",
            isAllDay: true
        )

        XCTAssertTrue(event.isAllDay)
    }

    // MARK: - Duration

    func testDurationCalculation() {
        let startTime = Date()
        let endTime = startTime.addingTimeInterval(7200) // 2 hours

        let event = CalendarEvent(
            id: UUID(),
            calendarId: UUID(),
            title: "Two Hour Event",
            startTime: startTime,
            endTime: endTime,
            details: "",
            isAllDay: false
        )

        XCTAssertEqual(event.duration, 7200, accuracy: 0.001)
    }

    func testDurationForAllDayEvent() {
        let startOfDay = Calendar.current.startOfDay(for: Date())
        let endOfDay = startOfDay.addingTimeInterval(86400) // 24 hours

        let event = CalendarEvent(
            id: UUID(),
            calendarId: UUID(),
            title: "All Day",
            startTime: startOfDay,
            endTime: endOfDay,
            details: "",
            isAllDay: true
        )

        XCTAssertEqual(event.duration, 86400, accuracy: 0.001)
    }

    // MARK: - Codable

    func testEncodingAndDecodingRoundtrip() throws {
        let originalEvent = CalendarEvent(
            id: UUID(),
            calendarId: UUID(),
            title: "Codable Test",
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "Testing encoding",
            isAllDay: false
        )

        let encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
        let data = try encoder.encode(originalEvent)

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        let decodedEvent = try decoder.decode(CalendarEvent.self, from: data)

        XCTAssertEqual(originalEvent.id, decodedEvent.id)
        XCTAssertEqual(originalEvent.calendarId, decodedEvent.calendarId)
        XCTAssertEqual(originalEvent.title, decodedEvent.title)
        XCTAssertEqual(originalEvent.details, decodedEvent.details)
        XCTAssertEqual(originalEvent.isAllDay, decodedEvent.isAllDay)
    }

    func testDecodingFromJSON() throws {
        let json = """
        {
            "id": "550E8400-E29B-41D4-A716-446655440000",
            "calendarId": "550E8400-E29B-41D4-A716-446655440001",
            "title": "Meeting",
            "startTime": "2025-01-15T10:00:00Z",
            "endTime": "2025-01-15T11:00:00Z",
            "details": "Team sync",
            "isAllDay": false
        }
        """

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        let event = try decoder.decode(CalendarEvent.self, from: json.data(using: .utf8)!)

        XCTAssertEqual(event.title, "Meeting")
        XCTAssertEqual(event.details, "Team sync")
        XCTAssertFalse(event.isAllDay)
    }

    // MARK: - Equatable

    func testEventsWithSameDataAreEqual() {
        let id = UUID()
        let calendarId = UUID()
        let startTime = Date()
        let endTime = startTime.addingTimeInterval(3600)

        let event1 = CalendarEvent(
            id: id,
            calendarId: calendarId,
            title: "Same Event",
            startTime: startTime,
            endTime: endTime,
            details: "Details",
            isAllDay: false
        )

        let event2 = CalendarEvent(
            id: id,
            calendarId: calendarId,
            title: "Same Event",
            startTime: startTime,
            endTime: endTime,
            details: "Details",
            isAllDay: false
        )

        XCTAssertEqual(event1, event2)
    }

    func testEventsWithDifferentIdsAreNotEqual() {
        let calendarId = UUID()
        let startTime = Date()
        let endTime = startTime.addingTimeInterval(3600)

        let event1 = CalendarEvent(
            id: UUID(),
            calendarId: calendarId,
            title: "Event",
            startTime: startTime,
            endTime: endTime,
            details: "",
            isAllDay: false
        )

        let event2 = CalendarEvent(
            id: UUID(),
            calendarId: calendarId,
            title: "Event",
            startTime: startTime,
            endTime: endTime,
            details: "",
            isAllDay: false
        )

        XCTAssertNotEqual(event1, event2)
    }
}
