/**
 * Date utility functions for calendar operations
 */

/**
 * Format header text based on view and date
 */
export function formatHeaderText(date: Date, view: 'day' | 'week' | 'month' | 'dynamic'): string {
  const options: Intl.DateTimeFormatOptions = { month: 'long', year: 'numeric' };
  
  switch (view) {
    case 'day':
      return date.toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
        year: 'numeric',
      });
    
    case 'week': {
      const weekStart = getWeekStart(date);
      const weekEnd = new Date(weekStart);
      weekEnd.setDate(weekStart.getDate() + 6);
      
      if (weekStart.getMonth() === weekEnd.getMonth()) {
        return `${weekStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${weekEnd.getDate()}, ${weekEnd.getFullYear()}`;
      } else {
        return `${weekStart.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${weekEnd.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`;
      }
    }
    
    case 'month':
    case 'dynamic':
    default:
      return date.toLocaleDateString('en-US', options);
  }
}

/**
 * Get the start of the week (Sunday) for a given date
 */
export function getWeekStart(date: Date): Date {
  const result = new Date(date);
  result.setDate(date.getDate() - date.getDay());
  result.setHours(0, 0, 0, 0);
  return result;
}

/**
 * Get the 7 days of the week containing the given date
 */
export function getWeekDays(date: Date): Date[] {
  const weekStart = getWeekStart(date);
  return Array.from({ length: 7 }, (_, i) => {
    const day = new Date(weekStart);
    day.setDate(weekStart.getDate() + i);
    return day;
  });
}

/**
 * Get days for month view (42 days / 6 weeks)
 */
export function getMonthDays(date: Date): Date[] {
  const year = date.getFullYear();
  const month = date.getMonth();
  
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
}

/**
 * Get the date range for a view (for fetching events)
 */
export function getViewRange(date: Date, view: 'day' | 'week' | 'month' | 'dynamic'): { start: Date; end: Date } {
  let start: Date;
  let end: Date;

  switch (view) {
    case 'day':
      start = new Date(date);
      start.setHours(0, 0, 0, 0);
      end = new Date(date);
      end.setHours(23, 59, 59, 999);
      break;
    
    case 'week':
      start = getWeekStart(date);
      end = new Date(start);
      end.setDate(start.getDate() + 6);
      end.setHours(23, 59, 59, 999);
      break;
    
    case 'month':
    case 'dynamic':
    default:
      // Load extra for month view (prev/next month days visible)
      start = new Date(date.getFullYear(), date.getMonth(), 1);
      start.setDate(start.getDate() - 7);
      end = new Date(date.getFullYear(), date.getMonth() + 1, 0);
      end.setDate(end.getDate() + 7);
      break;
  }

  return { start, end };
}

/**
 * Check if a date is today
 */
export function isToday(date: Date): boolean {
  const today = new Date();
  return date.toDateString() === today.toDateString();
}

/**
 * Check if two dates are the same day
 */
export function isSameDay(date1: Date, date2: Date): boolean {
  return date1.toDateString() === date2.toDateString();
}

/**
 * Check if a date is in the given month
 */
export function isInMonth(date: Date, month: number, year: number): boolean {
  return date.getMonth() === month && date.getFullYear() === year;
}

/**
 * Format time from a Date object
 */
export function formatTime(date: Date): string {
  return date.toLocaleTimeString('en-US', { 
    hour: 'numeric', 
    minute: '2-digit',
    hour12: true 
  });
}

/**
 * Format time from an ISO string
 */
export function formatTimeFromISO(isoString: string): string {
  return formatTime(new Date(isoString));
}

/**
 * Format duration between two dates
 */
export function formatDuration(start: Date, end: Date): string {
  const diffMs = end.getTime() - start.getTime();
  const diffMins = Math.round(diffMs / (1000 * 60));
  
  if (diffMins < 60) {
    return `${diffMins} min`;
  }
  
  const hours = Math.floor(diffMins / 60);
  const mins = diffMins % 60;
  
  if (mins === 0) {
    return hours === 1 ? '1 hr' : `${hours} hrs`;
  }
  
  return `${hours} hr ${mins} min`;
}

/**
 * Format duration from ISO strings
 */
export function formatDurationFromISO(startISO: string, endISO: string): string {
  return formatDuration(new Date(startISO), new Date(endISO));
}

/**
 * Format hour for time column (e.g., "9 AM", "12 PM")
 */
export function formatHour(hour: number): string {
  if (hour === 0) return '12 AM';
  if (hour < 12) return `${hour} AM`;
  if (hour === 12) return '12 PM';
  return `${hour - 12} PM`;
}

/**
 * Get date string in YYYY-MM-DD format
 */
export function toDateString(date: Date): string {
  return date.toISOString().split('T')[0];
}

/**
 * Parse date string in YYYY-MM-DD format
 */
export function parseDate(dateString: string): Date {
  const [year, month, day] = dateString.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/**
 * Add days to a date
 */
export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

/**
 * Add months to a date
 */
export function addMonths(date: Date, months: number): Date {
  const result = new Date(date);
  result.setMonth(result.getMonth() + months);
  return result;
}

/**
 * Get the start of day (midnight)
 */
export function startOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);
  return result;
}

/**
 * Get the end of day (23:59:59.999)
 */
export function endOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(23, 59, 59, 999);
  return result;
}

/**
 * Create a date with specific time
 */
export function setTime(date: Date, hours: number, minutes: number = 0): Date {
  const result = new Date(date);
  result.setHours(hours, minutes, 0, 0);
  return result;
}

/**
 * Get default event end time (1 hour after start)
 */
export function getDefaultEndTime(startTime: Date): Date {
  const result = new Date(startTime);
  result.setHours(result.getHours() + 1);
  return result;
}
