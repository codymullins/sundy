//
//  Calendar.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import Foundation
import SwiftData

@Model
final class SundyCalendar {
    var name: String
    var color: String // Hex color string
    var isVisible: Bool
    var createdDate: Date
    
    // Relationship to events
    @Relationship(deleteRule: .cascade, inverse: \JSCalendarEvent.calendar)
    var events: [JSCalendarEvent] = []
    
    init(name: String, color: String, isVisible: Bool = true) {
        self.name = name
        self.color = color
        self.isVisible = isVisible
        self.createdDate = Date()
    }
}
