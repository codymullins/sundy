<script lang="ts">
  import { useUI, useEvents, useCalendars } from '$lib/stores';
  import type { CalendarEvent } from '$lib/tauri/types';
  import CurrentTimeIndicator from './CurrentTimeIndicator.svelte';

  const ui = useUI();
  const events = useEvents();
  const calendars = useCalendars();

  // Generate hours for the time column
  const hours = Array.from({ length: 24 }, (_, i) => i);

  // Get the days for the current week
  const weekDays = $derived.by(() => {
    const date = ui.currentDate;
    const dayOfWeek = date.getDay();
    const weekStart = new Date(date);
    weekStart.setDate(date.getDate() - dayOfWeek);
    
    return Array.from({ length: 7 }, (_, i) => {
      const day = new Date(weekStart);
      day.setDate(weekStart.getDate() + i);
      return day;
    });
  });

  // Filter events to only show visible calendars
  const visibleEvents = $derived(
    events.filterByCalendars(calendars.visibleCalendarIds)
  );

  // Get regular (non-all-day) events for a specific day
  function getEventsForDay(date: Date): CalendarEvent[] {
    const dateStr = date.toISOString().split('T')[0];
    return visibleEvents.filter(event => {
      const eventDate = event.startTime.split('T')[0];
      return eventDate === dateStr && !event.isAllDay;
    });
  }

  // Get all-day events for a specific day
  function getAllDayEventsForDay(date: Date): CalendarEvent[] {
    const dateStr = date.toISOString().split('T')[0];
    return visibleEvents.filter(event => {
      const eventDate = event.startTime.split('T')[0];
      return eventDate === dateStr && event.isAllDay;
    });
  }

  // Check if there are any all-day events in the week
  const hasAllDayEvents = $derived.by(() => {
    return weekDays.some(day => getAllDayEventsForDay(day).length > 0);
  });

  // Calculate event position and height
  function getEventStyle(event: CalendarEvent): string {
    const start = new Date(event.startTime);
    const end = new Date(event.endTime);
    
    const startMinutes = start.getHours() * 60 + start.getMinutes();
    const endMinutes = end.getHours() * 60 + end.getMinutes();
    const duration = endMinutes - startMinutes;
    
    const top = (startMinutes / 60) * 48; // 48px per hour
    const height = Math.max((duration / 60) * 48, 20); // minimum 20px
    
    return `top: ${top}px; height: ${height}px;`;
  }

  // Get calendar color for an event
  function getEventColor(event: CalendarEvent): string {
    const calendar = calendars.getById(event.calendarId);
    return calendar?.color ?? '#3b82f6';
  }

  // Format time
  function formatHour(hour: number): string {
    if (hour === 0) return '12 AM';
    if (hour < 12) return `${hour} AM`;
    if (hour === 12) return '12 PM';
    return `${hour - 12} PM`;
  }

  // Check if a date is today
  function isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  // Handle clicking on a time slot
  function handleTimeSlotClick(date: Date, hour: number) {
    const startTime = new Date(date);
    startTime.setHours(hour, 0, 0, 0);
    ui.openEventModal(undefined, date, startTime.toISOString());
  }

  // Handle clicking on an event
  function handleEventClick(event: CalendarEvent, e: MouseEvent) {
    e.stopPropagation();
    ui.openEventModal(event);
  }
</script>

<div class="week-view">
  <!-- Header with day names -->
  <div class="week-header">
    <div class="time-gutter-header"></div>
    {#each weekDays as day, i (i)}
      <div class="day-header" class:today={isToday(day)}>
        <span class="day-name">{day.toLocaleDateString('en-US', { weekday: 'short' })}</span>
        <span class="day-number" class:today={isToday(day)}>{day.getDate()}</span>
      </div>
    {/each}
  </div>
  
  <!-- All-day events section -->
  {#if hasAllDayEvents}
    <div class="all-day-section">
      <div class="all-day-label">All day</div>
      <div class="all-day-events">
        {#each weekDays as day, i (i)}
          <div class="all-day-cell" class:today={isToday(day)}>
            {#each getAllDayEventsForDay(day) as event (event.id)}
              <button
                class="all-day-event"
                style="background-color: {getEventColor(event)};"
                onclick={(e) => handleEventClick(event, e)}
              >
                {event.title}
              </button>
            {/each}
          </div>
        {/each}
      </div>
    </div>
  {/if}
  
  <!-- Scrollable time grid -->
  <div class="week-body">
    <div class="time-grid">
      <!-- Time column -->
      <div class="time-gutter">
        {#each hours as hour}
          <div class="time-slot-label">
            <span>{formatHour(hour)}</span>
          </div>
        {/each}
      </div>
      
      <!-- Day columns -->
      {#each weekDays as day, dayIndex (dayIndex)}
        <div class="day-column" class:today={isToday(day)}>
          {#each hours as hour}
            <button 
              class="time-slot"
              onclick={() => handleTimeSlotClick(day, hour)}
            ></button>
          {/each}
          
          <!-- Events for this day -->
          <div class="events-container">
            {#each getEventsForDay(day) as event (event.id)}
              <button
                class="event-block"
                style="{getEventStyle(event)} background-color: {getEventColor(event)};"
                onclick={(e) => handleEventClick(event, e)}
              >
                <span class="event-time">
                  {new Date(event.startTime).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })}
                </span>
                <span class="event-title">{event.title}</span>
              </button>
            {/each}
            
            <!-- Current time indicator (only shown on today's column) -->
            {#if isToday(day)}
              <CurrentTimeIndicator hourHeight={48} showLabel={false} />
            {/if}
          </div>
        </div>
      {/each}
    </div>
  </div>
</div>

<style>
  .week-view {
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
  }

  .week-header {
    display: flex;
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    background: var(--bg-primary, #fff);
    flex-shrink: 0;
  }

  .time-gutter-header {
    width: 60px;
    flex-shrink: 0;
  }

  .day-header {
    flex: 1;
    padding: 8px;
    text-align: center;
    border-left: 1px solid var(--border-color, #e0e0e0);
  }

  .day-header.today {
    background: var(--accent-bg, #eff6ff);
  }

  .day-name {
    display: block;
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary, #666);
    text-transform: uppercase;
  }

  .day-number {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    font-size: 18px;
    font-weight: 600;
    color: var(--text-primary, #1a1a1a);
    border-radius: 50%;
  }

  .day-number.today {
    background: var(--accent-color, #3b82f6);
    color: white;
  }

  /* All-day events section */
  .all-day-section {
    display: flex;
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    background: var(--bg-primary, #fff);
    min-height: 32px;
    flex-shrink: 0;
  }

  .all-day-label {
    width: 60px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    padding-right: 8px;
    font-size: 11px;
    color: var(--text-secondary, #666);
    border-right: 1px solid var(--border-color, #e0e0e0);
  }

  .all-day-events {
    flex: 1;
    display: flex;
  }

  .all-day-cell {
    flex: 1;
    padding: 4px;
    border-left: 1px solid var(--border-color, #e0e0e0);
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .all-day-cell.today {
    background: var(--accent-bg, #eff6ff);
  }

  .all-day-event {
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 11px;
    font-weight: 500;
    color: white;
    border: none;
    cursor: pointer;
    text-align: left;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    transition: opacity 0.15s;
  }

  .all-day-event:hover {
    opacity: 0.85;
  }

  .week-body {
    flex: 1;
    overflow-y: auto;
  }

  .time-grid {
    display: flex;
    position: relative;
  }

  .time-gutter {
    width: 60px;
    flex-shrink: 0;
    border-right: 1px solid var(--border-color, #e0e0e0);
  }

  .time-slot-label {
    height: 48px;
    padding: 0 8px;
    display: flex;
    align-items: flex-start;
    justify-content: flex-end;
  }

  .time-slot-label span {
    font-size: 11px;
    color: var(--text-secondary, #666);
    transform: translateY(-6px);
  }

  .day-column {
    flex: 1;
    position: relative;
    border-left: 1px solid var(--border-color, #e0e0e0);
  }

  .day-column.today {
    background: var(--accent-bg, #eff6ff);
  }

  .time-slot {
    height: 48px;
    width: 100%;
    border: none;
    background: transparent;
    border-bottom: 1px solid var(--border-color-light, #f0f0f0);
    cursor: pointer;
    transition: background-color 0.1s;
  }

  .time-slot:hover {
    background: var(--bg-hover, rgba(0, 0, 0, 0.03));
  }

  .events-container {
    position: absolute;
    top: 0;
    left: 4px;
    right: 4px;
    pointer-events: none;
  }

  .event-block {
    position: absolute;
    left: 0;
    right: 0;
    padding: 4px 6px;
    border-radius: 4px;
    color: white;
    font-size: 12px;
    overflow: hidden;
    cursor: pointer;
    pointer-events: auto;
    border: none;
    text-align: left;
    transition: opacity 0.15s, transform 0.15s;
  }

  .event-block:hover {
    opacity: 0.9;
    transform: scale(1.01);
  }

  .event-time {
    display: block;
    font-size: 10px;
    opacity: 0.9;
  }

  .event-title {
    display: block;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
</style>
