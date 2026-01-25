<script lang="ts">
  import { useCalendars, useUI } from '$lib/stores';
  import CalendarItem from './CalendarItem.svelte';

  const calendars = useCalendars();
  const ui = useUI();

  // Close sidebar on mobile when clicking overlay
  function handleOverlayClick() {
    ui.setSidebarOpen(false);
  }
</script>

<!-- Mobile overlay -->
{#if ui.sidebarOpen}
  <button 
    class="sidebar-overlay" 
    class:visible={ui.sidebarOpen}
    onclick={handleOverlayClick}
    aria-label="Close sidebar"
  ></button>
{/if}

<aside class="calendar-sidebar" class:open={ui.sidebarOpen}>
  <!-- Drag region for macOS traffic lights -->
  <div class="sidebar-drag-region"></div>
  
  <!-- Mobile close button row -->
  <div class="sidebar-toggle-row">
    <button class="sidebar-toggle-btn" onclick={() => ui.setSidebarOpen(false)} aria-label="Close sidebar">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18"></line>
        <line x1="6" y1="6" x2="18" y2="18"></line>
      </svg>
    </button>
  </div>

  <div class="sidebar-header">
    <h2>Calendars</h2>
  </div>
  
  <div class="calendar-list">
    {#if calendars.loading}
      <div class="loading">Loading calendars...</div>
    {:else if calendars.calendars.length === 0}
      <div class="empty">No calendars found</div>
    {:else}
      {#each calendars.calendarsByType.local as calendar (calendar.id)}
        <CalendarItem {calendar} />
      {/each}
      
      {#if calendars.calendarsByType.microsoft.length > 0}
        {#each calendars.calendarsByType.microsoft as calendar (calendar.id)}
          <CalendarItem {calendar} />
        {/each}
      {/if}
    {/if}
  </div>
  
  <div class="sidebar-footer">
    <button class="settings-btn" onclick={() => ui.openSettingsModal()} title="Settings (,)">
      <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="3"></circle>
        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
      </svg>
      Settings
    </button>
  </div>
</aside>

<style>
  /* Mobile overlay */
  .sidebar-overlay {
    display: none;
  }

  .calendar-sidebar {
    width: 280px;
    background-color: var(--bg-secondary);
    border-right: 1px solid var(--border-color);
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
    margin-left: -280px;
    transition: margin-left 0.3s ease;
  }

  .calendar-sidebar.open {
    margin-left: 0;
  }

  /* Drag region for macOS traffic lights */
  .sidebar-drag-region {
    height: 28px;
    -webkit-app-region: drag;
    app-region: drag;
    flex-shrink: 0;
  }

  .sidebar-toggle-row {
    display: none;
    justify-content: flex-end;
    padding: 12px 16px 0;
  }

  .sidebar-toggle-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    background-color: var(--bg-tertiary);
    border: none;
    border-radius: 50%;
    color: #e0e0e0;
    cursor: pointer;
    transition: background-color 0.15s;
    box-shadow: none;
    padding: 0;
  }

  .sidebar-toggle-btn:hover {
    background-color: var(--bg-hover);
  }

  .sidebar-header {
    padding: 12px 20px 16px;
    border-bottom: 1px solid var(--border-color);
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .sidebar-header h2 {
    margin: 0;
    font-size: 16px;
    font-weight: 500;
    color: var(--text-primary);
  }

  .calendar-list {
    flex: 1;
    padding: 12px 16px;
    overflow-y: auto;
  }

  .loading, .empty {
    padding: 16px;
    text-align: center;
    color: var(--text-secondary);
    font-size: 14px;
  }

  .sidebar-footer {
    padding: 16px;
    border-top: 1px solid var(--border-color);
  }

  .settings-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    background: transparent;
    border: none;
    color: var(--text-secondary);
    cursor: pointer;
    padding: 8px;
    border-radius: 6px;
    font-size: 14px;
    width: 100%;
    box-shadow: none;
    transition: background-color 0.15s, color 0.15s;
  }

  .settings-btn:hover {
    background-color: var(--bg-tertiary);
    color: var(--text-primary);
  }

  /* Mobile behavior - flyout overlay */
  @media (max-width: 640px) {
    .sidebar-overlay {
      display: block;
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0);
      z-index: 99;
      pointer-events: none;
      transition: background-color 0.3s ease;
      border: none;
      cursor: pointer;
    }

    .sidebar-overlay.visible {
      background-color: rgba(0, 0, 0, 0.5);
      pointer-events: auto;
    }

    .calendar-sidebar {
      position: fixed;
      top: 0;
      left: 0;
      bottom: 0;
      z-index: 100;
      margin-left: 0;
      transform: translateX(-100%);
      box-shadow: none;
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .calendar-sidebar.open {
      transform: translateX(0);
      box-shadow: 4px 0 20px rgba(0, 0, 0, 0.3);
    }

    .sidebar-toggle-row {
      display: flex;
    }
  }
</style>
