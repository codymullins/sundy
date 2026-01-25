<script lang="ts">
  import { useToasts, type Toast } from '$lib/stores/toasts.svelte';
  import { fly, fade } from 'svelte/transition';

  const toasts = useToasts();

  function getIcon(type: Toast['type']): string {
    switch (type) {
      case 'success':
        return 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z';
      case 'error':
        return 'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z';
      case 'warning':
        return 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z';
      case 'info':
      default:
        return 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z';
    }
  }
</script>

<div class="toast-container" role="region" aria-label="Notifications">
  {#each toasts.toasts as toast (toast.id)}
    <div 
      class="toast toast-{toast.type}"
      role="alert"
      in:fly={{ x: 300, duration: 300 }}
      out:fade={{ duration: 200 }}
    >
      <div class="toast-icon">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d={getIcon(toast.type)} />
        </svg>
      </div>
      <span class="toast-message">{toast.message}</span>
      {#if toast.dismissible}
        <button 
          class="toast-close" 
          onclick={() => toasts.remove(toast.id)}
          aria-label="Dismiss notification"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      {/if}
    </div>
  {/each}
</div>

<style>
  .toast-container {
    position: fixed;
    bottom: 20px;
    right: 20px;
    z-index: 9999;
    display: flex;
    flex-direction: column;
    gap: 8px;
    max-width: 400px;
    pointer-events: none;
  }

  .toast {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px 18px;
    border-radius: 12px;
    background-color: var(--bg-tertiary);
    border: 1px solid var(--bg-hover);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
    pointer-events: auto;
    animation: slideIn 0.3s ease-out;
  }

  .toast-icon {
    flex-shrink: 0;
    width: 24px;
    height: 24px;
  }

  .toast-icon svg {
    width: 100%;
    height: 100%;
  }

  .toast-message {
    flex: 1;
    font-size: 14px;
    font-weight: 500;
    color: #e0e0e0;
    line-height: 1.4;
  }

  .toast-close {
    flex-shrink: 0;
    width: 24px;
    height: 24px;
    padding: 0;
    border: none;
    background: transparent;
    color: var(--text-tertiary);
    cursor: pointer;
    border-radius: 6px;
    transition: color 0.15s, background-color 0.15s;
    box-shadow: none;
  }

  .toast-close:hover {
    color: var(--text-primary);
    background-color: var(--bg-hover);
  }

  .toast-close svg {
    width: 100%;
    height: 100%;
  }

  /* Toast variants */
  .toast-success {
    border-left: 4px solid #22c55e;
  }

  .toast-success .toast-icon {
    color: #22c55e;
  }

  .toast-error {
    border-left: 4px solid #ff6b6b;
  }

  .toast-error .toast-icon {
    color: #ff6b6b;
  }

  .toast-warning {
    border-left: 4px solid #f59e0b;
  }

  .toast-warning .toast-icon {
    color: #f59e0b;
  }

  .toast-info {
    border-left: 4px solid var(--accent-color);
  }

  .toast-info .toast-icon {
    color: var(--accent-color);
  }

  /* Mobile positioning */
  @media (max-width: 480px) {
    .toast-container {
      left: 12px;
      right: 12px;
      bottom: 12px;
      max-width: none;
    }
  }

  @keyframes slideIn {
    from {
      transform: translateX(100%);
      opacity: 0;
    }
    to {
      transform: translateX(0);
      opacity: 1;
    }
  }
</style>
