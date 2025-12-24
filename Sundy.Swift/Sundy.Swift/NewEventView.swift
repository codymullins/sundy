//
//  NewEventView.swift
//  Sundy.Swift
//
//  Created by Joshua Mullins on 12/22/25.
//

import SwiftUI
import SwiftData

struct NewEventView: View {
    @Environment(\.dismiss) private var dismiss
    @Environment(\.modelContext) private var modelContext
    @Query private var calendars: [SundyCalendar]
    
    @State private var title = ""
    @State private var startDate = Date()
    @State private var endDate = Date().addingTimeInterval(3600) // 1 hour later
    @State private var isAllDay = false
    @State private var notes = ""
    @State private var selectedCalendar: SundyCalendar?
    @State private var showingDatePicker = false
    
    var body: some View {
        ZStack {
            // Dark background
            Color(nsColor: .windowBackgroundColor)
                .ignoresSafeArea()
            
            VStack(spacing: 0) {
                // Header
                HStack {
                    Text("New Event")
                        .font(.title2)
                        .fontWeight(.bold)
                        .foregroundColor(.primary)
                    
                    Spacer()
                    
                    // Calendar picker
                    Menu {
                        ForEach(calendars) { calendar in
                            Button(action: {
                                selectedCalendar = calendar
                            }) {
                                HStack {
                                    Circle()
                                        .fill(Color(hex: calendar.color))
                                        .frame(width: 12, height: 12)
                                    Text(calendar.name)
                                    if selectedCalendar?.id == calendar.id {
                                        Image(systemName: "checkmark")
                                    }
                                }
                            }
                        }
                    } label: {
                        HStack(spacing: 8) {
                            Circle()
                                .fill(Color(hex: selectedCalendar?.color ?? "#3B82F6"))
                                .frame(width: 14, height: 14)
                            Text(selectedCalendar?.name ?? "My Calendar")
                                .foregroundColor(.primary)
                            Image(systemName: "chevron.down")
                                .font(.caption)
                                .foregroundColor(.secondary)
                        }
                        .padding(.horizontal, 12)
                        .padding(.vertical, 6)
                        .background(
                            RoundedRectangle(cornerRadius: 6)
                                .fill(Color(nsColor: .controlBackgroundColor))
                        )
                    }
                    .menuStyle(.borderlessButton)
                }
                .padding(24)
                
                // Content
                ScrollView {
                    VStack(alignment: .leading, spacing: 24) {
                        // Title field
                        VStack(alignment: .leading, spacing: 8) {
                            Text("Title *")
                                .font(.subheadline)
                                .fontWeight(.medium)
                                .foregroundColor(.primary)
                            
                            TextField("Event title", text: $title)
                                .textFieldStyle(.plain)
                                .padding(12)
                                .background(
                                    RoundedRectangle(cornerRadius: 8)
                                        .fill(Color(nsColor: .textBackgroundColor).opacity(0.5))
                                )
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .strokeBorder(Color.secondary.opacity(0.2), lineWidth: 1)
                                )
                        }
                        
                        // Date & Time section
                        VStack(alignment: .leading, spacing: 8) {
                            Text("Date & Time *")
                                .font(.subheadline)
                                .fontWeight(.medium)
                                .foregroundColor(.primary)
                            
                            Button(action: {
                                showingDatePicker.toggle()
                            }) {
                                HStack(spacing: 16) {
                                    // Calendar icon
                                    ZStack {
                                        RoundedRectangle(cornerRadius: 10)
                                            .fill(Color.purple)
                                            .frame(width: 44, height: 44)
                                        
                                        Image(systemName: "calendar")
                                            .font(.title3)
                                            .foregroundColor(.white)
                                    }
                                    
                                    // Date and time info
                                    VStack(alignment: .leading, spacing: 4) {
                                        Text(formatDateHeader(startDate))
                                            .font(.body)
                                            .fontWeight(.medium)
                                            .foregroundColor(.primary)
                                        
                                        if isAllDay {
                                            Text("All day")
                                                .font(.subheadline)
                                                .foregroundColor(.secondary)
                                        } else {
                                            HStack(spacing: 4) {
                                                Text(formatTime(startDate))
                                                Image(systemName: "arrow.right")
                                                    .font(.caption)
                                                Text(formatTime(endDate))
                                            }
                                            .font(.subheadline)
                                            .foregroundColor(.secondary)
                                            
                                            Text("Duration: \(formatDuration())")
                                                .font(.caption)
                                                .foregroundColor(.secondary.opacity(0.8))
                                        }
                                    }
                                    
                                    Spacer()
                                    
                                    Image(systemName: "pencil")
                                        .font(.subheadline)
                                        .foregroundColor(.secondary)
                                }
                                .padding(16)
                                .background(
                                    RoundedRectangle(cornerRadius: 12)
                                        .fill(Color(nsColor: .textBackgroundColor).opacity(0.5))
                                )
                                .overlay(
                                    RoundedRectangle(cornerRadius: 12)
                                        .strokeBorder(Color.secondary.opacity(0.2), lineWidth: 1)
                                )
                            }
                            .buttonStyle(.plain)
                            
                            // All day toggle
                            Toggle(isOn: $isAllDay) {
                                Text("All day event")
                                    .foregroundColor(.primary)
                            }
                            .toggleStyle(.checkbox)
                            .padding(.leading, 4)
                        }
                        
                        // Description field
                        VStack(alignment: .leading, spacing: 8) {
                            Text("Description")
                                .font(.subheadline)
                                .fontWeight(.medium)
                                .foregroundColor(.primary)
                            
                            TextEditor(text: $notes)
                                .font(.body)
                                .foregroundColor(.primary)
                                .frame(minHeight: 120)
                                .padding(8)
                                .background(
                                    RoundedRectangle(cornerRadius: 8)
                                        .fill(Color(nsColor: .textBackgroundColor).opacity(0.5))
                                )
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .strokeBorder(Color.secondary.opacity(0.2), lineWidth: 1)
                                )
                                .overlay(alignment: .topLeading) {
                                    if notes.isEmpty {
                                        Text("Add description")
                                            .foregroundColor(.secondary.opacity(0.6))
                                            .padding(.horizontal, 12)
                                            .padding(.vertical, 16)
                                            .allowsHitTesting(false)
                                    }
                                }
                        }
                    }
                    .padding(.horizontal, 24)
                    .padding(.bottom, 24)
                }
                
                // Bottom action buttons
                HStack(spacing: 12) {
                    Button("Cancel") {
                        dismiss()
                    }
                    .buttonStyle(.bordered)
                    .controlSize(.large)
                    .frame(maxWidth: .infinity)
                    
                    Button("Create") {
                        saveEvent()
                    }
                    .buttonStyle(.borderedProminent)
                    .controlSize(.large)
                    .frame(maxWidth: .infinity)
                    .disabled(title.isEmpty)
                }
                .padding(24)
                .background(
                    Rectangle()
                        .fill(Color(nsColor: .windowBackgroundColor).opacity(0.95))
                        .shadow(color: .black.opacity(0.1), radius: 10, y: -5)
                )
            }
        }
        .frame(width: 640, height: 600)
        .onAppear {
            // Select first calendar by default if none selected
            if selectedCalendar == nil {
                selectedCalendar = calendars.first
            }
        }
        .sheet(isPresented: $showingDatePicker) {
            DateTimePickerSheet(
                startDate: $startDate,
                endDate: $endDate,
                isAllDay: $isAllDay
            )
        }
    }
    
    private func formatDateHeader(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "EEEE, MMMM d, yyyy"
        return formatter.string(from: date)
    }
    
    private func formatTime(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        return formatter.string(from: date)
    }
    
    private func formatDuration() -> String {
        let duration = endDate.timeIntervalSince(startDate)
        let hours = Int(duration) / 3600
        let minutes = Int(duration) / 60 % 60
        
        if hours > 0 && minutes > 0 {
            return "\(hours) hr \(minutes) min"
        } else if hours > 0 {
            return "\(hours) hr"
        } else {
            return "\(minutes) min"
        }
    }
    
    private func saveEvent() {
        let newEvent = JSCalendarEvent(
            title: title,
            startDate: startDate,
            endDate: endDate,
            isAllDay: isAllDay,
            notes: notes,
            calendar: selectedCalendar ?? calendars.first
        )
        
        modelContext.insert(newEvent)
        
        do {
            try modelContext.save()
        } catch {
            print("Error saving event: \(error)")
        }
        
        dismiss()
    }
}

// Date/Time picker sheet with visual timeline
struct DateTimePickerSheet: View {
    @Environment(\.dismiss) private var dismiss
    @Binding var startDate: Date
    @Binding var endDate: Date
    @Binding var isAllDay: Bool
    
    @State private var currentMonth = Date()
    @State private var selectedDate: Date
    @State private var viewMode: ViewMode = .day
    @State private var draggedHandle: DragHandle?
    
    enum ViewMode {
        case day, week
    }
    
    enum DragHandle {
        case start, end
    }
    
    private let calendar = Calendar.current
    private let hourHeight: CGFloat = 80
    private let hours = Array(0...23)
    
    init(startDate: Binding<Date>, endDate: Binding<Date>, isAllDay: Binding<Bool>) {
        _startDate = startDate
        _endDate = endDate
        _isAllDay = isAllDay
        _selectedDate = State(initialValue: startDate.wrappedValue)
    }
    
    var body: some View {
        ZStack {
            Color(nsColor: .windowBackgroundColor)
                .ignoresSafeArea()
            
            VStack(spacing: 0) {
                // Header with month/year and view toggle
                HStack {
                    Text(currentMonth.formatted(.dateTime.month(.wide).year()))
                        .font(.title2)
                        .fontWeight(.bold)
                        .foregroundColor(.secondary)
                    
                    Spacer()
                    
                    HStack(spacing: 12) {
                        HStack(spacing: 0) {
                            Button(action: { viewMode = .day }) {
                                Text("Day")
                                    .frame(width: 60)
                                    .padding(.vertical, 6)
                                    .background(viewMode == .day ? Color.purple : Color.clear)
                                    .foregroundColor(viewMode == .day ? .white : .secondary)
                            }
                            .buttonStyle(.plain)
                            
                            Button(action: { viewMode = .week }) {
                                Text("Week")
                                    .frame(width: 60)
                                    .padding(.vertical, 6)
                                    .background(viewMode == .week ? Color.purple : Color.clear)
                                    .foregroundColor(viewMode == .week ? .white : .secondary)
                            }
                            .buttonStyle(.plain)
                        }
                        .background(Color(nsColor: .controlBackgroundColor))
                        .cornerRadius(6)
                        
                        // Close button
                        Button(action: { dismiss() }) {
                            Image(systemName: "xmark.circle.fill")
                                .font(.title2)
                                .foregroundColor(.secondary)
                        }
                        .buttonStyle(.plain)
                        .help("Close")
                        .keyboardShortcut(.cancelAction)
                    }
                }
                .padding(.horizontal, 24)
                .padding(.top, 24)
                .padding(.bottom, 16)
                
                // Week calendar selector
                HStack(spacing: 12) {
                    Button(action: previousWeek) {
                        Image(systemName: "chevron.left")
                            .font(.title3)
                            .foregroundColor(.secondary)
                    }
                    .buttonStyle(.plain)
                    
                    Spacer()
                    
                    HStack(spacing: 8) {
                        ForEach(weekDays(), id: \.self) { date in
                            VStack(spacing: 4) {
                                Text(dayOfWeekLetter(date))
                                    .font(.caption)
                                    .fontWeight(.semibold)
                                    .foregroundColor(.secondary)
                                
                                Text("\(calendar.component(.day, from: date))")
                                    .font(.body)
                                    .fontWeight(.medium)
                                    .foregroundColor(isSameDay(date, selectedDate) ? .white : .primary)
                                    .frame(width: 40, height: 40)
                                    .background(
                                        Circle()
                                            .fill(isSameDay(date, selectedDate) ? Color.purple : Color.clear)
                                    )
                                    .contentShape(Circle())
                                    .onTapGesture {
                                        selectedDate = date
                                        updateStartDateToSelectedDay()
                                    }
                            }
                        }
                    }
                    
                    Spacer()
                    
                    Button(action: nextWeek) {
                        Image(systemName: "chevron.right")
                            .font(.title3)
                            .foregroundColor(.secondary)
                    }
                    .buttonStyle(.plain)
                }
                .padding(.horizontal, 24)
                .padding(.bottom, 24)
                
                Divider()
                
                // Timeline view
                ScrollViewReader { proxy in
                    ScrollView {
                        HStack(alignment: .top, spacing: 0) {
                            // Hour labels
                            VStack(spacing: 0) {
                                ForEach(hours, id: \.self) { hour in
                                    VStack(spacing: 0) {
                                        Text(formatHour(hour))
                                            .font(.caption)
                                            .foregroundColor(.secondary)
                                            .frame(width: 60, alignment: .trailing)
                                            .frame(height: 20)
                                            .offset(y: -10)
                                        Spacer()
                                            .frame(height: hourHeight - 20)
                                    }
                                    .frame(height: hourHeight)
                                }
                            }
                            .padding(.trailing, 8)
                            
                            // Timeline grid with event block
                            ZStack(alignment: .topLeading) {
                                // Grid lines
                                VStack(spacing: 0) {
                                    ForEach(hours, id: \.self) { hour in
                                        VStack(spacing: 0) {
                                            Divider()
                                            Spacer()
                                                .frame(height: hourHeight - 1)
                                        }
                                        .frame(height: hourHeight)
                                        .id(hour)
                                    }
                                }
                                
                                // Event block
                                if !isAllDay {
                                    EventBlockView(
                                        startDate: $startDate,
                                        endDate: $endDate,
                                        selectedDate: selectedDate,
                                        hourHeight: hourHeight,
                                        draggedHandle: $draggedHandle
                                    )
                                }
                            }
                            .frame(maxWidth: .infinity)
                            .frame(height: CGFloat(hours.count) * hourHeight)
                            .background(Color(nsColor: .textBackgroundColor).opacity(0.3))
                            .cornerRadius(12)
                            .padding(.trailing, 24)
                        }
                        .padding(.leading, 24)
                        .padding(.top, 16)
                    }
                    .onAppear {
                        // Scroll to current hour
                        let currentHour = calendar.component(.hour, from: startDate)
                        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                            withAnimation {
                                proxy.scrollTo(max(0, currentHour - 2), anchor: .top)
                            }
                        }
                    }
                }
                
                Divider()
                    .padding(.top, 16)
                
                // Bottom actions
                HStack(spacing: 12) {
                    Button("Cancel") {
                        dismiss()
                    }
                    .buttonStyle(.bordered)
                    .controlSize(.large)
                    .frame(maxWidth: .infinity)
                    
                    Button("Confirm") {
                        dismiss()
                    }
                    .buttonStyle(.borderedProminent)
                    .controlSize(.large)
                    .frame(maxWidth: .infinity)
                }
                .padding(24)
            }
        }
        .frame(width: 800, height: 700)
        .onAppear {
            // Ensure currentMonth is set correctly
            currentMonth = selectedDate
        }
    }
    
    private func weekDays() -> [Date] {
        guard let weekInterval = calendar.dateInterval(of: .weekOfMonth, for: selectedDate) else {
            return []
        }
        
        var dates: [Date] = []
        var currentDate = weekInterval.start
        
        for _ in 0..<7 {
            dates.append(currentDate)
            guard let nextDate = calendar.date(byAdding: .day, value: 1, to: currentDate) else {
                break
            }
            currentDate = nextDate
        }
        
        return dates
    }
    
    private func dayOfWeekLetter(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "E"
        return formatter.string(from: date).uppercased()
    }
    
    private func isSameDay(_ date1: Date, _ date2: Date) -> Bool {
        calendar.isDate(date1, inSameDayAs: date2)
    }
    
    private func formatHour(_ hour: Int) -> String {
        let date = calendar.date(bySettingHour: hour, minute: 0, second: 0, of: Date()) ?? Date()
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        return formatter.string(from: date)
    }
    
    private func previousWeek() {
        guard let newDate = calendar.date(byAdding: .weekOfYear, value: -1, to: selectedDate) else { return }
        selectedDate = newDate
        currentMonth = newDate
        updateStartDateToSelectedDay()
    }
    
    private func nextWeek() {
        guard let newDate = calendar.date(byAdding: .weekOfYear, value: 1, to: selectedDate) else { return }
        selectedDate = newDate
        currentMonth = newDate
        updateStartDateToSelectedDay()
    }
    
    private func updateStartDateToSelectedDay() {
        let startHour = calendar.component(.hour, from: startDate)
        let startMinute = calendar.component(.minute, from: startDate)
        
        if let newStart = calendar.date(bySettingHour: startHour, minute: startMinute, second: 0, of: selectedDate) {
            startDate = newStart
            
            let duration = endDate.timeIntervalSince(startDate)
            endDate = newStart.addingTimeInterval(duration)
        }
    }
}

// Event block with draggable handles
struct EventBlockView: View {
    @Binding var startDate: Date
    @Binding var endDate: Date
    let selectedDate: Date
    let hourHeight: CGFloat
    @Binding var draggedHandle: DateTimePickerSheet.DragHandle?
    
    @State private var dragOffset: CGFloat = 0
    @State private var isDraggingBody = false
    @State private var initialStartOffset: CGFloat = 0
    @State private var initialEndOffset: CGFloat = 0
    
    private let calendar = Calendar.current
    
    var startOffset: CGFloat {
        let hour = calendar.component(.hour, from: startDate)
        let minute = calendar.component(.minute, from: startDate)
        let offset = CGFloat(hour) * hourHeight + (CGFloat(minute) / 60.0) * hourHeight
        return isDraggingBody ? offset + dragOffset : offset
    }
    
    var endOffset: CGFloat {
        let hour = calendar.component(.hour, from: endDate)
        let minute = calendar.component(.minute, from: endDate)
        let offset = CGFloat(hour) * hourHeight + (CGFloat(minute) / 60.0) * hourHeight
        return isDraggingBody ? offset + dragOffset : offset
    }
    
    var blockHeight: CGFloat {
        max(endOffset - startOffset, 40)
    }
    
    var body: some View {
        ZStack(alignment: .topLeading) {
            // Main event block
            VStack(spacing: 4) {
                Text("\(formatTime(startDate)) → \(formatTime(endDate))")
                    .font(.headline)
                    .foregroundColor(.white)
                
                Text(formatDuration())
                    .font(.subheadline)
                    .foregroundColor(.white.opacity(0.9))
            }
            .frame(maxWidth: .infinity, maxHeight: .infinity)
            .background(
                RoundedRectangle(cornerRadius: 12)
                    .fill(Color.purple)
            )
            .frame(height: blockHeight)
            .offset(y: startOffset)
            .gesture(
                DragGesture()
                    .onChanged { value in
                        isDraggingBody = true
                        dragOffset = value.translation.height
                    }
                    .onEnded { value in
                        let finalOffset = startOffset
                        moveEvent(to: finalOffset)
                        dragOffset = 0
                        isDraggingBody = false
                    }
            )
            
            // Top handle (start time)
            Circle()
                .fill(Color.white)
                .frame(width: 16, height: 16)
                .shadow(color: .black.opacity(0.2), radius: 2)
                .offset(x: -8, y: startOffset - 8)
                .gesture(
                    DragGesture(minimumDistance: 0)
                        .onChanged { value in
                            if draggedHandle == nil {
                                draggedHandle = .start
                                initialStartOffset = startOffset
                            }
                            let newY = initialStartOffset + value.translation.height
                            updateStartTime(from: newY)
                        }
                        .onEnded { _ in
                            draggedHandle = nil
                            initialStartOffset = 0
                        }
                )
            
            // Bottom handle (end time)
            Circle()
                .fill(Color.white)
                .frame(width: 16, height: 16)
                .shadow(color: .black.opacity(0.2), radius: 2)
                .offset(x: -8, y: endOffset - 8)
                .gesture(
                    DragGesture(minimumDistance: 0)
                        .onChanged { value in
                            if draggedHandle == nil {
                                draggedHandle = .end
                                initialEndOffset = endOffset
                            }
                            let newY = initialEndOffset + value.translation.height
                            updateEndTime(from: newY)
                        }
                        .onEnded { _ in
                            draggedHandle = nil
                            initialEndOffset = 0
                        }
                )
        }
        .frame(maxWidth: .infinity)
        .padding(.horizontal, 8)
    }
    
    private func moveEvent(to offset: CGFloat) {
        let totalMinutes = Int((offset / hourHeight) * 60)
        let hour = min(23, max(0, totalMinutes / 60))
        let minute = (totalMinutes % 60 / 15) * 15 // Snap to 15-minute intervals
        
        if let newStart = calendar.date(bySettingHour: hour, minute: minute, second: 0, of: selectedDate) {
            let duration = endDate.timeIntervalSince(startDate)
            startDate = newStart
            endDate = newStart.addingTimeInterval(duration)
        }
    }
    
    private func updateStartTime(from offset: CGFloat) {
        let totalMinutes = Int((offset / hourHeight) * 60)
        let hour = min(23, max(0, totalMinutes / 60))
        let minute = (totalMinutes % 60 / 15) * 15 // Snap to 15-minute intervals
        
        if let newStart = calendar.date(bySettingHour: hour, minute: minute, second: 0, of: selectedDate),
           newStart < endDate {
            startDate = newStart
        }
    }
    
    private func updateEndTime(from offset: CGFloat) {
        let totalMinutes = Int((offset / hourHeight) * 60)
        let hour = min(23, max(0, totalMinutes / 60))
        let minute = (totalMinutes % 60 / 15) * 15 // Snap to 15-minute intervals
        
        if let newEnd = calendar.date(bySettingHour: hour, minute: minute, second: 0, of: selectedDate),
           newEnd > startDate {
            endDate = newEnd
        }
    }
    
    private func formatTime(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        return formatter.string(from: date)
    }
    
    private func formatDuration() -> String {
        let duration = endDate.timeIntervalSince(startDate)
        let hours = Int(duration) / 3600
        let minutes = Int(duration) / 60 % 60
        
        if hours > 0 && minutes > 0 {
            return "\(hours) hr \(minutes) min"
        } else if hours > 0 {
            return "\(hours) hr"
        } else {
            return "\(minutes) min"
        }
    }
}

#Preview {
    NewEventView()
        .modelContainer(for: [SundyCalendar.self, JSCalendarEvent.self], inMemory: true)
}
