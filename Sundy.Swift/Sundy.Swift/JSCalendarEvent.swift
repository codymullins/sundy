//
//  JSCalendarEvent.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import Foundation
import SwiftData

@Model
final class JSCalendarEvent {
    var title: String
    var startDate: Date
    var endDate: Date
    var isAllDay: Bool
    var notes: String
    var createdDate: Date
    
    // Relationship to calendar
    var calendar: SundyCalendar?
    
    init(
        title: String,
        startDate: Date,
        endDate: Date,
        isAllDay: Bool = false,
        notes: String = "",
        calendar: SundyCalendar? = nil
    ) {
        self.title = title
        self.startDate = startDate
        self.endDate = endDate
        self.isAllDay = isAllDay
        self.notes = notes
        self.calendar = calendar
        self.createdDate = Date()
    }
}
