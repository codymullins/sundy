/**
 * Events store using Svelte 5 runes.
 * Manages calendar events and synchronization with the Rust backend.
 */

import type { CalendarEvent, NewEvent } from '$lib/tauri/types';
import * as commands from '$lib/tauri/commands';

// State
let events = $state<CalendarEvent[]>([]);
let loading = $state(false);
let error = $state<string | null>(null);
let currentRange = $state<{ start: string; end: string } | null>(null);

// Derived state - events indexed by date (YYYY-MM-DD)
const eventsByDate = $derived(
  events.reduce((acc, event) => {
    // Get the date portion of the start time
    const date = event.startTime.split('T')[0];
    if (!acc[date]) {
      acc[date] = [];
    }
    acc[date].push(event);
    return acc;
  }, {} as Record<string, CalendarEvent[]>)
);

// Events indexed by calendar ID
const eventsByCalendar = $derived(
  events.reduce((acc, event) => {
    if (!acc[event.calendarId]) {
      acc[event.calendarId] = [];
    }
    acc[event.calendarId].push(event);
    return acc;
  }, {} as Record<string, CalendarEvent[]>)
);

/**
 * Load events for a date range
 */
async function load(
  start: string,
  end: string,
  calendarIds?: string[]
): Promise<void> {
  loading = true;
  error = null;
  
  try {
    events = await commands.getEventsInRange(start, end, calendarIds);
    currentRange = { start, end };
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to load events:', error);
  } finally {
    loading = false;
  }
}

/**
 * Refresh events for the current range
 */
async function refresh(calendarIds?: string[]): Promise<void> {
  if (!currentRange) return;
  await load(currentRange.start, currentRange.end, calendarIds);
}

/**
 * Create a new event
 */
async function create(newEvent: NewEvent): Promise<CalendarEvent | null> {
  error = null;
  
  try {
    const created = await commands.createEvent(newEvent);
    events = [...events, created];
    return created;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to create event:', error);
    return null;
  }
}

/**
 * Update an existing event
 */
async function update(event: CalendarEvent): Promise<boolean> {
  error = null;
  
  try {
    await commands.updateEvent(event);
    events = events.map(e => e.id === event.id ? event : e);
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to update event:', error);
    return false;
  }
}

/**
 * Delete an event (soft delete)
 */
async function remove(id: string): Promise<boolean> {
  error = null;
  
  try {
    await commands.deleteEvent(id);
    events = events.filter(e => e.id !== id);
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to delete event:', error);
    return false;
  }
}

/**
 * Get events for a specific date
 */
function getForDate(date: string): CalendarEvent[] {
  return eventsByDate[date] ?? [];
}

/**
 * Get events for a specific calendar
 */
function getForCalendar(calendarId: string): CalendarEvent[] {
  return eventsByCalendar[calendarId] ?? [];
}

/**
 * Get an event by ID
 */
function getById(id: string): CalendarEvent | undefined {
  return events.find(e => e.id === id);
}

/**
 * Filter events by visible calendar IDs
 */
function filterByCalendars(calendarIds: string[]): CalendarEvent[] {
  const idSet = new Set(calendarIds);
  return events.filter(e => idSet.has(e.calendarId));
}

// Export the store
export function useEvents() {
  return {
    // Getters for reactive state
    get events() { return events; },
    get loading() { return loading; },
    get error() { return error; },
    get currentRange() { return currentRange; },
    get eventsByDate() { return eventsByDate; },
    get eventsByCalendar() { return eventsByCalendar; },
    
    // Actions
    load,
    refresh,
    create,
    update,
    remove,
    getForDate,
    getForCalendar,
    getById,
    filterByCalendars,
  };
}

// Create a singleton instance
const eventsStore = useEvents();
export default eventsStore;
