<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { useUI, useCalendars, useEvents, useSettings } from '$lib/stores';
  import Toolbar from '$lib/components/Toolbar.svelte';
  import Sidebar from '$lib/components/sidebar/Sidebar.svelte';
  import WeekView from '$lib/components/calendar/WeekView.svelte';
  import MonthView from '$lib/components/calendar/MonthView.svelte';
  import DayView from '$lib/components/calendar/DayView.svelte';
  import EventDialog from '$lib/components/dialogs/EventDialog.svelte';
  import SettingsDialog from '$lib/components/dialogs/SettingsDialog.svelte';
  import ToastContainer from '$lib/components/ToastContainer.svelte';

  const ui = useUI();
  const calendars = useCalendars();
  const events = useEvents();
  const settings = useSettings();

  let initialized = $state(false);
  let error = $state<string | null>(null);

  // Keyboard shortcuts handler
  function handleKeydown(e: KeyboardEvent) {
    // Don't trigger shortcuts when typing in inputs
    if (e.target instanceof HTMLInputElement || 
        e.target instanceof HTMLTextAreaElement ||
        e.target instanceof HTMLSelectElement) {
      return;
    }

    // Don't trigger when modals are open (except Escape)
    if ((ui.showEventModal || ui.showSettingsModal) && e.key !== 'Escape') {
      return;
    }

    switch (e.key) {
      case 'ArrowLeft':
        e.preventDefault();
        ui.goToPrevious();
        break;
      case 'ArrowRight':
        e.preventDefault();
        ui.goToNext();
        break;
      case 't':
      case 'T':
        e.preventDefault();
        ui.goToToday();
        break;
      case 'd':
      case 'D':
        e.preventDefault();
        ui.setView('day');
        break;
      case 'w':
      case 'W':
        e.preventDefault();
        ui.setView('week');
        break;
      case 'm':
      case 'M':
        e.preventDefault();
        ui.setView('month');
        break;
      case 'n':
      case 'N':
        e.preventDefault();
        ui.openEventModal();
        break;
      case 'Escape':
        if (ui.showEventModal) {
          ui.closeEventModal();
        } else if (ui.showSettingsModal) {
          ui.closeSettingsModal();
        }
        break;
      case ',':
        // Open settings with comma (like many apps)
        if (!ui.showEventModal) {
          e.preventDefault();
          ui.openSettingsModal();
        }
        break;
    }
  }

  // Load data on mount
  onMount(async () => {
    // Add keyboard listener
    window.addEventListener('keydown', handleKeydown);

    try {
      // Load all initial data
      await Promise.all([
        calendars.load(),
        settings.load(),
      ]);

      // Load events for the current view range
      await loadEventsForCurrentView();
      
      initialized = true;
    } catch (e) {
      error = e instanceof Error ? e.message : String(e);
      console.error('Failed to initialize:', e);
    }
  });

  onDestroy(() => {
    window.removeEventListener('keydown', handleKeydown);
  });

  // Reload events when date, view, or visible calendars change
  $effect(() => {
    if (initialized) {
      // Access reactive values to trigger effect
      const _ = ui.currentDate;
      const __ = ui.currentView;
      const ___ = calendars.visibleCalendarIds;
      loadEventsForCurrentView();
    }
  });

  async function loadEventsForCurrentView() {
    const { start, end } = getViewDateRange();
    await events.load(start, end, calendars.visibleCalendarIds);
  }

  function getViewDateRange(): { start: string; end: string } {
    const date = ui.currentDate;
    let start: Date;
    let end: Date;

    switch (ui.currentView) {
      case 'day':
        start = new Date(date);
        start.setHours(0, 0, 0, 0);
        end = new Date(date);
        end.setHours(23, 59, 59, 999);
        break;
      
      case 'week':
        start = new Date(date);
        start.setDate(date.getDate() - date.getDay());
        start.setHours(0, 0, 0, 0);
        end = new Date(start);
        end.setDate(start.getDate() + 6);
        end.setHours(23, 59, 59, 999);
        break;
      
      case 'month':
      case 'dynamic':
      default:
        // Load a bit extra for month view (prev/next month days)
        start = new Date(date.getFullYear(), date.getMonth(), 1);
        start.setDate(start.getDate() - 7);
        end = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        end.setDate(end.getDate() + 7);
        break;
    }

    return {
      start: start.toISOString(),
      end: end.toISOString(),
    };
  }
</script>

<div class="app-container">
  {#if error}
    <div class="error-screen">
      <h1>Failed to load</h1>
      <p>{error}</p>
      <button onclick={() => window.location.reload()}>Retry</button>
    </div>
  {:else if !initialized}
    <div class="loading-screen">
      <div class="spinner"></div>
      <p>Loading Sundycal...</p>
    </div>
  {:else}
    <Toolbar />
    <div class="main-content">
      <Sidebar />
      <main class="calendar-container">
        {#if ui.currentView === 'day'}
          <DayView />
        {:else if ui.currentView === 'week'}
          <WeekView />
        {:else if ui.currentView === 'month'}
          <MonthView />
        {:else}
          <MonthView />
        {/if}
      </main>
    </div>
    
    <!-- Dialogs -->
    <EventDialog />
    <SettingsDialog />
  {/if}
  
  <!-- Toast notifications (always visible) -->
  <ToastContainer />
</div>

<style>
  .app-container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    width: 100vw;
    overflow: hidden;
    background: var(--bg-primary, #fff);
  }

  .main-content {
    display: flex;
    flex: 1;
    overflow: hidden;
  }

  .calendar-container {
    flex: 1;
    overflow: hidden;
    background: var(--bg-primary, #fff);
  }

  .loading-screen, .error-screen {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    gap: 16px;
  }

  .loading-screen p, .error-screen p {
    color: var(--text-secondary, #666);
    font-size: 14px;
  }

  .error-screen h1 {
    color: var(--text-primary, #1a1a1a);
    font-size: 24px;
    margin: 0;
  }

  .error-screen button {
    padding: 8px 16px;
    border: none;
    background: var(--accent-color, #3b82f6);
    color: white;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
  }

  .spinner {
    width: 40px;
    height: 40px;
    border: 3px solid var(--border-color, #e0e0e0);
    border-top-color: var(--accent-color, #3b82f6);
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
