<script lang="ts">
  import { useUI } from '$lib/stores';
  import type { CalendarView } from '$lib/tauri/types';

  const ui = useUI();

  // Mobile view dropdown state
  let showViewDropdown = $state(false);

  const viewLabels: Record<CalendarView, string> = {
    day: 'Day',
    week: 'Week',
    month: 'Month',
    dynamic: 'Dynamic',
  };

  // Calculate slider position and width for the animated view toggle
  function getSliderStyle(view: CalendarView): string {
    const views: CalendarView[] = ['day', 'week', 'month'];
    const index = views.indexOf(view);
    if (index === -1) return 'transform: translateX(0); width: 52px;';
    
    // Each button is approximately 52px wide (padding + text)
    const widths = [42, 52, 60]; // Day, Week, Month
    const positions = [0, 42, 94]; // cumulative positions
    
    return `transform: translateX(${positions[index]}px); width: ${widths[index]}px;`;
  }

  function selectView(view: CalendarView) {
    ui.setView(view);
    showViewDropdown = false;
  }

  function handleClickOutside(e: MouseEvent) {
    const target = e.target as HTMLElement;
    if (!target.closest('.view-dropdown-mobile')) {
      showViewDropdown = false;
    }
  }
</script>

<svelte:window onclick={handleClickOutside} />

<div class="calendar-toolbar" data-tauri-drag-region>
  <div class="toolbar-left">
    <button class="menu-btn" onclick={() => ui.toggleSidebar()} title="Toggle sidebar (B)">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <line x1="3" y1="12" x2="21" y2="12"></line>
        <line x1="3" y1="6" x2="21" y2="6"></line>
        <line x1="3" y1="18" x2="21" y2="18"></line>
      </svg>
    </button>
    
    <button class="today-btn" onclick={() => ui.goToToday()} title="Go to today (T)">Today</button>
    
    <button class="nav-btn" onclick={() => ui.goToPrevious()} title="Previous (Left arrow)">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="15 18 9 12 15 6"></polyline>
      </svg>
    </button>
    <button class="nav-btn" onclick={() => ui.goToNext()} title="Next (Right arrow)">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="9 18 15 12 9 6"></polyline>
      </svg>
    </button>
    
    <h1 class="current-month">{ui.headerText}</h1>
  </div>
  
  <div class="toolbar-right">
    <!-- Desktop view toggle with animated slider -->
    <div class="view-toggle">
      <div class="view-slider" style={getSliderStyle(ui.currentView)}></div>
      <button 
        class="view-btn" 
        class:active={ui.currentView === 'day'}
        onclick={() => ui.setView('day')}
        title="Day view (D)"
      >
        Day
      </button>
      <button 
        class="view-btn" 
        class:active={ui.currentView === 'week'}
        onclick={() => ui.setView('week')}
        title="Week view (W)"
      >
        Week
      </button>
      <button 
        class="view-btn" 
        class:active={ui.currentView === 'month'}
        onclick={() => ui.setView('month')}
        title="Month view (M)"
      >
        Month
      </button>
    </div>

    <!-- Mobile view dropdown -->
    <div class="view-dropdown-mobile">
      <button 
        class="view-dropdown-btn"
        onclick={(e) => { e.stopPropagation(); showViewDropdown = !showViewDropdown; }}
        aria-haspopup="true"
        aria-expanded={showViewDropdown}
        title="Change view"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="7" height="7"></rect>
          <rect x="14" y="3" width="7" height="7"></rect>
          <rect x="14" y="14" width="7" height="7"></rect>
          <rect x="3" y="14" width="7" height="7"></rect>
        </svg>
      </button>
      
      {#if showViewDropdown}
        <div class="view-dropdown-menu" role="menu">
          <button 
            class="view-dropdown-item" 
            class:active={ui.currentView === 'day'}
            onclick={() => selectView('day')}
            role="menuitem"
          >
            Day
          </button>
          <button 
            class="view-dropdown-item" 
            class:active={ui.currentView === 'week'}
            onclick={() => selectView('week')}
            role="menuitem"
          >
            Week
          </button>
          <button 
            class="view-dropdown-item" 
            class:active={ui.currentView === 'month'}
            onclick={() => selectView('month')}
            role="menuitem"
          >
            Month
          </button>
        </div>
      {/if}
    </div>
    
    <button class="new-event-btn" onclick={() => ui.openEventModal()} title="New event (N)">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <line x1="12" y1="5" x2="12" y2="19"></line>
        <line x1="5" y1="12" x2="19" y2="12"></line>
      </svg>
      <span>New Event</span>
    </button>
  </div>
</div>

<style>
  .calendar-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 20px;
    height: 60px;
    min-height: 60px;
    max-height: 60px;
    background-color: var(--bg-secondary);
    border-bottom: 1px solid var(--border-color);
    flex-shrink: 0;
    flex-wrap: nowrap;
    position: relative;
    z-index: 10;
    -webkit-app-region: drag;
    app-region: drag;
  }

  .toolbar-left {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-wrap: nowrap;
    min-width: 0;
    overflow: hidden;
    position: relative;
    z-index: 1;
    pointer-events: none;
  }

  .toolbar-right {
    display: flex;
    align-items: center;
    gap: 12px;
    flex-shrink: 0;
    position: relative;
    z-index: 1;
    pointer-events: none;
  }

  /* Re-enable pointer events on interactive elements */
  .toolbar-left button,
  .toolbar-right button,
  .view-toggle,
  .view-dropdown-mobile {
    pointer-events: auto;
    -webkit-app-region: no-drag;
    app-region: no-drag;
  }

  .menu-btn,
  .nav-btn {
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

  .menu-btn:hover,
  .nav-btn:hover {
    background-color: var(--bg-hover);
  }

  .today-btn {
    display: flex;
    align-items: center;
    height: 36px;
    padding: 0 16px;
    background-color: var(--bg-tertiary);
    border: none;
    border-radius: 20px;
    color: #e0e0e0;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 0.15s;
    box-shadow: none;
  }

  .today-btn:hover {
    background-color: var(--bg-hover);
  }

  .current-month {
    margin: 0;
    margin-left: 12px;
    font-size: 20px;
    font-weight: 500;
    color: var(--text-primary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    min-width: 0;
    pointer-events: none;
  }

  /* View toggle with animated slider */
  .view-toggle {
    display: flex;
    align-items: center;
    height: 36px;
    background-color: var(--bg-tertiary);
    border-radius: 20px;
    overflow: hidden;
    position: relative;
    padding: 0 4px;
  }

  .view-slider {
    position: absolute;
    top: 4px;
    bottom: 4px;
    left: 4px;
    background: var(--accent-gradient);
    border-radius: 14px;
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1),
                width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 2px 8px rgba(124, 58, 237, 0.4);
    z-index: 0;
  }

  .view-btn {
    padding: 6px 14px;
    background: transparent;
    border: none;
    color: var(--text-secondary);
    font-size: 14px;
    cursor: pointer;
    transition: color 0.2s ease;
    box-shadow: none;
    position: relative;
    z-index: 1;
    border-radius: 6px;
  }

  .view-btn:hover {
    color: #e0e0e0;
  }

  .view-btn.active {
    color: #ffffff;
    background: transparent;
  }

  /* Mobile view dropdown */
  .view-dropdown-mobile {
    display: none;
    position: relative;
  }

  .view-dropdown-btn {
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

  .view-dropdown-btn:hover {
    background-color: var(--bg-hover);
  }

  .view-dropdown-menu {
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 8px;
    background-color: var(--bg-tertiary);
    border-radius: 20px;
    padding: 4px;
    min-width: 120px;
    z-index: 1000;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
  }

  .view-dropdown-item {
    display: flex;
    align-items: center;
    gap: 10px;
    width: 100%;
    padding: 8px 14px;
    background: transparent;
    border: none;
    border-radius: 0;
    color: var(--text-secondary);
    font-size: 14px;
    text-align: left;
    cursor: pointer;
    transition: color 0.2s ease;
    box-shadow: none;
  }

  .view-dropdown-item:hover {
    color: #e0e0e0;
  }

  .view-dropdown-item:first-child {
    border-radius: 14px 14px 0 0;
  }

  .view-dropdown-item:last-child {
    border-radius: 0 0 14px 14px;
  }

  .view-dropdown-item:only-child {
    border-radius: 14px;
  }

  .view-dropdown-item.active {
    background: var(--accent-gradient);
    color: #ffffff;
  }

  /* New event button */
  .new-event-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    height: 36px;
    padding: 0 16px;
    background-color: var(--accent-color);
    border: none;
    border-radius: 20px;
    color: #ffffff;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 0.15s;
    box-shadow: none;
    flex-shrink: 0;
    white-space: nowrap;
  }

  .new-event-btn:hover {
    background-color: var(--accent-color-hover);
  }

  /* Responsive */
  @media (max-width: 640px) {
    .view-toggle {
      display: none !important;
    }

    .view-dropdown-mobile {
      display: block;
    }

    .calendar-toolbar {
      padding: 0 12px;
    }

    .current-month {
      font-size: 16px;
    }
  }

  @media (max-width: 900px) {
    .new-event-btn span {
      display: none;
    }

    .new-event-btn {
      padding: 8px 12px;
    }
  }
</style>
