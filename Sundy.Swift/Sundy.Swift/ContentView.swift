//
//  ContentView.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

struct ContentView: View {
    var body: some View {
        NavigationSplitView {
            // Left sidebar: Calendar list + settings at bottom
            CalendarListView()
#if os(macOS)
                .navigationSplitViewColumnWidth(min: 220, ideal: 250)
#endif
        } detail: {
            // Main content: Toolbar + month grid
            MonthGridView()
        }
#if os(macOS)
        .navigationSplitViewStyle(.balanced)
#endif
    }
}

#Preview {
    ContentView()
        .modelContainer(for: [SundyCalendar.self, JSCalendarEvent.self], inMemory: true)
}
