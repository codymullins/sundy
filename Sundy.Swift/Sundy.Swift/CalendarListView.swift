//
//  CalendarListView.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

struct CalendarListView: View {
    @Environment(\.modelContext) private var modelContext
    @Query(sort: \SundyCalendar.createdDate) private var calendars: [SundyCalendar]
    
    var body: some View {
        VStack(spacing: 0) {
            // Calendar list
            List {
                Section("Calendars") {
                    ForEach(calendars) { calendar in
                        CalendarRowView(calendar: calendar)
                    }
                    .onDelete(perform: deleteCalendars)
                    
                    Button(action: addCalendar) {
                        Label("Add Calendar", systemImage: "plus.circle.fill")
                    }
                }
            }
            
            Divider()
            
            // Settings button at bottom
            Button(action: {
                // Open settings
            }) {
                Label("Settings", systemImage: "gear")
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding()
            }
            .buttonStyle(.plain)
        }
    }
    
    private func addCalendar() {
        withAnimation {
            let colors = ["#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F"]
            let newCalendar = SundyCalendar(
                name: "New Calendar",
                color: colors.randomElement() ?? "#4ECDC4"
            )
            modelContext.insert(newCalendar)
        }
    }
    
    private func deleteCalendars(offsets: IndexSet) {
        withAnimation {
            for index in offsets {
                modelContext.delete(calendars[index])
            }
        }
    }
}

struct CalendarRowView: View {
    @Bindable var calendar: SundyCalendar
    
    var body: some View {
        HStack {
            Circle()
                .fill(Color(hex: calendar.color))
                .frame(width: 12, height: 12)
            
            Text(calendar.name)
            
            Spacer()
            
            Toggle("", isOn: $calendar.isVisible)
                .labelsHidden()
        }
    }
}

#Preview {
    CalendarListView()
        .modelContainer(for: SundyCalendar.self, inMemory: true)
}
