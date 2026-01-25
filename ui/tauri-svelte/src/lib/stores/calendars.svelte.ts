/**
 * Calendars store using Svelte 5 runes.
 * Manages calendar data and synchronization with the Rust backend.
 */

import type { Calendar, NewCalendar } from '$lib/tauri/types';
import * as commands from '$lib/tauri/commands';

// State
let calendars = $state<Calendar[]>([]);
let loading = $state(false);
let error = $state<string | null>(null);

// Derived state
const calendarLookup = $derived(
  calendars.reduce((acc, cal) => {
    acc[cal.id] = cal;
    return acc;
  }, {} as Record<string, Calendar>)
);

const visibleCalendars = $derived(
  calendars.filter(cal => !cal.isHidden)
);

const visibleCalendarIds = $derived(
  visibleCalendars.map(cal => cal.id)
);

// Group calendars by type
const calendarsByType = $derived({
  local: calendars.filter(cal => cal.calendarType === 'Local'),
  microsoft: calendars.filter(cal => cal.calendarType === 'Microsoft'),
  google: calendars.filter(cal => cal.calendarType === 'Google'),
});

/**
 * Load all calendars from the backend
 */
async function load(): Promise<void> {
  loading = true;
  error = null;
  
  try {
    calendars = await commands.getAllCalendars();
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to load calendars:', error);
  } finally {
    loading = false;
  }
}

/**
 * Create a new calendar
 */
async function create(newCalendar: NewCalendar): Promise<Calendar | null> {
  error = null;
  
  try {
    const created = await commands.createCalendar(newCalendar);
    calendars = [...calendars, created];
    return created;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to create calendar:', error);
    return null;
  }
}

/**
 * Update an existing calendar
 */
async function update(calendar: Calendar): Promise<boolean> {
  error = null;
  
  try {
    await commands.updateCalendar(calendar);
    calendars = calendars.map(c => c.id === calendar.id ? calendar : c);
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to update calendar:', error);
    return false;
  }
}

/**
 * Delete a calendar (soft delete)
 */
async function remove(id: string): Promise<boolean> {
  error = null;
  
  try {
    await commands.deleteCalendar(id);
    calendars = calendars.filter(c => c.id !== id);
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to delete calendar:', error);
    return false;
  }
}

/**
 * Toggle calendar visibility
 */
async function toggleVisibility(id: string): Promise<boolean> {
  const calendar = calendarLookup[id];
  if (!calendar) return false;
  
  return update({
    ...calendar,
    isHidden: !calendar.isHidden,
  });
}

/**
 * Get a calendar by ID
 */
function getById(id: string): Calendar | undefined {
  return calendarLookup[id];
}

// Export the store
export function useCalendars() {
  return {
    // Getters for reactive state
    get calendars() { return calendars; },
    get loading() { return loading; },
    get error() { return error; },
    get calendarLookup() { return calendarLookup; },
    get visibleCalendars() { return visibleCalendars; },
    get visibleCalendarIds() { return visibleCalendarIds; },
    get calendarsByType() { return calendarsByType; },
    
    // Actions
    load,
    create,
    update,
    remove,
    toggleVisibility,
    getById,
  };
}

// Create a singleton instance
const calendarsStore = useCalendars();
export default calendarsStore;
