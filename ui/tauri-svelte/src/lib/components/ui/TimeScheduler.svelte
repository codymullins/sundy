<script lang="ts">
  interface Props {
    startTime: Date;
    endTime: Date;
    onTimeChange: (start: Date, end: Date) => void;
  }

  let { startTime, endTime, onTimeChange }: Props = $props();

  // Internal state
  let selectedDate = $state(new Date(startTime));
  let weekStart = $state(getWeekStart(new Date(startTime)));
  let startHour = $state(startTime.getHours());
  let startMinutes = $state(Math.floor(startTime.getMinutes() / 30) * 30);
  let endHour = $state(endTime.getHours());
  let endMinutes = $state(Math.floor(endTime.getMinutes() / 30) * 30);
  
  // Drag state
  let isDragging = $state(false);
  let dragType = $state<'move' | 'top' | 'bottom' | null>(null);
  let dragStartY = $state(0);
  let dragStartTop = $state(0);
  let dragStartBottom = $state(0);

  // Time grid configuration
  const SLOT_HEIGHT = 24; // pixels per 30 min slot
  const HOUR_HEIGHT = SLOT_HEIGHT * 2;
  const START_HOUR = 0;
  const END_HOUR = 24;

  // Generate hours array (0-23)
  const hours = Array.from({ length: END_HOUR - START_HOUR }, (_, i) => i + START_HOUR);

  // Generate week days
  const weekDays = $derived(getWeekDays(weekStart));

  function getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Adjust for Sunday
    d.setDate(diff);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  function getWeekDays(start: Date): Date[] {
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(start);
      d.setDate(d.getDate() + i);
      return d;
    });
  }

  function formatDayAbbrev(date: Date): string {
    return date.toLocaleDateString('en-US', { weekday: 'short' }).toUpperCase();
  }

  function formatHour(hour: number): string {
    if (hour === 0) return '12 AM';
    if (hour === 12) return '12 PM';
    if (hour < 12) return `${hour} AM`;
    return `${hour - 12} PM`;
  }

  function formatTimeRange(sHour: number, sMin: number, eHour: number, eMin: number): string {
    const formatPart = (h: number, m: number) => {
      const period = h >= 12 ? 'PM' : 'AM';
      const hour = h === 0 ? 12 : h > 12 ? h - 12 : h;
      return m === 0 ? `${hour} ${period}` : `${hour}:${String(m).padStart(2, '0')} ${period}`;
    };
    return `${formatPart(sHour, sMin)} - ${formatPart(eHour, eMin)}`;
  }

  function getDuration(sHour: number, sMin: number, eHour: number, eMin: number): string {
    const totalMinutes = (eHour * 60 + eMin) - (sHour * 60 + sMin);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    if (hours === 0) return `${minutes} min`;
    if (minutes === 0) return hours === 1 ? '1 hour' : `${hours} hours`;
    return `${hours}h ${minutes}m`;
  }

  function isSameDay(d1: Date, d2: Date): boolean {
    return d1.getFullYear() === d2.getFullYear() &&
           d1.getMonth() === d2.getMonth() &&
           d1.getDate() === d2.getDate();
  }

  function isToday(date: Date): boolean {
    return isSameDay(date, new Date());
  }

  // Computed indicator position
  const indicatorTop = $derived((startHour - START_HOUR) * HOUR_HEIGHT + (startMinutes / 30) * SLOT_HEIGHT);
  const indicatorHeight = $derived(
    ((endHour - startHour) * 60 + (endMinutes - startMinutes)) / 30 * SLOT_HEIGHT
  );

  // Navigate week
  function prevWeek() {
    const newStart = new Date(weekStart);
    newStart.setDate(newStart.getDate() - 7);
    weekStart = newStart;
  }

  function nextWeek() {
    const newStart = new Date(weekStart);
    newStart.setDate(newStart.getDate() + 7);
    weekStart = newStart;
  }

  // Select day
  function selectDay(date: Date) {
    selectedDate = new Date(date);
    emitTimeChange();
  }

  // Emit time change to parent
  function emitTimeChange() {
    const start = new Date(selectedDate);
    start.setHours(startHour, startMinutes, 0, 0);
    
    const end = new Date(selectedDate);
    end.setHours(endHour, endMinutes, 0, 0);
    
    onTimeChange(start, end);
  }

  // Click on time slot to set start time
  function handleSlotClick(hour: number) {
    startHour = hour;
    startMinutes = 0;
    // Default 1 hour duration
    endHour = hour + 1;
    endMinutes = 0;
    if (endHour >= 24) {
      endHour = 23;
      endMinutes = 30;
    }
    emitTimeChange();
  }

  // Drag handlers
  function handleMouseDown(e: MouseEvent, type: 'move' | 'top' | 'bottom') {
    e.preventDefault();
    e.stopPropagation();
    isDragging = true;
    dragType = type;
    dragStartY = e.clientY;
    dragStartTop = indicatorTop;
    dragStartBottom = indicatorTop + indicatorHeight;
    
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', handleMouseUp);
  }

  function handleMouseMove(e: MouseEvent) {
    if (!isDragging || !dragType) return;
    
    const deltaY = e.clientY - dragStartY;
    const slotDelta = Math.round(deltaY / SLOT_HEIGHT);
    
    if (dragType === 'move') {
      // Move entire block
      let newStartSlots = Math.round(dragStartTop / SLOT_HEIGHT) + slotDelta;
      const durationSlots = Math.round(indicatorHeight / SLOT_HEIGHT);
      
      // Clamp to bounds
      newStartSlots = Math.max(0, Math.min(newStartSlots, (END_HOUR - START_HOUR) * 2 - durationSlots));
      
      const newStartMinutes = newStartSlots * 30;
      startHour = Math.floor(newStartMinutes / 60) + START_HOUR;
      startMinutes = newStartMinutes % 60;
      
      const newEndMinutes = (newStartSlots + durationSlots) * 30;
      endHour = Math.floor(newEndMinutes / 60) + START_HOUR;
      endMinutes = newEndMinutes % 60;
    } else if (dragType === 'top') {
      // Resize from top
      let newTopSlots = Math.round(dragStartTop / SLOT_HEIGHT) + slotDelta;
      const bottomSlots = Math.round(dragStartBottom / SLOT_HEIGHT);
      
      // Min 30 min duration, max at end
      newTopSlots = Math.max(0, Math.min(newTopSlots, bottomSlots - 1));
      
      const newStartMinutes = newTopSlots * 30;
      startHour = Math.floor(newStartMinutes / 60) + START_HOUR;
      startMinutes = newStartMinutes % 60;
    } else if (dragType === 'bottom') {
      // Resize from bottom
      let newBottomSlots = Math.round(dragStartBottom / SLOT_HEIGHT) + slotDelta;
      const topSlots = Math.round(dragStartTop / SLOT_HEIGHT);
      
      // Min 30 min duration, max at 24:00
      newBottomSlots = Math.max(topSlots + 1, Math.min(newBottomSlots, (END_HOUR - START_HOUR) * 2));
      
      const newEndMinutes = newBottomSlots * 30;
      endHour = Math.floor(newEndMinutes / 60) + START_HOUR;
      endMinutes = newEndMinutes % 60;
    }
  }

  function handleMouseUp() {
    if (isDragging) {
      emitTimeChange();
    }
    isDragging = false;
    dragType = null;
    window.removeEventListener('mousemove', handleMouseMove);
    window.removeEventListener('mouseup', handleMouseUp);
  }

  // Sync from props when they change externally
  $effect(() => {
    const newDate = new Date(startTime);
    if (!isSameDay(selectedDate, newDate)) {
      selectedDate = newDate;
      weekStart = getWeekStart(newDate);
    }
    startHour = startTime.getHours();
    startMinutes = Math.floor(startTime.getMinutes() / 30) * 30;
    endHour = endTime.getHours();
    endMinutes = Math.floor(endTime.getMinutes() / 30) * 30;
  });
</script>

<div class="scheduler">
  <!-- Week Navigation -->
  <div class="scheduler-week-nav">
    <button type="button" class="scheduler-nav-btn" onclick={prevWeek} aria-label="Previous week">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="15 18 9 12 15 6"></polyline>
      </svg>
    </button>
    
    <div class="scheduler-week-days">
      {#each weekDays as day}
        <button 
          type="button"
          class="scheduler-day" 
          class:selected={isSameDay(day, selectedDate)}
          onclick={() => selectDay(day)}
        >
          <span class="day-abbrev">{formatDayAbbrev(day)}</span>
          <span class="day-num" class:today={isToday(day)} class:selected={isSameDay(day, selectedDate)}>
            {day.getDate()}
          </span>
        </button>
      {/each}
    </div>
    
    <button type="button" class="scheduler-nav-btn" onclick={nextWeek} aria-label="Next week">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="9 18 15 12 9 6"></polyline>
      </svg>
    </button>
  </div>

  <!-- Time Grid -->
  <div class="scheduler-time-grid" class:dragging={isDragging}>
    {#each hours as hour}
      <button 
        type="button"
        class="time-slot" 
        onclick={() => handleSlotClick(hour)}
        aria-label={`Select ${formatHour(hour)}`}
      >
        <span class="time-label">{formatHour(hour)}</span>
        <span class="time-slot-line"></span>
      </button>
    {/each}
    
    <!-- Time Range Indicator -->
    <div 
      class="time-range-indicator"
      class:dragging={isDragging && dragType === 'move'}
      style="top: {indicatorTop}px; height: {indicatorHeight}px;"
      onmousedown={(e) => handleMouseDown(e, 'move')}
      role="slider"
      aria-label="Event time range"
      aria-valuemin={0}
      aria-valuemax={24}
      aria-valuenow={startHour}
      tabindex="0"
    >
      <div 
        class="range-handle top" 
        onmousedown={(e) => handleMouseDown(e, 'top')}
        role="button"
        aria-label="Adjust start time"
        tabindex="0"
      ></div>
      <div class="range-content">
        <span class="range-time">{formatTimeRange(startHour, startMinutes, endHour, endMinutes)}</span>
        <span class="range-duration">{getDuration(startHour, startMinutes, endHour, endMinutes)}</span>
      </div>
      <div 
        class="range-handle bottom" 
        onmousedown={(e) => handleMouseDown(e, 'bottom')}
        role="button"
        aria-label="Adjust end time"
        tabindex="0"
      ></div>
    </div>
  </div>
</div>

<style>
  .scheduler {
    display: flex;
    flex-direction: column;
    background-color: var(--bg-secondary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    overflow: hidden;
  }

  /* Week Navigation */
  .scheduler-week-nav {
    display: flex;
    align-items: center;
    padding: 16px;
    border-bottom: 1px solid var(--bg-hover);
    gap: 8px;
  }

  .scheduler-nav-btn {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: transparent;
    border: none;
    border-radius: 6px;
    color: #e0e0e0;
    cursor: pointer;
    transition: background-color 0.15s;
    box-shadow: none;
    padding: 0;
    flex-shrink: 0;
  }

  .scheduler-nav-btn:hover {
    background-color: var(--bg-hover);
  }

  .scheduler-week-days {
    display: flex;
    flex: 1;
    justify-content: space-around;
    gap: 4px;
  }

  .scheduler-day {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    cursor: pointer;
    padding: 6px 10px;
    border-radius: 8px;
    transition: background-color 0.15s;
    background: transparent;
    border: none;
    box-shadow: none;
  }

  .scheduler-day:hover {
    background-color: var(--bg-hover);
  }

  .day-abbrev {
    font-size: 11px;
    color: var(--text-tertiary);
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .day-num {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    color: #e0e0e0;
    border-radius: 50%;
    transition: background-color 0.15s, color 0.15s;
  }

  .day-num.today {
    border: 2px solid var(--accent-color);
  }

  .day-num.selected {
    background-color: var(--accent-color);
    color: #ffffff;
  }

  /* Time Grid */
  .scheduler-time-grid {
    position: relative;
    max-height: 350px;
    overflow-y: auto;
    padding: 0 16px;
    user-select: none;
  }

  .scheduler-time-grid.dragging,
  .scheduler-time-grid.dragging * {
    cursor: grabbing !important;
  }

  .time-slot {
    display: flex;
    align-items: center;
    height: 48px;
    cursor: pointer;
    position: relative;
    background: transparent;
    border: none;
    width: 100%;
    padding: 0;
    box-shadow: none;
  }

  .time-slot:hover .time-slot-line {
    background-color: #404040;
  }

  .time-label {
    width: 60px;
    font-size: 12px;
    color: var(--text-tertiary);
    flex-shrink: 0;
    text-align: left;
  }

  .time-slot-line {
    flex: 1;
    height: 1px;
    background-color: var(--bg-hover);
    transition: background-color 0.15s;
  }

  /* Time Range Indicator */
  .time-range-indicator {
    position: absolute;
    left: 76px;
    right: 16px;
    background-color: var(--accent-color);
    border-radius: 8px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding: 8px 16px;
    z-index: 10;
    box-shadow: 0 4px 12px rgba(124, 58, 237, 0.4);
    cursor: grab;
    transition: box-shadow 0.15s;
  }

  .time-range-indicator:hover {
    box-shadow: 0 6px 20px rgba(124, 58, 237, 0.5);
  }

  .time-range-indicator.dragging {
    cursor: grabbing;
    box-shadow: 0 8px 30px rgba(124, 58, 237, 0.6);
    z-index: 100;
  }

  .range-handle {
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    width: 12px;
    height: 12px;
    background-color: var(--bg-secondary);
    border: 2px solid var(--accent-color);
    border-radius: 50%;
    cursor: ns-resize;
    opacity: 0;
    transition: opacity 0.15s, transform 0.15s;
  }

  .time-range-indicator:hover .range-handle,
  .time-range-indicator.dragging .range-handle {
    opacity: 1;
  }

  .range-handle:hover {
    transform: translateX(-50%) scale(1.2);
    background-color: var(--accent-color);
    border-color: #ffffff;
  }

  .range-handle.top {
    top: -6px;
  }

  .range-handle.bottom {
    bottom: -6px;
  }

  .range-content {
    color: #ffffff;
    pointer-events: none;
  }

  .range-time {
    font-size: 14px;
    font-weight: 500;
    display: block;
  }

  .range-duration {
    font-size: 12px;
    font-weight: 400;
    color: rgba(255, 255, 255, 0.7);
    margin-top: 2px;
    display: block;
  }

  /* Scrollbar styling */
  .scheduler-time-grid::-webkit-scrollbar {
    width: 8px;
  }

  .scheduler-time-grid::-webkit-scrollbar-track {
    background: var(--bg-tertiary);
    border-radius: 4px;
  }

  .scheduler-time-grid::-webkit-scrollbar-thumb {
    background: #505050;
    border-radius: 4px;
  }

  .scheduler-time-grid::-webkit-scrollbar-thumb:hover {
    background: #606060;
  }
</style>
