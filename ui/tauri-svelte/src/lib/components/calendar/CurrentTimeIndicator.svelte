<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { formatTime } from '$lib/utils/date';

  interface Props {
    /** Height of each hour slot in pixels */
    hourHeight?: number;
    /** Whether to show the time label */
    showLabel?: boolean;
    /** Whether to show just a dot (for compact views) */
    compact?: boolean;
  }

  let { 
    hourHeight = 48, 
    showLabel = true,
    compact = false,
  }: Props = $props();

  let currentTime = $state(new Date());
  let intervalId: ReturnType<typeof setInterval>;

  // Calculate position based on current time
  const topPosition = $derived.by(() => {
    const hours = currentTime.getHours();
    const minutes = currentTime.getMinutes();
    const totalMinutes = hours * 60 + minutes;
    return (totalMinutes / 60) * hourHeight;
  });

  // Format current time for label
  const timeLabel = $derived(formatTime(currentTime));

  // Check if we're in today's view
  const isVisible = $derived.by(() => {
    const today = new Date();
    return currentTime.toDateString() === today.toDateString();
  });

  onMount(() => {
    // Update time every minute
    intervalId = setInterval(() => {
      currentTime = new Date();
    }, 60000); // 60 seconds
  });

  onDestroy(() => {
    if (intervalId) {
      clearInterval(intervalId);
    }
  });
</script>

{#if isVisible}
  <div 
    class="current-time-indicator" 
    class:compact
    style="top: {topPosition}px;"
  >
    {#if showLabel && !compact}
      <span class="time-label">{timeLabel}</span>
    {/if}
    <div class="indicator-dot"></div>
    <div class="indicator-line"></div>
  </div>
{/if}

<style>
  .current-time-indicator {
    position: absolute;
    left: 0;
    right: 0;
    display: flex;
    align-items: center;
    z-index: 10;
    pointer-events: none;
  }

  .time-label {
    font-size: 10px;
    font-weight: 600;
    color: #ef4444;
    background: var(--bg-primary, #fff);
    padding: 0 4px;
    margin-right: 2px;
    white-space: nowrap;
  }

  .indicator-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: #ef4444;
    flex-shrink: 0;
    box-shadow: 0 0 4px rgba(239, 68, 68, 0.5);
  }

  .indicator-line {
    height: 2px;
    flex: 1;
    background: #ef4444;
    box-shadow: 0 0 4px rgba(239, 68, 68, 0.3);
  }

  /* Compact mode - just dot and line */
  .current-time-indicator.compact .indicator-dot {
    width: 6px;
    height: 6px;
  }

  .current-time-indicator.compact .indicator-line {
    height: 1px;
  }
</style>
