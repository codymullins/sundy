<script lang="ts">
  import { useUI, useSettings, useCalendars } from '$lib/stores';
  import { getCalendarDisplayName } from '$lib/tauri/types';
  import Dialog from '$lib/components/ui/Dialog.svelte';
  import Button from '$lib/components/ui/Button.svelte';
  import Switch from '$lib/components/ui/Switch.svelte';
  import Select from '$lib/components/ui/Select.svelte';

  const ui = useUI();
  const settings = useSettings();
  const calendars = useCalendars();

  // Tabs
  type Tab = 'general' | 'calendars' | 'privacy' | 'about';
  let activeTab = $state<Tab>('general');

  const tabs: { id: Tab; label: string }[] = [
    { id: 'general', label: 'General' },
    { id: 'calendars', label: 'Calendars' },
    { id: 'privacy', label: 'Privacy' },
    { id: 'about', label: 'About' },
  ];

  // Sync interval options
  const syncIntervalOptions = [
    { value: '1', label: '1 minute' },
    { value: '5', label: '5 minutes' },
    { value: '15', label: '15 minutes' },
    { value: '30', label: '30 minutes' },
    { value: '60', label: '1 hour' },
  ];

  // Local state bound to settings
  let syncInterval = $state(String(settings.syncIntervalMinutes));
  let privacyMode = $state(settings.privacyMode);
  let hideEmails = $state(settings.privacyHideEmails);
  let hideEventTitles = $state(settings.privacyHideEventTitles);
  let collapsePastEvents = $state(settings.collapsePastEvents);
  let dynamicViewEnabled = $state(settings.dynamicViewEnabled);

  // Sync local state with settings store
  $effect(() => {
    syncInterval = String(settings.syncIntervalMinutes);
    privacyMode = settings.privacyMode;
    hideEmails = settings.privacyHideEmails;
    hideEventTitles = settings.privacyHideEventTitles;
    collapsePastEvents = settings.collapsePastEvents;
    dynamicViewEnabled = settings.dynamicViewEnabled;
  });

  // Update settings when local state changes
  async function updateSyncInterval(value: string) {
    syncInterval = value;
    await settings.set('syncIntervalMinutes', Number(value));
  }

  async function updatePrivacyMode(value: boolean) {
    privacyMode = value;
    await settings.set('privacyMode', value);
  }

  async function updateHideEmails(value: boolean) {
    hideEmails = value;
    await settings.set('privacyHideEmails', value);
  }

  async function updateHideEventTitles(value: boolean) {
    hideEventTitles = value;
    await settings.set('privacyHideEventTitles', value);
  }

  async function updateCollapsePastEvents(value: boolean) {
    collapsePastEvents = value;
    await settings.set('collapsePastEvents', value);
  }

  async function updateDynamicViewEnabled(value: boolean) {
    dynamicViewEnabled = value;
    await settings.set('dynamicViewEnabled', value);
  }

  // Reset active tab when dialog closes
  $effect(() => {
    if (!ui.showSettingsModal) {
      activeTab = 'general';
    }
  });
</script>

<Dialog 
  bind:open={ui.showSettingsModal} 
  onOpenChange={(open) => !open && ui.closeSettingsModal()}
  title="Settings"
>
  <div class="settings-layout">
    <nav class="settings-tabs">
      {#each tabs as tab}
        <button 
          class="tab-button" 
          class:active={activeTab === tab.id}
          onclick={() => activeTab = tab.id}
        >
          {tab.label}
        </button>
      {/each}
    </nav>

    <div class="settings-content">
      {#if activeTab === 'general'}
        <div class="settings-section">
          <h3>Sync</h3>
          <Select
            label="Sync interval"
            options={syncIntervalOptions}
            value={syncInterval}
            onValueChange={updateSyncInterval}
          />
        </div>

        <div class="settings-section">
          <h3>View</h3>
          <Switch
            label="Collapse past events"
            checked={collapsePastEvents}
            onCheckedChange={updateCollapsePastEvents}
          />
          <Switch
            label="Enable dynamic view"
            checked={dynamicViewEnabled}
            onCheckedChange={updateDynamicViewEnabled}
          />
        </div>
      {/if}

      {#if activeTab === 'calendars'}
        <div class="settings-section">
          <h3>My Calendars</h3>
          {#if calendars.calendars.length === 0}
            <p class="empty-message">No calendars found</p>
          {:else}
            <div class="calendar-list">
              {#each calendars.calendars as calendar (calendar.id)}
                <div class="calendar-item">
                  <span class="color-dot" style="background-color: {calendar.color}"></span>
                  <span class="calendar-name">{getCalendarDisplayName(calendar)}</span>
                  <span class="calendar-type">{calendar.calendarType}</span>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      {/if}

      {#if activeTab === 'privacy'}
        <div class="settings-section">
          <h3>Privacy Mode</h3>
          <p class="section-description">
            Privacy mode helps protect sensitive information when screen sharing or in public.
          </p>
          <Switch
            label="Enable privacy mode"
            checked={privacyMode}
            onCheckedChange={updatePrivacyMode}
          />
          
          {#if privacyMode}
            <div class="sub-settings">
              <Switch
                label="Hide email addresses"
                checked={hideEmails}
                onCheckedChange={updateHideEmails}
              />
              <Switch
                label="Hide event titles"
                checked={hideEventTitles}
                onCheckedChange={updateHideEventTitles}
              />
            </div>
          {/if}
        </div>
      {/if}

      {#if activeTab === 'about'}
        <div class="settings-section">
          <h3>Sundycal</h3>
          <p class="version">Version 0.1.0</p>
          <p class="description">
            A modern calendar application built with Tauri and Svelte.
          </p>
        </div>
      {/if}
    </div>
  </div>

  {#snippet footer()}
    <Button variant="secondary" onclick={() => ui.closeSettingsModal()}>
      Close
    </Button>
  {/snippet}
</Dialog>

<style>
  .settings-layout {
    display: flex;
    gap: 0;
    min-height: 350px;
  }

  .settings-tabs {
    display: flex;
    flex-direction: column;
    gap: 0;
    width: 160px;
    background-color: var(--bg-secondary);
    border-right: 1px solid var(--bg-hover);
    margin: -24px;
    margin-right: 0;
    padding: 0;
  }

  .tab-button {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px 20px;
    border: none;
    background: transparent;
    text-align: left;
    font-size: 14px;
    color: var(--text-secondary);
    cursor: pointer;
    transition: background-color 0.15s, color 0.15s;
    border-left: 3px solid transparent;
    box-shadow: none;
  }

  .tab-button:hover {
    background-color: var(--bg-tertiary);
    color: #e0e0e0;
  }

  .tab-button.active {
    background-color: var(--bg-tertiary);
    color: #ffffff;
    border-left-color: var(--accent-color);
  }

  .settings-content {
    flex: 1;
    min-width: 0;
    padding: 0 24px;
    overflow-y: auto;
  }

  .settings-section {
    margin-bottom: 32px;
  }

  .settings-section h3 {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-tertiary);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin: 0 0 16px;
  }

  .section-description {
    font-size: 13px;
    color: var(--text-secondary);
    margin: 0 0 16px;
    line-height: 1.5;
  }

  .settings-section > :global(.switch-wrapper),
  .settings-section > :global(.select-wrapper) {
    margin-bottom: 12px;
  }

  .sub-settings {
    margin-top: 16px;
    padding-left: 24px;
    border-left: 2px solid var(--accent-color);
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .empty-message {
    font-size: 14px;
    color: var(--text-secondary);
  }

  .calendar-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .calendar-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 16px;
    background-color: var(--bg-secondary);
    border-radius: 8px;
  }

  .color-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  .calendar-name {
    flex: 1;
    font-size: 14px;
    color: #e0e0e0;
  }

  .calendar-type {
    font-size: 12px;
    color: var(--text-tertiary);
    background-color: var(--bg-tertiary);
    padding: 4px 10px;
    border-radius: 12px;
  }

  .version {
    font-size: 14px;
    color: var(--text-secondary);
    margin: 0 0 8px;
  }

  .description {
    font-size: 14px;
    color: var(--text-secondary);
    margin: 0;
    line-height: 1.5;
  }
</style>
