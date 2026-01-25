/**
 * TypeScript types matching the Rust backend models.
 * Keep these in sync with src-tauri/src/models/*.rs
 */

// Calendar types
export type CalendarType = 'Local' | 'Microsoft' | 'Google';

export interface Calendar {
  id: string;
  name: string;
  displayName: string | null;
  color: string;
  calendarType: CalendarType;
  externalAccountId: string | null;
  externalId: string | null;
  isHidden: boolean;
  isDeleted: boolean;
  enableBlocking: boolean;
  receiveBlocks: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface NewCalendar {
  name: string;
  color: string;
  displayName?: string;
}

// Event types
export interface CalendarEvent {
  id: string;
  calendarId: string;
  title: string;
  description: string | null;
  location: string | null;
  startTime: string;  // ISO 8601 datetime
  endTime: string;    // ISO 8601 datetime
  isAllDay: boolean;
  externalId: string | null;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface NewEvent {
  calendarId: string;
  title: string;
  description?: string;
  location?: string;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
}

export interface UpdateEvent {
  id: string;
  calendarId?: string;
  title?: string;
  description?: string;
  location?: string;
  startTime?: string;
  endTime?: string;
  isAllDay?: boolean;
}

// Account types
export type ProviderType = 'Microsoft' | 'Google';

export type AccountStatus = 'Active' | 'TokenExpired' | 'Error' | 'Disconnected';

export interface ConnectedAccount {
  id: string;
  email: string;
  displayName: string | null;
  providerType: ProviderType;
  accessToken: string;
  refreshToken: string | null;
  tokenExpiresAt: string | null;
  status: AccountStatus;
  lastSyncAt: string | null;
  createdAt: string;
  updatedAt: string;
}

// Settings types
export interface Setting {
  key: string;
  value: string;
}

// App settings interface for typed access
export interface AppSettings {
  syncIntervalMinutes: number;
  privacyMode: boolean;
  privacyHideEmails: boolean;
  privacyHideEventTitles: boolean;
  collapsePastEvents: boolean;
  dynamicViewEnabled: boolean;
}

// OAuth types (matching lib.rs)
export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  scope: string;
  refresh_token?: string;
}

// Sync types
export type SyncStatus = 'idle' | 'syncing' | 'success' | 'error';

export interface SyncState {
  status: SyncStatus;
  lastSyncAt: string | null;
  error: string | null;
}

// UI types
export type CalendarView = 'day' | 'week' | 'month' | 'dynamic';

// Helper to get display name for a calendar
export function getCalendarDisplayName(calendar: Calendar): string {
  return calendar.displayName ?? calendar.name;
}
