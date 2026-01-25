/**
 * Typed wrappers for Tauri commands.
 * These provide type-safe access to the Rust backend.
 */

import { invoke } from '@tauri-apps/api/core';
import type {
  Calendar,
  NewCalendar,
  CalendarEvent,
  NewEvent,
  Setting,
  TokenResponse,
} from './types';

// ============================================
// Calendar Commands
// ============================================

/**
 * Get all calendars (excluding deleted ones)
 */
export async function getAllCalendars(): Promise<Calendar[]> {
  return invoke<Calendar[]>('get_all_calendars');
}

/**
 * Get a calendar by ID
 */
export async function getCalendarById(id: string): Promise<Calendar | null> {
  return invoke<Calendar | null>('get_calendar_by_id', { id });
}

/**
 * Create a new calendar
 */
export async function createCalendar(calendar: NewCalendar): Promise<Calendar> {
  return invoke<Calendar>('create_calendar', { calendar });
}

/**
 * Update an existing calendar
 */
export async function updateCalendar(calendar: Calendar): Promise<void> {
  return invoke<void>('update_calendar', { calendar });
}

/**
 * Delete a calendar (soft delete)
 */
export async function deleteCalendar(id: string): Promise<void> {
  return invoke<void>('delete_calendar', { id });
}

// ============================================
// Event Commands
// ============================================

/**
 * Get events within a date range
 * @param start ISO 8601 datetime string
 * @param end ISO 8601 datetime string
 * @param calendarIds Optional array of calendar IDs to filter by
 */
export async function getEventsInRange(
  start: string,
  end: string,
  calendarIds?: string[]
): Promise<CalendarEvent[]> {
  return invoke<CalendarEvent[]>('get_events_in_range', {
    start,
    end,
    calendarIds: calendarIds ?? null,
  });
}

/**
 * Get an event by ID
 */
export async function getEventById(id: string): Promise<CalendarEvent | null> {
  return invoke<CalendarEvent | null>('get_event_by_id', { id });
}

/**
 * Create a new event
 */
export async function createEvent(event: NewEvent): Promise<CalendarEvent> {
  return invoke<CalendarEvent>('create_event', { event });
}

/**
 * Update an existing event
 */
export async function updateEvent(event: CalendarEvent): Promise<void> {
  return invoke<void>('update_event', { event });
}

/**
 * Delete an event (soft delete)
 */
export async function deleteEvent(id: string): Promise<void> {
  return invoke<void>('delete_event', { id });
}

// ============================================
// Settings Commands
// ============================================

/**
 * Get a setting value by key
 */
export async function getSetting(key: string): Promise<string | null> {
  return invoke<string | null>('get_setting', { key });
}

/**
 * Set a setting value
 */
export async function setSetting(key: string, value: string): Promise<void> {
  return invoke<void>('set_setting', { key, value });
}

/**
 * Get all settings
 */
export async function getAllSettings(): Promise<Setting[]> {
  return invoke<Setting[]>('get_all_settings');
}

// ============================================
// OAuth Commands
// ============================================

/**
 * Exchange an authorization code for OAuth tokens
 */
export async function exchangeOAuthCode(
  code: string,
  codeVerifier: string,
  redirectUri: string
): Promise<TokenResponse> {
  return invoke<TokenResponse>('exchange_oauth_code', {
    code,
    codeVerifier,
    redirectUri,
  });
}

/**
 * Refresh OAuth tokens using a refresh token
 */
export async function refreshOAuthToken(refreshToken: string): Promise<TokenResponse> {
  return invoke<TokenResponse>('refresh_oauth_token', { refreshToken });
}

// ============================================
// Helper Functions
// ============================================

/**
 * Get a typed setting value
 */
export async function getTypedSetting<T>(key: string, defaultValue: T): Promise<T> {
  const value = await getSetting(key);
  if (value === null) return defaultValue;
  
  try {
    // Handle booleans stored as "true"/"false"
    if (typeof defaultValue === 'boolean') {
      return (value === 'true') as T;
    }
    // Handle numbers
    if (typeof defaultValue === 'number') {
      return Number(value) as T;
    }
    // Handle JSON objects
    if (typeof defaultValue === 'object') {
      return JSON.parse(value) as T;
    }
    return value as T;
  } catch {
    return defaultValue;
  }
}

/**
 * Set a typed setting value
 */
export async function setTypedSetting<T>(key: string, value: T): Promise<void> {
  let stringValue: string;
  
  if (typeof value === 'boolean') {
    stringValue = value ? 'true' : 'false';
  } else if (typeof value === 'number') {
    stringValue = String(value);
  } else if (typeof value === 'object') {
    stringValue = JSON.stringify(value);
  } else {
    stringValue = String(value);
  }
  
  await setSetting(key, stringValue);
}
