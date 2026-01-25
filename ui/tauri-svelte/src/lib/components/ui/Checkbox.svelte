<script lang="ts">
  import { Checkbox } from 'bits-ui';

  interface Props {
    checked: boolean;
    onCheckedChange?: (checked: boolean) => void;
    label?: string;
    disabled?: boolean;
  }

  let { 
    checked = $bindable(), 
    onCheckedChange,
    label,
    disabled = false,
  }: Props = $props();

  function handleCheckedChange(newChecked: boolean | 'indeterminate') {
    if (typeof newChecked === 'boolean') {
      checked = newChecked;
      onCheckedChange?.(newChecked);
    }
  }
</script>

<label class="checkbox-wrapper" class:disabled>
  <Checkbox.Root 
    class="checkbox" 
    {checked} 
    onCheckedChange={handleCheckedChange}
    {disabled}
  >
    {#snippet children({ checked })}
      {#if checked}
        <svg class="checkbox-indicator" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="20 6 9 17 4 12"></polyline>
        </svg>
      {/if}
    {/snippet}
  </Checkbox.Root>
  {#if label}
    <span class="checkbox-label">{label}</span>
  {/if}
</label>

<style>
  .checkbox-wrapper {
    display: flex;
    align-items: center;
    gap: 10px;
    cursor: pointer;
  }

  .checkbox-wrapper.disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  :global(.checkbox) {
    width: 20px;
    height: 20px;
    border: 2px solid var(--bg-hover);
    border-radius: 4px;
    background-color: var(--bg-secondary);
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.15s;
    box-shadow: none;
  }

  :global(.checkbox:hover) {
    border-color: #505050;
  }

  :global(.checkbox[data-state="checked"]) {
    background-color: var(--accent-color);
    border-color: var(--accent-color);
  }

  :global(.checkbox:focus-visible) {
    outline: none;
    border-color: var(--accent-color);
  }

  .checkbox-indicator {
    color: white;
  }

  .checkbox-label {
    font-size: 14px;
    color: #e0e0e0;
    user-select: none;
  }
</style>
