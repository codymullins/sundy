<script lang="ts">
  import { useUI, useEvents, useCalendars } from '$lib/stores';
  import type { CalendarEvent } from '$lib/tauri/types';
  import CurrentTimeIndicator from './CurrentTimeIndicator.svelte';

  const ui = useUI();
  const events = useEvents();
  const calendars = useCalendars();

  // Generate hours for the time column
  const hours = Array.from({ length: 24 }, (_, i) => i);

  // Get the current day
  const currentDay = $derived(ui.currentDate);

  // Filter events to only show visible calendars
  const visibleEvents = $derived(
    events.filterByCalendars(calendars.visibleCalendarIds)
  );

  // Get regular (non-all-day) events for the current day
  const dayEvents = $derived.by(() => {
    const dateStr = currentDay.toISOString().split('T')[0];
    return visibleEvents.filter(event => {
      const eventDate = event.startTime.split('T')[0];
      return eventDate === dateStr && !event.isAllDay;
    });
  });

  // Get all-day events for the current day
  const allDayEvents = $derived.by(() => {
    const dateStr = currentDay.toISOString().split('T')[0];
    return visibleEvents.filter(event => {
      const eventDate = event.startTime.split('T')[0];
      return eventDate === dateStr && event.isAllDay;
    });
  });

  // Calculate event position and height
  function getEventStyle(event: CalendarEvent): string {
    const start = new Date(event.startTime);
    const end = new Date(event.endTime);
    
    const startMinutes = start.getHours() * 60 + start.getMinutes();
    const endMinutes = end.getHours() * 60 + end.getMinutes();
    const duration = endMinutes - startMinutes;
    
    const top = (startMinutes / 60) * 60; // 60px per hour
    const height = Math.max((duration / 60) * 60, 24); // minimum 24px
    
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
  function handleTimeSlotClick(hour: number) {
    const startTime = new Date(currentDay);
    startTime.setHours(hour, 0, 0, 0);
    ui.openEventModal(undefined, currentDay, startTime.toISOString());
  }

  // Handle clicking on an event
  function handleEventClick(event: CalendarEvent, e: MouseEvent) {
    e.stopPropagation();
    ui.openEventModal(event);
  }
</script>

<div class="day-view">
  <!-- Header -->
  <div class="day-header" class:today={isToday(currentDay)}>
    <span class="day-name">{currentDay.toLocaleDateString('en-US', { weekday: 'long' })}</span>
    <span class="day-date">{currentDay.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' })}</span>
  </div>
  
  <!-- All-day events section -->
  {#if allDayEvents.length > 0}
    <div class="all-day-section">
      <div class="all-day-label">All day</div>
      <div class="all-day-events">
        {#each allDayEvents as event (event.id)}
          <button
            class="all-day-event"
            style="background-color: {getEventColor(event)};"
            onclick={(e) => handleEventClick(event, e)}
          >
            {event.title}
          </button>
        {/each}
      </div>
    </div>
  {/if}
  
  <!-- Scrollable time grid -->
  <div class="day-body">
    <div class="time-grid">
      <!-- Time column -->
      <div class="time-gutter">
        {#each hours as hour}
          <div class="time-slot-label">
            <span>{formatHour(hour)}</span>
          </div>
        {/each}
      </div>
      
      <!-- Day column -->
      <div class="day-column" class:today={isToday(currentDay)}>
        {#each hours as hour}
          <button 
            class="time-slot"
            onclick={() => handleTimeSlotClick(hour)}
          ></button>
        {/each}
        
        <!-- Events -->
        <div class="events-container">
          {#each dayEvents as event (event.id)}
            <button
              class="event-block"
              style="{getEventStyle(event)} background-color: {getEventColor(event)};"
              onclick={(e) => handleEventClick(event, e)}
            >
              <span class="event-time">
                {new Date(event.startTime).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })} - 
                {new Date(event.endTime).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })}
              </span>
              <span class="event-title">{event.title}</span>
              {#if event.description}
                <span class="event-description">{event.description}</span>
              {/if}
            </button>
          {/each}
          
          <!-- Current time indicator (only shown if viewing today) -->
          {#if isToday(currentDay)}
            <CurrentTimeIndicator hourHeight={60} showLabel={true} />
          {/if}
        </div>
      </div>
    </div>
  </div>
</div>

<style>
  .day-view {
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
  }

  .day-header {
    padding: 16px;
    text-align: center;
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    background: var(--bg-primary, #fff);
    flex-shrink: 0;
  }

  .day-header.today {
    background: var(--accent-bg, #eff6ff);
  }

  .day-name {
    display: block;
    font-size: 14px;
    font-weight: 500;
    color: var(--text-secondary, #666);
    text-transform: uppercase;
    margin-bottom: 4px;
  }

  .day-date {
    display: block;
    font-size: 20px;
    font-weight: 600;
    color: var(--text-primary, #1a1a1a);
  }

  /* All-day events section */
  .all-day-section {
    display: flex;
    border-bottom: 1px solid var(--border-color, #e0e0e0);
    background: var(--bg-primary, #fff);
    min-height: 40px;
    flex-shrink: 0;
  }

  .all-day-label {
    width: 70px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    padding-right: 12px;
    font-size: 12px;
    color: var(--text-secondary, #666);
    border-right: 1px solid var(--border-color, #e0e0e0);
  }

  .all-day-events {
    flex: 1;
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
    padding: 8px;
  }

  .all-day-event {
    padding: 4px 10px;
    border-radius: 4px;
    font-size: 13px;
    font-weight: 500;
    color: white;
    border: none;
    cursor: pointer;
    white-space: nowrap;
    transition: opacity 0.15s;
  }

  .all-day-event:hover {
    opacity: 0.85;
  }

  .day-body {
    flex: 1;
    overflow-y: auto;
  }

  .time-grid {
    display: flex;
    position: relative;
  }

  .time-gutter {
    width: 70px;
    flex-shrink: 0;
    border-right: 1px solid var(--border-color, #e0e0e0);
  }

  .time-slot-label {
    height: 60px;
    padding: 0 12px;
    display: flex;
    align-items: flex-start;
    justify-content: flex-end;
  }

  .time-slot-label span {
    font-size: 12px;
    color: var(--text-secondary, #666);
    transform: translateY(-8px);
  }

  .day-column {
    flex: 1;
    position: relative;
  }

  .day-column.today {
    background: var(--accent-bg, #eff6ff);
  }

  .time-slot {
    height: 60px;
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
    left: 8px;
    right: 8px;
    pointer-events: none;
  }

  .event-block {
    position: absolute;
    left: 0;
    right: 0;
    padding: 8px 12px;
    border-radius: 6px;
    color: white;
    font-size: 13px;
    overflow: hidden;
    cursor: pointer;
    pointer-events: auto;
    border: none;
    text-align: left;
    transition: opacity 0.15s, transform 0.15s;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .event-block:hover {
    opacity: 0.9;
    transform: scale(1.005);
  }

  .event-time {
    font-size: 11px;
    opacity: 0.9;
  }

  .event-title {
    font-weight: 600;
    font-size: 14px;
  }

  .event-description {
    font-size: 12px;
    opacity: 0.85;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
</style>
