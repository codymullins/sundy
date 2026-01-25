/**
 * Settings store using Svelte 5 runes.
 * Manages application settings with type-safe access.
 */

import type { AppSettings } from '$lib/tauri/types';
import * as commands from '$lib/tauri/commands';

// Default settings
const defaultSettings: AppSettings = {
  syncIntervalMinutes: 15,
  privacyMode: false,
  privacyHideEmails: false,
  privacyHideEventTitles: false,
  collapsePastEvents: false,
  dynamicViewEnabled: true,
};

// State
let settings = $state<AppSettings>({ ...defaultSettings });
let loading = $state(false);
let error = $state<string | null>(null);

/**
 * Load all settings from the backend
 */
async function load(): Promise<void> {
  loading = true;
  error = null;
  
  try {
    const allSettings = await commands.getAllSettings();
    
    // Parse settings into typed object
    const parsed: Partial<AppSettings> = {};
    
    for (const setting of allSettings) {
      switch (setting.key) {
        case 'syncIntervalMinutes':
          parsed.syncIntervalMinutes = Number(setting.value) || defaultSettings.syncIntervalMinutes;
          break;
        case 'privacyMode':
          parsed.privacyMode = setting.value === 'true';
          break;
        case 'privacyHideEmails':
          parsed.privacyHideEmails = setting.value === 'true';
          break;
        case 'privacyHideEventTitles':
          parsed.privacyHideEventTitles = setting.value === 'true';
          break;
        case 'collapsePastEvents':
          parsed.collapsePastEvents = setting.value === 'true';
          break;
        case 'dynamicViewEnabled':
          parsed.dynamicViewEnabled = setting.value === 'true';
          break;
      }
    }
    
    settings = { ...defaultSettings, ...parsed };
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to load settings:', error);
  } finally {
    loading = false;
  }
}

/**
 * Update a single setting
 */
async function set<K extends keyof AppSettings>(
  key: K,
  value: AppSettings[K]
): Promise<boolean> {
  error = null;
  
  try {
    // Convert value to string for storage
    const stringValue = typeof value === 'boolean' 
      ? (value ? 'true' : 'false')
      : String(value);
    
    await commands.setSetting(key, stringValue);
    settings = { ...settings, [key]: value };
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to update setting:', error);
    return false;
  }
}

/**
 * Get a single setting value
 */
function get<K extends keyof AppSettings>(key: K): AppSettings[K] {
  return settings[key];
}

/**
 * Reset all settings to defaults
 */
async function reset(): Promise<boolean> {
  error = null;
  
  try {
    // Update each setting in the backend
    const entries = Object.entries(defaultSettings) as [keyof AppSettings, AppSettings[keyof AppSettings]][];
    
    for (const [key, value] of entries) {
      const stringValue = typeof value === 'boolean' 
        ? (value ? 'true' : 'false')
        : String(value);
      await commands.setSetting(key, stringValue);
    }
    
    settings = { ...defaultSettings };
    return true;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
    console.error('Failed to reset settings:', error);
    return false;
  }
}

// Export the store
export function useSettings() {
  return {
    // Getters for reactive state
    get settings() { return settings; },
    get loading() { return loading; },
    get error() { return error; },
    
    // Convenience getters for common settings
    get syncIntervalMinutes() { return settings.syncIntervalMinutes; },
    get privacyMode() { return settings.privacyMode; },
    get privacyHideEmails() { return settings.privacyHideEmails; },
    get privacyHideEventTitles() { return settings.privacyHideEventTitles; },
    get collapsePastEvents() { return settings.collapsePastEvents; },
    get dynamicViewEnabled() { return settings.dynamicViewEnabled; },
    
    // Actions
    load,
    set,
    get,
    reset,
  };
}

// Create a singleton instance
const settingsStore = useSettings();
export default settingsStore;
