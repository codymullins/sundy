<script lang="ts">
  import { Switch } from 'bits-ui';

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

  function handleCheckedChange(newChecked: boolean) {
    checked = newChecked;
    onCheckedChange?.(newChecked);
  }
</script>

<label class="switch-wrapper" class:disabled>
  {#if label}
    <span class="switch-label">{label}</span>
  {/if}
  <Switch.Root 
    class="switch" 
    {checked} 
    onCheckedChange={handleCheckedChange}
    {disabled}
  >
    <Switch.Thumb class="switch-thumb" />
  </Switch.Root>
</label>

<style>
  .switch-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    cursor: pointer;
  }

  .switch-wrapper.disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .switch-label {
    font-size: 14px;
    color: #e0e0e0;
    user-select: none;
  }

  :global(.switch) {
    width: 44px;
    height: 24px;
    background-color: var(--bg-hover);
    border-radius: 12px;
    position: relative;
    transition: background-color 0.2s;
    flex-shrink: 0;
    border: none;
    box-shadow: none;
  }

  :global(.switch:hover) {
    background-color: #4a4a4a;
  }

  :global(.switch[data-state="checked"]) {
    background-color: var(--accent-color);
  }

  :global(.switch[data-state="checked"]:hover) {
    background-color: var(--accent-color-hover);
  }

  :global(.switch:focus-visible) {
    outline: none;
    box-shadow: 0 0 0 2px rgba(124, 58, 237, 0.3);
  }

  :global(.switch-thumb) {
    width: 20px;
    height: 20px;
    background: white;
    border-radius: 50%;
    position: absolute;
    top: 2px;
    left: 2px;
    transition: transform 0.2s;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  }

  :global(.switch[data-state="checked"] .switch-thumb) {
    transform: translateX(20px);
  }
</style>
