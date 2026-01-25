<script lang="ts">
  import { Dialog } from 'bits-ui';
  import type { Snippet } from 'svelte';

  interface Props {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
    title?: string;
    description?: string;
    children: Snippet;
    footer?: Snippet;
  }

  let { 
    open = $bindable(), 
    onOpenChange,
    title,
    description,
    children,
    footer,
  }: Props = $props();

  function handleOpenChange(newOpen: boolean) {
    open = newOpen;
    onOpenChange?.(newOpen);
  }
</script>

<Dialog.Root bind:open onOpenChange={handleOpenChange}>
  <Dialog.Portal>
    <Dialog.Overlay class="dialog-overlay" />
    <Dialog.Content class="dialog-content">
      {#if title}
        <Dialog.Title class="dialog-title">{title}</Dialog.Title>
      {/if}
      {#if description}
        <Dialog.Description class="dialog-description">{description}</Dialog.Description>
      {/if}
      
      <div class="dialog-body">
        {@render children()}
      </div>
      
      {#if footer}
        <div class="dialog-footer">
          {@render footer()}
        </div>
      {/if}
      
      <Dialog.Close class="dialog-close">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"></line>
          <line x1="6" y1="6" x2="18" y2="18"></line>
        </svg>
      </Dialog.Close>
    </Dialog.Content>
  </Dialog.Portal>
</Dialog.Root>

<style>
  :global(.dialog-overlay) {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.6);
    z-index: 1000;
    animation: fadeIn 0.15s ease;
  }

  :global(.dialog-content) {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: var(--bg-tertiary);
    border-radius: 12px;
    border: 1px solid var(--bg-hover);
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
    max-width: 500px;
    width: calc(100% - 32px);
    max-height: calc(100vh - 64px);
    overflow-y: auto;
    z-index: 1001;
    animation: slideIn 0.2s ease;
    display: flex;
    flex-direction: column;
  }

  :global(.dialog-title) {
    font-size: 20px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 0;
    padding: 20px 24px;
    padding-right: 48px;
    border-bottom: 1px solid var(--bg-hover);
  }

  :global(.dialog-description) {
    font-size: 14px;
    color: var(--text-secondary);
    margin: 0;
    padding: 0 24px;
    padding-top: 16px;
  }

  :global(.dialog-body) {
    padding: 24px;
  }

  :global(.dialog-footer) {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding: 16px 24px;
    border-top: 1px solid var(--bg-hover);
  }

  :global(.dialog-close) {
    position: absolute;
    top: 16px;
    right: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    padding: 0;
    border: none;
    background: transparent;
    color: var(--text-secondary);
    cursor: pointer;
    border-radius: 6px;
    transition: background-color 0.15s, color 0.15s;
    box-shadow: none;
  }

  :global(.dialog-close:hover) {
    background-color: var(--bg-hover);
    color: var(--text-primary);
  }

  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }

  @keyframes slideIn {
    from { 
      opacity: 0;
      transform: translate(-50%, -48%);
    }
    to { 
      opacity: 1;
      transform: translate(-50%, -50%);
    }
  }

  /* Mobile responsive adjustments */
  @media (max-width: 640px) {
    :global(.dialog-content) {
      width: calc(100% - 24px);
      max-height: calc(100vh - 48px);
      max-height: calc(100dvh - 48px);
      border-radius: 16px;
    }

    :global(.dialog-title) {
      font-size: 18px;
      padding: 16px 20px;
      padding-right: 48px;
    }

    :global(.dialog-description) {
      padding: 0 20px;
      padding-top: 12px;
    }

    :global(.dialog-body) {
      padding: 20px;
    }

    :global(.dialog-footer) {
      padding: 12px 20px;
      flex-wrap: wrap;
    }

    :global(.dialog-close) {
      top: 12px;
      right: 12px;
    }
  }

  /* Very small screens - bottom sheet style */
  @media (max-width: 480px) {
    :global(.dialog-content) {
      top: auto;
      bottom: 0;
      left: 0;
      right: 0;
      transform: none;
      width: 100%;
      max-width: 100%;
      border-radius: 20px 20px 0 0;
      max-height: 90vh;
      max-height: 90dvh;
      animation: slideUp 0.25s ease;
    }
  }

  @keyframes slideUp {
    from { 
      opacity: 0;
      transform: translateY(100%);
    }
    to { 
      opacity: 1;
      transform: translateY(0);
    }
  }
</style>
