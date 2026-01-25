<script lang="ts">
  import { useUI, useCalendars, useEvents, useToasts } from '$lib/stores';
  import type { NewEvent, CalendarEvent } from '$lib/tauri/types';
  import { getCalendarDisplayName } from '$lib/tauri/types';
  import { getDefaultEndTime } from '$lib/utils/date';
  import Dialog from '$lib/components/ui/Dialog.svelte';
  import Button from '$lib/components/ui/Button.svelte';
  import Input from '$lib/components/ui/Input.svelte';
  import Textarea from '$lib/components/ui/Textarea.svelte';
  import Select from '$lib/components/ui/Select.svelte';
  import Checkbox from '$lib/components/ui/Checkbox.svelte';
  import TimeScheduler from '$lib/components/ui/TimeScheduler.svelte';

  const ui = useUI();
  const calendars = useCalendars();
  const events = useEvents();
  const toasts = useToasts();

  // Form state
  let title = $state('');
  let description = $state('');
  let location = $state('');
  let calendarId = $state('');
  let startDateTime = $state(new Date());
  let endDateTime = $state(new Date());
  let isAllDay = $state(false);
  let saving = $state(false);
  let error = $state<string | null>(null);

  // Determine if editing existing event
  const isEditing = $derived(!!ui.editingEvent);
  const dialogTitle = $derived(isEditing ? 'Edit Event' : 'New Event');

  // Calendar options for select
  const calendarOptions = $derived(
    calendars.calendars.map(cal => ({
      value: cal.id,
      label: getCalendarDisplayName(cal),
      color: cal.color,
    }))
  );

  // Get selected calendar color
  const selectedCalendarColor = $derived(
    calendars.calendars.find(c => c.id === calendarId)?.color ?? '#7c3aed'
  );

  // Initialize form when dialog opens or editing event changes
  $effect(() => {
    if (ui.showEventModal) {
      if (ui.editingEvent) {
        // Editing existing event
        const event = ui.editingEvent;
        title = event.title;
        description = event.description ?? '';
        location = event.location ?? '';
        calendarId = event.calendarId;
        isAllDay = event.isAllDay;
        startDateTime = new Date(event.startTime);
        endDateTime = new Date(event.endTime);
      } else {
        // New event
        title = '';
        description = '';
        location = '';
        calendarId = calendars.visibleCalendarIds[0] || calendars.calendars[0]?.id || '';
        isAllDay = false;
        
        // Use selected date/time if available
        let start: Date;
        if (ui.selectedStartTime) {
          start = new Date(ui.selectedStartTime);
        } else if (ui.selectedDate) {
          start = new Date(ui.selectedDate);
          start.setHours(9, 0, 0, 0); // Default to 9 AM
        } else {
          start = new Date();
          // Round to next hour
          start.setMinutes(0, 0, 0);
          start.setHours(start.getHours() + 1);
        }
        
        startDateTime = start;
        endDateTime = getDefaultEndTime(start);
      }
      error = null;
    }
  });

  // Handle time change from scheduler
  function handleTimeChange(start: Date, end: Date) {
    startDateTime = start;
    endDateTime = end;
  }

  // Format date for display in all-day mode
  function formatDateDisplay(date: Date): string {
    return date.toLocaleDateString('en-US', { 
      weekday: 'long',
      month: 'long', 
      day: 'numeric',
      year: 'numeric'
    });
  }

  // Format date for input
  function toDateString(date: Date): string {
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }

  async function handleSave() {
    // Validate
    if (!title.trim()) {
      error = 'Title is required';
      return;
    }
    
    if (!calendarId) {
      error = 'Please select a calendar';
      return;
    }

    saving = true;
    error = null;

    try {
      let finalStartTime: Date;
      let finalEndTime: Date;

      if (isAllDay) {
        finalStartTime = new Date(startDateTime);
        finalStartTime.setHours(0, 0, 0, 0);
        finalEndTime = new Date(startDateTime);
        finalEndTime.setHours(23, 59, 59, 999);
      } else {
        finalStartTime = startDateTime;
        finalEndTime = endDateTime;
      }

      if (finalEndTime <= finalStartTime && !isAllDay) {
        error = 'End time must be after start time';
        saving = false;
        return;
      }

      if (isEditing && ui.editingEvent) {
        // Update existing event
        const updatedEvent: CalendarEvent = {
          ...ui.editingEvent,
          title: title.trim(),
          description: description.trim() || null,
          location: location.trim() || null,
          calendarId,
          startTime: finalStartTime.toISOString(),
          endTime: finalEndTime.toISOString(),
          isAllDay,
        };
        
        const success = await events.update(updatedEvent);
        if (!success) {
          error = events.error || 'Failed to update event';
          return;
        }
        toasts.success('Event updated');
      } else {
        // Create new event
        const newEvent: NewEvent = {
          calendarId,
          title: title.trim(),
          description: description.trim() || undefined,
          location: location.trim() || undefined,
          startTime: finalStartTime.toISOString(),
          endTime: finalEndTime.toISOString(),
          isAllDay,
        };
        
        const created = await events.create(newEvent);
        if (!created) {
          error = events.error || 'Failed to create event';
          return;
        }
        toasts.success('Event created');
      }

      ui.closeEventModal();
    } catch (e) {
      error = e instanceof Error ? e.message : 'An error occurred';
    } finally {
      saving = false;
    }
  }

  async function handleDelete() {
    if (!ui.editingEvent) return;
    
    if (!confirm('Are you sure you want to delete this event?')) return;

    saving = true;
    error = null;

    try {
      const success = await events.remove(ui.editingEvent.id);
      if (success) {
        toasts.success('Event deleted');
        ui.closeEventModal();
      } else {
        error = events.error || 'Failed to delete event';
      }
    } catch (e) {
      error = e instanceof Error ? e.message : 'An error occurred';
    } finally {
      saving = false;
    }
  }
</script>

<Dialog 
  bind:open={ui.showEventModal} 
  onOpenChange={(open) => !open && ui.closeEventModal()}
  title={dialogTitle}
>
  <form class="event-form" onsubmit={(e) => { e.preventDefault(); handleSave(); }}>
    {#if error}
      <div class="form-error">{error}</div>
    {/if}

    <Input
      label="Title"
      placeholder="Add title"
      bind:value={title}
      required
    />

    <Select
      label="Calendar"
      options={calendarOptions}
      bind:value={calendarId}
    />

    <Checkbox
      bind:checked={isAllDay}
      label="All day"
    />

    {#if isAllDay}
      <!-- All-day mode: just show date picker -->
      <div class="datetime-picker">
        <div class="datetime-icon" style="background-color: {selectedCalendarColor}">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="16" y1="2" x2="16" y2="6"></line>
            <line x1="8" y1="2" x2="8" y2="6"></line>
            <line x1="3" y1="10" x2="21" y2="10"></line>
          </svg>
        </div>
        <div class="datetime-details">
          <span class="datetime-date">{formatDateDisplay(startDateTime)}</span>
          <span class="datetime-duration">All day</span>
        </div>
        <input 
          type="date" 
          class="hidden-date-input"
          value={toDateString(startDateTime)}
          onchange={(e) => {
            const newDate = new Date(e.currentTarget.value + 'T00:00:00');
            startDateTime = newDate;
            endDateTime = newDate;
          }}
        />
      </div>
    {:else}
      <!-- Time scheduler for specific times -->
      <TimeScheduler 
        startTime={startDateTime}
        endTime={endDateTime}
        onTimeChange={handleTimeChange}
      />
    {/if}

    <Input
      label="Location"
      placeholder="Add location"
      bind:value={location}
    />

    <Textarea
      label="Description"
      placeholder="Add description"
      bind:value={description}
    />
  </form>

  {#snippet footer()}
    <div class="dialog-actions">
      {#if isEditing}
        <Button 
          variant="danger" 
          onclick={handleDelete}
          disabled={saving}
        >
          Delete
        </Button>
      {/if}
      <div class="dialog-actions-right">
        <Button 
          variant="secondary" 
          onclick={() => ui.closeEventModal()}
          disabled={saving}
        >
          Cancel
        </Button>
        <Button 
          variant="primary" 
          onclick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : (isEditing ? 'Save' : 'Create')}
        </Button>
      </div>
    </div>
  {/snippet}
</Dialog>

<style>
  .event-form {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .form-error {
    padding: 12px 14px;
    background-color: rgba(255, 107, 107, 0.1);
    border: 1px solid rgba(255, 107, 107, 0.3);
    border-radius: 8px;
    color: #ff6b6b;
    font-size: 14px;
  }

  .datetime-picker {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 16px;
    background-color: var(--bg-secondary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    cursor: pointer;
    transition: border-color 0.15s;
    position: relative;
  }

  .datetime-picker:hover {
    border-color: #505050;
  }

  .datetime-icon {
    width: 48px;
    height: 48px;
    background-color: var(--accent-color);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    color: #ffffff;
  }

  .datetime-details {
    flex: 1;
  }

  .datetime-date {
    font-size: 15px;
    font-weight: 500;
    color: var(--text-primary);
    display: block;
    margin-bottom: 4px;
  }

  .datetime-duration {
    font-size: 13px;
    color: var(--text-tertiary);
    display: block;
  }

  .hidden-date-input {
    position: absolute;
    inset: 0;
    opacity: 0;
    cursor: pointer;
    width: 100%;
    height: 100%;
  }

  .dialog-actions {
    display: flex;
    justify-content: space-between;
    width: 100%;
  }

  .dialog-actions-right {
    display: flex;
    gap: 12px;
    margin-left: auto;
  }
</style>
