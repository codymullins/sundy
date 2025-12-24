//
//  MonthMinimapView.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

struct MonthMinimapView: View {
    @Environment(\.modelContext) private var modelContext
    @Query private var events: [JSCalendarEvent]
    @Query private var calendars: [SundyCalendar]
    
    @State private var currentDate = Date()
    
    private let calendar = Calendar.current
    private let daysOfWeek = ["S", "M", "T", "W", "T", "F", "S"]
    
    var body: some View {
        VStack(spacing: 12) {
            // Header with month navigation
            HStack {
                Button(action: previousMonth) {
                    Image(systemName: "chevron.left")
                        .font(.caption)
                }
                .buttonStyle(.plain)
                
                Spacer()
                
                Text(currentDate.formatted(.dateTime.month(.wide).year()))
                    .font(.headline)
                
                Spacer()
                
                Button(action: nextMonth) {
                    Image(systemName: "chevron.right")
                        .font(.caption)
                }
                .buttonStyle(.plain)
            }
            
            // Days of week header
            HStack(spacing: 4) {
                ForEach(daysOfWeek, id: \.self) { day in
                    Text(day)
                        .font(.system(size: 9))
                        .fontWeight(.semibold)
                        .foregroundStyle(.secondary)
                        .frame(maxWidth: .infinity)
                }
            }
            
            // Days grid
            LazyVGrid(columns: Array(repeating: GridItem(.flexible(), spacing: 4), count: 7), spacing: 4) {
                ForEach(daysInMonth(), id: \.self) { date in
                    MinimapDayCell(
                        date: date,
                        currentMonth: currentDate,
                        hasEvents: hasEventsForDate(date)
                    )
                }
            }
            
            Divider()
            
            // Quick actions
            HStack {
                Button("Today") {
                    currentDate = Date()
                }
                .buttonStyle(.plain)
                .font(.caption)
                
                Spacer()
                
                Button("Open Calendar") {
                    // This will activate the main window
                    NSApp.activate(ignoringOtherApps: true)
                    // Find and bring forward the main window
                    if let window = NSApp.windows.first(where: { $0.title.contains("Sundy") || $0.isMainWindow }) {
                        window.makeKeyAndOrderFront(nil)
                    }
                }
                .buttonStyle(.plain)
                .font(.caption)
            }
        }
        .padding(12)
        .frame(width: 240)
    }
    
    private func daysInMonth() -> [Date] {
        guard let monthInterval = calendar.dateInterval(of: .month, for: currentDate),
              let monthFirstWeek = calendar.dateInterval(of: .weekOfMonth, for: monthInterval.start) else {
            return []
        }
        
        let monthLastDay = calendar.date(byAdding: DateComponents(day: -1), to: monthInterval.end) ?? monthInterval.end
        guard let monthLastWeek = calendar.dateInterval(of: .weekOfMonth, for: monthLastDay) else {
            return []
        }
        
        var dates: [Date] = []
        var currentDate = monthFirstWeek.start
        
        while currentDate < monthLastWeek.end {
            dates.append(currentDate)
            guard let nextDate = calendar.date(byAdding: .day, value: 1, to: currentDate) else {
                break
            }
            currentDate = nextDate
        }
        
        return dates
    }
    
    private func previousMonth() {
        guard let newDate = calendar.date(byAdding: .month, value: -1, to: currentDate) else { return }
        currentDate = newDate
    }
    
    private func nextMonth() {
        guard let newDate = calendar.date(byAdding: .month, value: 1, to: currentDate) else { return }
        currentDate = newDate
    }
    
    private func hasEventsForDate(_ date: Date) -> Bool {
        events.contains { event in
            // Only show events for visible calendars
            guard event.calendar?.isVisible ?? true else { return false }
            
            // Check if event occurs on this date
            if event.isAllDay {
                return calendar.isDate(event.startDate, inSameDayAs: date)
            } else {
                let startOfDay = calendar.startOfDay(for: date)
                let endOfDay = calendar.date(byAdding: .day, value: 1, to: startOfDay) ?? startOfDay
                
                // Event starts or ends on this day, or spans across it
                return (event.startDate >= startOfDay && event.startDate < endOfDay) ||
                       (event.endDate >= startOfDay && event.endDate < endOfDay) ||
                       (event.startDate < startOfDay && event.endDate >= endOfDay)
            }
        }
    }
}

struct MinimapDayCell: View {
    let date: Date
    let currentMonth: Date
    let hasEvents: Bool
    
    private let calendar = Calendar.current
    
    private var isToday: Bool {
        calendar.isDateInToday(date)
    }
    
    private var isInCurrentMonth: Bool {
        calendar.isDate(date, equalTo: currentMonth, toGranularity: .month)
    }
    
    var body: some View {
        ZStack {
            if isToday {
                Circle()
                    .fill(Color.accentColor)
            }
            
            Text("\(calendar.component(.day, from: date))")
                .font(.system(size: 11))
                .fontWeight(isToday ? .bold : .regular)
                .foregroundStyle(isToday ? Color.white : (isInCurrentMonth ? Color.primary : Color.secondary.opacity(0.5)))
            
            // Event indicator dot
            if hasEvents && isInCurrentMonth {
                Circle()
                    .fill(isToday ? .white : Color.accentColor)
                    .frame(width: 3, height: 3)
                    .offset(y: 10)
            }
        }
        .frame(width: 28, height: 28)
        .contentShape(Rectangle())
    }
}

#Preview {
    MonthMinimapView()
        .modelContainer(for: [SundyCalendar.self, JSCalendarEvent.self], inMemory: true)
}
