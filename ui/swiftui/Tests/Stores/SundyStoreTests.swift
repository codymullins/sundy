import XCTest
@testable import Sundy

@MainActor
final class SundyStoreTests: XCTestCase {

    private var store: SundyStore!

    override func setUp() async throws {
        // Create a fresh store for each test
        // SundyDataStore will write to the app's normal location
        // but we reset data at the start of each test
        store = SundyStore()
        store.resetData()
    }

    override func tearDown() async throws {
        store = nil
    }

    // MARK: - Calendar Operations

    func testAddCalendarIncreasesCalendarCount() {
        let initialCount = store.calendars.count

        store.addCalendar(name: "Work", color: ColorValue.fromRGB(255, 0, 0))

        XCTAssertEqual(store.calendars.count, initialCount + 1)
    }

    func testAddCalendarSetsCorrectName() {
        store.addCalendar(name: "Personal", color: ColorValue.fromRGB(0, 255, 0))

        let addedCalendar = store.calendars.last
        XCTAssertEqual(addedCalendar?.name, "Personal")
    }

    func testAddCalendarSetsCorrectColor() {
        let color = ColorValue.fromRGB(128, 64, 32)

        store.addCalendar(name: "Test", color: color)

        let addedCalendar = store.calendars.last
        XCTAssertEqual(addedCalendar?.color, color)
    }

    func testRenameCalendarUpdatesDisplayName() {
        store.addCalendar(name: "Original", color: ColorValue.fromRGB(0, 0, 255))
        let calendarId = store.calendars.last!.id

        store.renameCalendar(id: calendarId, newName: "Renamed")

        let calendar = store.calendars.first { $0.id == calendarId }
        XCTAssertEqual(calendar?.displayName, "Renamed")
        XCTAssertEqual(calendar?.effectiveName, "Renamed")
    }

    func testDeleteCalendarRemovesCalendar() {
        store.addCalendar(name: "ToDelete", color: ColorValue.fromRGB(255, 255, 0))
        let calendarId = store.calendars.last!.id
        let countBefore = store.calendars.count

        store.deleteCalendar(id: calendarId)

        XCTAssertEqual(store.calendars.count, countBefore - 1)
        XCTAssertNil(store.calendars.first { $0.id == calendarId })
    }

    func testDeleteCalendarRemovesAssociatedEvents() {
        store.addCalendar(name: "CalWithEvents", color: ColorValue.fromRGB(0, 0, 0))
        let calendarId = store.calendars.last!.id

        let draft = EventDraft(
            title: "Test Event",
            date: Date(),
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "",
            isAllDay: false,
            calendarId: calendarId
        )
        store.createEvent(from: draft)
        XCTAssertTrue(store.events.contains { $0.calendarId == calendarId })

        store.deleteCalendar(id: calendarId)

        XCTAssertFalse(store.events.contains { $0.calendarId == calendarId })
    }

    func testToggleCalendarVisibility() {
        store.addCalendar(name: "Toggleable", color: ColorValue.fromRGB(100, 100, 100))
        let calendarId = store.calendars.last!.id
        let initialVisibility = store.calendars.last!.isVisible

        store.toggleCalendarVisibility(id: calendarId)

        let calendar = store.calendars.first { $0.id == calendarId }
        XCTAssertEqual(calendar?.isVisible, !initialVisibility)
    }

    // MARK: - Event Operations

    func testCreateEventAddsEvent() {
        let initialCount = store.events.count
        let draft = EventDraft(
            title: "New Event",
            date: Date(),
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "Event details",
            isAllDay: false,
            calendarId: SundyCalendar.defaultCalendar.id
        )

        store.createEvent(from: draft)

        XCTAssertEqual(store.events.count, initialCount + 1)
    }

    func testCreateEventTrimsTitle() {
        let draft = EventDraft(
            title: "  Padded Title  ",
            date: Date(),
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "",
            isAllDay: false,
            calendarId: SundyCalendar.defaultCalendar.id
        )

        store.createEvent(from: draft)

        let event = store.events.last
        XCTAssertEqual(event?.title, "Padded Title")
    }

    func testUpdateEventModifiesEvent() {
        let draft = EventDraft(
            title: "Original",
            date: Date(),
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "",
            isAllDay: false,
            calendarId: SundyCalendar.defaultCalendar.id
        )
        store.createEvent(from: draft)
        let eventId = store.events.last!.id

        var updatedDraft = draft
        updatedDraft.title = "Updated"
        store.updateEvent(id: eventId, from: updatedDraft)

        let event = store.events.first { $0.id == eventId }
        XCTAssertEqual(event?.title, "Updated")
    }

    func testDeleteEventRemovesEvent() {
        let draft = EventDraft(
            title: "ToDelete",
            date: Date(),
            startTime: Date(),
            endTime: Date().addingTimeInterval(3600),
            details: "",
            isAllDay: false,
            calendarId: SundyCalendar.defaultCalendar.id
        )
        store.createEvent(from: draft)
        let eventId = store.events.last!.id
        let countBefore = store.events.count

        store.deleteEvent(id: eventId)

        XCTAssertEqual(store.events.count, countBefore - 1)
        XCTAssertNil(store.events.first { $0.id == eventId })
    }

    // MARK: - Toast Operations

    func testShowToastAddsToast() {
        XCTAssertTrue(store.toasts.isEmpty)

        store.showToast(message: "Test toast", type: .success)

        XCTAssertEqual(store.toasts.count, 1)
        XCTAssertEqual(store.toasts.first?.message, "Test toast")
        XCTAssertEqual(store.toasts.first?.type, .success)
    }

    func testDismissToastRemovesToast() {
        store.showToast(message: "Toast 1", type: .info)
        store.showToast(message: "Toast 2", type: .error)
        let toastId = store.toasts.first!.id

        store.dismissToast(id: toastId)

        XCTAssertEqual(store.toasts.count, 1)
        XCTAssertNil(store.toasts.first { $0.id == toastId })
    }

    // MARK: - Log Operations

    func testAddLogCreatesEntry() {
        let initialCount = store.logs.count

        store.addLog(message: "Test log", level: .info)

        XCTAssertEqual(store.logs.count, initialCount + 1)
        XCTAssertEqual(store.logs.first?.message, "Test log")
        XCTAssertEqual(store.logs.first?.level, .info)
    }

    func testClearLogsRemovesAllLogs() {
        store.addLog(message: "Log 1", level: .info)
        store.addLog(message: "Log 2", level: .warning)
        XCTAssertFalse(store.logs.isEmpty)

        store.clearLogs()

        XCTAssertTrue(store.logs.isEmpty)
    }

    // MARK: - Reset

    func testResetDataClearsAllData() {
        store.addCalendar(name: "Extra", color: ColorValue.fromRGB(0, 0, 0))
        store.addLog(message: "Test", level: .info)
        store.showToast(message: "Toast", type: .success)

        store.resetData()

        XCTAssertEqual(store.calendars.count, 1) // Default calendar only
        XCTAssertEqual(store.calendars.first?.id, SundyCalendar.defaultId)
        XCTAssertTrue(store.events.isEmpty)
        XCTAssertTrue(store.logs.isEmpty)
    }
}
