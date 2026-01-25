/**
 * UI state store using Svelte 5 runes.
 * Manages global UI state like current view, date, modals, etc.
 */

import type { CalendarView, CalendarEvent } from '$lib/tauri/types';

// State
let sidebarOpen = $state(true);
let currentView = $state<CalendarView>('week');
let currentDate = $state(new Date());
let showEventModal = $state(false);
let showSettingsModal = $state(false);
let editingEvent = $state<CalendarEvent | null>(null);
let selectedDate = $state<Date | null>(null);
let selectedStartTime = $state<string | null>(null);

// Derived state
const currentMonth = $derived(currentDate.getMonth());
const currentYear = $derived(currentDate.getFullYear());
const currentDayOfWeek = $derived(currentDate.getDay());

/**
 * Format the header text based on current view and date
 */
const headerText = $derived.by(() => {
  const options: Intl.DateTimeFormatOptions = { month: 'long', year: 'numeric' };
  
  switch (currentView) {
    case 'day':
      return currentDate.toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
        year: 'numeric',
      });
    case 'week':
      // Show the week range
      const weekStart = new Date(currentDate);
      weekStart.setDate(currentDate.getDate() - currentDate.getDay());
      const weekEnd = new Date(weekStart);
      weekEnd.setDate(weekStart.getDate() + 6);
      
      if (weekStart.getMonth() === weekEnd.getMonth()) {
        return `${weekStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${weekEnd.getDate()}, ${weekEnd.getFullYear()}`;
      } else {
        return `${weekStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${weekEnd.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`;
      }
    case 'month':
    case 'dynamic':
    default:
      return currentDate.toLocaleDateString('en-US', options);
  }
});

// Actions
function toggleSidebar(): void {
  sidebarOpen = !sidebarOpen;
}

function setSidebarOpen(open: boolean): void {
  sidebarOpen = open;
}

function setView(view: CalendarView): void {
  currentView = view;
}

function setCurrentDate(date: Date): void {
  currentDate = date;
}

function goToToday(): void {
  currentDate = new Date();
}

function goToPrevious(): void {
  const newDate = new Date(currentDate);
  
  switch (currentView) {
    case 'day':
      newDate.setDate(newDate.getDate() - 1);
      break;
    case 'week':
      newDate.setDate(newDate.getDate() - 7);
      break;
    case 'month':
    case 'dynamic':
      newDate.setMonth(newDate.getMonth() - 1);
      break;
  }
  
  currentDate = newDate;
}

function goToNext(): void {
  const newDate = new Date(currentDate);
  
  switch (currentView) {
    case 'day':
      newDate.setDate(newDate.getDate() + 1);
      break;
    case 'week':
      newDate.setDate(newDate.getDate() + 7);
      break;
    case 'month':
    case 'dynamic':
      newDate.setMonth(newDate.getMonth() + 1);
      break;
  }
  
  currentDate = newDate;
}

function openEventModal(event?: CalendarEvent, date?: Date, startTime?: string): void {
  editingEvent = event ?? null;
  selectedDate = date ?? null;
  selectedStartTime = startTime ?? null;
  showEventModal = true;
}

function closeEventModal(): void {
  showEventModal = false;
  editingEvent = null;
  selectedDate = null;
  selectedStartTime = null;
}

function openSettingsModal(): void {
  showSettingsModal = true;
}

function closeSettingsModal(): void {
  showSettingsModal = false;
}

// Export the store
export function useUI() {
  return {
    // Getters for reactive state
    get sidebarOpen() { return sidebarOpen; },
    get currentView() { return currentView; },
    get currentDate() { return currentDate; },
    get showEventModal() { return showEventModal; },
    get showSettingsModal() { return showSettingsModal; },
    get editingEvent() { return editingEvent; },
    get selectedDate() { return selectedDate; },
    get selectedStartTime() { return selectedStartTime; },
    get currentMonth() { return currentMonth; },
    get currentYear() { return currentYear; },
    get currentDayOfWeek() { return currentDayOfWeek; },
    get headerText() { return headerText; },
    
    // Actions
    toggleSidebar,
    setSidebarOpen,
    setView,
    setCurrentDate,
    goToToday,
    goToPrevious,
    goToNext,
    openEventModal,
    closeEventModal,
    openSettingsModal,
    closeSettingsModal,
  };
}

// Create a singleton instance
const uiStore = useUI();
export default uiStore;
