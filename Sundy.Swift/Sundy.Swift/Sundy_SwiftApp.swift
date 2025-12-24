//
//  Sundy_SwiftApp.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

@main
struct Sundy_SwiftApp: App {
    var sharedModelContainer: ModelContainer = {
        let schema = Schema([
            SundyCalendar.self,
            JSCalendarEvent.self,
        ])
        let modelConfiguration = ModelConfiguration(schema: schema, isStoredInMemoryOnly: false)

        do {
            return try ModelContainer(for: schema, configurations: [modelConfiguration])
        } catch {
            fatalError("Could not create ModelContainer: \(error)")
        }
    }()

    var body: some Scene {
        WindowGroup {
            ContentView()
        }
        .modelContainer(sharedModelContainer)
        
#if os(macOS)
        MenuBarExtra {
            MonthMinimapView()
                .modelContainer(sharedModelContainer)
        } label: {
            // Show current day of month
            Text("\(Calendar.current.component(.day, from: Date()))")
                .font(.system(size: 13, weight: .medium, design: .rounded))
        }
        .menuBarExtraStyle(.window)
#endif
    }
}
