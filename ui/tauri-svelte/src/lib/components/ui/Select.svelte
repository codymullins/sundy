<script lang="ts">
  import { Select } from 'bits-ui';

  interface Option {
    value: string;
    label: string;
    color?: string;
  }

  interface Props {
    options: Option[];
    value: string;
    onValueChange?: (value: string) => void;
    label?: string;
    placeholder?: string;
  }

  let { 
    options, 
    value = $bindable(), 
    onValueChange,
    label,
    placeholder = 'Select...',
  }: Props = $props();

  const selectedOption = $derived(options.find(o => o.value === value));

  function handleValueChange(newValue: string | undefined) {
    if (newValue) {
      value = newValue;
      onValueChange?.(newValue);
    }
  }
</script>

<div class="select-wrapper">
  {#if label}
    <span class="select-label">{label}</span>
  {/if}
  
  <Select.Root type="single" {value} onValueChange={handleValueChange}>
    <Select.Trigger class="select-trigger">
      {#if selectedOption}
        <span class="select-value">
          {#if selectedOption.color}
            <span class="color-dot" style="background-color: {selectedOption.color}"></span>
          {/if}
          {selectedOption.label}
        </span>
      {:else}
        <span class="select-placeholder">{placeholder}</span>
      {/if}
      <svg class="select-icon" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="6 9 12 15 18 9"></polyline>
      </svg>
    </Select.Trigger>
    
    <Select.Portal>
      <Select.Content class="select-content" sideOffset={4}>
        {#each options as option (option.value)}
          <Select.Item class="select-item" value={option.value}>
            {#if option.color}
              <span class="color-dot" style="background-color: {option.color}"></span>
            {/if}
            {option.label}
          </Select.Item>
        {/each}
      </Select.Content>
    </Select.Portal>
  </Select.Root>
</div>

<style>
  .select-wrapper {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .select-label {
    font-size: 14px;
    font-weight: 500;
    color: #e0e0e0;
  }

  :global(.select-trigger) {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    padding: 12px 14px;
    background-color: var(--bg-secondary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    cursor: pointer;
    font-size: 14px;
    transition: border-color 0.15s;
    box-shadow: none;
  }

  :global(.select-trigger:hover) {
    border-color: #505050;
  }

  :global(.select-trigger:focus) {
    outline: none;
    border-color: var(--accent-color);
  }

  .select-value {
    display: flex;
    align-items: center;
    gap: 10px;
    color: var(--text-primary);
  }

  .select-placeholder {
    color: var(--text-muted);
  }

  .select-icon {
    color: var(--text-tertiary);
  }

  .color-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  :global(.select-content) {
    background-color: var(--bg-tertiary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
    padding: 4px;
    min-width: var(--bits-select-trigger-width);
    z-index: 1002;
    animation: fadeIn 0.1s ease;
    overflow: hidden;
  }

  :global(.select-item) {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 14px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    color: #e0e0e0;
    transition: background-color 0.15s;
  }

  :global(.select-item:hover),
  :global(.select-item[data-highlighted]) {
    background-color: var(--bg-hover);
  }

  :global(.select-item[data-selected]) {
    background-color: var(--accent-color);
    color: #ffffff;
  }

  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
</style>
