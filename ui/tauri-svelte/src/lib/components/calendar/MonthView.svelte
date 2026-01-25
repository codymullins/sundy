<script lang="ts">
  import { useUI, useEvents, useCalendars } from '$lib/stores';
  import type { CalendarEvent } from '$lib/tauri/types';

  const ui = useUI();
  const events = useEvents();
  const calendars = useCalendars();

  const weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  // Get the days for the current month view (6 weeks)
  const monthDays = $derived.by(() => {
    const year = ui.currentYear;
    const month = ui.currentMonth;
    
    // First day of the month
    const firstDay = new Date(year, month, 1);
    // Start from the Sunday of the week containing the first day
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - firstDay.getDay());
    
    // Generate 42 days (6 weeks)
    return Array.from({ length: 42 }, (_, i) => {
      const day = new Date(startDate);
      day.setDate(startDate.getDate() + i);
      return day;
    });
  });

  // Filter events to only show visible calendars
  const visibleEvents = $derived(
    events.filterByCalendars(calendars.visibleCalendarIds)
  );

  // Maximum events to display before showing "+N more"
  const MAX_VISIBLE_EVENTS = 3;

  // Get events for a specific day
  function getEventsForDay(date: Date): CalendarEvent[] {
    const dateStr = date.toISOString().split('T')[0];
    return visibleEvents.filter(event => {
      const eventDate = event.startTime.split('T')[0];
      return eventDate === dateStr;
    });
  }

  // Get events to display (limited)
  function getVisibleEventsForDay(date: Date): CalendarEvent[] {
    return getEventsForDay(date).slice(0, MAX_VISIBLE_EVENTS);
  }

  // Get count of additional events
  function getMoreEventsCount(date: Date): number {
    const allEvents = getEventsForDay(date);
    return Math.max(0, allEvents.length - MAX_VISIBLE_EVENTS);
  }

  // Check if date is in current month
  function isCurrentMonth(date: Date): boolean {
    return date.getMonth() === ui.currentMonth;
  }

  // Check if a date is today
  function isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  // Get calendar color for an event
  function getEventColor(event: CalendarEvent): string {
    const calendar = calendars.getById(event.calendarId);
    return calendar?.color ?? '#3b82f6';
  }

  // Handle day click
  function handleDayClick(date: Date) {
    ui.openEventModal(undefined, date);
  }

  // Handle event click
  function handleEventClick(event: CalendarEvent, e: MouseEvent) {
    e.stopPropagation();
    ui.openEventModal(event);
  }
</script>

<div class="month-view">
  <!-- Weekday headers -->
  <div class="weekday-header">
    {#each weekDays as day}
      <div class="weekday">{day}</div>
    {/each}
  </div>
  
  <!-- Calendar grid -->
  <div class="month-grid">
    {#each monthDays as day, i (i)}
      <div 
        class="day-cell"
        class:other-month={!isCurrentMonth(day)}
        class:today={isToday(day)}
        role="button"
        tabindex="0"
        onclick={() => handleDayClick(day)}
        onkeydown={(e) => e.key === 'Enter' && handleDayClick(day)}
      >
        <span class="day-number" class:today={isToday(day)}>{day.getDate()}</span>
        <div class="day-events">
          {#each getVisibleEventsForDay(day) as event (event.id)}
            <button
              class="event-pill"
              style="background-color: {getEventColor(event)};"
              onclick={(e) => handleEventClick(event, e)}
            >
              {event.title}
            </button>
          {/each}
          {#if getMoreEventsCount(day) > 0}
            <button 
              class="more-events"
              onclick={(e) => { e.stopPropagation(); ui.setCurrentDate(day); ui.setView('day'); }}
            >
              +{getMoreEventsCount(day)} more
            </button>
          {/if}
        </div>
      </div>
    {/each}
  </div>
</div>

<style>
  .month-view {
    display: flex;
    flex-direction: column;
    height: 100%;
  }

  .weekday-header {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    background: var(--bg-primary, #fff);
  }

  .weekday {
    padding: 12px;
    text-align: center;
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
    color: var(--text-secondary, #666);
  }

  .month-grid {
    flex: 1;
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    grid-template-rows: repeat(6, 1fr);
  }

  .day-cell {
    border: none;
    background: var(--bg-primary, #fff);
    border-right: 1px solid var(--border-color, #e0e0e0);
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    padding: 4px;
    text-align: left;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    min-height: 100px;
    transition: background-color 0.1s;
  }

  .day-cell:hover {
    background: var(--bg-hover, #f8f9fa);
  }

  .day-cell.other-month {
    background: var(--bg-secondary, #f8f9fa);
  }

  .day-cell.other-month .day-number {
    color: var(--text-tertiary, #999);
  }

  .day-cell.today {
    background: var(--accent-bg, #eff6ff);
  }

  .day-number {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    font-size: 14px;
    font-weight: 500;
    color: var(--text-primary, #1a1a1a);
    border-radius: 50%;
    margin-bottom: 4px;
  }

  .day-number.today {
    background: var(--accent-color, #3b82f6);
    color: white;
  }

  .day-events {
    display: flex;
    flex-direction: column;
    gap: 2px;
    overflow: hidden;
  }

  .event-pill {
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 11px;
    font-weight: 500;
    color: white;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    border: none;
    text-align: left;
    cursor: pointer;
    transition: opacity 0.15s;
  }

  .event-pill:hover {
    opacity: 0.85;
  }

  .more-events {
    font-size: 11px;
    color: var(--text-secondary, #666);
    background: transparent;
    border: none;
    padding: 2px 6px;
    cursor: pointer;
    text-align: left;
    border-radius: 4px;
    transition: background-color 0.15s, color 0.15s;
  }

  .more-events:hover {
    background: var(--bg-hover, #f0f0f0);
    color: var(--text-primary, #1a1a1a);
  }
</style>
