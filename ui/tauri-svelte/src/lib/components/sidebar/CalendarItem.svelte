<script lang="ts">
  import type { Calendar } from '$lib/tauri/types';
  import { useCalendars } from '$lib/stores';
  import { getCalendarDisplayName } from '$lib/tauri/types';

  interface Props {
    calendar: Calendar;
  }

  let { calendar }: Props = $props();
  
  const calendars = useCalendars();
</script>

<div class="calendar-item">
  <label class="checkbox-wrapper">
    <input 
      type="checkbox" 
      checked={!calendar.isHidden}
      onchange={() => calendars.toggleVisibility(calendar.id)}
    />
    <span class="color-dot" style="background-color: {calendar.color}"></span>
    <span class="calendar-name">{getCalendarDisplayName(calendar)}</span>
  </label>
</div>

<style>
  .calendar-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 8px 4px;
    cursor: pointer;
    border-radius: 6px;
    transition: background-color 0.15s;
  }

  .calendar-item:hover {
    background-color: var(--bg-tertiary);
  }

  .checkbox-wrapper {
    display: flex;
    align-items: center;
    gap: 12px;
    cursor: pointer;
    font-size: 14px;
    color: #e0e0e0;
    width: 100%;
  }

  .checkbox-wrapper input[type="checkbox"] {
    width: 18px;
    height: 18px;
    accent-color: var(--accent-color);
    cursor: pointer;
    margin: 0;
    padding: 0;
    box-shadow: none;
    border-radius: 4px;
  }

  .color-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  .calendar-name {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #e0e0e0;
  }
</style>
