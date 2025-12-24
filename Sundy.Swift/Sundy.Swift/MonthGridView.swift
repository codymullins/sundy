//
//  MonthGridView.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

struct MonthGridView: View {
    @Environment(\.modelContext) private var modelContext
    @Query private var events: [JSCalendarEvent]
    @Query private var calendars: [SundyCalendar]
    
    @State private var currentDate = Date()
    @State private var showingNewEvent = false
    @State private var viewMode: ViewMode = .month
    @State private var selectedDate: Date?
    
    enum ViewMode {
        case day, week, month
    }
    
    private let calendar = Calendar.current
    private let daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    
    var body: some View {
        ZStack {
            // Dark background
            Color(nsColor: .windowBackgroundColor)
                .ignoresSafeArea()
            
            VStack(spacing: 0) {
                // Toolbar
                toolbar
                
                // Days of week header
                HStack(spacing: 0) {
                    ForEach(daysOfWeek, id: \.self) { day in
                        Text(day)
                            .font(.subheadline)
                            .fontWeight(.medium)
                            .foregroundStyle(.secondary)
                            .frame(maxWidth: .infinity)
                            .padding(.vertical, 12)
                    }
                }
                .background(Color(nsColor: .controlBackgroundColor).opacity(0.2))
                
                // Days grid
                LazyVGrid(columns: Array(repeating: GridItem(.flexible(), spacing: 0.5), count: 7), spacing: 0.5) {
                    ForEach(daysInMonth(), id: \.self) { date in
                        DayCell(
                            date: date,
                            currentMonth: currentDate,
                            events: eventsForDate(date),
                            calendars: calendars,
                            isSelected: selectedDate != nil && calendar.isDate(date, inSameDayAs: selectedDate!)
                        )
                        .onTapGesture {
                            selectedDate = date
                        }
                    }
                }
                .background(Color.black.opacity(0.08))
            }
        }
        .sheet(isPresented: $showingNewEvent) {
            NewEventView()
        }
    }
    
    private var toolbar: some View {
        HStack(spacing: 16) {
            // Today button
            Button("Today") {
                currentDate = Date()
                selectedDate = Date()
            }
            .buttonStyle(.bordered)
            
            // Navigation buttons
            HStack(spacing: 4) {
                Button(action: previousMonth) {
                    Image(systemName: "chevron.left")
                        .font(.body)
                }
                .buttonStyle(.bordered)
                
                Button(action: nextMonth) {
                    Image(systemName: "chevron.right")
                        .font(.body)
                }
                .buttonStyle(.bordered)
            }
            
            // Month and year title
            Text(currentDate.formatted(.dateTime.month(.wide).year()))
                .font(.title2)
                .fontWeight(.semibold)
            
            Spacer()
            
            // View mode toggle
            HStack(spacing: 0) {
                Button(action: { viewMode = .day }) {
                    Text("Day")
                        .font(.subheadline)
                        .frame(width: 65)
                        .padding(.vertical, 6)
                        .background(viewMode == .day ? Color.accentColor.opacity(0.15) : Color.clear)
                        .foregroundColor(viewMode == .day ? .accentColor : .primary)
                }
                .buttonStyle(.plain)
                
                Button(action: { viewMode = .week }) {
                    Text("Week")
                        .font(.subheadline)
                        .frame(width: 65)
                        .padding(.vertical, 6)
                        .background(viewMode == .week ? Color.accentColor.opacity(0.15) : Color.clear)
                        .foregroundColor(viewMode == .week ? .accentColor : .primary)
                }
                .buttonStyle(.plain)
                
                Button(action: { viewMode = .month }) {
                    Text("Month")
                        .font(.subheadline)
                        .frame(width: 65)
                        .padding(.vertical, 6)
                        .background(viewMode == .month ? Color.accentColor : Color.clear)
                        .foregroundColor(viewMode == .month ? .white : .primary)
                }
                .buttonStyle(.plain)
            }
            .background(Color(nsColor: .controlBackgroundColor).opacity(0.5))
            .cornerRadius(7)
            
            // New Event button
            Button(action: { showingNewEvent = true }) {
                HStack(spacing: 6) {
                    Image(systemName: "plus")
                        .font(.subheadline.weight(.semibold))
                    Text("New Event")
                }
                .padding(.horizontal, 4)
            }
            .buttonStyle(.borderedProminent)
            .controlSize(.regular)
        }
        .padding(.horizontal, 24)
        .padding(.vertical, 14)
        .background(Color(nsColor: .windowBackgroundColor))
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
    
    private func eventsForDate(_ date: Date) -> [JSCalendarEvent] {
        events.filter { event in
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

struct DayCell: View {
    let date: Date
    let currentMonth: Date
    let events: [JSCalendarEvent]
    let calendars: [SundyCalendar]
    let isSelected: Bool
    
    private let calendar = Calendar.current
    
    private var isToday: Bool {
        calendar.isDateInToday(date)
    }
    
    private var isInCurrentMonth: Bool {
        calendar.isDate(date, equalTo: currentMonth, toGranularity: .month)
    }
    
    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            // Day number
            HStack {
                ZStack {
                    if isToday {
                        Circle()
                            .fill(Color.accentColor)
                            .frame(width: 30, height: 30)
                    }
                    
                    Text("\(calendar.component(.day, from: date))")
                        .font(.system(.body, design: .rounded))
                        .fontWeight(isToday ? .semibold : .regular)
                        .foregroundColor(isToday ? .white : (isInCurrentMonth ? .primary : .secondary))
                        .opacity(isToday || isInCurrentMonth ? 1.0 : 0.4)
                }
                .padding(.leading, 8)
                
                Spacer()
            }
            
            // Events
            VStack(alignment: .leading, spacing: 3) {
                ForEach(events.prefix(3)) { event in
                    HStack(spacing: 6) {
                        RoundedRectangle(cornerRadius: 2)
                            .fill(Color(hex: event.calendar?.color ?? "#999999"))
                            .frame(width: 3, height: 12)
                        
                        Text(event.title)
                            .font(.caption)
                            .lineLimit(1)
                            .foregroundColor(isInCurrentMonth ? .primary : .secondary)
                            .opacity(isInCurrentMonth ? 1.0 : 0.5)
                    }
                    .padding(.horizontal, 4)
                }
                
                if events.count > 3 {
                    Text("+\(events.count - 3)")
                        .font(.caption2)
                        .foregroundStyle(.secondary)
                        .padding(.horizontal, 8)
                }
            }
            
            Spacer()
        }
        .frame(maxWidth: .infinity, minHeight: 110, alignment: .topLeading)
        .padding(.top, 8)
        .padding(.bottom, 4)
        .background(isInCurrentMonth ? Color(nsColor: .controlBackgroundColor).opacity(0.3) : Color.clear)
        .overlay(
            Rectangle()
                .strokeBorder(isSelected ? Color.accentColor.opacity(0.6) : Color.clear, lineWidth: 2)
        )
        .contentShape(Rectangle())
    }
}

#Preview {
    MonthGridView()
        .modelContainer(for: [SundyCalendar.self, JSCalendarEvent.self], inMemory: true)
}
