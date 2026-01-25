# Sundycal Tauri + Svelte Implementation Plan

This document tracks the implementation of the Tauri + Svelte version of Sundycal.
Each task is designed to be atomic and completable by a single developer in 1-4 hours.

## Status Legend
- [ ] Not started
- [x] Completed
- [~] In progress
- [!] Blocked

---

## Phase 1: Project Setup

### 1.1 Initialize Project
- [x] **1.1.1** Create Tauri + SvelteKit project using `npm create tauri-app@latest` in this directory
  - Select: SvelteKit, TypeScript, npm
- [x] **1.1.2** Verify project runs with `npm run tauri dev`

### 1.2 Configure SvelteKit
- [x] **1.2.1** Install `@sveltejs/adapter-static` and configure in `svelte.config.js`
- [x] **1.2.2** Set `prerender` options for static generation
- [x] **1.2.3** Configure TypeScript strict mode in `tsconfig.json`
- [x] **1.2.4** Add path aliases (`$lib`, `$components`, etc.)

### 1.3 Install Dependencies
- [x] **1.3.1** Install bits-ui: `npm install bits-ui`
- [x] **1.3.2** Install date library: `npm install @internationalized/date`
- [x] **1.3.3** Install Tauri plugins: `npm install @tauri-apps/plugin-sql`
- [x] **1.3.4** Add Tauri SQL plugin to Cargo.toml: `tauri-plugin-sql`

### 1.4 Configure Tauri
- [x] **1.4.1** Update `tauri.conf.json` with app identifier `com.voltern.sundy`
- [x] **1.4.2** Set window defaults: 1200x800, title "sundy"
- [x] **1.4.3** Configure CSP settings (set to null for development)
- [x] **1.4.4** Copy icon assets from Blazor project (`src-tauri/icons/`)

### 1.5 Port Existing Assets
- [x] **1.5.1** Copy `app.css` from `client/ui/tauri/Sundy/src/wwwroot/css/app.css`
- [x] **1.5.2** Create `src/app.css` and import in layout
- [x] **1.5.3** Verify CSS variables and styles render correctly

---

## Phase 2: Rust Backend - Core Setup

### 2.1 Project Structure
- [x] **2.1.1** Create `src-tauri/src/commands/mod.rs` module
- [x] **2.1.2** Create `src-tauri/src/db/mod.rs` module
- [x] **2.1.3** Create `src-tauri/src/models/mod.rs` module
- [x] **2.1.4** Update `lib.rs` to include new modules

### 2.2 Database Setup
- [x] **2.2.1** Add `rusqlite` and `serde` dependencies to Cargo.toml
- [x] **2.2.2** Create `db/schema.rs` with table definitions
- [x] **2.2.3** Create `db/migrations.rs` with migration logic
- [x] **2.2.4** Implement `initialize_database()` function
- [x] **2.2.5** Create database file in app data directory on startup

### 2.3 Core Models
- [x] **2.3.1** Create `models/calendar.rs` with `Calendar` struct
  - Fields: id, name, display_name, color, calendar_type, external_account_id, is_hidden, enable_blocking, receive_blocks
- [x] **2.3.2** Create `models/event.rs` with `CalendarEvent` struct
  - Fields: id, calendar_id, title, start_time, end_time, description, location, external_id, is_all_day
- [x] **2.3.3** Create `models/account.rs` with `ConnectedAccount` struct
  - Fields: id, email, display_name, provider_type, access_token, refresh_token, token_expires_at, status
- [x] **2.3.4** Implement Serialize/Deserialize for all models

---

## Phase 3: Rust Backend - Calendar Commands

### 3.1 Calendar CRUD
- [x] **3.1.1** Create `commands/calendars.rs` file
- [x] **3.1.2** Implement `get_all_calendars` command
- [x] **3.1.3** Implement `get_calendar_by_id` command
- [x] **3.1.4** Implement `create_calendar` command
- [x] **3.1.5** Implement `update_calendar` command
- [x] **3.1.6** Implement `delete_calendar` command (soft delete)
- [x] **3.1.7** Register all calendar commands in `lib.rs`
- [ ] **3.1.8** Test commands via Tauri dev tools

---

## Phase 4: Rust Backend - Event Commands

### 4.1 Event CRUD
- [x] **4.1.1** Create `commands/events.rs` file
- [x] **4.1.2** Implement `get_events_in_range` command (with calendar_ids filter)
- [x] **4.1.3** Implement `get_event_by_id` command
- [x] **4.1.4** Implement `create_event` command
- [x] **4.1.5** Implement `update_event` command
- [x] **4.1.6** Implement `delete_event` command (soft delete)
- [x] **4.1.7** Register all event commands in `lib.rs`
- [ ] **4.1.8** Test commands via Tauri dev tools

---

## Phase 5: Rust Backend - Settings Commands

### 5.1 Settings Storage
- [x] **5.1.1** Create `commands/settings.rs` file
- [x] **5.1.2** Create settings table in database schema
- [x] **5.1.3** Implement `get_setting` command (returns JSON value)
- [x] **5.1.4** Implement `set_setting` command (stores JSON value)
- [x] **5.1.5** Implement `get_all_settings` command
- [x] **5.1.6** Register settings commands in `lib.rs`

---

## Phase 6: Svelte - TypeScript Types & Tauri Wrappers

### 6.1 Type Definitions
- [x] **6.1.1** Create `src/lib/tauri/types.ts`
- [x] **6.1.2** Define `Calendar` interface matching Rust struct
- [x] **6.1.3** Define `CalendarEvent` interface matching Rust struct
- [x] **6.1.4** Define `ConnectedAccount` interface matching Rust struct
- [x] **6.1.5** Define `CalendarType` enum (Local, Microsoft, Google)
- [x] **6.1.6** Define `SyncState` type for sync status

### 6.2 Command Wrappers
- [x] **6.2.1** Create `src/lib/tauri/commands.ts`
- [x] **6.2.2** Create typed wrapper for `get_all_calendars`
- [x] **6.2.3** Create typed wrapper for `create_calendar`
- [x] **6.2.4** Create typed wrapper for `update_calendar`
- [x] **6.2.5** Create typed wrapper for `delete_calendar`
- [x] **6.2.6** Create typed wrapper for `get_events_in_range`
- [x] **6.2.7** Create typed wrapper for `create_event`
- [x] **6.2.8** Create typed wrapper for `update_event`
- [x] **6.2.9** Create typed wrapper for `delete_event`
- [x] **6.2.10** Create typed wrappers for settings commands

---

## Phase 7: Svelte - State Management

### 7.1 Calendar Store
- [x] **7.1.1** Create `src/lib/stores/calendars.svelte.ts`
- [x] **7.1.2** Implement `calendars` state with $state rune
- [x] **7.1.3** Implement `loadCalendars()` function
- [x] **7.1.4** Implement `createCalendar()` function
- [x] **7.1.5** Implement `updateCalendar()` function
- [x] **7.1.6** Implement `deleteCalendar()` function
- [x] **7.1.7** Implement `calendarLookup` derived state (id -> calendar map)
- [x] **7.1.8** Implement `visibleCalendarIds` derived state

### 7.2 Events Store
- [x] **7.2.1** Create `src/lib/stores/events.svelte.ts`
- [x] **7.2.2** Implement `events` state with $state rune
- [x] **7.2.3** Implement `loadEvents(start, end, calendarIds)` function
- [x] **7.2.4** Implement `createEvent()` function
- [x] **7.2.5** Implement `updateEvent()` function
- [x] **7.2.6** Implement `deleteEvent()` function
- [x] **7.2.7** Implement `getEventsForDate(date)` helper

### 7.3 Settings Store
- [x] **7.3.1** Create `src/lib/stores/settings.svelte.ts`
- [x] **7.3.2** Implement typed settings state
- [x] **7.3.3** Implement `loadSettings()` function
- [x] **7.3.4** Implement `updateSetting(key, value)` function
- [x] **7.3.5** Add settings: syncIntervalMinutes, privacyMode, privacyHideEmails, privacyHideEventTitles, collapsePastEvents, dynamicViewEnabled

### 7.4 UI Store
- [x] **7.4.1** Create `src/lib/stores/ui.svelte.ts`
- [x] **7.4.2** Implement `sidebarOpen` state
- [x] **7.4.3** Implement `currentView` state (day/week/month/dynamic)
- [x] **7.4.4** Implement `currentDate` state
- [x] **7.4.5** Implement `showEventModal` state
- [x] **7.4.6** Implement `showSettingsModal` state
- [x] **7.4.7** Implement `editingEvent` state (null or CalendarEvent)

---

## Phase 8: Svelte - Utility Functions

### 8.1 Date Utilities
- [x] **8.1.1** Create `src/lib/utils/date.ts`
- [x] **8.1.2** Implement `formatHeaderText(date, view)` - "January 2024", "Jan 1 - 7, 2024", etc.
- [x] **8.1.3** Implement `getWeekDays(date)` - returns 7 days for week view
- [x] **8.1.4** Implement `getMonthDays(date)` - returns 42 days (6 weeks) for month view
- [x] **8.1.5** Implement `getViewRange(date, view)` - returns start/end for data fetching
- [x] **8.1.6** Implement `isToday(date)` helper
- [x] **8.1.7** Implement `isSameDay(date1, date2)` helper
- [x] **8.1.8** Implement `formatTime(timespan)` - "9:00 AM"
- [x] **8.1.9** Implement `formatDuration(start, end)` - "1 hr 30 min"

### 8.2 Privacy Utilities
- [x] **8.2.1** Create `src/lib/utils/privacy.ts`
- [x] **8.2.2** Implement `maskEmail(email)` - "j***@***.com"
- [x] **8.2.3** Implement `getEventDisplayTitle(title, privacyMode)` - "Private Event" or actual title

---

## Phase 9: Svelte - bits-ui Component Wrappers

### 9.1 Dialog Components
- [x] **9.1.1** Create `src/lib/components/ui/Dialog.svelte` wrapper
- [x] **9.1.2** Style Dialog.Overlay with app CSS (dark overlay)
- [x] **9.1.3** Style Dialog.Content with app CSS (modal-card)
- [x] **9.1.4** Add close button styling

### 9.2 Form Components
- [x] **9.2.1** Create `src/lib/components/ui/Button.svelte` with variants (primary, secondary, cancel)
- [x] **9.2.2** Create `src/lib/components/ui/Input.svelte` styled wrapper
- [x] **9.2.3** Create `src/lib/components/ui/Textarea.svelte` styled wrapper
- [x] **9.2.4** Create `src/lib/components/ui/Checkbox.svelte` using bits-ui
- [x] **9.2.5** Create `src/lib/components/ui/Switch.svelte` using bits-ui
- [x] **9.2.6** Create `src/lib/components/ui/Select.svelte` using bits-ui

### 9.3 Other Components
- [ ] **9.3.1** Create `src/lib/components/ui/ContextMenu.svelte` using bits-ui
- [ ] **9.3.2** Create `src/lib/components/ui/Collapsible.svelte` using bits-ui
- [ ] **9.3.3** Create `src/lib/components/ui/ToggleGroup.svelte` using bits-ui
- [ ] **9.3.4** Create `src/lib/components/ui/Tabs.svelte` using bits-ui

---

## Phase 10: Svelte - Calendar View Components

### 10.1 Shared Components
- [ ] **10.1.1** Create `src/lib/components/calendar/EventBlock.svelte`
  - Props: event, calendar, hideTitle
  - Display: colored bar, time, title
- [x] **10.1.2** Create `src/lib/components/calendar/CurrentTimeIndicator.svelte`
  - Red line showing current time position
  - Auto-updates every minute
- [ ] **10.1.3** Create `src/lib/components/calendar/AllDayEvent.svelte`
  - For all-day events in week/day views (implemented inline in views)

### 10.2 Month View
- [x] **10.2.1** Create `src/lib/components/calendar/MonthView.svelte`
- [x] **10.2.2** Implement 6-week grid layout
- [x] **10.2.3** Implement day cell with date number
- [x] **10.2.4** Implement event pills (max 3-4 per cell)
- [x] **10.2.5** Implement "+N more" overflow indicator
- [x] **10.2.6** Implement day click handler (opens new event modal)
- [x] **10.2.7** Implement event click handler (opens edit event modal)
- [x] **10.2.8** Style today's date differently
- [x] **10.2.9** Style days outside current month (muted)

### 10.3 Week View
- [x] **10.3.1** Create `src/lib/components/calendar/WeekView.svelte`
- [x] **10.3.2** Implement 7-column layout with day headers
- [x] **10.3.3** Implement 24-hour time column on left
- [x] **10.3.4** Implement hourly grid lines
- [x] **10.3.5** Implement event positioning (top: minutes from midnight, height: duration)
- [ ] **10.3.6** Implement overlapping event handling
- [x] **10.3.7** Implement all-day events section at top
- [x] **10.3.8** Implement current time indicator
- [x] **10.3.9** Implement time slot click handler
- [x] **10.3.10** Implement event click handler

### 10.4 Day View
- [x] **10.4.1** Create `src/lib/components/calendar/DayView.svelte`
- [x] **10.4.2** Implement single-column layout
- [x] **10.4.3** Implement 24-hour time column on left
- [x] **10.4.4** Implement hourly grid lines
- [x] **10.4.5** Implement event positioning
- [x] **10.4.6** Implement all-day events section at top
- [x] **10.4.7** Implement current time indicator
- [x] **10.4.8** Implement time slot click handler
- [x] **10.4.9** Implement event click handler

### 10.5 Dynamic Month View (Lower Priority)
- [ ] **10.5.1** Create `src/lib/components/calendar/DynamicMonthView.svelte`
- [ ] **10.5.2** Implement virtual scrolling for months
- [ ] **10.5.3** Implement lazy loading of events
- [ ] **10.5.4** Implement "load more" functionality

---

## Phase 11: Svelte - Sidebar Components

### 11.1 Sidebar Structure
- [x] **11.1.1** Create `src/lib/components/sidebar/Sidebar.svelte`
- [x] **11.1.2** Implement sidebar container with open/closed states
- [x] **11.1.3** Implement sidebar header "My Calendars"
- [x] **11.1.4** Implement sidebar footer with settings button
- [ ] **11.1.5** Implement mobile overlay when sidebar open
- [ ] **11.1.6** Implement Tauri drag region for window dragging

### 11.2 Calendar Groups
- [x] **11.2.1** Create `src/lib/components/sidebar/CalendarGroup.svelte` (implemented inline in Sidebar.svelte)
- [x] **11.2.2** Props: groupName, calendars, isExpanded
- [ ] **11.2.3** Implement collapsible header with expand/collapse icon
- [x] **11.2.4** Implement calendar count badge

### 11.3 Calendar Items
- [x] **11.3.1** Create `src/lib/components/sidebar/CalendarItem.svelte`
- [x] **11.3.2** Props: calendar, onToggleVisibility, onRename
- [x] **11.3.3** Implement checkbox for visibility toggle
- [x] **11.3.4** Implement color dot indicator
- [x] **11.3.5** Implement calendar name display
- [ ] **11.3.6** Implement sync status indicator (for external calendars)
- [ ] **11.3.7** Implement right-click context menu (rename)

### 11.4 Rename Calendar Modal
- [ ] **11.4.1** Create `src/lib/components/sidebar/RenameCalendarModal.svelte`
- [ ] **11.4.2** Implement modal with input field
- [ ] **11.4.3** Show original name hint for external calendars
- [ ] **11.4.4** Implement save/cancel buttons

---

## Phase 12: Svelte - Toolbar Component

### 12.1 Main Toolbar
- [x] **12.1.1** Create `src/lib/components/Toolbar.svelte`
- [x] **12.1.2** Implement hamburger menu button (toggles sidebar)
- [x] **12.1.3** Implement "Today" button
- [x] **12.1.4** Implement prev/next navigation buttons
- [x] **12.1.5** Implement header text display (month/week/day name)
- [x] **12.1.6** Implement view toggle buttons (Day/Week/Month/Dynamic)
- [x] **12.1.7** Implement "New Event" button
- [x] **12.1.8** Implement Tauri drag region
- [ ] **12.1.9** Implement mobile view dropdown (replaces toggle buttons)

---

## Phase 13: Svelte - Event Dialog

### 13.1 Event Form Modal
- [x] **13.1.1** Create `src/lib/components/dialogs/EventDialog.svelte`
- [x] **13.1.2** Implement modal header with "New Event" / "Edit Event" title
- [x] **13.1.3** Implement calendar selector dropdown
- [x] **13.1.4** Implement title input field
- [x] **13.1.5** Implement date/time picker trigger (using native inputs)
- [x] **13.1.6** Implement "All day" checkbox
- [x] **13.1.7** Implement description textarea
- [x] **13.1.8** Implement Cancel and Create/Save buttons
- [x] **13.1.9** Implement form validation (title required)
- [x] **13.1.10** Wire up to events store (create/update)

### 13.2 Scheduler Modal (Optional - Lower Priority)
- [ ] **13.2.1** Create `src/lib/components/dialogs/SchedulerModal.svelte`
- [ ] **13.2.2** Implement week navigator (prev/next week buttons)
- [ ] **13.2.3** Implement day selector row (7 days)
- [ ] **13.2.4** Implement 24-hour time grid
- [ ] **13.2.5** Implement draggable time range indicator
- [ ] **13.2.6** Implement drag top handle (adjust start time)
- [ ] **13.2.7** Implement drag bottom handle (adjust end time)
- [ ] **13.2.8** Implement drag middle (move entire range)
- [ ] **13.2.9** Implement time range display (e.g., "9:00 AM -> 10:00 AM")
- [ ] **13.2.10** Implement duration display (e.g., "1 hr")
- [ ] **13.2.11** Implement Confirm/Cancel buttons

---

## Phase 14: Svelte - Settings Dialog

### 14.1 Settings Modal Structure
- [x] **14.1.1** Create `src/lib/components/dialogs/SettingsDialog.svelte`
- [x] **14.1.2** Implement modal with tabs navigation
- [x] **14.1.3** Implement "General" tab
- [x] **14.1.4** Implement "Calendars" tab
- [ ] **14.1.5** Implement "Accounts" tab
- [x] **14.1.6** Implement "Privacy" tab
- [ ] **14.1.7** Implement "Data" tab
- [x] **14.1.8** Implement "About" tab

### 14.2 General Settings Tab
- [x] **14.2.1** Implement sync interval dropdown (1, 5, 15, 30 minutes)
- [x] **14.2.2** Implement "Collapse past events" toggle
- [x] **14.2.3** Implement "Enable dynamic view" toggle

### 14.3 Calendars Tab
- [x] **14.3.1** Implement list of calendars
- [ ] **14.3.2** Implement "Add Calendar" button
- [ ] **14.3.3** Implement calendar color picker
- [ ] **14.3.4** Implement calendar delete button

### 14.4 Accounts Tab
- [ ] **14.4.1** Implement list of connected accounts
- [ ] **14.4.2** Implement "Connect Microsoft Account" button
- [ ] **14.4.3** Implement account disconnect button
- [ ] **14.4.4** Implement account sync status display

### 14.5 Privacy Tab
- [x] **14.5.1** Implement "Privacy mode" master toggle
- [x] **14.5.2** Implement "Hide email addresses" toggle
- [x] **14.5.3** Implement "Hide event titles" toggle

### 14.6 Data Tab
- [ ] **14.6.1** Implement "Export data" button (placeholder)
- [ ] **14.6.2** Implement "Import data" button (placeholder)
- [ ] **14.6.3** Implement "Reset database" button with confirmation

### 14.7 About Tab
- [x] **14.7.1** Implement version display
- [ ] **14.7.2** Implement links to website/docs

---

## Phase 15: Svelte - Main Page Assembly

### 15.1 Layout
- [x] **15.1.1** Create `src/routes/+layout.svelte`
- [x] **15.1.2** Import and apply global CSS
- [x] **15.1.3** Initialize stores on mount
- [x] **15.1.4** Set up app-wide error handling

### 15.2 Main Calendar Page
- [x] **15.2.1** Create `src/routes/+page.svelte`
- [x] **15.2.2** Implement loading state
- [x] **15.2.3** Implement error state
- [x] **15.2.4** Assemble Sidebar + Main content layout
- [x] **15.2.5** Render Toolbar
- [x] **15.2.6** Conditionally render calendar view (Day/Week/Month/Dynamic)
- [x] **15.2.7** Render EventDialog when showEventModal is true
- [x] **15.2.8** Render SettingsDialog when showSettingsModal is true
- [x] **15.2.9** Implement keyboard shortcuts (arrow keys for navigation)

---

## Phase 16: Rust Backend - Microsoft OAuth

### 16.1 Port Existing OAuth
- [ ] **16.1.1** Copy existing OAuth structs from Blazor `lib.rs`
- [ ] **16.1.2** Verify `exchange_oauth_code` command works
- [ ] **16.1.3** Verify `refresh_oauth_token` command works
- [ ] **16.1.4** Add token storage in connected_accounts table

### 16.2 Account Commands
- [ ] **16.2.1** Create `commands/accounts.rs`
- [ ] **16.2.2** Implement `get_connected_accounts` command
- [ ] **16.2.3** Implement `add_connected_account` command
- [ ] **16.2.4** Implement `update_account_tokens` command
- [ ] **16.2.5** Implement `remove_connected_account` command
- [ ] **16.2.6** Register account commands in `lib.rs`

---

## Phase 17: Rust Backend - Microsoft Graph API

### 17.1 Graph Client
- [ ] **17.1.1** Create `src-tauri/src/outlook/mod.rs`
- [ ] **17.1.2** Create `src-tauri/src/outlook/client.rs`
- [ ] **17.1.3** Add `reqwest` dependency for HTTP requests
- [ ] **17.1.4** Implement `get_user_profile()` - fetch email/display name
- [ ] **17.1.5** Implement `list_calendars()` - fetch user's calendars
- [ ] **17.1.6** Implement `list_events(calendar_id, start, end)` - fetch events
- [ ] **17.1.7** Implement `create_event(calendar_id, event)` - create event
- [ ] **17.1.8** Implement `update_event(calendar_id, event_id, event)` - update event
- [ ] **17.1.9** Implement `delete_event(calendar_id, event_id)` - delete event

### 17.2 Delta Sync
- [ ] **17.2.1** Create `src-tauri/src/outlook/sync.rs`
- [ ] **17.2.2** Create delta_tokens table in database
- [ ] **17.2.3** Implement `get_delta_token(calendar_id)` helper
- [ ] **17.2.4** Implement `save_delta_token(calendar_id, token)` helper
- [ ] **17.2.5** Implement `list_events_delta(calendar_id, delta_token)` - delta sync
- [ ] **17.2.6** Implement delta response parsing (added, modified, deleted)

---

## Phase 18: Rust Backend - Sync Commands

### 18.1 Sync Operations
- [ ] **18.1.1** Create `commands/sync.rs`
- [ ] **18.1.2** Implement `sync_account(account_id)` command
  - Refresh token if needed
  - Fetch calendars
  - Sync events for each calendar using delta
- [ ] **18.1.3** Implement `sync_all_accounts()` command
- [ ] **18.1.4** Implement `get_sync_status(calendar_id)` command
- [ ] **18.1.5** Register sync commands in `lib.rs`

### 18.2 Background Sync
- [ ] **18.2.1** Implement sync state tracking (per calendar)
- [ ] **18.2.2** Create sync_metadata table
- [ ] **18.2.3** Store last_synced_at timestamp
- [ ] **18.2.4** Store sync error messages

---

## Phase 19: Svelte - Microsoft Account Integration

### 19.1 OAuth Flow
- [ ] **19.1.1** Create `src/routes/auth-callback/+page.svelte`
- [ ] **19.1.2** Parse authorization code from URL
- [ ] **19.1.3** Call `exchange_oauth_code` Tauri command
- [ ] **19.1.4** Close popup window and notify parent
- [ ] **19.1.5** Create `src/lib/auth/microsoft.ts` helper
- [ ] **19.1.6** Implement `startMicrosoftOAuth()` - opens popup
- [ ] **19.1.7** Implement PKCE code verifier/challenge generation

### 19.2 Account Store Updates
- [ ] **19.2.1** Create `src/lib/stores/accounts.svelte.ts`
- [ ] **19.2.2** Implement `accounts` state
- [ ] **19.2.3** Implement `loadAccounts()` function
- [ ] **19.2.4** Implement `connectMicrosoftAccount()` function
- [ ] **19.2.5** Implement `disconnectAccount(id)` function

### 19.3 Sync Store
- [ ] **19.3.1** Create `src/lib/stores/sync.svelte.ts`
- [ ] **19.3.2** Implement `syncStatus` state (per calendar)
- [ ] **19.3.3** Implement `syncAccount(accountId)` function
- [ ] **19.3.4** Implement `syncAllAccounts()` function
- [ ] **19.3.5** Implement auto-sync timer based on settings

### 19.4 Sync Status Indicator
- [ ] **19.4.1** Create `src/lib/components/SyncStatusIndicator.svelte`
- [ ] **19.4.2** Props: calendarId
- [ ] **19.4.3** Display spinning icon when syncing
- [ ] **19.4.4** Display checkmark when synced
- [ ] **19.4.5** Display error icon on sync failure

---

## Phase 20: Svelte - Polish & UX

### 20.1 Toast Notifications
- [x] **20.1.1** Create `src/lib/components/ToastContainer.svelte`
- [x] **20.1.2** Create toast store with `addToast(message, type)` function
- [x] **20.1.3** Implement toast auto-dismiss after 3 seconds
- [x] **20.1.4** Style success/error/info toast variants
- [x] **20.1.5** Add toast notifications for CRUD operations

### 20.2 Status Bar (Optional - Lower Priority)
- [ ] **20.2.1** Create `src/lib/components/StatusBar.svelte`
- [ ] **20.2.2** Implement activity log display
- [ ] **20.2.3** Show recent sync activities
- [ ] **20.2.4** Position at bottom of window

### 20.3 Demo Mode Banner (Optional - Lower Priority)
- [ ] **20.3.1** Create `src/lib/components/DemoModeBanner.svelte`
- [ ] **20.3.2** Check demo mode setting on mount
- [ ] **20.3.3** Implement dismissible banner
- [ ] **20.3.4** Save dismissed state to settings

### 20.4 Responsive Design
- [x] **20.4.1** Test and fix sidebar behavior on mobile widths
- [x] **20.4.2** Implement mobile view dropdown in toolbar
- [ ] **20.4.3** Test touch interactions for scheduler drag
- [x] **20.4.4** Ensure modals are usable on small screens

### 20.5 Keyboard Navigation
- [x] **20.5.1** Implement left/right arrow for prev/next navigation
- [x] **20.5.2** Implement 'd', 'w', 'm' keys for view switching
- [x] **20.5.3** Implement 't' key for "today"
- [x] **20.5.4** Implement 'n' key for new event
- [x] **20.5.5** Implement Escape to close modals

---

## Phase 21: Testing & Bug Fixes

### 21.1 Manual Testing
- [ ] **21.1.1** Test all calendar CRUD operations
- [ ] **21.1.2** Test all event CRUD operations
- [ ] **21.1.3** Test all calendar views render correctly
- [ ] **21.1.4** Test Microsoft account connection flow
- [ ] **21.1.5** Test sync operations
- [ ] **21.1.6** Test settings persistence
- [ ] **21.1.7** Test on macOS
- [ ] **21.1.8** Test on Windows
- [ ] **21.1.9** Test on Linux

### 21.2 Bug Fixes
- [ ] **21.2.1** (Reserved for bug fixes discovered during testing)
- [ ] **21.2.2** (Reserved for bug fixes discovered during testing)
- [ ] **21.2.3** (Reserved for bug fixes discovered during testing)

---

## Completion Log

| Date | Task ID | Completed By | Notes |
|------|---------|--------------|-------|
| 2024-12-31 | 1.1.1 - 1.5.3 | AI | Phase 1 complete: Project scaffolded with SvelteKit, bits-ui, Tauri SQL plugin, OAuth commands ported |
| 2024-12-31 | 2.1.1 - 2.3.4 | AI | Phase 2 complete: Rust models, database schema, migrations |
| 2024-12-31 | 3.1.1 - 3.1.7 | AI | Phase 3 complete: Calendar CRUD commands |
| 2024-12-31 | 4.1.1 - 4.1.7 | AI | Phase 4 complete: Event CRUD commands |
| 2024-12-31 | 5.1.1 - 5.1.6 | AI | Phase 5 complete: Settings commands |
| 2024-12-31 | 6.1.1 - 6.2.10 | AI | Phase 6 complete: TypeScript types and Tauri command wrappers |
| 2024-12-31 | 7.1.1 - 7.4.7 | AI | Phase 7 complete: Svelte 5 stores (calendars, events, settings, UI) |
| 2024-12-31 | 8.1.1 - 8.2.3 | AI | Phase 8 complete: Date and privacy utility functions |
| 2024-12-31 | 9.1.1 - 9.2.6 | AI | Phase 9 partial: Core UI components (Dialog, Button, Input, Textarea, Checkbox, Switch, Select) |
| 2024-12-31 | 10.2.x, 10.3.x, 10.4.x | AI | Phase 10 partial: MonthView, WeekView, DayView basic implementations |
| 2024-12-31 | 11.1.x, 11.3.x | AI | Phase 11 partial: Sidebar and CalendarItem components |
| 2024-12-31 | 12.1.1 - 12.1.8 | AI | Phase 12 mostly complete: Toolbar component |
| 2024-12-31 | 13.1.1 - 13.1.10 | AI | Phase 13.1 complete: EventDialog component |
| 2024-12-31 | 14.x | AI | Phase 14 partial: SettingsDialog with General, Calendars, Privacy, About tabs |
| 2024-12-31 | 15.1.x, 15.2.1-8 | AI | Phase 15 mostly complete: Main page assembly |
| 2024-12-31 | 20.1.x, 20.4.x, 20.5.x | AI | Phase 20 mostly complete: Toast notifications, responsive design, keyboard shortcuts |

---

## Architecture Reference

### Technology Stack
| Layer | Technology |
|-------|------------|
| Desktop Shell | Tauri 2.0 (Rust) |
| Frontend | SvelteKit + Svelte 5 |
| UI Components | bits-ui (headless) + existing CSS |
| Database | Tauri SQL plugin (SQLite in Rust) |
| State | Svelte 5 runes ($state, $derived) |
| Core Logic | Rust (all business logic via Tauri commands) |

### Key Directories
```
client/ui/tauri-svelte/
├── src/                          # SvelteKit frontend
│   ├── routes/                   # Pages
│   ├── lib/
│   │   ├── components/           # Svelte components
│   │   ├── stores/               # State management
│   │   ├── tauri/                # Tauri command wrappers
│   │   └── utils/                # Helper functions
│   └── app.css                   # Global styles
├── src-tauri/                    # Rust backend
│   ├── src/
│   │   ├── commands/             # Tauri commands
│   │   ├── db/                   # Database layer
│   │   ├── models/               # Data models
│   │   └── outlook/              # Microsoft Graph API
│   └── Cargo.toml
└── IMPLEMENTATION_PLAN.md        # This file
```

### bits-ui Components Used
| Sundycal Feature | bits-ui Component |
|------------------|-------------------|
| Event/Settings modals | Dialog |
| Calendar picker | Calendar, Date Picker |
| Sidebar groups | Collapsible |
| Context menu | Context Menu |
| View switcher | Toggle Group |
| Dropdowns | Select, Dropdown Menu |
| Checkboxes | Checkbox |
| Switches | Switch |
| Tabs | Tabs |
| Time picker | Time Field |

---

## Notes

- Each task should take 1-4 hours for a junior developer
- Tasks within a phase can often be parallelized
- Update this document as tasks are completed
- Add new tasks to "Bug Fixes" section as issues are discovered
- Reference the Blazor implementation at `client/ui/tauri/Sundy/` for UI patterns
